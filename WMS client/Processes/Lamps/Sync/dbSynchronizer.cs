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
        /// <summary></summary>
        private MobileLabel infoLabel;
        /// <summary>Список отложенных свойств</summary>
        private List<DataAboutDeferredProperty> deferredProperty;
        public const string PARAMETER = "Parameter";

        /// <summary>Синхронизация данных между ТСД и сервером</summary>
        /// <param name="MainProcess"></param>
        public dbSynchronizer(WMSClient MainProcess)
            : base(MainProcess, 1)
        {
            deferredProperty = new List<DataAboutDeferredProperty>();

            infoLabel.Text = "Контрагенты";
            SyncObjects<Contractors>(WaysOfSync.OneWay, FilterSettings.CanSynced);
            infoLabel.Text = "Карты";
            SyncObjects<Maps>(WaysOfSync.OneWay);
            infoLabel.Text = "Партии";
            SyncObjects<Party>(WaysOfSync.OneWay);
            infoLabel.Text = "Модели";
            SyncObjects<Models>(WaysOfSync.TwoWay);
            infoLabel.Text = "Лампы";
            SyncObjects<Lamps>(WaysOfSync.TwoWay);
            infoLabel.Text = "Ел.блоки";
            SyncObjects<ElectronicUnits>(WaysOfSync.TwoWay);
            infoLabel.Text = "Корпусы";
            SyncObjects<Cases>(WaysOfSync.TwoWay);

            updateDeferredProperties();
            PerformQuery("EndOfSync");

            infoLabel.Text = "Документы приемки новых комплектующих";
            SyncAccepmentsDocWithServer();
            infoLabel.Text = "Отправка на списание";
            SyncOutSending<SendingToCharge, SubSendingToChargeChargeTable>();
            SyncInSending<SendingToCharge, SubSendingToChargeChargeTable>();
            infoLabel.Text = "Отправка на ремонт";
            SyncOutSending<SendingToRepair, SubSendingToRepairRepairTable>();
            SyncInSending<SendingToRepair, SubSendingToRepairRepairTable>();

            MainProcess.ClearControls();
            MainProcess.Process = new SelectingLampProcess(MainProcess);
        }

        #region Sync ...
        /// <summary>Тестовая синхронизация</summary>
        /// <param name="wayOfSync">Способ синхронизации</param>
        public void SyncObjects<T>(WaysOfSync wayOfSync) where T : dbObject
        {
            SyncObjects<T>(typeof(T).Name, wayOfSync, FilterSettings.None);
        }

        /// <summary>Тестовая синхронизация</summary>
        /// <param name="wayOfSync">Способ синхронизации</param>
        /// /// <param name="filter"></param>
        public void SyncObjects<T>(WaysOfSync wayOfSync, FilterSettings filter) where T : dbObject
        {
            SyncObjects<T>(typeof(T).Name, wayOfSync, filter);
        }

        /// <summary>Тестовая синхронизация</summary>
        /// <param name="tableName">Имя таблицы</param>
        /// <param name="wayOfSync">Способ синхронизации</param>
        /// <param name="filter"></param>
        public void SyncObjects<T>(string tableName, WaysOfSync wayOfSync, FilterSettings filter) where T : dbObject
        {
            //Выбрать (Признак синхронизации, Штрих-код) всех не удаленных элементов с таблицы tableName
            string command = string.Format("SELECT RTRIM({0}){0},RTRIM({1}){1} FROM {2} WHERE {3}=0",
                                           dbObject.IS_SYNCED,
                                           dbObject.BARCODE_NAME,
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
                updateObjOnLocalDb<T>();

                if (wayOfSync == WaysOfSync.TwoWay)
                {
                    updateObjOnServDb<T>(tableName);
                }
            }
        }

        private void SyncAccepmentsDocWithServer()
        {
            DataTable acceptedDoc = AcceptanceOfNewComponents.GetAcceptedDocuments();
            DataTable notAcceptedDoc = AcceptanceOfNewComponents.GetNotAcceptedDocuments();

            PerformQuery("GetAcceptDocs",acceptedDoc,notAcceptedDoc);

            if (IsExistParameters)
            {
                AcceptanceOfNewComponents.ClearAcceptedDocuments();
                DataTable table = Parameters[0] as DataTable;

                if (table != null)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        AcceptanceOfNewComponents doc = new AcceptanceOfNewComponents
                                                            {
                                                                Id = Convert.ToInt64(row["Id"]),
                                                                Contractor = Convert.ToInt64(row["Contractor"]),
                                                                Date = Convert.ToDateTime(row["Date"]),
                                                                InvoiceDate = Convert.ToDateTime(row["InvoiceDate"]),
                                                                InvoiceNumber = Convert.ToInt64(row["InvoiceNumber"]),
                                                                MarkForDeleting = false,
                                                                Model = Convert.ToInt64(row["Model"]),
                                                                TypesOfWarrantly= (TypesOfLampsWarrantly)
                                                                    Convert.ToInt32(row["TypesOfWarrantly"]),
                                                                TypeOfAccessories =(TypeOfAccessories)
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
                            string markingBarcode = row["Marking"].ToString();
                            object idObj = BarcodeWorker.GetIdByBarcode(typeof (Models), markingBarcode);

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

        private void SyncOutSending<T, S>()
            where T : Sending
            where S : SubSending
        {
            string docName = typeof (T).Name;
            string tableName = typeof (S).Name;

            //1. Обновление на сервере
            string command = string.Format("SELECT Id, Document FROM {0} WHERE IsSynced=0", tableName);
            SqlCeCommand query = dbWorker.NewQuery(command);
            DataTable table = query.SelectToTable();
            PerformQuery("SetSendingDocs", docName, tableName, table);

            bool fullDeleteAccepted = typeof (T) == typeof (SendingToRepair);

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
                //2. Удаление обнавленных (? а нужно ли ... может когда весь документ удаляется)
                command = string.Format("DELETE FROM {0} WHERE IsSynced=0", tableName);
                query = dbWorker.NewQuery(command);
                query.ExecuteNonQuery();

                //3. Удаление полностью принятого документа
                command = string.Format(@"SELECT s.Id
FROM SendingToCharge s 
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

        private void SyncInSending<T, S>() where T:Sending where S:SubSending
        {
            PerformQuery("GetSendingDocs", typeof(T).Name);

            if (IsExistParameters)
            {
                DataTable table = Parameters[0] as DataTable;

                if(table!=null)
                {
                    T newDoc = null;

                    foreach (DataRow row in table.Rows)
                    {
                        int currId = Convert.ToInt32(row["Id"]);
                        
                        if(newDoc==null || newDoc.Id!=currId)
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
                        newSubDoc.Document = row["Document"].ToString();
                        newSubDoc.Id = currId;
                        newSubDoc.IsSynced = true;
                        newSubDoc.Sync<S>();
                    }
                }
            }
        }
        #endregion

        #region Update on ...
        /// <summary>Обновление локальной базы после синхронизации</summary>
        private void updateObjOnLocalDb<T>() where T : dbObject
        {
            //Данные только по измененным элементам с "сервера"
            DataTable changesForTsd = Parameters[1] as DataTable;
            //Обновление элементов на локальной базе
            CreateSyncObject<T>(changesForTsd, false, ref deferredProperty);
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
                    query.AddParameter(string.Concat(PARAMETER, index), row[dbObject.BARCODE_NAME]);
                    where.AppendFormat(" RTRIM([{0}].[{1}])=RTRIM(@{2}{3}) OR",
                                       tableName, dbObject.BARCODE_NAME, PARAMETER, index);
                    index++;
                }


                string whereStr = where.ToString(0, where.Length - 3);
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
                    DataTable changes = query.SelectToTable();

                    PerformQuery("SyncChangesForServer", tableName, changes);
                }
            }
        } 
        #endregion

        #region Create objects
        /// <summary>Создание объектов синхронизации</summary>
        /// <param name="table">Данные о объектах</param>
        /// <param name="skipExists">Пропустить существующие</param>
        /// <param name="deferredProperty">Список отложеных свойств</param>
        public static void CreateSyncObject<T>(DataTable table, bool skipExists, ref List<DataAboutDeferredProperty> deferredProperty) where T : dbObject
        {
            if (table != null)
            {
                Type type = typeof(T);
                PropertyInfo[] properties = type.GetProperties();
                string barcode = string.Empty;
                string barcodeName = dbObject.BARCODE_NAME.ToLower();
                int lastDeferredIndex = 0;

                foreach (DataRow row in table.Rows)
                {
                    object newObj = Activator.CreateInstance(typeof(T));
                    T newObject = (T)newObj;
                    bool needDeferred = false;

                    foreach (PropertyInfo property in properties)
                    {
                        dbAttributes attribute = Attribute.GetCustomAttribute(property, typeof(dbAttributes)) as dbAttributes;

                        if (attribute != null && table.Columns.Contains(property.Name))
                        {
                            object value = row[property.Name];

                            //Не существует ли елемент с таким штрих кодом?
                            if (property.Name.ToLower().Equals(barcodeName))
                            {
                                if (BarcodeWorker.IsBarcodeExist(value.ToString()))
                                {
                                    if (skipExists)
                                    {
                                        break;
                                    }

                                    newObject.SetNotNew();
                                    barcode = value.ToString();
                                }
                                else
                                {
                                    barcode = string.Empty;
                                }
                            }
                            else if (property.Name.ToLower().Equals(dbObject.IDENTIFIER_NAME))
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
                                    //todo: Отложенное сохранение
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

                            property.SetValue(newObject, value, null);
                        }
                    }

                    ISynced syncObject = newObject as ISynced;

                    if (syncObject != null)
                    {
                        syncObject.IsSynced = true;
                    }

                    if (!skipExists && !string.IsNullOrEmpty(barcode))
                    {
                        newObject.Id = Convert.ToInt64(BarcodeWorker.GetIdByBarcode(barcode));
                    }

                    newObject.Sync<T>();

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
        #endregion

        #region Deferred
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
                    object idValue = BarcodeWorker.GetIdByBarcode(
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

        #region Get data
        private string getUnsyncLinkedData(Type type, string whereClause)
        {
            StringBuilder selectClause = new StringBuilder();
            StringBuilder fromClause = new StringBuilder();
            PropertyInfo[] fields = type.GetProperties();

            foreach (PropertyInfo field in fields)
            {
                dbAttributes attribute = Attribute.GetCustomAttribute(field, typeof(dbAttributes)) as dbAttributes;

                if (attribute != null)
                {
                    if (attribute.dbObjectType == null)
                    {
                        selectClause.AppendFormat("    [{0}].[{1}],\r\n", type.Name, field.Name);
                    }
                    else
                    {
                        selectClause.AppendFormat("    [{0}].[{1}] [{2}],\r\n", attribute.dbObjectType.Name, dbObject.BARCODE_NAME, field.Name);
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
        #endregion

        #region Overrides of BusinessProcess
        public override void DrawControls()
        {
            MainProcess.ClearControls();

            MainProcess.CreateLabel("Синхронизация начата!", 5, 125, 230, MobileFontSize.Large, MobileFontPosition.Center, MobileFontColors.Info, FontStyle.Bold);
            MainProcess.CreateLabel("Сейчас обновляется:", 5, 155, 230, MobileFontSize.Normal, MobileFontPosition.Center);
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