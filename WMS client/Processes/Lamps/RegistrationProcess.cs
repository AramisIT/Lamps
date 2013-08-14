using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using WMS_client.db;
using WMS_client.Enums;
using WMS_client.Models;
using WMS_client.Repositories;
using Party = WMS_client.db.Party;

namespace WMS_client
    {
    /// <summary>Регистрация при входе</summary>
    public class RegistrationProcess : BusinessProcess
        {
        private MobileButton wifiOffButton;
        private MobileButton enterButton;

        #region Public methods
        /// <summary>Регистрация при входе</summary>
        public RegistrationProcess(WMSClient MainProcess)
            : base(MainProcess, 1)
            {
            BusinessProcessType = ProcessType.Registration;

            //bool systemReady = CheckNewDataBase();

            //if (!systemReady)
            //    {
            //    ShowMessage("База данных не актуальна. Нужно обновить приложение или базу данных.");
            //    enterButton.Enabled = false;
            //    }
            }

        private bool CheckNewDataBase()
            {
            bool ok = checkModels() && checkParties() && checkMaps();
            return ok;
            }

        private void checkAllLamps()
            {
//            #region sql command
//            const string sql = @"
//select
//
//Cast(SUBSTRING(c.barcode, 2, 24) as int) Id,
//c.Model CaseModel, c.Party CaseParty, 
//c.DateOfWarrantyEnd CaseWarrantyExpiryDate,
//c.Status CaseStatus, 
//c.TypeOfWarrantly CaseWarrantyType,
//cast(c.map as int) [Map],
//cast(c.Register as smallint) Register,
//cast(c.Position as TinyInt) Position,
//
//
//case when l.Model is null then 0 else l.Model end LampModel,
//case when l.Party is null then 0 else l.Party end LampParty,
//case when l.Status is null then 0 else l.Status end LampStatus,
//case when l.DateOfWarrantyEnd = cast('1753-01-01' as datetime) then null else l.DateOfWarrantyEnd end LampWarrantyExpiryDate,
//
//case when u.Model is null then 0 else u.Model end UnitModel,
//case when u.Party is null then 0 else u.Party end UnitParty,
//case when u.Status is null then 0 else u.Status end UnitStatus,
//case when u.TypeOfWarrantly is null then 0 else u.TypeOfWarrantly end UnitWarrantyType,
//
//case when u.DateOfWarrantyEnd = cast('1753-01-01' as datetime) then null else u.DateOfWarrantyEnd end UnitWarrantyExpiryDate,
//
//case when (u.barcode is null) or RTrim(u.barcode) = '' then 0 else Cast(SUBSTRING(u.barcode, 2, 24) as int) end UnitBarcode
//
//
//from cases c
//left join Lamps l on c.Lamp = l.Id
//left join ElectronicUnits u on c.ElectronicUnit = u.Id
//
//order by LampWarrantyExpiryDate
//";
//            #endregion

//            Dictionary<int, CaseInfo> existsCases = getExistsCases();

//            var cases = new List<Case>();
//            var lamps = new List<Lamp>();
//            var units = new List<Unit>();
//            int rowNumber = 1;

//            string fileRow;
//            var namedValues = new Dictionary<string, object>();

//            using (StreamReader sqlResultFile = File.OpenText(
//                Configuration.Current.PathToApplication + @"\result2.txt"))
//                {
//                while ((fileRow = sqlResultFile.ReadLine()) != null)
//                    {
//                    namedValues.Clear();
//                    string[] values = fileRow.Split('\t');

//                    int caseId = Convert.ToInt32(values[0]);

//                    CaseInfo caseInfo;
//                    existsCases.TryGetValue(caseId, out caseInfo);

//                    bool caseExists = caseInfo != null;
//                    if (caseExists && !caseInfo.HasNotLamp && !caseInfo.HasNotUnit && !caseInfo.HasNotPosition)
//                        {
//                        Trace.WriteLine("Skiped");
//                        continue;
//                        }

//                    namedValues.Add("Id", caseId);
//                    namedValues.Add("CaseModel", values[1]);
//                    namedValues.Add("CaseParty", values[2]);
//                    namedValues.Add("CaseWarrantyExpiryDate", values[3]);
//                    namedValues.Add("CaseStatus", values[4]);
//                    namedValues.Add("CaseWarrantyType", values[5]);
//                    namedValues.Add("Map", values[6]);
//                    namedValues.Add("Register", values[7]);
//                    namedValues.Add("Position", values[8]);
//                    namedValues.Add("LampModel", values[9]);
//                    namedValues.Add("LampParty", values[10]);
//                    namedValues.Add("LampStatus", values[11]);
//                    namedValues.Add("LampWarrantyExpiryDate", values[12]);
//                    namedValues.Add("UnitModel", values[13]);
//                    namedValues.Add("UnitParty", values[14]);
//                    namedValues.Add("UnitStatus", values[15]);
//                    namedValues.Add("UnitWarrantyType", values[16]);
//                    namedValues.Add("UnitWarrantyExpiryDate", values[17]);
//                    namedValues.Add("UnitBarcode", values[18]);


//                    //using (SqlCeCommand query = dbWorker.NewQuery(sql))
//                    //    {
//                    //    using (var reader = query.ExecuteReader())
//                    //        {
//                    //        while (reader.Read())

//                    var lamp = new Lamp();
//                    var unit = new Unit();
//                    var _Case = new Case();

//                    fillCase(_Case, namedValues);
//                    fillLamp(lamp, namedValues);
//                    fillUnit(unit, namedValues);

//                    if (caseExists)
//                        {
//                        bool updateCase = false;

//                        if (caseInfo.HasNotLamp)
//                            {
//                            if (lamp.Model <= 0)
//                                {
//                                lamp = null;
//                                }
//                            else
//                                {
//                                updateCase = true; // lamp will be written
//                                }
//                            }
//                        else
//                            {
//                            lamp.Id = caseInfo.Lamp;
//                            }

//                        if (caseInfo.HasNotUnit)
//                            {
//                            if (unit.Model <= 0)
//                                {
//                                unit = null;
//                                }
//                            else
//                                {
//                                updateCase = true; // unit will be written
//                                }
//                            }
//                        else
//                            {
//                            unit.Id = caseInfo.Unit;
//                            }

//                        if (caseInfo.HasNotPosition && _Case.Map > 0)
//                            {
//                            updateCase = true;
//                            }

//                        if (updateCase)
//                            {
//                            Trace.WriteLine("Updated");
//                            Configuration.Current.Repository.SaveAccessoriesSet(_Case, lamp, unit);
//                            }
//                        }
//                    else
//                        {
//                        if (lamp.Model > 0)
//                            {
//                            lamp.Id = Configuration.Current.Repository.GetNextLampId();
//                            _Case.Lamp = lamp.Id;
//                            lamps.Add(lamp);
//                            }

//                        if (unit.Model > 0)
//                            {
//                            unit.Id = Configuration.Current.Repository.GetNextUnitId();
//                            _Case.Unit = unit.Id;
//                            units.Add(unit);
//                            }

//                        cases.Add(_Case);
//                        Trace.WriteLine(rowNumber);
//                        rowNumber++;
//                        }

//                    }
//                }

//            bool ok = Configuration.Current.Repository.UpdateLamps(lamps, true);
//            ok = Configuration.Current.Repository.UpdateUnits(units, true) && ok;
//            ok = Configuration.Current.Repository.UpdateCases(cases, true) && ok;

//            Trace.WriteLine(string.Format("Total result: {0}", ok ? "OK" : "Failure"));

            return;
            }

        string connectionString = String.Format("Data Source='{0}';", Configuration.Current.PathToApplication + '\\' + SqlCeRepository.DATABASE_FILE_NAME);
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

        private Dictionary<int, CaseInfo> getExistsCases()
            {
            var result = new Dictionary<int, CaseInfo>();

            using (var conn = getOpenedConnection())
                {
                using (var cmd = conn.CreateCommand())
                    {
                    cmd.CommandText = @"select Id, Lamp, Unit, Map, Register, Position from cases";

                    using (var reader = cmd.ExecuteReader())
                        {
                        while (reader.Read())
                            {
                            var caseInfo = new CaseInfo();
                            result.Add((int)reader["Id"], caseInfo);

                            caseInfo.Lamp = (int)reader["Lamp"];
                            caseInfo.Unit = (int)reader["Unit"];
                            caseInfo.Map = (int)reader["Map"];

                            caseInfo.Register = (Int16)reader["Register"];
                            caseInfo.Position = (byte)reader["Position"];
                            }
                        }
                    }
                }
            return result;
            }

        private void fillUnit(Unit accessory, Dictionary<string, object> reader)
            {
            accessory.Model = Convert.ToInt16(reader["UnitModel"]);
            accessory.Party = Convert.ToInt32(reader["UnitParty"]);
            accessory.Status = Convert.ToByte(reader["UnitStatus"]);
            accessory.WarrantyExpiryDate = getDate(reader["UnitWarrantyExpiryDate"]);

            accessory.Barcode = Convert.ToInt32(reader["UnitBarcode"]);

            var warrantyType = (TypesOfLampsWarrantly)Convert.ToInt32(reader["UnitWarrantyType"]);
            accessory.RepairWarranty = warrantyType == TypesOfLampsWarrantly.Repair;
            }

        private DateTime getDate(object value)
            {
            bool emptyDate = "NULL".Equals(value) || value == null || DBNull.Value.Equals(value);
            return emptyDate ? DateTime.MinValue : Convert.ToDateTime(value);
            }

        private void fillLamp(Lamp accessory, Dictionary<string, object> reader)
            {
            accessory.Model = Convert.ToInt16(reader["LampModel"]);
            accessory.Party = Convert.ToInt32(reader["LampParty"]);
            accessory.Status = Convert.ToByte(reader["LampStatus"]);
            accessory.WarrantyExpiryDate = getDate(reader["LampWarrantyExpiryDate"]);
            }

        private void fillCase(Case accessory, Dictionary<string, object> reader)
            {
            accessory.Model = Convert.ToInt16(reader["CaseModel"]);
            accessory.Party = Convert.ToInt32(reader["CaseParty"]);
            accessory.Status = Convert.ToByte(reader["CaseStatus"]);
            accessory.WarrantyExpiryDate = getDate(reader["CaseWarrantyExpiryDate"]);


            accessory.Id = Convert.ToInt32(reader["Id"]);

            var warrantyType = (TypesOfLampsWarrantly)Convert.ToInt32(reader["CaseWarrantyType"]);
            accessory.RepairWarranty = warrantyType == TypesOfLampsWarrantly.Repair;

            accessory.Register = Convert.ToInt16(reader["Register"]);
            accessory.Map = Convert.ToInt32(reader["Map"]);
            accessory.Position = Convert.ToByte(reader["Position"]);
            }

        private bool checkMaps()
            {
            DataTable table = null;

            using (SqlCeCommand query =
                dbWorker.NewQuery(@"select Id, RTRIM(Description) Description from Maps"))
                {
                using (var reader = query.ExecuteReader())
                    {
                    var map = new Map();

                    while (reader.Read())
                        {
                        map.Description = reader["Description"] as string;
                        map.Id = Convert.ToInt16(reader["Id"]);

                        if (!Configuration.Current.Repository.WriteMap(map))
                            {
                            return false;
                            }
                        }
                    }
                }

            return true;
            }

        #region temporary methods

        private bool checkModels()
            {
            DataTable table = null;

            using (SqlCeCommand query =
                dbWorker.NewQuery(@"select Id, RTRIM(Description) Description from Models"))
                {
                using (var reader = query.ExecuteReader())
                    {
                    Model model = new Model();

                    while (reader.Read())
                        {
                        model.Description = reader["Description"] as string;
                        model.Id = Convert.ToInt16(reader["Id"]);

                        if (!Configuration.Current.Repository.WriteModel(model))
                            {
                            return false;
                            }
                        }
                    }
                }

            return true;
            }

        private bool checkParties()
            {
            DataTable table = null;

            using (SqlCeCommand query =
                dbWorker.NewQuery(@"select p.Id, RTRIM(p.Description) Description, 
case when Contractors.Description  is null then '' else RTRIM(Contractors.Description) end ContractorDescription,
DateOfActSet, DateParty [Date],
WarrantlyHours WarrantyHours, WarrantlyYears WarrantyYears from Party p

left join Contractors on Contractors.Id = p.Contractor"))
                {
                using (var reader = query.ExecuteReader())
                    {
                    var party = new PartyModel();

                    while (reader.Read())
                        {
                        party.Id = Convert.ToInt32(reader["Id"]);
                        party.Description = reader["Description"] as string;
                        party.ContractorDescription = reader["ContractorDescription"] as string;

                        party.Date = (DateTime)(reader["Date"]);
                        party.DateOfActSet = (DateTime)(reader["DateOfActSet"]);

                        party.WarrantyHours = Convert.ToInt16(reader["WarrantyHours"]);
                        party.WarrantyYears = Convert.ToInt16(reader["WarrantyYears"]);

                        if (!Configuration.Current.Repository.WriteParty(party))
                            {
                            return false;
                            }
                        }
                    }
                }

            return true;
            }

        #endregion
        public override void DrawControls()
            {
            MainProcess.CreateLabel("Отсканируйте код", 19, 165, 211, MobileFontSize.Large);
            MainProcess.ToDoCommand = "Регистрация в системе";


            //todo: заглушка
            enterButton = MainProcess.CreateButton("Enter", 10, 275, 220, 35, "enter", () => OnBarcode("L" + int.MaxValue.ToString()));



            // MainProcess.CreateButton("Load database", 10, 220, 220, 35, "WifiOff", checkAllLamps);

            wifiOffButton = MainProcess.CreateButton("Wifi on/off", 10, 65, 220, 35, "WifiOff", () =>
                {
                    bool startStatus = MainProcess.ConnectionAgent.WifiEnabled;
                    if (startStatus)
                        {
                        MainProcess.ConnectionAgent.StopConnection();
                        }
                    else
                        {
                        MainProcess.StartConnectionAgent();
                        }
                    updateWifiOnOffButtonState(!startStatus);
                });
            updateWifiOnOffButtonState(MainProcess.ConnectionAgent.WifiEnabled);


            }

        private void updateWifiOnOffButtonState(bool wifiEnabled)
            {
            wifiOffButton.Text = wifiEnabled ? "ВКЛЮЧ нажмите чтобы ВЫКЛ" : "ВЫКЛ нажмите чтобы ВКЛЮЧ";
            }

        public override void OnBarcode(string Barcode)
            {
            if (Barcode.IsAccessoryBarcode())
                {
                ////if (Barcode.IndexOf("SB_EM.") < 0 || Barcode.Length == 6 || !Number.IsNumber(Barcode.Substring(6))) 
                ////{
                ////    ShowMessage("Необходимо отсканировать штрих-код сотрудника");
                ////    return;
                ////}
                //PerformQuery("Registration", Int32.Parse(Barcode.Substring(6)));
                //if (Parameters == null || Parameters[0] == null) return;

                //if (!((bool)(Parameters[0])))
                //{
                //    ShowMessage("Сотрудник не найден в системе!");
                //    return;
                //}

                ////Регистрация успешна!
                ////string name = Parameters[1] as string;
                MainProcess.User = Int32.Parse(Barcode.Substring(6));
                MainProcess.ClearControls();
                //Открыть окно выбора процесса
                MainProcess.Process = new SelectingLampProcess(MainProcess);
                }
            }

        public override void OnHotKey(KeyAction TypeOfAction)
            {
            switch (TypeOfAction)
                {
                case KeyAction.Proceed:
                    break;
                }
            }
        #endregion
        }

    class CaseInfo
        {
        public int Lamp;

        public int Unit;

        public int Map;

        public Int16 Register;

        public byte Position;

        public bool HasNotLamp
            {
            get
                {
                return Lamp == 0;
                }
            }

        public bool HasNotUnit
            {
            get
                {
                return Unit == 0;
                }
            }

        public bool HasNotPosition
            {
            get
                {
                return Map == 0;
                }
            }
        }
    }
