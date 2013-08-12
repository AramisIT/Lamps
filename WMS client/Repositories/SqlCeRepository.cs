using System;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using WMS_client.db;
using WMS_client.Models;

namespace WMS_client.Repositories
    {
    public class SqlCeRepository : IRepository
        {

        public SqlCeRepository()
            {
            initConnectionString();
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

            const string sql =
               "select Id, Lamp, Unit, Model, Party, WarrantyExpiryDate, Status, RepairWarranty, Map, Register, Position from Cases where Id = @Predicate";

            return (Case)readAccessory(sql, id, createCase);
            }

        public Unit ReadUnit(int id)
            {
            if (id <= 0)
                {
                return null;
                }

            const string sql =
               "select Id, Model, Party, WarrantyExpiryDate, Status, Barcode, RepairWarranty from Units where Id = @Predicate";

            return (Unit)readAccessory(sql, id, createUnit);
            }

        public Unit ReadUnitByBarcode(int barcode)
            {
            if (barcode <= 0)
                {
                return null;
                }

            const string sql =
                "select Id, Model, Party, WarrantyExpiryDate, Status, Barcode, RepairWarranty from Units where Barcode = @Predicate";

            return (Unit)readAccessory(sql, barcode, createUnit);
            }

        public Lamp ReadLamp(int id)
            {
            if (id <= 0)
                {
                return null;
                }

            const string sql =
                 "select Id, Model, Party, WarrantyExpiryDate, Status, Barcode from Lamps where Id = @Predicate";

            return (Lamp)readAccessory(sql, id, createLamp);
            }

        public Lamp ReadLampByBarcode(int barcode)
            {
            if (barcode <= 0)
                {
                return null;
                }

            const string sql =
                "select Id, Model, Party, WarrantyExpiryDate, Status, Barcode from Lamps where Barcode = @Predicate";

            return (Lamp)readAccessory(sql, barcode, createLamp);
            }

        #region private

        private string connectionString;

        private const string DATABASE_FILE_NAME = "LampsBase.sdf";

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

        private Case createCase(SqlCeDataReader reader)
            {
            var _case = new Case();

            fillAccessoryFromDataReader(_case, reader);
            fillFixableAccessoryFromDataReader(_case, reader);

            _case.Lamp = (int)reader["Lamp"];
            _case.Unit = (int)reader["Unit"];
            _case.Map = (int)reader["Map"];
            _case.Register = (Int16)reader["Register"];
            _case.Position = (byte)reader["Position"];

            return _case;
            }

        private Unit createUnit(SqlCeDataReader reader)
            {
            var unit = new Unit();

            fillAccessoryFromDataReader(unit, reader);
            fillBarcodeAccessoryFromDataReader(unit, reader);
            fillFixableAccessoryFromDataReader(unit, reader);

            return unit;
            }

        private Lamp createLamp(SqlCeDataReader reader)
            {
            var lamp = new Lamp();

            fillAccessoryFromDataReader(lamp, reader);
            fillBarcodeAccessoryFromDataReader(lamp, reader);

            return lamp;
            }

        private void fillAccessoryFromDataReader(IAccessory accessory, SqlCeDataReader reader)
            {
            accessory.Id = (int)reader["Id"];
            accessory.Model = (Int16)reader["Model"];
            accessory.Party = (int)reader["Party"];

            object warrantyExpiryDateObj = reader["WarrantyExpiryDate"];
            accessory.WarrantyExpiryDate = DBNull.Value.Equals(warrantyExpiryDateObj)
                ? DateTime.MinValue
                : (DateTime)warrantyExpiryDateObj;

            accessory.Status = (byte)reader["Status"];
            }

        private void fillBarcodeAccessoryFromDataReader(IBarcodeAccessory accessory, SqlCeDataReader reader)
            {
            accessory.Barcode = (int)reader["Barcode"];
            }

        private void fillFixableAccessoryFromDataReader(IFixableAccessory accessory, SqlCeDataReader reader)
            {
            accessory.RepairWarranty = (bool)reader["RepairWarranty"];
            }

        #endregion






        }
    }
