using System;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using WMS_client.db;
using WMS_client.Enums;
using WMS_client.Models;
using System.Data;
using WMS_client.Repositories.Sql;
using WMS_client.Repositories.Sql.Updaters;

namespace WMS_client.Repositories
    {
    public class SqlCeRepository : IRepository
        {
        private const int LAST_UPDATED_LAMP_ID = 0;
        private const int LAST_UPDATED_UNIT_ID = 1;

        public SqlCeRepository()
            {
            initConnectionString();

            minAccessoryId = Configuration.Current.TerminalId * 10 * 1000 * 1000;
            maxAccessoryId = (Configuration.Current.TerminalId + 1) * 10 * 1000 * 1000 - 1;

            nextUnitId = getNextId("Units");
            nextLampId = getNextId("Lamps");

            lastUpdatedLampId = readDatabaseParameter(LAST_UPDATED_LAMP_ID);
            lastUpdatedUnitId = readDatabaseParameter(LAST_UPDATED_UNIT_ID);
            }

        private int readDatabaseParameter(int parameterId)
            {
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
                            return result.GetInt32(DatabaseParameters.Value);
                            }
                        else
                            {
                            var newRow = result.CreateRecord();
                            newRow.SetInt32(DatabaseParameters.Value, 0);
                            newRow.SetInt32(DatabaseParameters.Id, parameterId);
                            result.Insert(newRow);
                            return 0;
                            }
                        }
                    }
                }
            }

        public bool LoadingDataFromGreenhouse { get; set; }

        public bool WriteModel(Model model)
            {
            using (var conn = getOpenedConnection())
                {
                using (var cmd = conn.CreateCommand())
                    {
                    cmd.CommandText = @"update models set Description=@Description where Id=@Id";
                    cmd.Parameters.AddWithValue("Description", model.Description);
                    cmd.Parameters.AddWithValue("Id", model.Id);

                    bool rowExists = cmd.ExecuteNonQuery() > 0;

                    if (!rowExists)
                        {
                        cmd.CommandText = @"insert into models(Id,Description) Values(@Id,@Description);";
                        try
                            {
                            return cmd.ExecuteNonQuery() > 0;
                            }
                        catch (Exception exception)
                            {
                            MessageBox.Show(string.Format("Ошибка вставки модели: {0}", exception.Message));
                            return false;
                            }
                        }

                    return true;
                    }
                }
            }

        public bool WriteParty(PartyModel party)
            {
            using (var conn = getOpenedConnection())
                {
                using (var cmd = conn.CreateCommand())
                    {
                    cmd.CommandText = @"update Parties set Date = @Date, DateOfActSet = @DateOfActSet, ContractorDescription=@ContractorDescription, Description=@Description, WarrantyHours=@WarrantyHours, WarrantyYears=@WarrantyYears where Id=@Id";
                    cmd.Parameters.AddWithValue("Description", party.Description);
                    cmd.Parameters.AddWithValue("Id", party.Id);
                    cmd.Parameters.AddWithValue("WarrantyHours", party.WarrantyHours);
                    cmd.Parameters.AddWithValue("WarrantyYears", party.WarrantyYears);
                    cmd.Parameters.AddWithValue("ContractorDescription", party.ContractorDescription);
                    cmd.Parameters.AddWithValue("DateOfActSet", party.DateOfActSet);
                    cmd.Parameters.AddWithValue("Date", party.Date);

                    bool rowExists = cmd.ExecuteNonQuery() > 0;

                    if (!rowExists)
                        {
                        cmd.CommandText = @"insert into Parties(Id, Description, ContractorDescription, [Date], DateOfActSet, WarrantyHours, WarrantyYears) Values(@Id,@Description,@ContractorDescription,@Date,@DateOfActSet,@WarrantyHours,@WarrantyYears);";
                        try
                            {
                            return cmd.ExecuteNonQuery() > 0;
                            }
                        catch (Exception exception)
                            {
                            MessageBox.Show(string.Format("Ошибка вставки партии: {0}", exception.Message));
                            return false;
                            }
                        }

                    return true;
                    }
                }
            }

        public bool WriteMap(Map map)
            {
            using (var conn = getOpenedConnection())
                {
                using (var cmd = conn.CreateCommand())
                    {
                    cmd.CommandText = @"update maps set Description=@Description where Id=@Id";
                    cmd.Parameters.AddWithValue("Description", map.Description);
                    cmd.Parameters.AddWithValue("Id", map.Id);

                    bool rowExists = cmd.ExecuteNonQuery() > 0;

                    if (!rowExists)
                        {
                        cmd.CommandText = @"insert into maps(Id,Description) Values(@Id,@Description);";
                        try
                            {
                            return cmd.ExecuteNonQuery() > 0;
                            }
                        catch (Exception exception)
                            {
                            MessageBox.Show(string.Format("Ошибка вставки карты: {0}", exception.Message));
                            return false;
                            }
                        }

                    return true;
                    }
                }
            }

        public bool UpdateCases(List<Case> cases, bool justInsert)
            {
            var updater = new CasesUpdater() { JustInsert = justInsert, LoadingDataFromGreenhouse = this.LoadingDataFromGreenhouse };
            updater.InitUpdater(cases, getOpenedConnection);

            return updater.Update();
            }

        public bool UpdateLamps(List<Lamp> lamps, bool justInsert)
            {
            var updater = new LampsUpdater() { JustInsert = justInsert, LoadingDataFromGreenhouse = this.LoadingDataFromGreenhouse };
            updater.Don_tAddNewToLog = true;
            updater.LastUploadedToGreenhouseId = lastUpdatedLampId;
            updater.MinAccessoryIdForCurrentPdt = minAccessoryId;
            updater.MaxAccessoryIdForCurrentPdt = maxAccessoryId;

            updater.InitUpdater(lamps, getOpenedConnection);
            return updater.Update();
            }

        public bool UpdateUnits(List<Unit> units, bool justInsert)
            {
            var updater = new UnitsUpdater() { JustInsert = justInsert, LoadingDataFromGreenhouse = this.LoadingDataFromGreenhouse };
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

        public List<Model> ModelsList
            {
            get
                {
                var cache = modelsCache ?? (modelsCache = buildModelsCache());
                return cache.CatalogList;
                }
            }

        public List<Map> MapsList
            {
            get
                {
                var cache = mapsCache ?? (mapsCache = buildMapsCache());
                return cache.CatalogList;
                }
            }

        public List<PartyModel> PartiesList
            {
            get
                {
                var cache = partiesCache ?? (partiesCache = buildPartiesCache());
                return cache.CatalogList;
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

        private List<T> getAccessories<T>(string tableName, string indexName, List<int> predicateValues, Func<SqlCeResultSet, T> createAccesoryMethod)
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
            connectionString = String.Format("Data Source='{0}';", Configuration.Current.PathToApplication + '\\' + DATABASE_FILE_NAME);
            }

        private SqlCeConnection getOpenedConnection()
            {
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

        public const ResultSetOptions UPDATABLE_RESULT_SET_OPTIONS = ResultSetOptions.Scrollable | ResultSetOptions.Sensitive | ResultSetOptions.Updatable;
        public const ResultSetOptions READ_ONLY_RESULT_SET_OPTIONS = ResultSetOptions.Scrollable | ResultSetOptions.Sensitive;

        private CatalogCache<int, PartyModel> partiesCache;
        private CatalogCache<int, Map> mapsCache;
        private CatalogCache<Int16, Model> modelsCache;
        private int lastUpdatedUnitId;
        private int lastUpdatedLampId;

        private CatalogCache<int, PartyModel> buildPartiesCache()
            {
            var result = new CatalogCache<int, PartyModel>();

            const string sql =
                "select Id, Description, ContractorDescription, DateOfActSet, Date, WarrantyHours, WarrantyYears from Parties order by Date desc";

            result.Load(sql, getOpenedConnection, (reader, catalog) =>
                {
                    catalog.Id = Convert.ToInt32(reader["Id"]);
                    catalog.WarrantyHours = Convert.ToInt16(reader["WarrantyHours"]);
                    catalog.WarrantyYears = Convert.ToInt16(reader["WarrantyYears"]);
                    catalog.Description = (reader["Description"] as string).TrimEnd();
                    catalog.ContractorDescription = (reader["ContractorDescription"] as string).TrimEnd();
                    catalog.Date = (DateTime)(reader["Date"]);
                    catalog.DateOfActSet = (DateTime)(reader["DateOfActSet"]);
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

        private IAccessory readAccessory(string sqlCommand, int predicateValue, Func<SqlCeDataReader, IAccessory> createAccesoryMethod)
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

        private void fillSqlCmdParametersFromFixableAccessory(SqlCeParameterCollection parameters, IFixableAccessory accessory)
            {
            parameters.AddWithValue("RepairWarranty", accessory.RepairWarranty);
            }

        private void fillSqlCmdParametersFromBarcodeAccessory(SqlCeParameterCollection parameters, IBarcodeAccessory accessory)
            {
            parameters.AddWithValue("Barcode", accessory.Barcode);
            }





        private int getNextId(string tableName)
            {
            string sqlCommand = string.Format("select max(Id) from {0} where Id>=@minLampUnitId and Id<=@maxLampUnitId", tableName);
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

       


        }
    }
