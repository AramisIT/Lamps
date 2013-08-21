using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using WMS_client.Enums;
using WMS_client.db;
using WMS_client.Models;
using WMS_client.Processes.Lamps.Sync;
using System.Diagnostics;
using WMS_client.Utils;

namespace WMS_client
    {
    /// <summary>Синхронизация данных между ТСД и сервером</summary>
    public class dbSynchronizer : BusinessProcess
        {
        /// <summary>
        /// Использовать для синхронизации ламп механизм логирования на сервере
        /// </summary>
        public bool useLoggingSyncronization = true;

        private IServerIdProvider serverIdProvider = null;
        /// <summary>Информационная метка для уведомления текущего процессе синхронизации</summary>
        private MobileLabel infoLabel;
        /// <summary>Список отложенных свойств</summary>
        private List<DataAboutDeferredProperty> deferredProperty;

        private StringBuilder logBuilder;
        private const string DATE_TIME_FORMAT = "dd.MM.yyyy";

        /// <summary>Константная часть имени параметра</summary>
        public const string PARAMETER = "Parameter";

        /// <summary>Синхронизация данных между ТСД и сервером</summary>
        /// <param name="MainProcess"></param>
        public dbSynchronizer(WMSClient MainProcess, IServerIdProvider serverIdProvider)
            : base(MainProcess, 1)
            {
            if (applicationIsClosing)
                {
                return;
                }
            StartNetworkConnection();

            if (serverIdProvider == null)
                {
                throw new ArgumentException("ServerIdProvider");
                }
            this.serverIdProvider = serverIdProvider;

            deferredProperty = new List<DataAboutDeferredProperty>();

            Configuration.Current.Repository.LoadingDataFromGreenhouse = true;
            SynchronizeWithGreenhouse(serverIdProvider);
            Configuration.Current.Repository.LoadingDataFromGreenhouse = false;

            ShowProgress(1, 1);
            }

        private void SynchronizeWithGreenhouse(IServerIdProvider serverIdProvider)
            {
            long lastUpdateRowId = getLastUpdateRowId();

            logBuilder = new StringBuilder(string.Format("Synchronizing start: {0}", DateTime.Now.ToString("HH:mm:ss dd.MM.yyyy")));
            logBuilder.AppendLine();
            Stopwatch totalTime = new Stopwatch();
            totalTime.Start();

            infoLabel.Text = "Експорт ламп";
            if (!uploadAccessories(TypeOfAccessories.Lamp, "UpdateLamps"))
                {
                return;
                }

            infoLabel.Text = "Експорт блоків";
            if (!uploadAccessories(TypeOfAccessories.ElectronicUnit, "UpdateUnits"))
                {
                return;
                }

            infoLabel.Text = "Експорт корпусів";
            if (!uploadAccessories(TypeOfAccessories.Case, "UpdateCases"))
                {
                return;
                }

            infoLabel.Text = "Імпорт ламп";
            if (!downloadAccessories<Lamp>(TypeOfAccessories.Lamp, "Lamps"))
                {
                return;
                }

            infoLabel.Text = "Імпорт блоків";
            if (!downloadAccessories<Unit>(TypeOfAccessories.ElectronicUnit, "ElectronicUnits"))
                {
                return;
                }

            infoLabel.Text = "Імпорт корпусів";
            if (!downloadAccessories<Case>(TypeOfAccessories.Case, "Cases"))
                {
                return;
                }

            infoLabel.Text = "Імпорт моделей";
            if (!downloadCatalogs<Model, Int16>("LampsModels"))
                {
                return;
                }

            infoLabel.Text = "Імпорт мап";
            if (!downloadCatalogs<Map, Int32>("Maps"))
                {
                return;
                }

            infoLabel.Text = "Імпорт партій";
            if (!downloadCatalogs<PartyModel, Int32>("Party"))
                {
                return;
                }

            return;
            //infoLabel.Text = "Контрагенти";
            //if (!SyncObjects<Contractors>(WaysOfSync.OneWay, FilterSettings.CanSynced))
            //    {
            //    return;
            //    }

            infoLabel.Text = "Карти";
            if (!SyncObjects<Maps>(false))
                {
                return;
                }

            infoLabel.Text = "Партії";
            if (!SyncObjects<Party>(WaysOfSync.OneWay))
                {
                return;
                }

            infoLabel.Text = "Моделі";
            if (!SyncObjects<WMS_client.db.Models>(WaysOfSync.TwoWay))
                {
                return;
                }

            //Прийомка нового
            infoLabel.Text = "Документи прийомки нового комплектучого";
            SyncAccepmentsDocWithServer();
            SyncAccepmentsDocFromServer();

            //Комплектуюче
            infoLabel.Text = "Лампи";
            if (!SyncObjects<Lamps>(WaysOfSync.TwoWay))
                {
                return;
                }

            infoLabel.Text = "Ел.блоки";
            if (!SyncObjects<ElectronicUnits>(WaysOfSync.TwoWay))
                {
                return;
                }

            infoLabel.Text = "Корпуси";
            if (!SyncObjects<Cases>(WaysOfSync.TwoWay))
                {
                return;
                }

            //Оновлення посилань
            infoLabel.Text = "Оновлення посилань";
            updateDeferredProperties();

            PerformQuery("EndOfSync");
            //Відправка на ...
            infoLabel.Text = "Відправка на списання";
            SyncOutSending<SendingToCharge, SubSendingToChargeChargeTable>();
            SyncInSending<SendingToCharge, SubSendingToChargeChargeTable>();

            infoLabel.Text = "Відправка на обмін";
            SyncOutSending<SendingToExchange, SubSendingToExchangeUploadTable>();
            SyncInSending<SendingToExchange, SubSendingToExchangeUploadTable>();

            infoLabel.Text = "Відправка на ремонт";
            SyncOutSending<SendingToRepair, SubSendingToRepairRepairTable>();
            SyncInSending<SendingToRepair, SubSendingToRepairRepairTable>();

            //Приймання комплектуючого з ...
            infoLabel.Text = "Приймання з ремонту";
            SyncOutSending<AcceptanceAccessoriesFromRepair, SubAcceptanceAccessoriesFromRepairRepairTable>(
                SyncModes.AcceptanceFromRepair);
            SyncInSending<AcceptanceAccessoriesFromRepair, SubAcceptanceAccessoriesFromRepairRepairTable>();

            infoLabel.Text = "Приймання з обміну";
            SyncOutAcceptanceFromExchange();
            SyncInSending<AcceptanceAccessoriesFromExchange, SubAcceptanceAccessoriesFromExchangeExchange>(
                SyncModes.SendingToExchange);

            //Переміщення
            infoLabel.Text = "Переміщення";
            SyncMovement();

            logBuilder.AppendLine();
            logBuilder.AppendLine(string.Format("Total: {0}", (int)(totalTime.ElapsedMilliseconds * 0.001)));
            logToFile("SynchLog.txt", logBuilder);
            }

        private bool downloadCatalogs<T, ID>(string greenhouseTableName) where T : ICatalog<ID>, new()
            {
            long lastDownLoadedId = Configuration.Current.Repository.GetLastDownloadedId(typeof(T));
            bool isParty = typeof(T) == typeof(PartyModel);

            try
                {
                PerformQuery("DownloadUpdatedCatalogs", greenhouseTableName, lastDownLoadedId);
                if (!SuccessQueryResult)
                    {
                    return false;
                    }

                var table = ResultParameters[1] as DataTable;
                lastDownLoadedId = Convert.ToInt32(ResultParameters[2]);

                if (table == null || table.Rows.Count == 0)
                    {
                    return true;
                    }

                List<T> list = new List<T>();
                foreach (DataRow row in table.Rows)
                    {
                    T catalog = new T();
                    catalog.Description = Convert.ToString(row["Description"]);
                    catalog.Deleted = Convert.ToBoolean(row["Deleted"]);

                    int id = Convert.ToInt32(row["Id"]);
                    if (catalog is ICatalog<Int16>)
                        {
                        ((ICatalog<Int16>)catalog).Id = (Int16)id;
                        }
                    else
                        {
                        ((ICatalog<Int32>)catalog).Id = id;
                        }

                    if (isParty)
                        {
                        var party = (PartyModel)((ICatalog<Int32>)catalog);

                        party.ContractorDescription = Convert.ToString(row["ContractorDescription"]);
                        party.WarrantyHours = Convert.ToInt16(row["WarrantyHours"]);
                        party.WarrantyYears = Convert.ToInt16(row["WarrantyYears"]);
                        party.Date = DateTime.ParseExact(row["WarrantyExpiryDate"] as string, DATE_TIME_FORMAT, null);
                        party.DateOfActSet = DateTime.ParseExact(row["DateOfActSet"] as string, DATE_TIME_FORMAT, null);
                        }

                    list.Add(catalog);
                    }

                if (!updateCatalogs(typeof(T), list))
                    {
                    return false;
                    }

                Configuration.Current.Repository.SetLastDownloadedId(typeof(T), lastDownLoadedId);
                }
            catch (Exception exp)
                {
                Trace.Write(string.Format(exp.Message));
                return false;
                }

            return true;
            }

        private bool updateCatalogs(Type catalogType, IList list)
            {
            var repository = Configuration.Current.Repository;

            if (catalogType == typeof(Map))
                {
                return repository.UpdateMaps((List<Map>)list);
                }
            else if (catalogType == typeof(Model))
                {
                return repository.UpdateModels((List<Model>)list);
                }
            else if (catalogType == typeof(PartyModel))
                {
                return repository.UpdateParties((List<PartyModel>)list);
                }

            return false;
            }

        private bool downloadAccessories<T>(TypeOfAccessories accessoryType, string greenhouseTableName) where T : IAccessory, new()
            {
            long lastDownLoadedId = Configuration.Current.Repository.GetLastDownloadedId(accessoryType);
            PerformQuery("GetUpdatesCount", greenhouseTableName, lastDownLoadedId);
            if (!SuccessQueryResult)
                {
                return false;
                }

            int updatesCount = Convert.ToInt32(ResultParameters[1]);

            double iterationsCountDouble = (double)updatesCount / RECORDS_QUANTITY_IN_TASK;
            int iterationsCount = (int)iterationsCountDouble;
            if (iterationsCountDouble > iterationsCount)
                {
                iterationsCount++;
                }

            bool barcodeAccessory = typeof(T) != typeof(Case);
            bool fixableAccessory = typeof(T) != typeof(Lamp);
            bool isCase = typeof(T) == typeof(Case);

            ShowProgress(0, iterationsCount);
            int iterationNumber = 0;
            try
                {
                while (true)
                    {
                    iterationNumber++;
                    PerformQuery("DownloadUpdatedAccessories", (int)accessoryType, lastDownLoadedId,
                        RECORDS_QUANTITY_IN_TASK);

                    if (!SuccessQueryResult)
                        {
                        return false;
                        }

                    var table = ResultParameters[1] as DataTable;
                    int lastAcceptedRowId = Convert.ToInt32(ResultParameters[2]);
                    if (table == null || table.Rows.Count == 0)
                        {
                        break;
                        }

                    List<T> list = new List<T>();
                    foreach (DataRow row in table.Rows)
                        {
                        T accessory = new T();

                        accessory.Id = Convert.ToInt32(row["Id"]);
                        accessory.Model = Convert.ToInt16(row["Model"]);
                        accessory.Party = Convert.ToInt32(row["Party"]);
                        accessory.Status = Convert.ToByte(row["Status"]);
                        accessory.WarrantyExpiryDate = DateTime.ParseExact(row["WarrantyExpiryDate"] as string,
                            DATE_TIME_FORMAT, null);

                        if (barcodeAccessory)
                            {
                            ((IBarcodeAccessory)accessory).Barcode = Convert.ToInt32(row["Barcode"]);
                            }

                        if (fixableAccessory)
                            {
                            ((IFixableAccessory)accessory).RepairWarranty = Convert.ToBoolean(row["RepairWarranty"]);
                            }

                        if (isCase)
                            {
                            var _Case = accessory as Case;

                            _Case.Lamp = Convert.ToInt32(row["Lamp"]);
                            _Case.Unit = Convert.ToInt32(row["Unit"]);
                            _Case.Map = Convert.ToInt32(row["Map"]);
                            _Case.Register = Convert.ToInt16(row["Register"]);
                            _Case.Position = Convert.ToByte(row["Position"]);
                            }

                        list.Add(accessory);
                        }

                    if (!updateAccessories(accessoryType, list))
                        {
                        return false;
                        }

                    lastDownLoadedId = lastAcceptedRowId;
                    Configuration.Current.Repository.SetLastDownloadedId(accessoryType, lastAcceptedRowId);
                    ShowProgress(iterationNumber, iterationsCount);
                    }
                }
            catch (Exception exp)
                {
                Trace.Write(string.Format(exp.Message));
                return false;
                }

            ShowProgress(iterationsCount, iterationsCount);
            return true;
            }

        private bool updateAccessories(TypeOfAccessories accessoryType, IList list)
            {
            switch (accessoryType)
                {
                case TypeOfAccessories.Lamp:
                    return Configuration.Current.Repository.UpdateLamps((List<Lamp>)list, false);

                case TypeOfAccessories.ElectronicUnit:
                    return Configuration.Current.Repository.UpdateUnits((List<Unit>)list, false);

                case TypeOfAccessories.Case:
                    return Configuration.Current.Repository.UpdateCases((List<Case>)list, false);
                }

            return false;
            }

        private const int RECORDS_QUANTITY_IN_TASK = 50;

        private bool uploadAccessories(TypeOfAccessories accessoriesType, string remoteMethodName)
            {
            List<List<int>> tasks = Configuration.Current.Repository.GetUpdateTasks(accessoriesType, RECORDS_QUANTITY_IN_TASK);

            int totalTasks = tasks.Count;
            ShowProgress(0, totalTasks);

            for (int taskIndex = 0; taskIndex < totalTasks; taskIndex++)
                {
                var task = tasks[taskIndex];

                DataTable table;
                switch (accessoriesType)
                    {
                    case TypeOfAccessories.Lamp:
                        table = buildDataTable<Lamp>(Configuration.Current.Repository.ReadLamps(task));
                        break;

                    case TypeOfAccessories.ElectronicUnit:
                        table = buildDataTable<Unit>(Configuration.Current.Repository.ReadUnits(task));
                        break;

                    default:
                        table = buildDataTable<Case>(Configuration.Current.Repository.ReadCases(task));
                        break;
                    }

                PerformQuery(remoteMethodName, table);

                if (!SuccessQueryResult)
                    {
                    return false;
                    }

                ShowProgress(taskIndex + 1, totalTasks);
                }

            return Configuration.Current.Repository.ResetUpdateLog(accessoriesType);
            }

        private DataTable buildDataTable<T>(List<T> list) where T : IAccessory
            {
            if (list == null || list.Count == 0)
                {
                return null;
                }

            var resultTable = new DataTable("Accessories");
            resultTable.Columns.AddRange(new DataColumn[] {
            new DataColumn("Id", typeof(int)), 
            new DataColumn("Model", typeof(int)), 
            new DataColumn("Party", typeof(int)), 
            new DataColumn("Status", typeof(int)), 
            new DataColumn("WarrantyExpiryDate", typeof(string))});

            bool barcodeAccessory = list[0] is IBarcodeAccessory;
            bool fixableAccessory = list[0] is IFixableAccessory;
            bool isCase = typeof(T) == typeof(Case);

            if (barcodeAccessory)
                {
                resultTable.Columns.Add(new DataColumn("Barcode", typeof(Int32)));
                }

            if (fixableAccessory)
                {
                resultTable.Columns.Add(new DataColumn("RepairWarranty", typeof(bool)));
                }

            if (isCase)
                {
                resultTable.Columns.Add(new DataColumn("Lamp", typeof(Int32)));
                resultTable.Columns.Add(new DataColumn("Unit", typeof(Int32)));
                resultTable.Columns.Add(new DataColumn("Map", typeof(Int32)));
                resultTable.Columns.Add(new DataColumn("Register", typeof(Int32)));
                resultTable.Columns.Add(new DataColumn("Position", typeof(Int32)));
                }

            foreach (var accessory in list)
                {
                var newRow = resultTable.NewRow();

                newRow["Id"] = Convert.ToInt32(accessory.Id);
                newRow["Model"] = Convert.ToInt32(accessory.Model);
                newRow["Party"] = Convert.ToInt32(accessory.Party);
                newRow["Status"] = Convert.ToInt32(accessory.Status);
                newRow["WarrantyExpiryDate"] = accessory.WarrantyExpiryDate.ToString(DATE_TIME_FORMAT);

                if (barcodeAccessory)
                    {
                    newRow["Barcode"] = Convert.ToInt32(((IBarcodeAccessory)accessory).Barcode);
                    }

                if (fixableAccessory)
                    {
                    newRow["RepairWarranty"] = ((IFixableAccessory)accessory).RepairWarranty;
                    }

                if (isCase)
                    {
                    var _Case = accessory as Case;

                    newRow["Unit"] = _Case.Unit;
                    newRow["Lamp"] = _Case.Lamp;
                    newRow["Map"] = _Case.Map;
                    newRow["Register"] = Convert.ToInt32(_Case.Register);
                    newRow["Position"] = Convert.ToInt32(_Case.Position);
                    }

                resultTable.Rows.Add(newRow);
                }

            return resultTable;
            }

        private long getLastUpdateRowId()
            {
            PerformQuery("GetLastTSDSyncronizationRowId");
            if (SuccessQueryResult)
                {
                return Convert.ToInt64(ResultParameters[1] ?? -1);
                }
            else
                {
                return -1;
                }
            }

        private void logToFile(string fileName, StringBuilder logBuilder)
            {
            string PathToFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            try
                {
                using (StreamWriter myFile = File.CreateText(PathToFile + "\\" + fileName))
                    {
                    myFile.Write(logBuilder.ToString());
                    myFile.Close();
                    }
                }
            catch (Exception e)
                {
                string s = e.Message;
                }
            }

        #region Sync.Objects
        /// <summary>Синхронизация объекта</summary>
        /// <param name="updId">Нужно обновить ID</param>
        private bool SyncObjects<T>(bool updId) where T : dbObject
            {
            return SyncObjects<T>(typeof(T).Name, WaysOfSync.OneWay, FilterSettings.None, false, false);
            }

        /// <summary>Синхронизация объекта</summary>
        /// <param name="wayOfSync">Способ синхронизации</param>
        private bool SyncObjects<T>(WaysOfSync wayOfSync) where T : dbObject
            {
            return SyncObjects<T>(typeof(T).Name, wayOfSync, FilterSettings.None, false, true);
            }

        /// <summary>Синхронизация объекта</summary>
        /// <param name="wayOfSync">Способ синхронизации</param>
        /// <param name="filter">Фильтры</param>
        private bool SyncObjects<T>(WaysOfSync wayOfSync, FilterSettings filter) where T : dbObject
            {
            return SyncObjects<T>(typeof(T).Name, wayOfSync, filter, false, true);
            }




        /// <summary>Синхронизация объекта</summary>
        /// <param name="tableName">Имя таблицы</param>
        /// <param name="wayOfSync">Способ синхронизации</param>
        /// <param name="filter">Фильтры</param>
        /// <param name="skipExists">Пропустить существующие</param>
        /// <param name="updId">Нужно обновить ID</param>
        private bool SyncObjects<T>(string tableName, WaysOfSync wayOfSync, FilterSettings filter, bool skipExists, bool updId) where T : dbObject
            {
            CatalogSynchronizer synchronizer = null;
            synchronizer = this.getCatalogSynchronizer(tableName);
            logBuilder.AppendLine();
            logBuilder.AppendLine();
            logBuilder.AppendLine(string.Format("{0}:", typeof(T).Name));

            Stopwatch totalWatch = new Stopwatch();
            totalWatch.Start();
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            string forWarehousesAndMapsSelect = string.Format(@"SELECT RTRIM({0}){0},RTRIM({1}){1},RTRIM({2}) {2} FROM {3} WHERE {4}=0",
                                           dbObject.IS_SYNCED,
                                           dbObject.BARCODE_NAME,
                                           dbObject.SYNCREF_NAME,
                                           tableName,
                                           CatalogObject.MARK_FOR_DELETING);

            string forByLogProcessedSelect = string.Format(@"SELECT RTRIM({0}){0},RTRIM({1}){1},RTRIM({2}) {2} FROM {3} WHERE {4}=0 AND {5} = 0",
                                           dbObject.IS_SYNCED,
                                           dbObject.BARCODE_NAME,
                                           dbObject.SYNCREF_NAME,
                                           tableName,
                                           CatalogObject.MARK_FOR_DELETING, CatalogObject.IS_SYNCED);

            bool isWarehouseTable = tableName.Equals("Contractors") || tableName.Equals("Maps");

            //Выбрать (Признак синхронизации, Штрих-код) всех не удаленных элементов с таблицы tableName
            string command = (useLoggingSyncronization & !isWarehouseTable) ?
                forByLogProcessedSelect :
                forWarehousesAndMapsSelect;
            DataTable table = null;
            using (SqlCeCommand query = dbWorker.NewQuery(command))
                {
                table = query.SelectToTable();
                }
            int rowsCount = (table ?? new DataTable()).Rows.Count;
            logBuilder.AppendLine(string.Format("init pdt query: {0} msec; rows: {1}", stopWatch.ElapsedMilliseconds, rowsCount));

            stopWatch.Reset();
            stopWatch.Start();

            if (filter == FilterSettings.None)
                {
                PerformQuery("StartSyncProcess", this.serverIdProvider.ServerId, tableName, table, (int)FilterSettings.NotMarkForDelete);
                }
            else
                {
                PerformQuery("StartSyncProcess", this.serverIdProvider.ServerId, tableName, table, (int)FilterSettings.NotMarkForDelete, (int)filter);
                }

            logBuilder.AppendLine(string.Format("StartSyncProcess: {0} msec; rows: {1}", stopWatch.ElapsedMilliseconds, rowsCount));

            stopWatch.Reset();
            stopWatch.Start();

            if (IsAnswerIsTrue)
                {
                removeMarkedObject(tableName);
                logBuilder.AppendLine(string.Format("removeMarkedObject: {0} msec", stopWatch.ElapsedMilliseconds));

                stopWatch.Reset();
                stopWatch.Start();

                updateObjOnLocalDb<T>(synchronizer, skipExists, updId);

                int localRowsCount = ((ResultParameters[1] as DataTable) ?? new DataTable()).Rows.Count;
                logBuilder.AppendLine(string.Format("updateObjOnLocalDb: {0} msec; rows: {1}", stopWatch.ElapsedMilliseconds, localRowsCount));

                stopWatch.Reset();
                stopWatch.Start();

                if (wayOfSync == WaysOfSync.TwoWay)
                    {
                    int remoteTableRows = ((ResultParameters[2] as DataTable) ?? new DataTable()).Rows.Count;

                    updateObjOnServDb<T>(tableName);

                    logBuilder.AppendLine(string.Format("update greenhouse: {0} msec; rows:{1}", stopWatch.ElapsedMilliseconds, remoteTableRows));

                    stopWatch.Reset();
                    stopWatch.Start();
                    }

                logBuilder.AppendLine(string.Format("{0} total: {1} msec", typeof(T).Name, totalWatch.ElapsedMilliseconds));

                return true;
                }
            else
                {
                return false;
                }
            }

        private CatalogSynchronizer getCatalogSynchronizer(string tableName)
            {
            switch (tableName)
                {
                case "Cases":
                    return new CasesSynchronizer();

                case "Lamps":
                    return new AccessorySynchronizer("Lamps");

                case "ElectronicUnits":
                    return new AccessorySynchronizer("ElectronicUnits");

                default:
                    return null;
                }
            }

        /// <summary>Обновление локальной базы после синхронизации</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="skipExists">Пропустить существующие</param>
        /// <param name="updId">Нужно обновить ID</param>
        private void updateObjOnLocalDb<T>(CatalogSynchronizer synchronizer, bool skipExists, bool updId) where T : dbObject
            {
            //Данные только по измененным элементам с "сервера"
            DataTable changesForTsd = ResultParameters[1] as DataTable;
            if (changesForTsd != null)
                {
                //Обновление элементов на локальной базе
                CreateSyncObject<T>(synchronizer, changesForTsd, skipExists, ref deferredProperty, updId);
                }
            }

        /// <summary>Выборка данных для обновления сервера после синхронизации</summary>
        /// <param name="tableName"></param>
        private void updateObjOnServDb<T>(string tableName) where T : dbObject
            {
            //Штрихкода элементов, которые нужно обновить на "сервере"
            DataTable changesForServ = ResultParameters[2] as DataTable;
            using (SqlCeCommand query = dbWorker.NewQuery(string.Empty))
                {
                StringBuilder where = new StringBuilder();

                if (changesForServ != null && changesForServ.Rows.Count != 0)
                    {
                    //Формирование комманды для выбора данных по всем элементам 
                    //из таблицы tableName, которые необходимо обновить на "сервере" 
                    int index = 0;

                    foreach (DataRow row in changesForServ.Rows)
                        {
                        //Добавление параметров
                        query.AddParameter(string.Concat(PARAMETER, index), row[dbObject.SYNCREF_NAME]);
                        @where.AppendFormat(" RTRIM([{0}].[{1}])=RTRIM(@{2}{3}) OR",
                                            tableName, dbObject.SYNCREF_NAME, PARAMETER, index);
                        index++;
                        }


                    string whereStr = @where.ToString(0, @where.Length - 3);
                    //Обновление статуса синхронизации для локальной базы
                    query.CommandText = string.Format("UPDATE {0} SET {1}=1 WHERE {2}",
                                                      tableName,
                                                      dbObject.IS_SYNCED,
                                                      whereStr);
                    query.ExecuteNonQuery();

                    if (changesForServ.Rows.Count > 0)
                        {
                        //Выборка данных
                        query.CommandText = getUnsyncLinkedData(typeof(T), whereStr);
                        DataTable changes = query.SelectToTable(new Dictionary<string, Enum>
                                {
                                    {BaseFormatName.DateTime, DateTimeFormat.OnlyDate}
                                });

                        PerformQuery("SyncChangesForServer", tableName, changes);
                        }
                    }
                }
            }

        /// <summary>Получить не синхронизированные связанные данные</summary>
        /// <param name="type">Тип объекта</param>
        /// <param name="whereClause">where клауза</param>
        /// <returns>SQL команда для получения данных</returns>
        private string getUnsyncLinkedData(Type type, string whereClause)
            {
            StringBuilder selectClause = new StringBuilder();
            StringBuilder fromClause = new StringBuilder();
            PropertyInfo[] fields = type.GetProperties();

            foreach (PropertyInfo field in fields)
                {
                dbFieldAtt attribute = Attribute.GetCustomAttribute(field, typeof(dbFieldAtt)) as dbFieldAtt;

                if (attribute != null)
                    {
                    if (attribute.dbObjectType == null)
                        {
                        selectClause.AppendFormat("    [{0}].[{1}],\r\n", type.Name, field.Name);
                        }
                    else
                        {
                        selectClause.AppendFormat("    [{0}].[{1}] [{2}],\r\n", attribute.dbObjectType.Name, dbObject.SYNCREF_NAME, field.Name);
                        fromClause.AppendFormat("LEFT JOIN [{0}] ON [{0}].[{1}]=[{2}].[{3}]\r\n",
                                                attribute.dbObjectType.Name,
                                                dbObject.IDENTIFIER_NAME,
                                                type.Name,
                                                field.Name);
                        }
                    }
                }

            string command = string.Format("SELECT\r\n{0}\r\nFROM [{1}]\r\n{2}\r\nWHERE\r\n    {3}",
                                           selectClause.Length > 3 ? selectClause.ToString(0, selectClause.Length - 3) : string.Empty,
                                           type.Name,
                                           fromClause.Length > 2 ? fromClause.ToString(0, fromClause.Length - 2) : string.Empty,
                                           whereClause);
            return command;
            }

        /// <summary>Создание объектов синхронизации</summary>
        /// <param name="table">Данные о объектах</param>
        /// <param name="skipExists">Пропустить существующие</param>
        /// <param name="deferredProperty">Список отложеных свойств</param>
        /// <param name="updId">Нужно обновить ID</param>
        private static void CreateSyncObject<T>(CatalogSynchronizer synchronizer, DataTable table, bool skipExists, ref List<DataAboutDeferredProperty> deferredProperty, bool updId) where T : dbObject
            {
            if (synchronizer != null)
                {
                int rowsCount = table.Rows.Count;
                for (int rowIndex = 0; rowIndex < rowsCount; rowIndex++)
                    {
                    DataRow row = table.Rows[rowIndex];
                    synchronizer.Merge(row);

                    //if (rowIndex % 10 == 0)
                        {
                        Trace.WriteLine(string.Format("{1} %, rowIndex = {0} from {2}", rowIndex, (int)(100 * rowIndex / rowsCount), rowsCount));
                        }

                    //if (rowIndex % 500 == 0)
                    //    {
                    //    GC.Collect();
                    //    GC.WaitForPendingFinalizers();
                    //    }
                    }
                return;
                }

            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();
            string syncRef = string.Empty;
            string syncRefName = dbObject.SYNCREF_NAME.ToLower();
            int lastDeferredIndex = deferredProperty.Count;

            foreach (DataRow row in table.Rows)
                {

                object newObj = Activator.CreateInstance(typeof(T));
                T newObject = (T)newObj;
                bool needDeferred = false;

                foreach (PropertyInfo property in properties)
                    {
                    dbFieldAtt attribute = Attribute.GetCustomAttribute(property, typeof(dbFieldAtt)) as dbFieldAtt;

                    if (attribute != null && table.Columns.Contains(property.Name))
                        {
                        object value = row[property.Name];

                        //Не существует ли елемент с такой ссылкой?
                        if (property.Name.ToLower().Equals(syncRefName))
                            {
                            if (BarcodeWorker.IsRefExist(type, value.ToString()))
                                {
                                if (skipExists)
                                    {
                                    break;
                                    }

                                newObject.SetNotNew();
                                }

                            syncRef = value.ToString();
                            }
                        else if (updId && property.Name.ToLower().Equals(dbObject.IDENTIFIER_NAME))
                            {
                            continue;
                            }

                        if (property.PropertyType == typeof(int))
                            {
                            value = string.IsNullOrEmpty(value.ToString()) ? 0 : Convert.ToInt32(value);
                            }
                        else if (property.PropertyType == typeof(double))
                            {
                            value = string.IsNullOrEmpty(value.ToString()) ? 0D : Convert.ToDouble(value);
                            }
                        else if (property.PropertyType == typeof(long))
                            {
                            if (attribute.dbObjectType == null)
                                {
                                value = string.IsNullOrEmpty(value.ToString()) ? 0L : Convert.ToInt64(value);
                                }
                            else
                                {
                                if (value != null && !string.IsNullOrEmpty(value.ToString()))
                                    {
                                    DataAboutDeferredProperty data = new DataAboutDeferredProperty(type, attribute.dbObjectType, property.Name, value);
                                    deferredProperty.Add(data);
                                    needDeferred = true;
                                    }

                                value = 0L;
                                }
                            }
                        else if (property.PropertyType == typeof(bool))
                            {
                            value = Convert.ToBoolean(value);
                            }
                        else if (property.PropertyType == typeof(DateTime))
                            {
                            value = StringParser.ParseDateTime(value);
                            }
                        else if (property.PropertyType.IsEnum)
                            {
                            value = Enum.Parse(property.PropertyType, value.ToString(), false);
                            }
                        else if (property.PropertyType == typeof(string))
                            {
                            string fullValue = value.ToString();
                            value = value.ToString();

                            if (property.Name != CatalogObject.DESCRIPTION)
                                {
                                int length = attribute.StrLength == 0
                                                 ? dbFieldAtt.DEFAULT_STR_LENGTH
                                                 : attribute.StrLength;

                                if (fullValue.Length > length)
                                    {
                                    value = fullValue.Substring(0, length);
                                    }
                                }
                            }

                        property.SetValue(newObject, value, null);
                        }
                    }

                ISynced syncObject = newObject as ISynced;

                if (syncObject != null)
                    {
                    syncObject.IsSynced = true;
                    }

                if (updId && !skipExists && !string.IsNullOrEmpty(syncRef))
                    {
                    newObject.Id = Convert.ToInt64(BarcodeWorker.GetIdByRef(type, syncRef));
                    }

                CatalogObject catalog = newObject as CatalogObject;

                if (catalog != null)
                    {
                    dbElementAtt attribute = Attribute.GetCustomAttribute(newObject.GetType(), typeof(dbElementAtt)) as dbElementAtt;
                    int length = attribute == null || attribute.DescriptionLength == 0
                                     ? dbElementAtt.DEFAULT_DES_LENGTH
                                     : attribute.DescriptionLength;

                    if (catalog.Description.Length > length)
                        {
                        catalog.Description = catalog.Description.Substring(0, length);
                        }
                    }

                newObject.Sync<T>(updId);

                if (needDeferred)
                    {
                    for (int i = lastDeferredIndex; i < deferredProperty.Count; i++)
                        {
                        deferredProperty[i].Id = newObject.Id;
                        }

                    lastDeferredIndex = deferredProperty.Count;
                    }
                }

            }

        private void removeMarkedObject(string tableName)
            {
            if (ResultParameters.Length >= 4 && ResultParameters[3] != null)
                {
                DataTable table = ResultParameters[3] as DataTable;

                if (table != null && table.Rows.Count > 0)
                    {
                    StringBuilder command = new StringBuilder();
                    command.AppendFormat("DELETE FROM {0} WHERE 1=0 ", tableName);

                    int index = 0;
                    Dictionary<string, object> parameters = new Dictionary<string, object>();

                    foreach (DataRow row in table.Rows)
                        {
                        string currParameter = string.Concat(PARAMETER, index++);
                        command.AppendFormat(" OR RTRIM({0})=RTRIM(@{1})", dbObject.SYNCREF_NAME, currParameter);
                        parameters.Add(currParameter, row[dbObject.SYNCREF_NAME]);
                        }

                    using (SqlCeCommand query = dbWorker.NewQuery(command.ToString()))
                        {
                        query.AddParameters(parameters);
                        query.ExecuteNonQuery();
                        }
                    }
                }
            }
        #endregion

        #region Sync.other
        /// <summary>Синхронизировать принятое комплектующее с сервером</summary>
        private void SyncAccepmentsDocWithServer()
            {
            DataTable data = AcceptanceOfNewComponentsDetails.GetAllData();
            PerformQuery("SetAcceptDocs", data);
            dbArchitector.ClearAllDataFromTable(typeof(AcceptanceOfNewComponentsDetails).Name);
            }

        /// <summary>Синхронизировать документы "Приемка нового компл." с сервера</summary>
        private void SyncAccepmentsDocFromServer()
            {
            DataTable acceptedDoc = AcceptanceOfNewComponents.GetAcceptedDocuments();
            DataTable notAcceptedDoc = AcceptanceOfNewComponents.GetNotAcceptedDocuments();

            PerformQuery("GetAcceptDocs", acceptedDoc, notAcceptedDoc);

            if (IsExistParameters)
                {
                AcceptanceOfNewComponents.ClearAcceptedDocuments();
                DataTable table = ResultParameters[0] as DataTable;

                if (table != null)
                    {
                    foreach (DataRow row in table.Rows)
                        {
                        string contractorBarcode = row["Contractor"].ToString();
                        string partyRef = row["InvoiceNumber"].ToString();
                        string caseModelRef = row["CaseModel"].ToString();
                        string lampModelRef = row["LampModel"].ToString();
                        string unitModelRef = row["UnitModel"].ToString();
                        object contractorObj = BarcodeWorker.GetIdByBarcode(typeof(Contractors), contractorBarcode);
                        object partyObj = BarcodeWorker.GetIdByRef(typeof(Party), partyRef);
                        object caseModelObj = BarcodeWorker.GetIdByRef(typeof(WMS_client.db.Models), caseModelRef);
                        object lampModelObj = BarcodeWorker.GetIdByRef(typeof(WMS_client.db.Models), lampModelRef);
                        object unitModelObj = BarcodeWorker.GetIdByRef(typeof(WMS_client.db.Models), unitModelRef);

                        AcceptanceOfNewComponents doc = new AcceptanceOfNewComponents
                                                            {
                                                                Id = Convert.ToInt64(row["Id"]),
                                                                Contractor = Convert.ToInt64(contractorObj),
                                                                Date = Convert.ToDateTime(row["Date"]),
                                                                InvoiceDate = Convert.ToDateTime(row["InvoiceDate"]),
                                                                InvoiceNumber = Convert.ToInt64(partyObj),
                                                                MarkForDeleting = false,
                                                                CaseModel = Convert.ToInt64(caseModelObj),
                                                                LampModel = Convert.ToInt64(lampModelObj),
                                                                UnitModel = Convert.ToInt64(unitModelObj),
                                                                TypesOfWarrantly = (TypesOfLampsWarrantly)
                                                                    Convert.ToInt32(row["TypesOfWarrantly"]),
                                                                TypeOfAccessories = (TypeOfAccessories)
                                                                    Convert.ToInt32(row["TypeOfAccessories"]),
                                                                WarrantlyHours = Convert.ToInt32(row["WarrantlyHours"]),
                                                                WarrantlyYears = Convert.ToInt32(row["WarrantlyYears"]),
                                                                State = (TypesOfLampsStatus)Convert.ToInt32(row["State"])
                                                            };
                        doc.Sync<AcceptanceOfNewComponents>();
                        }
                    }
                }
            }

        /// <summary>Синхронизировать ("Отправить") данные по перемещениям</summary>
        private void SyncMovement()
            {
            string tableName = typeof(Movement).Name;

            //sync
            string docCommand = string.Format("SELECT {0},{1},Date,Operation,Map,Register,Position FROM {2}",
                                              dbObject.BARCODE_NAME, dbObject.SYNCREF_NAME, tableName);
            DataTable table = null;
            using (SqlCeCommand query = dbWorker.NewQuery(docCommand))
                {
                table = query.SelectToTable(new Dictionary<string, Enum>
                    {
                        {BaseFormatName.DateTime, DateTimeFormat.OnlyDate}
                    });
                }
            PerformQuery("SyncMovement", table);

            if (ResultParameters != null && (bool)ResultParameters[0])
                {
                //Видалення записів
                string delCommand = string.Concat("DELETE FROM ", tableName);
                using (SqlCeCommand query = dbWorker.NewQuery(delCommand))
                    {
                    query.ExecuteNonQuery();
                    }
                }
            }

        #endregion

        #region Sync.Sending
        /// <summary>Синхронизировать изменения по док "Отправить на .." на ТСД</summary>
        /// <typeparam name="T">Документ</typeparam>
        /// <typeparam name="S">Таблица</typeparam>
        private void SyncInSending<T, S>()
            where T : Sending
            where S : SubSending
            {
            SyncInSending<T, S>(SyncModes.StandartToX);
            }

        /// <summary>Синхронизировать изменения по док "Отправить на .." на ТСД</summary>
        /// <typeparam name="T">Документ</typeparam>
        /// <typeparam name="S">Таблица</typeparam>
        private void SyncInSending<T, S>(SyncModes mode)
            where T : Sending
            where S : SubSending
            {
            string docName = typeof(T).Name;
            string tableName = typeof(S).Name;

            PerformQuery("GetSendingDocs", docName, tableName, (int)mode);

            if (IsExistParameters)
                {
                DataTable table = ResultParameters[0] as DataTable;

                if (table != null)
                    {
                    T newDoc = null;

                    foreach (DataRow row in table.Rows)
                        {
                        int currId = Convert.ToInt32(row["Id"]);

                        if (newDoc == null || newDoc.Id != currId)
                            {
                            newDoc = (T)Activator.CreateInstance(typeof(T));
                            newDoc.Contractor = Convert.ToInt32(row["Contractor"]);
                            newDoc.Date = Convert.ToDateTime(row["Date"]);
                            newDoc.TypeOfAccessory = (TypeOfAccessories)Convert.ToInt32(row["TypeOfAccessories"]);
                            newDoc.BarCode = currId.ToString();
                            newDoc.IsSynced = true;
                            newDoc.Sync<T>();
                            }

                        S newSubDoc = (S)Activator.CreateInstance(typeof(S));
                        newSubDoc.TypeOfAccessory = newDoc.TypeOfAccessory;
                        newSubDoc.Id = currId;
                        newSubDoc.IsSynced = true;

                        switch (mode)
                            {
                            case SyncModes.StandartToX:
                                newSubDoc.Document = row["Document"].ToString();
                                break;
                            case SyncModes.SendingToExchange:
                                string syncRef = row["Nomenclature"].ToString();
                                object modelId = BarcodeWorker.GetIdByRef(typeof(WMS_client.db.Models), syncRef);

                                newSubDoc.SetValue("Nomenclature", modelId);
                                break;
                            }

                        newSubDoc.Sync<S>();
                        }
                    }
                }
            }

        /// <summary>Синхронизировать изменения по док "Отправить на .." на сервере</summary>
        /// <typeparam name="T">Документ</typeparam>
        /// <typeparam name="S">Таблица</typeparam>
        private void SyncOutSending<T, S>()
            where T : Sending
            where S : SubSending
            {
            SyncOutSending<T, S>(SyncModes.StandartToX);
            }

        /// <summary>Синхронизировать изменения по док "Отправить на .." на сервере</summary>
        /// <typeparam name="T">Документ</typeparam>
        /// <typeparam name="S">Таблица</typeparam>
        /// <param name="mode">Режим синхронізації</param>
        private void SyncOutSending<T, S>(SyncModes mode)
            where T : Sending
            where S : SubSending
            {
            string docName = typeof(T).Name;
            string tableName = typeof(S).Name;

            //1. Обновление на сервере
            string command = string.Format("SELECT Id, Document FROM {0} WHERE IsSynced=0", tableName);
            DataTable table = null;
            using (SqlCeCommand query = dbWorker.NewQuery(command))
                {
                table = query.SelectToTable();
                }
            PerformQuery("SetSendingDocs", docName, tableName, table, (int)mode);

            bool fullDeleteAccepted = typeof(T) == typeof(SendingToRepair) || typeof(T) == typeof(SendingToCharge);

            if (fullDeleteAccepted)
                {
                command = string.Format("SELECT DISTINCT Id FROM {0} WHERE IsSynced=0", tableName);
                using (SqlCeCommand query = dbWorker.NewQuery(command))
                    {
                    table = query.SelectToTable();
                    }
                StringBuilder removeCommand = new StringBuilder("DELETE FROM {0} WHERE 1=0 ");
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                int index = 0;

                foreach (DataRow row in table.Rows)
                    {
                    object id = row["Id"];
                    parameters.Add(string.Concat(PARAMETER, index), id);
                    removeCommand.AppendFormat(" OR Id=@{0}{1}", PARAMETER, index);
                    index++;
                    }

                using (SqlCeCommand query = dbWorker.NewQuery(string.Format(removeCommand.ToString(), docName)))
                    {
                    query.AddParameters(parameters);
                    query.ExecuteNonQuery();
                    }

                using (SqlCeCommand query = dbWorker.NewQuery(string.Format(removeCommand.ToString(), tableName)))
                    {
                    query.AddParameters(parameters);
                    query.ExecuteNonQuery();
                    }
                }
            else
                {
                //2. Удаление обновленных (? а нужно ли ... может когда весь документ удаляется)
                command = string.Format("DELETE FROM {0} WHERE IsSynced=0", tableName);
                using (SqlCeCommand query = dbWorker.NewQuery(command))
                    {
                    query.ExecuteNonQuery();
                    }

                //3. Удаление полностью принятого документа
                command = string.Format(@"SELECT s.Id
FROM {0} s 
LEFT JOIN (
    SELECT t1.Id, Count(1) Count
    FROM {0} t1
    JOIN {1} t2 ON t2.Id=t1.Id
    GROUP BY t1.Id)t ON s.Id=t.Id
WHERE t.Count=0 OR t.Id IS NULL", docName, tableName);
                using (SqlCeCommand query = dbWorker.NewQuery(command))
                    {
                    table = query.SelectToTable();
                    }

                StringBuilder removeCommand = new StringBuilder();
                removeCommand.AppendFormat("DELETE FROM {0} WHERE 1=0", docName);
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                int index = 0;

                foreach (DataRow row in table.Rows)
                    {
                    object id = row["Id"];
                    removeCommand.AppendFormat(" OR {0}=@{1}{2}", dbObject.IDENTIFIER_NAME, PARAMETER, index);
                    parameters.Add(string.Concat(PARAMETER, index), id);
                    index++;
                    }

                using (SqlCeCommand query = dbWorker.NewQuery(removeCommand.ToString()))
                    {
                    query.AddParameters(parameters);
                    query.ExecuteNonQuery();
                    }
                }
            }

        /// <summary>Синхронизация данных "Приемка с обмена" для сервера</summary>
        private void SyncOutAcceptanceFromExchange()
            {
            string tableName = typeof(AcceptanceAccessoriesFromExchangeDetails).Name;
            string command = string.Format(
                "SELECT d.Id,m.{0} Nomenclature,d.{1} FROM {2} d LEFT JOIN Models m ON m.Id=d.Nomenclature ORDER BY Id,Nomenclature",
                dbObject.SYNCREF_NAME, dbObject.BARCODE_NAME, tableName);
            DataTable table = null;
            using (SqlCeCommand query = dbWorker.NewQuery(command))
                {
                table = query.SelectToTable();
                }
            //Send
            PerformQuery("SetSendingExchangeDocs", table);
            //Clear
            dbArchitector.ClearAllDataFromTable(tableName);
            }
        #endregion

        #region Deferred
        /// <summary>Обновить связанные свойства</summary>
        private void updateDeferredProperties()
            {
            dbObject lastObject = null;

            foreach (DataAboutDeferredProperty propertyData in deferredProperty)
                {
                if (lastObject == null ||
                    lastObject.GetType() != propertyData.AccessoryType ||
                    lastObject.Id != propertyData.Id)
                    {
                    lastObject = (dbObject)Activator.CreateInstance(propertyData.AccessoryType);
                    lastObject.Read(propertyData.Id);
                    }

                if (lastObject.Id != 0)
                    {
                    object idValue = BarcodeWorker.GetIdByRef(
                        propertyData.PropertyType,
                        propertyData.Value.ToString());
                    long id = Convert.ToInt64(idValue);

                    if (!id.Equals(0L))
                        {
                        lastObject.SetValue(propertyData.PropertyName, idValue);
                        lastObject.Sync();
                        }
                    }
                }
            }
        #endregion

        #region Overrides of BusinessProcess
        public override void DrawControls()
            {
            MainProcess.ClearControls();

            MainProcess.CreateLabel("Синхронізація!", 5, 125, 230, MobileFontSize.Large, MobileFontPosition.Center, MobileFontColors.Info, FontStyle.Bold);
            MainProcess.CreateLabel("Зараз оновлюється:", 5, 155, 230, MobileFontSize.Normal, MobileFontPosition.Center);
            infoLabel = MainProcess.CreateLabel(string.Empty, 5, 175, 230, MobileFontSize.Normal, MobileFontPosition.Center, MobileFontColors.Default, FontStyle.Bold);
            }

        public override void OnBarcode(string Barcode)
            {
            }

        public override void OnHotKey(KeyAction Key)
            {
            }
        #endregion
        }


    }