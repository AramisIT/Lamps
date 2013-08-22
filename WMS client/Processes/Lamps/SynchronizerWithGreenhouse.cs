using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using WMS_client.Enums;
using WMS_client.Models;

namespace WMS_client
    {
    /// <summary>Синхронизация данных между ТСД и сервером</summary>
    public class SynchronizerWithGreenhouse : BusinessProcess
        {
        /// <summary>
        /// Использовать для синхронизации ламп механизм логирования на сервере
        /// </summary>
        public bool useLoggingSyncronization = true;

        /// <summary>Информационная метка для уведомления текущего процессе синхронизации</summary>
        private MobileLabel infoLabel;

        private StringBuilder logBuilder;
        private const string DATE_TIME_FORMAT = "dd.MM.yyyy";

        /// <summary>Константная часть имени параметра</summary>
        public const string PARAMETER = "Parameter";

        /// <summary>Синхронизация данных между ТСД и сервером</summary>
        /// <param name="MainProcess"></param>
        public SynchronizerWithGreenhouse(WMSClient MainProcess)
            : base(MainProcess, 1)
            {
            if (applicationIsClosing)
                {
                return;
                }
            StartNetworkConnection();
           
            Configuration.Current.Repository.LoadingDataFromGreenhouse = true;

            SynchronizeWithGreenhouse();

            Configuration.Current.Repository.LoadingDataFromGreenhouse = false;

            ShowProgress(1, 1);
            }

        private void SynchronizeWithGreenhouse()
            {
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

            bool catalogChanged;
            var repository = Configuration.Current.Repository;

            infoLabel.Text = "Імпорт моделей";
            if (!downloadCatalogs<Model, Int16>("LampsModels", out catalogChanged))
                {
                return;
                }
            if (catalogChanged)
                {
                repository.ResetModels();
                }

            infoLabel.Text = "Імпорт мап";
            if (!downloadCatalogs<Map, Int32>("Maps", out catalogChanged))
                {
                return;
                }
            if (catalogChanged)
                {
                repository.ResetMaps();
                }

            infoLabel.Text = "Імпорт партій";
            if (!downloadCatalogs<PartyModel, Int32>("Party", out catalogChanged))
                {
                return;
                }
            if (catalogChanged)
                {
                repository.ResetParties();
                }
            }

        private bool downloadCatalogs<T, ID>(string greenhouseTableName, out bool catalogChanged) where T : ICatalog<ID>, new()
            {
            catalogChanged = false;
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

                catalogChanged = true;

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
                        party.Date = DateTime.ParseExact(row["Date"] as string, DATE_TIME_FORMAT, null);
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