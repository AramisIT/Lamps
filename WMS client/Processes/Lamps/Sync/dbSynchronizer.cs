using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Drawing;
using System.Reflection;
using System.Text;
using WMS_client.Enums;
using WMS_client.db;

namespace WMS_client
{
    /// <summary>Синхронизация данных между ТСД и сервером</summary>
    public class dbSynchronizer : BusinessProcess
    {
        /// <summary>Информационная метка для уведомления текущего процессе синхронизации</summary>
        private MobileLabel infoLabel;
        /// <summary>Список отложенных свойств</summary>
        private List<DataAboutDeferredProperty> deferredProperty;
        /// <summary>Константная часть имени параметра</summary>
        public const string PARAMETER = "Parameter";

        /// <summary>Синхронизация данных между ТСД и сервером</summary>
        /// <param name="MainProcess"></param>
        public dbSynchronizer(WMSClient MainProcess)
            : base(MainProcess, 1)
        {
            deferredProperty = new List<DataAboutDeferredProperty>();

            //Документи
            infoLabel.Text = "Контрагенти";
            SyncObjects<Contractors>(WaysOfSync.OneWay, FilterSettings.CanSynced);
            infoLabel.Text = "Карти";
            SyncObjects<Maps>(false);
            infoLabel.Text = "Партії";
            SyncObjects<Party>(WaysOfSync.OneWay);
            infoLabel.Text = "Моделі";
            SyncObjects<Models>(WaysOfSync.TwoWay);
            infoLabel.Text = "Лампи";
            SyncObjects<Lamps>(WaysOfSync.TwoWay);
            infoLabel.Text = "Ел.блоки";
            SyncObjects<ElectronicUnits>(WaysOfSync.TwoWay);
            infoLabel.Text = "Корпуси";
            SyncObjects<Cases>(WaysOfSync.TwoWay);
            //Оновлення посилань
            infoLabel.Text = "Оновлення посилань";
            updateDeferredProperties();
            PerformQuery("EndOfSync");
            //Прийомка
            infoLabel.Text = "Документи прийомки нового комплектучого";
            SyncAccepmentsDocWithServer();
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
            SyncOutSending<AcceptanceAccessoriesFromRepair, SubAcceptanceAccessoriesFromRepairRepairTable>();
            SyncInSending<AcceptanceAccessoriesFromRepair, SubAcceptanceAccessoriesFromRepairRepairTable>();
            infoLabel.Text = "Приймання з обміну";
            SyncOutAcceptanceFromExchange();
            SyncInSending<AcceptanceAccessoriesFromExchange, SubAcceptanceAccessoriesFromExchangeExchange>(SyncModes.SendingToExchange);
            //Переміщення
            infoLabel.Text = "Переміщення";
            SyncMovement();

            MainProcess.ClearControls();
            MainProcess.Process = new SelectingLampProcess(MainProcess);
        }

        #region Sync.Objects
        /// <summary>Синхронизация объекта</summary>
        /// <param name="updId">Нужно обновить ID</param>
        public void SyncObjects<T>(bool updId) where T : dbObject
        {
            SyncObjects<T>(typeof(T).Name, WaysOfSync.OneWay, FilterSettings.None, false, false);
        }

        /// <summary>Синхронизация объекта</summary>
        /// <param name="wayOfSync">Способ синхронизации</param>
        public void SyncObjects<T>(WaysOfSync wayOfSync) where T : dbObject
        {
            SyncObjects<T>(typeof(T).Name, wayOfSync, FilterSettings.None, false, true);
        }

        /// <summary>Синхронизация объекта</summary>
        /// <param name="wayOfSync">Способ синхронизации</param>
        /// <param name="skipExists">Пропустить существующие</param>
        public void SyncObjects<T>(WaysOfSync wayOfSync, bool skipExists) where T : dbObject
        {
            SyncObjects<T>(typeof(T).Name, wayOfSync, FilterSettings.None, skipExists, true);
        }

        /// <summary>Синхронизация объекта</summary>
        /// <param name="wayOfSync">Способ синхронизации</param>
        /// <param name="filter">Фильтры</param>
        public void SyncObjects<T>(WaysOfSync wayOfSync, FilterSettings filter) where T : dbObject
        {
            SyncObjects<T>(typeof(T).Name, wayOfSync, filter, false, true);
        }

        /// <summary>Синхронизация объекта</summary>
        /// <param name="wayOfSync">Способ синхронизации</param>
        /// <param name="filter">Фильтры</param>
        /// <param name="skipExists">Пропустить существующие</param>
        public void SyncObjects<T>(WaysOfSync wayOfSync, FilterSettings filter, bool skipExists) where T : dbObject
        {
            SyncObjects<T>(typeof(T).Name, wayOfSync, filter, skipExists, true);
        }

        /// <summary>Синхронизация объекта</summary>
        /// <param name="tableName">Имя таблицы</param>
        /// <param name="wayOfSync">Способ синхронизации</param>
        /// <param name="filter">Фильтры</param>
        /// <param name="skipExists">Пропустить существующие</param>
        /// <param name="updId">Нужно обновить ID</param>
        public void SyncObjects<T>(string tableName, WaysOfSync wayOfSync, FilterSettings filter, bool skipExists, bool updId) where T : dbObject
        {
            //Выбрать (Признак синхронизации, Штрих-код) всех не удаленных элементов с таблицы tableName
            string command = string.Format("SELECT RTRIM({0}){0},RTRIM({1}){1},RTRIM({2}) {2} FROM {3} WHERE {4}=0",
                                           dbObject.IS_SYNCED,
                                           dbObject.BARCODE_NAME,
                                           dbObject.SYNCREF_NAME,
                                           tableName,
                                           CatalogObject.MARK_FOR_DELETING);
            SqlCeCommand query = dbWorker.NewQuery(command);
            DataTable table = query.SelectToTable();

            if (filter == FilterSettings.None)
            {
                PerformQuery("StartSyncProcess", tableName, table, (int)FilterSettings.NotMarkForDelete);
            }
            else
            {
                PerformQuery("StartSyncProcess", tableName, table, (int)FilterSettings.NotMarkForDelete, (int)filter);
            }

            if (IsAnswerIsTrue)
            {
                removeMarkedObject(tableName);
                updateObjOnLocalDb<T>(skipExists, updId);

                if (wayOfSync == WaysOfSync.TwoWay)
                {
                    updateObjOnServDb<T>(tableName);
                }
            }
        }

        /// <summary>Обновление локальной базы после синхронизации</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="skipExists">Пропустить существующие</param>
        /// <param name="updId">Нужно обновить ID</param>
        private void updateObjOnLocalDb<T>(bool skipExists, bool updId) where T : dbObject
        {
            //Данные только по измененным элементам с "сервера"
            DataTable changesForTsd = Parameters[1] as DataTable;
            //Обновление элементов на локальной базе
            CreateSyncObject<T>(changesForTsd, skipExists, ref deferredProperty, updId);
        }

        /// <summary>Выборка данных для обновления сервера после синхронизации</summary>
        /// <param name="tableName"></param>
        private void updateObjOnServDb<T>(string tableName) where T : dbObject
        {
            //Штрихкода элементов, которые нужно обновить на "сервере"
            DataTable changesForServ = Parameters[2] as DataTable;
            SqlCeCommand query = dbWorker.NewQuery(string.Empty);
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
        public static void CreateSyncObject<T>(DataTable table, bool skipExists, ref List<DataAboutDeferredProperty> deferredProperty, bool updId) where T : dbObject
        {
            if (table != null)
            {
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
                                const char separator = '.';
                                string[] parts = value.ToString().Substring(0, 10).Split(separator);

                                if (parts.Length == 3)
                                {
                                    value = Convert.ToDateTime(string.Concat(
                                        parts[1],
                                        separator,
                                        parts[0],
                                        separator,
                                        parts[2]));
                                }
                                else
                                {
                                    value = new DateTime();
                                }
                            }
                            else if (property.PropertyType.IsEnum)
                            {
                                value = Enum.Parse(property.PropertyType, value.ToString(), false);
                            }
                            else if(property.PropertyType == typeof(string))
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

                    if(catalog!=null)
                    {
                        dbElementAtt attribute = Attribute.GetCustomAttribute(newObject.GetType(), typeof(dbElementAtt)) as dbElementAtt;
                        int length = attribute==null || attribute.DescriptionLength == 0
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
        }

        private void removeMarkedObject(string tableName)
        {
            if (Parameters.Length >= 4 && Parameters[3] != null)
            {
                DataTable table = Parameters[3] as DataTable;

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

                    SqlCeCommand query = dbWorker.NewQuery(command.ToString());
                    query.AddParameters(parameters);
                    query.ExecuteNonQuery();
                }
            }
        }
        #endregion

        #region Sync.other
        /// <summary>Синхронизировать "Приемку" с сервером</summary>
        private void SyncAccepmentsDocWithServer()
        {
            DataTable acceptedDoc = AcceptanceOfNewComponents.GetAcceptedDocuments();
            DataTable notAcceptedDoc = AcceptanceOfNewComponents.GetNotAcceptedDocuments();

            PerformQuery("GetAcceptDocs", acceptedDoc, notAcceptedDoc);

            if (IsExistParameters)
            {
                AcceptanceOfNewComponents.ClearAcceptedDocuments();
                DataTable table = Parameters[0] as DataTable;

                if (table != null)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        string contractorBarcode = row["Contractor"].ToString();
                        string partyRef = row["InvoiceNumber"].ToString();
                        string modelRef = row["Model"].ToString();
                        object contractorObj = BarcodeWorker.GetIdByBarcode(typeof(Contractors), contractorBarcode);
                        object partyObj = BarcodeWorker.GetIdByRef(typeof(Party), partyRef);
                        object modelObj = BarcodeWorker.GetIdByRef(typeof(Models), modelRef);

                        AcceptanceOfNewComponents doc = new AcceptanceOfNewComponents
                                                            {
                                                                Id = Convert.ToInt64(row["Id"]),
                                                                Contractor = Convert.ToInt64(contractorObj),
                                                                Date = Convert.ToDateTime(row["Date"]),
                                                                InvoiceDate = Convert.ToDateTime(row["InvoiceDate"]),
                                                                InvoiceNumber = Convert.ToInt64(partyObj),
                                                                MarkForDeleting = false,
                                                                Model = Convert.ToInt64(modelObj),
                                                                TypesOfWarrantly = (TypesOfLampsWarrantly)
                                                                    Convert.ToInt32(row["TypesOfWarrantly"]),
                                                                TypeOfAccessories = (TypeOfAccessories)
                                                                    Convert.ToInt32(row["TypeOfAccessories"]),
                                                                WarrantlyHours = Convert.ToInt32(row["WarrantlyHours"]),
                                                                WarrantlyYears = Convert.ToInt32(row["WarrantlyYears"])
                                                            };
                        doc.Sync<AcceptanceOfNewComponents>();
                    }
                }

                PerformQuery("GetAcceptSubDocs", notAcceptedDoc);

                if (IsExistParameters)
                {
                    table = Parameters[0] as DataTable;

                    if (table != null)
                    {
                        foreach (DataRow row in table.Rows)
                        {
                            string markingRef = row["Marking"].ToString();
                            object idObj = BarcodeWorker.GetIdByRef(typeof(Models), markingRef);

                            SubAcceptanceOfNewComponentsMarkingInfo subDoc = new SubAcceptanceOfNewComponentsMarkingInfo
                                                                                 {
                                                                                     Id = Convert.ToInt64(row["IdDoc"]),
                                                                                     Marking = Convert.ToInt64(idObj),
                                                                                     Plan = Convert.ToInt32(row["Plan"]),
                                                                                     Fact = 0
                                                                                 };
                            subDoc.Save();
                        }
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
            SqlCeCommand query = dbWorker.NewQuery(docCommand);
            DataTable table = query.SelectToTable(new Dictionary<string, Enum>
                                                      {
                                                          {BaseFormatName.DateTime, DateTimeFormat.OnlyDate}
                                                      });
            PerformQuery("SyncMovement", table);

            if (Parameters != null && (bool)Parameters[0])
            {
                //Видалення записів
                string delCommand = string.Concat("DELETE FROM ", tableName);
                query = dbWorker.NewQuery(delCommand);
                query.ExecuteNonQuery();
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
                DataTable table = Parameters[0] as DataTable;

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
                                object modelId = BarcodeWorker.GetIdByRef(typeof(Models), syncRef);

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
            SqlCeCommand query = dbWorker.NewQuery(command);
            DataTable table = query.SelectToTable();
            PerformQuery("SetSendingDocs", docName, tableName, table, (int)mode);

            bool fullDeleteAccepted = typeof (T) == typeof (SendingToRepair) || typeof (T) == typeof (SendingToCharge);

            if (fullDeleteAccepted)
            {
                command = string.Format("SELECT DISTINCT Id FROM {0} WHERE IsSynced=0", tableName);
                query = dbWorker.NewQuery(command);
                table = query.SelectToTable();
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

                query = dbWorker.NewQuery(string.Format(removeCommand.ToString(), docName));
                query.AddParameters(parameters);
                query.ExecuteNonQuery();

                query = dbWorker.NewQuery(string.Format(removeCommand.ToString(), tableName));
                query.AddParameters(parameters);
                query.ExecuteNonQuery();
            }
            else
            {
                //2. Удаление обновленных (? а нужно ли ... может когда весь документ удаляется)
                command = string.Format("DELETE FROM {0} WHERE IsSynced=0", tableName);
                query = dbWorker.NewQuery(command);
                query.ExecuteNonQuery();

                //3. Удаление полностью принятого документа
                command = string.Format(@"SELECT s.Id
FROM {0} s 
LEFT JOIN (
    SELECT t1.Id, Count(1) Count
    FROM {0} t1
    JOIN {1} t2 ON t2.Id=t1.Id
    GROUP BY t1.Id)t ON s.Id=t.Id
WHERE t.Count=0 OR t.Id IS NULL", docName, tableName);
                query = dbWorker.NewQuery(command);
                table = query.SelectToTable();

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

                query = dbWorker.NewQuery(removeCommand.ToString());
                query.AddParameters(parameters);
                query.ExecuteNonQuery();
            }
        }

        /// <summary>Синхронизация данных "Приемка с обмена" для сервера</summary>
        private void SyncOutAcceptanceFromExchange()
        {
            string tableName = typeof (AcceptanceAccessoriesFromExchangeDetails).Name;
            string command = string.Format(
                "SELECT d.Id,m.{0} Nomenclature,d.{1} FROM {2} d LEFT JOIN Models m ON m.Id=d.Nomenclature ORDER BY Id,Nomenclature",
                dbObject.SYNCREF_NAME, dbObject.BARCODE_NAME, tableName);
            SqlCeCommand query = dbWorker.NewQuery(command);
            DataTable table = query.SelectToTable();

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

                if(lastObject.Id!=0)
                {
                    object idValue = BarcodeWorker.GetIdByRef(
                        propertyData.PropertyType,
                        propertyData.Value.ToString());
                    long id = Convert.ToInt64(idValue);

                    if(!id.Equals(0L))
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