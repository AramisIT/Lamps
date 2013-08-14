using System;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using WMS_client.db;
using WMS_client.Models;
using System.Data;

namespace WMS_client.Repositories
    {
    public class SqlCeRepository : IRepository
        {
        public SqlCeRepository()
            {
            initConnectionString();

            minLampUnitId = Configuration.Current.TerminalId * 10 * 1000 * 1000;
            maxLampUnitId = (Configuration.Current.TerminalId + 1) * 10 * 1000 * 1000 - 1;

            nextUnitId = getNextId("Units");
            nextLampId = getNextId("Lamps");
            }

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

        public bool SaveGroupOfSets(Case _Case, Lamp lamp, Unit unit, List<int> barcodes)
            {
            List<Unit> units = new List<Unit>();
            List<Lamp> lamps = new List<Lamp>();
            List<Case> cases = new List<Case>();

            foreach (int barcode in barcodes)
                {
                var newLamp = lamp.Copy<Lamp>();
                newLamp.Id = GetNextLampId();
                lamps.Add(newLamp);

                var newUnit = unit.Copy<Unit>();
                newUnit.Id = GetNextUnitId();
                units.Add(newUnit);

                var newCase = _Case.Copy<Case>();
                newCase.Id = barcode;
                newCase.Lamp = newLamp.Id;
                newCase.Unit = newUnit.Id;
                cases.Add(newCase);
                }

            return InsertUnits(units) && InsertLamps(lamps) && InsertCases(cases);
            }

        public bool SaveAccessoriesSet(Case _Case, Lamp lamp, Unit unit)
            {
            bool ok = true;

            if (unit != null)
                {
                if (unit.Id <= 0)
                    {
                    unit.Id = GetNextUnitId();
                    ok = ok && InsertUnits(new List<Unit>() { unit });
                    }
                else
                    {
                    ok = ok && updateUnit(unit);
                    }
                }
            if (!ok)
                {
                return false;
                }


            if (lamp != null)
                {
                if (lamp.Id <= 0)
                    {
                    lamp.Id = GetNextLampId();
                    ok = ok && InsertLamps(new List<Lamp>() { lamp });
                    }
                else
                    {
                    ok = ok && updateLamp(lamp);
                    }
                }
            if (!ok)
                {
                return false;
                }


            if (_Case != null)
                {
                _Case.Lamp = lamp == null ? 0 : lamp.Id;
                _Case.Unit = unit == null ? 0 : unit.Id;

                if (!UpdateCase(_Case))
                    {
                    ok = ok && InsertCases(new List<Case>() { _Case });
                    }
                }

            return ok;
            }

        public bool UpdateCase(Case _Case)
            {
            using (var conn = getOpenedConnection())
                {
                using (var cmd = conn.CreateCommand())
                    {
                    cmd.CommandType = CommandType.TableDirect;
                    cmd.CommandText = "Cases";
                    cmd.Connection = conn;
                    cmd.IndexName = "PK_Cases";

                    using (var result = cmd.ExecuteResultSet(UPDATABLE_RESULT_SET_OPTIONS))
                        {
                        if (result.Seek(DbSeekOptions.FirstEqual, _Case.Id))
                            {
                            result.Read();

                            result.SetInt16(CasesTable.Model, _Case.Model);
                            result.SetInt32(CasesTable.Party, _Case.Party);
                            result.SetByte(CasesTable.Status, _Case.Status);
                            result.SetBoolean(CasesTable.RepairWarranty, _Case.RepairWarranty);

                            result.SetValue(CasesTable.WarrantyExpiryDate, getSqlDateTime(_Case.WarrantyExpiryDate));

                            result.SetInt32(CasesTable.Lamp, _Case.Lamp);
                            result.SetInt32(CasesTable.Unit, _Case.Unit);

                            result.SetInt32(CasesTable.Map, _Case.Map);
                            result.SetInt16(CasesTable.Register, _Case.Register);
                            result.SetByte(CasesTable.Position, _Case.Position);

                            result.Update();

                            return true;
                            }
                        else
                            {
                            return false;
                            }
                        }
                    }
                }
            }

        private object getSqlDateTime(DateTime dateTime)
            {
            object result = DBNull.Value;
            if (!DateTime.MinValue.Equals(dateTime))
                {
                result = dateTime;
                }
            return result;
            }

        private bool updateLamp(Lamp lamp)
            {
            const string sql = @"update Lamps 
set Model = @Model, Party = @Party, WarrantyExpiryDate = @WarrantyExpiryDate, Status = @Status,
Barcode = @Barcode
where Id = @Id";

            return updateAccessory(false, lamp.Id, "LampsUpdating", sql, (parameters) =>
                {
                    fillSqlCmdParametersFromAccessory(parameters, lamp);
                    fillSqlCmdParametersFromBarcodeAccessory(parameters, lamp);
                });
            }

        public bool InsertCases(List<Case> list)
            {
            var accessoryInserter = new AccessoryInserter<Case>("Cases", list, getOpenedConnection);

            bool saved = accessoryInserter.InsertAccessories((newRow, accessory) =>
            {
                Case _Case = (Case)accessory;
                newRow["Lamp"] = _Case.Lamp;
                newRow["Unit"] = _Case.Unit;

                newRow["Map"] = _Case.Map;
                newRow["Register"] = _Case.Register;
                newRow["Position"] = _Case.Position;
            });

            if (!saved)
                {
                return false;
                }

            var logger = new AccessoryLogger<Case>("CasesUpdating", list, getOpenedConnection);
            return logger.Log();
            }

        public bool InsertLamps(List<Lamp> list)
            {
            var accessoryInserter = new AccessoryInserter<Lamp>("Lamps", list, getOpenedConnection);
            return accessoryInserter.InsertAccessories(null);
            }

        public bool InsertUnits(List<Unit> list)
            {
            var accessoryInserter = new AccessoryInserter<Unit>("Units", list, getOpenedConnection);
            return accessoryInserter.InsertAccessories(null);
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

        private bool updateUnit(Unit unit)
            {
            const string sql = @"update Units 
set Model = @Model, Party = @Party, WarrantyExpiryDate = @WarrantyExpiryDate, Status = @Status,
RepairWarranty = @RepairWarranty, Barcode = @Barcode
where Id = @Id";

            return updateAccessory(false, unit.Id, "UnitsUpdating", sql, (parameters) =>
                {
                    fillSqlCmdParametersFromAccessory(parameters, unit);
                    fillSqlCmdParametersFromFixableAccessory(parameters, unit);
                    fillSqlCmdParametersFromBarcodeAccessory(parameters, unit);
                });
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
            IAccessory accessory = ReadCase(accessoryBarcode);
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

        public Case ReadCase(int id)
            {
            if (id <= 0)
                {
                return null;
                }
            return (Case)getAccessory("Cases", "PK_Cases", id, createCase);
            }

        public Case FintCaseByLamp(int lampId)
            {
            if (lampId <= 0)
                {
                return null;
                }
            return (Case)getAccessory("Cases", "Cases_Lamp", lampId, createCase);
            }

        private IAccessory getAccessory(string tableName, string indexName, int predicateValue, Func<SqlCeResultSet, IAccessory> createAccesoryMethod)
            {
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
                        if (result.Seek(DbSeekOptions.FirstEqual, predicateValue))
                            {
                            result.Read();

                            return createAccesoryMethod(result);
                            }
                        else
                            {
                            return null;
                            }
                        }
                    }
                }
            }

        public Case FintCaseByUnit(int unitId)
            {
            if (unitId <= 0)
                {
                return null;
                }

            return (Case)getAccessory("Cases", "Cases_Unit", unitId, createCase);
            }

        public Unit ReadUnit(int id)
            {
            if (id <= 0)
                {
                return null;
                }

            return (Unit)getAccessory("Units", "PK_Units", id, createUnit);
            }

        public Unit ReadUnitByBarcode(int barcode)
            {
            if (barcode <= 0)
                {
                return null;
                }

            return (Unit)getAccessory("Units", "Units_Barcode", barcode, createUnit);
            }

        public Lamp ReadLamp(int id)
            {
            if (id <= 0)
                {
                return null;
                }

            return (Lamp)getAccessory("Lamps", "PK_Lamps", id, createLamp);
            }

        public Lamp ReadLampByBarcode(int barcode)
            {
            if (barcode <= 0)
                {
                return null;
                }

            return (Lamp)getAccessory("Lamps", "Lamps_Barcode", barcode, createLamp);
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

        private readonly int minLampUnitId;
        private readonly int maxLampUnitId;

        public const ResultSetOptions UPDATABLE_RESULT_SET_OPTIONS = ResultSetOptions.Scrollable | ResultSetOptions.Sensitive | ResultSetOptions.Updatable;
        public const ResultSetOptions READ_ONLY_RESULT_SET_OPTIONS = ResultSetOptions.Scrollable | ResultSetOptions.Sensitive;

        private CatalogCache<int, PartyModel> partiesCache;
        private CatalogCache<int, Map> mapsCache;
        private CatalogCache<Int16, Model> modelsCache;

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

        private bool updateAccessory(bool isCase, int accessoryId, string logTableName, string sqlCommand, Action<SqlCeParameterCollection> setParametersMethod)
            {
            using (var conn = getOpenedConnection())
                {
                using (var cmd = conn.CreateCommand())
                    {
                    cmd.CommandText = sqlCommand;

                    setParametersMethod(cmd.Parameters);
                    try
                        {
                        bool updated = cmd.ExecuteNonQuery() > 0;
                        if (updated)
                            {
                            cmd.CommandText = string.Format("select Id from {0} where Id = @Id", logTableName);
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("Id", accessoryId);
                            object result = cmd.ExecuteScalar();
                            bool recordExists = result != null && !DBNull.Value.Equals(result);

                            if (!recordExists)
                                {
                                if (isCase)
                                    {
                                    cmd.CommandText = string.Format("insert into {0}(Id, New) values(@Id, 0)", logTableName);
                                    }
                                else
                                    {
                                    cmd.CommandText = string.Format("insert into {0}(Id) values(@Id)", logTableName);
                                    }
                                cmd.ExecuteNonQuery();
                                }
                            }
                        return updated;
                        }
                    catch (Exception exp)
                        {
                        Debug.WriteLine(string.Format("Ошибка обновления комплектующей - {0}", exp.Message));
                        return false;
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
                    cmd.Parameters.AddWithValue("minLampUnitId", minLampUnitId);
                    cmd.Parameters.AddWithValue("maxLampUnitId", maxLampUnitId);

                    object lastId = cmd.ExecuteScalar();

                    if (lastId == null || System.DBNull.Value.Equals(lastId))
                        {
                        return minLampUnitId;
                        }

                    return Convert.ToInt32(lastId) + 1;
                    }
                }
            }

        #endregion





        }
    }
