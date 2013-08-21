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

      
        #region temporary methods

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
