using System;
using System.Data;
using System.Data.SqlServerCe;
using WMS_client.db;
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
            bool ok = checkModels() && checkParties() && checkMaps();
            return ok;
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
            enterButton = MainProcess.CreateButton("Enter", 10, 275, 220, 35, "enter", () => OnBarcode("L9786175660690"));


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
            if (Barcode.IsValidBarcode())
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
