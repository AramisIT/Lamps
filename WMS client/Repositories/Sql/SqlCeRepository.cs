using System;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Microsoft.WindowsMobile.Status;
using WMS_client.db;
using WMS_client.Enums;
using WMS_client.Models;
using System.Data;
using WMS_client.Repositories.Sql;
using WMS_client.Repositories.Sql.Updaters;
using WMS_client.Utils;

namespace WMS_client.Repositories
    {

    public class SqlCeRepository : IRepository
        {
        public SqlCeRepository()
            {
            initConnectionString();

            minAccessoryId = Configuration.Current.TerminalId * 10 * 1000 * 1000;
            maxAccessoryId = (Configuration.Current.TerminalId + 1) * 10 * 1000 * 1000 - 1;

            nextUnitId = getNextId("Units");
            nextLampId = getNextId("Lamps");

            lastUpdatedLampId = (int)readDatabaseParameter(DatabaseParametersConsts.LAST_UPLOADED_LAMP_ID);
            lastUpdatedUnitId = (int)readDatabaseParameter(DatabaseParametersConsts.LAST_UPLOADED_UNIT_ID);
            }

        private long readDatabaseParameter(DatabaseParametersConsts databaseParameterId)
            {
            int parameterId = (int)databaseParameterId;
            using (var conn = getOpenedConnection())
                {
                using (var cmd = conn.CreateCommand())
                    {
                    cmd.CommandType = CommandType.TableDirect;
                    cmd.CommandText = "DatabaseParameters";
                    cmd.IndexName = "PK_DatabaseParameters";

                    using (var result = cmd.ExecuteResultSet(UPDATABLE_RESULT_SET_OPTIONS))
                        {
                        if (result.Seek(DbSeekOptions.FirstEqual, parameterId))
                            {
                            result.Read();
                            return result.GetInt64(DatabaseParameters.Value);
                            }
                        else
                            {
                            var newRow = result.CreateRecord();
                            newRow.SetInt64(DatabaseParameters.Value, 0);
                            newRow.SetInt32(DatabaseParameters.Id, parameterId);
                            result.Insert(newRow);
                            return 0;
                            }
                        }
                    }
                }
            }

        private void updateDatabaseParameter(DatabaseParametersConsts databaseParameter, long value)
            {
            int parameterId = (int)databaseParameter;
            using (var conn = getOpenedConnection())
                {
                using (var cmd = conn.CreateCommand())
                    {
                    cmd.CommandType = CommandType.TableDirect;
                    cmd.CommandText = "DatabaseParameters";
                    cmd.IndexName = "PK_DatabaseParameters";

                    using (var result = cmd.ExecuteResultSet(UPDATABLE_RESULT_SET_OPTIONS))
                        {
                        if (result.Seek(DbSeekOptions.FirstEqual, parameterId))
                            {
                            result.Read();
                            result.SetInt64(DatabaseParameters.Value, value);
                            result.Update();
                            }
                        else
                            {
                            var newRow = result.CreateRecord();
                            newRow.SetInt64(DatabaseParameters.Value, value);
                            newRow.SetInt32(DatabaseParameters.Id, parameterId);
                            result.Insert(newRow);
                            }
                        }
                    }
                }
            }

        public bool LoadingDataFromGreenhouse { get; set; }

        public bool IsIntactDatabase(out bool lowPower)
            {
            using (var conn = getOpenedConnection())
                {
                lowPower = BatteryChargeStatus.Critical;

                if (conn == null)
                    {
                    return lowPower;
                    }

                using (var cmd = conn.CreateCommand())
                    {
                    const string sql = @"
select Id from Lamps where Id <= 0
union all
select Id from Units where Id <= 0
union all 
select Id from Cases where Id <= 0
";
                    cmd.CommandText = sql;

                    try
                        {
                        cmd.ExecuteScalar();
                        }
                    catch (Exception exp)
                        {
                        Trace.WriteLine(string.Format(
                            "Ошибка выполнения тестового запроса к основным таблицам: {0}",
                            exp.Message));
                        return false;
                        }
                    }
                }

            return true;
            }

        public bool UpdateMaps(List<Map> maps)
            {
            var updater = new MapsUpdater();
            updater.InitUpdater(maps, getOpenedConnection);
            return updater.Update();
            }

        public bool UpdateParties(List<PartyModel> parties)
            {
            var updater = new PartiesUpdater();
            updater.InitUpdater(parties, getOpenedConnection);
            return updater.Update();
            }

        public bool UpdateModels(List<Model> models)
            {
            var updater = new ModelsUpdater();
            updater.InitUpdater(models, getOpenedConnection);
            return updater.Update();
            }

        public bool UpdateCases(List<Case> cases, bool justInsert)
            {
            var updater = new CasesUpdater()
            {
                JustInsert = justInsert,
                LoadingDataFromGreenhouse = this.LoadingDataFromGreenhouse
            };
            updater.InitUpdater(cases, getOpenedConnection);

            return updater.Update();
            }

        public bool UpdateLamps(List<Lamp> lamps, bool justInsert)
            {
            var updater = new LampsUpdater()
            {
                JustInsert = justInsert,
                LoadingDataFromGreenhouse = this.LoadingDataFromGreenhouse
            };
            updater.Don_tAddNewToLog = true;
            updater.LastUploadedToGreenhouseId = lastUpdatedLampId;
            updater.MinAccessoryIdForCurrentPdt = minAccessoryId;
            updater.MaxAccessoryIdForCurrentPdt = maxAccessoryId;

            updater.InitUpdater(lamps, getOpenedConnection);
            return updater.Update();
            }

        public bool UpdateUnits(List<Unit> units, bool justInsert)
            {
            var updater = new UnitsUpdater()
            {
                JustInsert = justInsert,
                LoadingDataFromGreenhouse = this.LoadingDataFromGreenhouse
            };
            updater.Don_tAddNewToLog = true;
            updater.LastUploadedToGreenhouseId = lastUpdatedUnitId;
            updater.MinAccessoryIdForCurrentPdt = minAccessoryId;
            updater.MaxAccessoryIdForCurrentPdt = maxAccessoryId;

            updater.InitUpdater(units, getOpenedConnection);
            return updater.Update();
            }

        public int GetNextUnitId()
            {
            int nextId = nextUnitId;
            nextUnitId++;

            return nextId;
            }

        public int GetNextLampId()
            {
            int nextId = nextLampId;
            nextLampId++;

            return nextId;
            }

        public List<CatalogItem> ModelsList
            {
            get
                {
                var cache = modelsCache ?? (modelsCache = buildModelsCache());
                return cache.ItemsList;
                }
            }

        public List<CatalogItem> MapsList
            {
            get
                {
                var cache = mapsCache ?? (mapsCache = buildMapsCache());
                return cache.ItemsList;
                }
            }

        public List<CatalogItem> PartiesList
            {
            get
                {
                var cache = partiesCache ?? (partiesCache = buildPartiesCache());
                return cache.ItemsList;
                }
            }

        public Model GetModel(short id)
            {
            var cache = modelsCache ?? (modelsCache = buildModelsCache());
            return cache.GetCatalogItem(id);
            }

        public Map GetMap(int id)
            {
            var cache = mapsCache ?? (mapsCache = buildMapsCache());
            return cache.GetCatalogItem(id);
            }

        public PartyModel GetParty(int partyId)
            {
            var cache = partiesCache ?? (partiesCache = buildPartiesCache());
            return cache.GetCatalogItem(partyId);
            }

        public IAccessory FindAccessory(int accessoryBarcode)
            {
            IAccessory accessory = this.ReadCase(accessoryBarcode);
            if (accessory != null)
                {
                return accessory;
                }

            accessory = ReadUnitByBarcode(accessoryBarcode);
            if (accessory != null)
                {
                return accessory;
                }

            accessory = ReadLampByBarcode(accessoryBarcode);

            return accessory;
            }

        public List<Lamp> ReadLamps(List<int> ids)
            {
            return getAccessories<Lamp>("Lamps", "PK_Lamps", ids, createLamp);
            }

        public List<Case> ReadCases(List<int> ids)
            {
            return getAccessories<Case>("Cases", "PK_Cases", ids, createCase);
            }

        public List<Unit> ReadUnits(List<int> ids)
            {
            return getAccessories<Unit>("Units", "PK_Units", ids, createUnit);
            }

        public Case FintCaseByLamp(int lampId)
            {
            if (lampId == 0)
                {
                return null;
                }
            var result = getAccessories<Case>("Cases", "Cases_Lamp", new List<int>() { lampId }, createCase);
            return result.Count > 0 ? result[0] : null;
            }

        private List<T> getAccessories<T>(string tableName, string indexName, List<int> predicateValues,
            Func<SqlCeResultSet, T> createAccesoryMethod)
            {
            var resultList = new List<T>();

            using (var conn = getOpenedConnection())
                {
                using (var cmd = conn.CreateCommand())
                    {
                    cmd.CommandType = CommandType.TableDirect;
                    cmd.CommandText = tableName;
                    cmd.Connection = conn;
                    cmd.IndexName = indexName;

                    using (var result = cmd.ExecuteResultSet(READ_ONLY_RESULT_SET_OPTIONS))
                        {
                        foreach (var predicateValue in predicateValues)
                            {
                            if (result.Seek(DbSeekOptions.FirstEqual, predicateValue))
                                {
                                result.Read();

                                resultList.Add(createAccesoryMethod(result));
                                }
                            }
                        }
                    }
                }

            return resultList;
            }

        public Case FintCaseByUnit(int unitId)
            {
            if (unitId == 0)
                {
                return null;
                }
            var result = getAccessories<Case>("Cases", "Cases_Unit", new List<int>() { unitId }, createCase);
            return result.Count > 0 ? result[0] : null;
            }

        public Unit ReadUnitByBarcode(int barcode)
            {
            if (barcode == 0)
                {
                return null;
                }
            var result = getAccessories<Unit>("Units", "Units_Barcode", new List<int>() { barcode }, createUnit);
            return result.Count > 0 ? result[0] : null;
            }

        public Lamp ReadLampByBarcode(int barcode)
            {
            if (barcode == 0)
                {
                return null;
                }
            var result = getAccessories<Lamp>("Lamps", "Lamps_Barcode", new List<int>() { barcode }, createLamp);
            return result.Count > 0 ? result[0] : null;
            }

        #region private

        private string connectionString;

        public const string DATABASE_FILE_NAME = "LampsBase.sdf";

        private int nextUnitId;
        private int nextLampId;

        private void initConnectionString()
            {
            connectionString = String.Format("Data Source='{0}';",
                Configuration.Current.PathToApplication + '\\' + DATABASE_FILE_NAME);
            }

        private SqlCeConnection getOpenedConnection()
            {
            if (BatteryChargeStatus.Critical)
                {
                MessageBox.Show("Акумулятор розряджений. Негайно поставте термінал на зарядку на натисніть ОК");

                if (BatteryChargeStatus.Critical)
                    {
                    return null;
                    }
                }

            var conn = new SqlCeConnection(connectionString);
            try
                {
                conn.Open();
                }
            catch (Exception exp)
                {
                MessageBox.Show(string.Format("Ошибка подключения к базе: {0}", exp.Message));
                return null;
                }

            return conn;
            }

        private readonly int minAccessoryId;
        private readonly int maxAccessoryId;

        public const ResultSetOptions UPDATABLE_RESULT_SET_OPTIONS =
            ResultSetOptions.Scrollable | ResultSetOptions.Sensitive | ResultSetOptions.Updatable;

        public const ResultSetOptions READ_ONLY_RESULT_SET_OPTIONS =
            ResultSetOptions.Scrollable | ResultSetOptions.Sensitive;

        private CatalogCache<int, PartyModel> partiesCache;
        private CatalogCache<int, Map> mapsCache;
        private CatalogCache<Int16, Model> modelsCache;
        private int lastUpdatedUnitId;
        private int lastUpdatedLampId;

        private CatalogCache<int, PartyModel> buildPartiesCache()
            {
            var result = new CatalogCache<int, PartyModel>();

            const string sql =
                "select Id, Description, ContractorDescription, DateOfActSet, Date, WarrantyHours, WarrantyMonths, WarrantyType from Parties order by Date desc";

            result.Load(sql, getOpenedConnection, (reader, catalog) =>
            {
                catalog.Id = Convert.ToInt32(reader["Id"]);
                catalog.WarrantyHours = Convert.ToInt16(reader["WarrantyHours"]);
                catalog.WarrantyMonths = Convert.ToInt16(reader["WarrantyMonths"]);
                catalog.Description = (reader["Description"] as string).TrimEnd();
                catalog.ContractorDescription = (reader["ContractorDescription"] as string).TrimEnd();
                catalog.Date = (DateTime)(reader["Date"]);
                catalog.WarrantyType = (byte)reader["WarrantyType"];
                object dateTimeValue = reader["DateOfActSet"];
                if (DBNull.Value.Equals(dateTimeValue))
                    {
                    dateTimeValue = DateTime.MinValue;
                    }
                catalog.DateOfActSet = (DateTime)dateTimeValue;
            });

            return result;
            }

        private CatalogCache<int, Map> buildMapsCache()
            {
            var result = new CatalogCache<int, Map>();

            const string sql = "select Id, Description from Maps order by Description";

            result.Load(sql, getOpenedConnection, (reader, catalog) =>
            {
                catalog.Description = (reader["Description"] as string).Trim();
                catalog.Id = Convert.ToInt32(reader["Id"]);
            });

            return result;
            }

        private CatalogCache<Int16, Model> buildModelsCache()
            {
            var result = new CatalogCache<Int16, Model>();

            const string sql = "select Id, Description from Models order by Description";

            result.Load(sql, getOpenedConnection, (reader, catalog) =>
            {
                catalog.Description = (reader["Description"] as string).Trim();
                catalog.Id = Convert.ToInt16(reader["Id"]);
            });

            return result;
            }

        private IAccessory readAccessory(string sqlCommand, int predicateValue,
            Func<SqlCeDataReader, IAccessory> createAccesoryMethod)
            {
            using (var conn = getOpenedConnection())
                {
                using (var cmd = conn.CreateCommand())
                    {
                    cmd.CommandText = sqlCommand;
                    cmd.Parameters.AddWithValue("Predicate", predicateValue);

                    using (var reader = cmd.ExecuteReader())
                        {
                        if (reader.Read())
                            {
                            return createAccesoryMethod(reader);
                            }

                        return null;
                        }
                    }
                }
            }

        private Case createCase(SqlCeResultSet resultSet)
            {
            var _Case = new Case();

            _Case.Id = resultSet.GetInt32(CasesTable.Id);
            _Case.Lamp = resultSet.GetInt32(CasesTable.Lamp);
            _Case.Unit = resultSet.GetInt32(CasesTable.Unit);

            _Case.Model = resultSet.GetInt16(CasesTable.Model);
            _Case.Party = resultSet.GetInt32(CasesTable.Party);

            object warrantyExpiryDateObj = resultSet.GetValue(CasesTable.WarrantyExpiryDate);
            _Case.WarrantyExpiryDate = DBNull.Value.Equals(warrantyExpiryDateObj)
                ? DateTime.MinValue
                : (DateTime)warrantyExpiryDateObj;

            _Case.Status = resultSet.GetByte(CasesTable.Status);
            _Case.RepairWarranty = resultSet.GetBoolean(CasesTable.RepairWarranty);

            _Case.Map = resultSet.GetInt32(CasesTable.Map);
            _Case.Register = resultSet.GetInt16(CasesTable.Register);
            _Case.Position = resultSet.GetByte(CasesTable.Position);

            return _Case;
            }

        private Unit createUnit(SqlCeResultSet resultSet)
            {
            var unit = new Unit();

            unit.Id = resultSet.GetInt32(UnitsTable.Id);
            unit.Model = resultSet.GetInt16(UnitsTable.Model);
            unit.Party = resultSet.GetInt32(UnitsTable.Party);

            object warrantyExpiryDateObj = resultSet.GetValue(UnitsTable.WarrantyExpiryDate);
            unit.WarrantyExpiryDate = DBNull.Value.Equals(warrantyExpiryDateObj)
                ? DateTime.MinValue
                : (DateTime)warrantyExpiryDateObj;

            unit.Status = resultSet.GetByte(UnitsTable.Status);
            unit.RepairWarranty = resultSet.GetBoolean(UnitsTable.RepairWarranty);
            unit.Barcode = resultSet.GetInt32(UnitsTable.Barcode);

            return unit;
            }

        private Lamp createLamp(SqlCeResultSet resultSet)
            {
            var lamp = new Lamp();

            lamp.Id = resultSet.GetInt32(LampsTable.Id);
            lamp.Model = resultSet.GetInt16(LampsTable.Model);
            lamp.Party = resultSet.GetInt32(LampsTable.Party);

            object warrantyExpiryDateObj = resultSet.GetValue(LampsTable.WarrantyExpiryDate);
            lamp.WarrantyExpiryDate = DBNull.Value.Equals(warrantyExpiryDateObj)
                ? DateTime.MinValue
                : (DateTime)warrantyExpiryDateObj;

            lamp.Status = resultSet.GetByte(LampsTable.Status);
            lamp.Barcode = resultSet.GetInt32(LampsTable.Barcode);

            return lamp;
            }

        private void fillSqlCmdParametersFromAccessory(SqlCeParameterCollection parameters, IAccessory accessory)
            {
            parameters.AddWithValue("Id", accessory.Id);
            parameters.AddWithValue("Model", accessory.Model);
            parameters.AddWithValue("Party", accessory.Party);
            parameters.AddWithValue("Status", accessory.Status);

            object warrantyExpiryDate = DBNull.Value;
            if (accessory.WarrantyExpiryDate != DateTime.MinValue)
                {
                warrantyExpiryDate = accessory.WarrantyExpiryDate;
                }
            parameters.AddWithValue("WarrantyExpiryDate", warrantyExpiryDate);
            }

        private void fillSqlCmdParametersFromFixableAccessory(SqlCeParameterCollection parameters,
            IFixableAccessory accessory)
            {
            parameters.AddWithValue("RepairWarranty", accessory.RepairWarranty);
            }

        private void fillSqlCmdParametersFromBarcodeAccessory(SqlCeParameterCollection parameters,
            IBarcodeAccessory accessory)
            {
            parameters.AddWithValue("Barcode", accessory.Barcode);
            }





        private int getNextId(string tableName)
            {
            string sqlCommand =
                string.Format("select max(Id) from {0} where Id>=@minLampUnitId and Id<=@maxLampUnitId", tableName);
            using (var conn = getOpenedConnection())
                {
                using (var cmd = conn.CreateCommand())
                    {
                    cmd.CommandText = sqlCommand;
                    cmd.Parameters.AddWithValue("minLampUnitId", minAccessoryId);
                    cmd.Parameters.AddWithValue("maxLampUnitId", maxAccessoryId);

                    object lastId = cmd.ExecuteScalar();

                    if (lastId == null || System.DBNull.Value.Equals(lastId))
                        {
                        return minAccessoryId;
                        }

                    return Convert.ToInt32(lastId) + 1;
                    }
                }
            }

        #endregion


        public List<List<int>> GetUpdateTasks(TypeOfAccessories accessoryType, int recordsQuantityInTask)
            {
            var result = new List<List<int>>();

            string sql = string.Empty;
            int minId = 0;
            int maxId = 0;

            switch (accessoryType)
                {
                case TypeOfAccessories.Case:
                    sql = @"select Id from CasesUpdating";
                    break;

                case TypeOfAccessories.Lamp:
                    minId = Math.Max(lastUpdatedLampId + 1, minAccessoryId);
                    maxId = maxAccessoryId;
                    sql = @"
select Id from LampsUpdating
union all
select Id from Lamps where Id between @minId and @maxId";
                    break;

                case TypeOfAccessories.ElectronicUnit:
                    minId = Math.Max(lastUpdatedUnitId + 1, minAccessoryId);
                    maxId = maxAccessoryId;
                    sql = @"
select Id from UnitsUpdating
union all
select Id from Units where Id between @minId and @maxId";
                    break;
                }

            using (var conn = getOpenedConnection())
                {
                using (var cmd = conn.CreateCommand())
                    {
                    cmd.CommandText = sql;

                    if (minId != maxId)
                        {
                        cmd.Parameters.AddWithValue("maxId", maxId);
                        cmd.Parameters.AddWithValue("minId", minId);
                        }

                    using (var reader = cmd.ExecuteReader())
                        {
                        int countInTask = 0;
                        List<int> currentTask = null;
                        while (reader.Read())
                            {
                            if (countInTask % recordsQuantityInTask == 0)
                                {
                                if (currentTask != null)
                                    {
                                    result.Add(currentTask);
                                    }
                                currentTask = new List<int>();
                                countInTask = 0;
                                }

                            int id = (int)(reader[0]);

                            currentTask.Add(id);
                            countInTask++;
                            }

                        if (currentTask != null)
                            {
                            result.Add(currentTask);
                            }
                        }
                    }
                }

            return result;
            }

        public bool ResetUpdateLog(TypeOfAccessories accessoriesType)
            {
            string tableName;
            string getLastIdSql = string.Empty;
            DatabaseParametersConsts parameterId = DatabaseParametersConsts.EMPTY_ID;

            switch (accessoriesType)
                {
                case TypeOfAccessories.Lamp:
                    tableName = "LampsUpdating";
                    parameterId = DatabaseParametersConsts.LAST_UPLOADED_LAMP_ID;
                    getLastIdSql = "select Max(Id) from Lamps where Id between @minId and @maxId";
                    break;

                case TypeOfAccessories.ElectronicUnit:
                    tableName = "UnitsUpdating";
                    parameterId = DatabaseParametersConsts.LAST_UPLOADED_UNIT_ID;
                    getLastIdSql = "select Max(Id) from Units where Id between @minId and @maxId";
                    break;

                default:
                    tableName = "CasesUpdating";
                    break;
                }

            string sql = string.Format("delete from {0};", tableName);

            using (var conn = getOpenedConnection())
                {
                using (var cmd = conn.CreateCommand())
                    {
                    cmd.CommandText = sql;
                    try
                        {
                        cmd.ExecuteNonQuery();

                        if (!string.IsNullOrEmpty(getLastIdSql))
                            {
                            cmd.CommandText = getLastIdSql;
                            cmd.AddParameter("minId", minAccessoryId);
                            cmd.AddParameter("maxId", maxAccessoryId);
                            object lastIdObj = cmd.ExecuteScalar();
                            if (DBNull.Value.Equals(lastIdObj) || lastIdObj == null)
                                {
                                lastIdObj = 0;
                                }
                            int lastId = Convert.ToInt32(lastIdObj);
                            updateDatabaseParameter(parameterId, lastId);

                            switch (accessoriesType)
                                {
                                case TypeOfAccessories.Lamp:
                                    lastUpdatedLampId = lastId;
                                    break;

                                case TypeOfAccessories.ElectronicUnit:
                                    lastUpdatedUnitId = lastId;
                                    break;
                                }
                            }
                        }
                    catch (Exception exp)
                        {
                        return false;
                        }
                    }
                }

            return true;
            }

        public long GetLastDownloadedId(TypeOfAccessories accessoryType)
            {
            var parameterId = getDownloadedParameterId(accessoryType);

            return readDatabaseParameter(parameterId);
            }

        public long GetLastDownloadedId(Type catalogType)
            {
            var parameterId = getDownloadedParameterId(catalogType);

            return readDatabaseParameter(parameterId);
            }

        private DatabaseParametersConsts getDownloadedParameterId(Type catalogType)
            {
            if (catalogType == typeof(Map))
                {
                return DatabaseParametersConsts.LAST_DOWNLOADED_MAP_ID;
                }
            else if (catalogType == typeof(Model))
                {
                return DatabaseParametersConsts.LAST_DOWNLOADED_MODEL_ID;
                }
            else if (catalogType == typeof(PartyModel))
                {
                return DatabaseParametersConsts.LAST_DOWNLOADED_PARTY_ID;
                }
            else
                {
                return DatabaseParametersConsts.EMPTY_ID;
                }
            }

        private DatabaseParametersConsts getDownloadedParameterId(TypeOfAccessories accessoryType)
            {
            switch (accessoryType)
                {
                case TypeOfAccessories.Case:
                    return DatabaseParametersConsts.LAST_DOWNLOADED_CASE_ID;

                case TypeOfAccessories.Lamp:
                    return DatabaseParametersConsts.LAST_DOWNLOADED_LAMP_ID;

                case TypeOfAccessories.ElectronicUnit:
                    return DatabaseParametersConsts.LAST_DOWNLOADED_UNIT_ID;
                }
            return DatabaseParametersConsts.EMPTY_ID;
            }

        public void SetLastDownloadedId(TypeOfAccessories accessoryType, long lastDownloadedId)
            {
            var parameterId = getDownloadedParameterId(accessoryType);

            updateDatabaseParameter(parameterId, lastDownloadedId);
            }

        public void SetLastDownloadedId(Type catalogType, long lastDownloadedId)
            {
            var parameterId = getDownloadedParameterId(catalogType);

            updateDatabaseParameter(parameterId, lastDownloadedId);
            }

        public List<int> GetCasesIds()
            {
            var result = new List<int>();

            const string sql = @"select Id from cases where Map>0 and (Lamp = 0 or Unit = 0)";
            using (var conn = getOpenedConnection())
                {
                using (var cmd = conn.CreateCommand())
                    {
                    cmd.CommandText = sql;
                    using (var reader = cmd.ExecuteReader())
                        {
                        while (reader.Read())
                            {
                            result.Add(Convert.ToInt32(reader[0]));
                            }
                        }
                    }
                }

            return result;
            }

        public void ResetModels()
            {
            modelsCache = null;
            }

        public void ResetMaps()
            {
            mapsCache = null;
            }

        public void ResetParties()
            {
            partiesCache = null;
            }

        #region IRepository Members




        #endregion

        public bool UpdateBrokenLightsRecord(BrokenLightsRecord brokenLightsRecord)
            {
            var updater = new BrokenLightsUpdater(brokenLightsRecord, getOpenedConnection);
            var result = updater.Update();
            return result;
            }

        public List<List<BrokenLightsRecord>> GetBrokenLightsData()
            {
            var result = new List<List<BrokenLightsRecord>>();

            using (var conn = getOpenedConnection())
                {
                using (var cmd = conn.CreateCommand())
                    {
                    cmd.CommandText = @"select Register, Amount, Map from BrokenLights order by Map, Register";
                    using (var reader = cmd.ExecuteReader())
                        {
                        var currentMap = 0;
                        List<BrokenLightsRecord> currentList = null;

                        while (reader.Read())
                            {
                            var brokenLightData = new BrokenLightsRecord();
                            brokenLightData.Map = Convert.ToInt32(reader[2]);
                            brokenLightData.RegisterNumber = Convert.ToInt16(reader[0]);
                            brokenLightData.Amount = Convert.ToByte(reader[1]);

                            if (currentMap != brokenLightData.Map)
                                {
                                currentList = new List<BrokenLightsRecord>();
                                result.Add(currentList);
                                currentMap = brokenLightData.Map;
                                }

                            currentList.Add(brokenLightData);
                            }
                        }
                    }
                }

            return result;
            }

        public int DeleteBrokenLightsForMap(int mapId)
            {
            using (var conn = getOpenedConnection())
                {
                using (var cmd = conn.CreateCommand())
                    {
                    const string sql = @"delete from BrokenLights where Map = @Map";
                    cmd.CommandText = sql;
                    cmd.AddParameter("Map", mapId);
                    try
                        {
                        return cmd.ExecuteNonQuery();
                        }
                    catch (Exception exp)
                        {
                        Trace.WriteLine(string.Format(
                            "Ошибка выполнения тестового запроса к основным таблицам: {0}",
                            exp.Message));
                        return -1;
                        }
                    }
                }
            }
        }

    enum DatabaseParametersConsts
        {
        LAST_UPLOADED_LAMP_ID = 0,
        LAST_UPLOADED_UNIT_ID = 1,

        LAST_DOWNLOADED_LAMP_ID = 2,
        LAST_DOWNLOADED_UNIT_ID = 3,
        LAST_DOWNLOADED_CASE_ID = 4,

        LAST_DOWNLOADED_MODEL_ID = 11,
        LAST_DOWNLOADED_PARTY_ID = 12,
        LAST_DOWNLOADED_MAP_ID = 13,

        EMPTY_ID = -1
        }

    }
