using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using WMS_client.db;
using WMS_client.Enums;
using WMS_client.Models;
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

            bool systemReady = CheckNewDataBase();

            if (!systemReady)
                {
                ShowMessage("База данных не актуальна. Нужно обновить приложение или базу данных.");
                enterButton.Enabled = false;
                }
            }

        private bool CheckNewDataBase()
            {
            bool ok = checkModels() && checkParties() && checkMaps() && checkAllLamps();
            return ok;
            }

        private bool checkAllLamps()
            {
            #region sql command
            const string sql = @"
select

Cast(SUBSTRING(c.barcode, 2, 24) as int) Id,
c.Model CaseModel, c.Party CaseParty, 
c.DateOfWarrantyEnd CaseWarrantyExpiryDate,
c.Status CaseStatus, 
c.TypeOfWarrantly CaseWarrantyType,
cast(c.map as int) [Map],
cast(c.Register as smallint) Register,
cast(c.Position as TinyInt) Position,


case when l.Model is null then 0 else l.Model end LampModel,
case when l.Party is null then 0 else l.Party end LampParty,
case when l.Status is null then 0 else l.Status end LampStatus,
case when l.DateOfWarrantyEnd = cast('1753-01-01' as datetime) then null else l.DateOfWarrantyEnd end LampWarrantyExpiryDate,

case when u.Model is null then 0 else u.Model end UnitModel,
case when u.Party is null then 0 else u.Party end UnitParty,
case when u.Status is null then 0 else u.Status end UnitStatus,
case when u.TypeOfWarrantly is null then 0 else u.TypeOfWarrantly end UnitWarrantyType,

case when u.DateOfWarrantyEnd = cast('1753-01-01' as datetime) then null else u.DateOfWarrantyEnd end UnitWarrantyExpiryDate,

case when (u.barcode is null) or RTrim(u.barcode) = '' then 0 else Cast(SUBSTRING(u.barcode, 2, 24) as int) end UnitBarcode


from cases c
left join Lamps l on c.Lamp = l.Id
left join ElectronicUnits u on c.ElectronicUnit = u.Id

order by LampWarrantyExpiryDate
";
            #endregion

            var cases = new List<Case>();
            var lamps = new List<Lamp>();
            var units = new List<Unit>();

            using (SqlCeCommand query = dbWorker.NewQuery(sql))
                {
                using (var reader = query.ExecuteReader())
                    {
                    while (reader.Read())
                        {
                        var lamp = new Lamp();
                        lamp.Id = Configuration.Current.Repository.GetNextLampId();

                        var unit = new Unit();
                        unit.Id = Configuration.Current.Repository.GetNextUnitId();

                        var _Case = new Case();
                        _Case.Lamp = lamp.Id;
                        _Case.Unit = unit.Id;

                        fillCase(_Case, reader);
                        fillLamp(lamp, reader);
                        fillUnit(unit, reader);

                        cases.Add(_Case);
                        lamps.Add(lamp);
                        units.Add(unit);
                        }
                    }
                }

            Configuration.Current.Repository.InsertLamps(lamps);
            Configuration.Current.Repository.InsertUnits(units);
            Configuration.Current.Repository.InsertCases(cases);

            return true;
            }

        private void fillUnit(Unit accessory, SqlCeDataReader reader)
            {
            accessory.Model = Convert.ToInt16(reader["UnitModel"]);
            accessory.Party = Convert.ToInt32(reader["UnitParty"]);
            accessory.Status = Convert.ToByte(reader["UnitStatus"]);

            object warrantyExpiryDate = reader["UnitWarrantyExpiryDate"];
            bool emptyDate = warrantyExpiryDate == null || DBNull.Value.Equals(warrantyExpiryDate);
            accessory.WarrantyExpiryDate = emptyDate ? DateTime.MinValue : (DateTime)warrantyExpiryDate;

            accessory.Barcode = Convert.ToInt32(reader["UnitBarcode"]);

            var warrantyType = (TypesOfLampsWarrantly)Convert.ToInt32(reader["UnitWarrantyType"]);
            accessory.RepairWarranty = warrantyType == TypesOfLampsWarrantly.Repair;
            }

        private void fillLamp(Lamp accessory, SqlCeDataReader reader)
            {
            accessory.Model = Convert.ToInt16(reader["LampModel"]);
            accessory.Party = Convert.ToInt32(reader["LampParty"]);
            accessory.Status = Convert.ToByte(reader["LampStatus"]);

            object warrantyExpiryDate = reader["LampWarrantyExpiryDate"];
            bool emptyDate = warrantyExpiryDate == null || DBNull.Value.Equals(warrantyExpiryDate);
            accessory.WarrantyExpiryDate = emptyDate ? DateTime.MinValue : (DateTime)warrantyExpiryDate;
            }

        private void fillCase(Case accessory, SqlCeDataReader reader)
            {
            accessory.Model = Convert.ToInt16(reader["CaseModel"]);
            accessory.Party = Convert.ToInt32(reader["CaseParty"]);
            accessory.Status = Convert.ToByte(reader["CaseStatus"]);

            object warrantyExpiryDate = reader["CaseWarrantyExpiryDate"];
            bool emptyDate = warrantyExpiryDate == null || DBNull.Value.Equals(warrantyExpiryDate);
            accessory.WarrantyExpiryDate = emptyDate ? DateTime.MinValue : (DateTime)warrantyExpiryDate;

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
    }
