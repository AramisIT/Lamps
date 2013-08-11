using System;
using WMS_client.db;

namespace WMS_client
    {
    /// <summary>Регистрация при входе</summary>
    public class RegistrationProcess : BusinessProcess
        {
        private MobileButton wifiOffButton;

        #region Public methods
        /// <summary>Регистрация при входе</summary>
        public RegistrationProcess(WMSClient MainProcess)
            : base(MainProcess, 1)
            {
            BusinessProcessType = ProcessType.Registration;
            }

        public override void DrawControls()
            {
            MainProcess.CreateLabel("Отсканируйте код", 19, 165, 211, MobileFontSize.Large);
            MainProcess.ToDoCommand = "Регистрация в системе";

            //todo: заглушка
            MainProcess.CreateButton("Enter", 10, 275, 220, 35, "enter", () => OnBarcode("L9786175660690"));

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
