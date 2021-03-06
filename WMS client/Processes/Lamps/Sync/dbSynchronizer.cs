using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using WMS_client.Enums;
using WMS_client.db;
using WMS_client.Processes.Lamps.Sync;
using System.Diagnostics;
using WMS_client.Utils;

namespace WMS_client
    {
    /// <summary>������������� ������ ����� ��� � ��������</summary>
    public class dbSynchronizer : BusinessProcess
        {
        /// <summary>
        /// ������������ ��� ������������� ���� �������� ����������� �� �������
        /// </summary>
        public bool useLoggingSyncronization = true;

        private IServerIdProvider serverIdProvider = null;
        /// <summary>�������������� ����� ��� ����������� �������� �������� �������������</summary>
        private MobileLabel infoLabel;
        /// <summary>������ ���������� �������</summary>
        private List<DataAboutDeferredProperty> deferredProperty;

        private StringBuilder logBuilder;

        /// <summary>����������� ����� ����� ���������</summary>
        public const string PARAMETER = "Parameter";

        /// <summary>������������� ������ ����� ��� � ��������</summary>
        /// <param name="MainProcess"></param>
        public dbSynchronizer(WMSClient MainProcess, IServerIdProvider serverIdProvider)
            : base(MainProcess, 1)
            {
            StartNetworkConnection();

            if (serverIdProvider == null)
                {
                throw new ArgumentException("ServerIdProvider");
                }
            this.serverIdProvider = serverIdProvider;

            deferredProperty = new List<DataAboutDeferredProperty>();

            SynchronizeWithGreenhouse(serverIdProvider);
            }

        private void SynchronizeWithGreenhouse(IServerIdProvider serverIdProvider)
            {
            logBuilder = new StringBuilder(string.Format("Synchronizing start: {0}", DateTime.Now.ToString("HH:mm:ss dd.MM.yyyy")));
            logBuilder.AppendLine();
            Stopwatch totalTime = new Stopwatch();
            totalTime.Start();
            //���������
            infoLabel.Text = "�����������";
            if (!SyncObjects<Contractors>(WaysOfSync.OneWay, FilterSettings.CanSynced))
                {
                return;
                }

            infoLabel.Text = "�����";
            if (!SyncObjects<Maps>(false))
                {
                return;
                }

            infoLabel.Text = "����";
            if (!SyncObjects<Party>(WaysOfSync.OneWay))
                {
                return;
                }

            infoLabel.Text = "�����";
            if (!SyncObjects<Models>(WaysOfSync.TwoWay))
                {
                return;
                }

            //�������� ������
            infoLabel.Text = "��������� �������� ������ �������������";
            SyncAccepmentsDocWithServer();
            SyncAccepmentsDocFromServer();

            //������������
            infoLabel.Text = "�����";
            if (!SyncObjects<Lamps>(WaysOfSync.TwoWay))
                {
                return;
                }

            infoLabel.Text = "��.�����";
            if (!SyncObjects<ElectronicUnits>(WaysOfSync.TwoWay))
                {
                return;
                }

            infoLabel.Text = "�������";
            if (!SyncObjects<Cases>(WaysOfSync.TwoWay))
                {
                return;
                }

            //��������� ��������
            infoLabel.Text = "��������� ��������";
            updateDeferredProperties();

            PerformQuery("EndOfSync");
            //³������� �� ...
            infoLabel.Text = "³������� �� ��������";
            SyncOutSending<SendingToCharge, SubSendingToChargeChargeTable>();
            SyncInSending<SendingToCharge, SubSendingToChargeChargeTable>();

            infoLabel.Text = "³������� �� ����";
            SyncOutSending<SendingToExchange, SubSendingToExchangeUploadTable>();
            SyncInSending<SendingToExchange, SubSendingToExchangeUploadTable>();

            infoLabel.Text = "³������� �� ������";
            SyncOutSending<SendingToRepair, SubSendingToRepairRepairTable>();
            SyncInSending<SendingToRepair, SubSendingToRepairRepairTable>();

            //��������� �������������� � ...
            infoLabel.Text = "��������� � �������";
            SyncOutSending<AcceptanceAccessoriesFromRepair, SubAcceptanceAccessoriesFromRepairRepairTable>(
                SyncModes.AcceptanceFromRepair);
            SyncInSending<AcceptanceAccessoriesFromRepair, SubAcceptanceAccessoriesFromRepairRepairTable>();

            infoLabel.Text = "��������� � �����";
            SyncOutAcceptanceFromExchange();
            SyncInSending<AcceptanceAccessoriesFromExchange, SubAcceptanceAccessoriesFromExchangeExchange>(
                SyncModes.SendingToExchange);

            //����������
            infoLabel.Text = "����������";
            SyncMovement();

            logBuilder.AppendLine();
            logBuilder.AppendLine(string.Format("Total: {0}", (int)(totalTime.ElapsedMilliseconds * 0.001)));
            logToFile("SynchLog.txt", logBuilder);
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
        /// <summary>������������� �������</summary>
        /// <param name="updId">����� �������� ID</param>
        private bool SyncObjects<T>(bool updId) where T : dbObject
            {
            return SyncObjects<T>(typeof(T).Name, WaysOfSync.OneWay, FilterSettings.None, false, false);
            }

        /// <summary>������������� �������</summary>
        /// <param name="wayOfSync">������ �������������</param>
        private bool SyncObjects<T>(WaysOfSync wayOfSync) where T : dbObject
            {
            return SyncObjects<T>(typeof(T).Name, wayOfSync, FilterSettings.None, false, true);
            }

        /// <summary>������������� �������</summary>
        /// <param name="wayOfSync">������ �������������</param>
        /// <param name="filter">�������</param>
        private bool SyncObjects<T>(WaysOfSync wayOfSync, FilterSettings filter) where T : dbObject
            {
            return SyncObjects<T>(typeof(T).Name, wayOfSync, filter, false, true);
            }




        /// <summary>������������� �������</summary>
        /// <param name="tableName">��� �������</param>
        /// <param name="wayOfSync">������ �������������</param>
        /// <param name="filter">�������</param>
        /// <param name="skipExists">���������� ������������</param>
        /// <param name="updId">����� �������� ID</param>
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

            //������� (������� �������������, �����-���) ���� �� ��������� ��������� � ������� tableName
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

        /// <summary>���������� ��������� ���� ����� �������������</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="skipExists">���������� ������������</param>
        /// <param name="updId">����� �������� ID</param>
        private void updateObjOnLocalDb<T>(CatalogSynchronizer synchronizer, bool skipExists, bool updId) where T : dbObject
            {
            //������ ������ �� ���������� ��������� � "�������"
            DataTable changesForTsd = ResultParameters[1] as DataTable;
            if (changesForTsd != null)
                {
                //���������� ��������� �� ��������� ����
                CreateSyncObject<T>(synchronizer, changesForTsd, skipExists, ref deferredProperty, updId);
                }
            }

        /// <summary>������� ������ ��� ���������� ������� ����� �������������</summary>
        /// <param name="tableName"></param>
        private void updateObjOnServDb<T>(string tableName) where T : dbObject
            {
            //��������� ���������, ������� ����� �������� �� "�������"
            DataTable changesForServ = ResultParameters[2] as DataTable;
            using (SqlCeCommand query = dbWorker.NewQuery(string.Empty))
                {
                StringBuilder where = new StringBuilder();

                if (changesForServ != null && changesForServ.Rows.Count != 0)
                    {
                    //������������ �������� ��� ������ ������ �� ���� ��������� 
                    //�� ������� tableName, ������� ���������� �������� �� "�������" 
                    int index = 0;

                    foreach (DataRow row in changesForServ.Rows)
                        {
                        //���������� ����������
                        query.AddParameter(string.Concat(PARAMETER, index), row[dbObject.SYNCREF_NAME]);
                        @where.AppendFormat(" RTRIM([{0}].[{1}])=RTRIM(@{2}{3}) OR",
                                            tableName, dbObject.SYNCREF_NAME, PARAMETER, index);
                        index++;
                        }


                    string whereStr = @where.ToString(0, @where.Length - 3);
                    //���������� ������� ������������� ��� ��������� ����
                    query.CommandText = string.Format("UPDATE {0} SET {1}=1 WHERE {2}",
                                                      tableName,
                                                      dbObject.IS_SYNCED,
                                                      whereStr);
                    query.ExecuteNonQuery();

                    if (changesForServ.Rows.Count > 0)
                        {
                        //������� ������
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

        /// <summary>�������� �� ������������������ ��������� ������</summary>
        /// <param name="type">��� �������</param>
        /// <param name="whereClause">where ������</param>
        /// <returns>SQL ������� ��� ��������� ������</returns>
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

        /// <summary>�������� �������� �������������</summary>
        /// <param name="table">������ � ��������</param>
        /// <param name="skipExists">���������� ������������</param>
        /// <param name="deferredProperty">������ ��������� �������</param>
        /// <param name="updId">����� �������� ID</param>
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

                        //�� ���������� �� ������� � ����� �������?
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
        /// <summary>���������������� �������� ������������� � ��������</summary>
        private void SyncAccepmentsDocWithServer()
            {
            DataTable data = AcceptanceOfNewComponentsDetails.GetAllData();
            PerformQuery("SetAcceptDocs", data);
            dbArchitector.ClearAllDataFromTable(typeof(AcceptanceOfNewComponentsDetails).Name);
            }

        /// <summary>���������������� ��������� "������� ������ �����." � �������</summary>
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
                        object caseModelObj = BarcodeWorker.GetIdByRef(typeof(Models), caseModelRef);
                        object lampModelObj = BarcodeWorker.GetIdByRef(typeof(Models), lampModelRef);
                        object unitModelObj = BarcodeWorker.GetIdByRef(typeof(Models), unitModelRef);

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

        /// <summary>���������������� ("���������") ������ �� ������������</summary>
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
                //��������� ������
                string delCommand = string.Concat("DELETE FROM ", tableName);
                using (SqlCeCommand query = dbWorker.NewQuery(delCommand))
                    {
                    query.ExecuteNonQuery();
                    }
                }
            }

        #endregion

        #region Sync.Sending
        /// <summary>���������������� ��������� �� ��� "��������� �� .." �� ���</summary>
        /// <typeparam name="T">��������</typeparam>
        /// <typeparam name="S">�������</typeparam>
        private void SyncInSending<T, S>()
            where T : Sending
            where S : SubSending
            {
            SyncInSending<T, S>(SyncModes.StandartToX);
            }

        /// <summary>���������������� ��������� �� ��� "��������� �� .." �� ���</summary>
        /// <typeparam name="T">��������</typeparam>
        /// <typeparam name="S">�������</typeparam>
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
                                object modelId = BarcodeWorker.GetIdByRef(typeof(Models), syncRef);

                                newSubDoc.SetValue("Nomenclature", modelId);
                                break;
                            }

                        newSubDoc.Sync<S>();
                        }
                    }
                }
            }

        /// <summary>���������������� ��������� �� ��� "��������� �� .." �� �������</summary>
        /// <typeparam name="T">��������</typeparam>
        /// <typeparam name="S">�������</typeparam>
        private void SyncOutSending<T, S>()
            where T : Sending
            where S : SubSending
            {
            SyncOutSending<T, S>(SyncModes.StandartToX);
            }

        /// <summary>���������������� ��������� �� ��� "��������� �� .." �� �������</summary>
        /// <typeparam name="T">��������</typeparam>
        /// <typeparam name="S">�������</typeparam>
        /// <param name="mode">����� �������������</param>
        private void SyncOutSending<T, S>(SyncModes mode)
            where T : Sending
            where S : SubSending
            {
            string docName = typeof(T).Name;
            string tableName = typeof(S).Name;

            //1. ���������� �� �������
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
                //2. �������� ����������� (? � ����� �� ... ����� ����� ���� �������� ���������)
                command = string.Format("DELETE FROM {0} WHERE IsSynced=0", tableName);
                using (SqlCeCommand query = dbWorker.NewQuery(command))
                    {
                    query.ExecuteNonQuery();
                    }

                //3. �������� ��������� ��������� ���������
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

        /// <summary>������������� ������ "������� � ������" ��� �������</summary>
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
        /// <summary>�������� ��������� ��������</summary>
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

            MainProcess.CreateLabel("�������������!", 5, 125, 230, MobileFontSize.Large, MobileFontPosition.Center, MobileFontColors.Info, FontStyle.Bold);
            MainProcess.CreateLabel("����� �����������:", 5, 155, 230, MobileFontSize.Normal, MobileFontPosition.Center);
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