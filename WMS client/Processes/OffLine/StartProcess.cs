using System.Collections.Generic;
using System.Diagnostics;
using WMS_client.Enums;
using System;
using System.Windows.Forms;
using System.Data.SqlServerCe;
using WMS_client.Models;
using WMS_client.Processes.Lamps;
using WMS_client.db;

namespace WMS_client
    {
    /// <summary>Выбор процесса (для светильников)</summary>
    public class StartProcess : BusinessProcess
        {
        private MobileButton wifiOffButton;
        private MobileLabel connectionStatusLabel;

        /// <summary>Выбор процесса (для светильников)</summary>
        /// <param name="MainProcess">Основной процесс</param>
        public StartProcess()//, IServerIdProvider serverIdProvider)
            : base( 1)
            {
            BusinessProcessType = ProcessType.Selecting;
            }

        #region Override methods
        public override void DrawControls()
            {
            MainProcess.ToDoCommand = "Оберіть процес";

            MainProcess.CreateButton("Режим сканера", 10, 75, 220, 40, string.Empty, scannerMode_Click);
            MainProcess.CreateButton("Реєстрація", 10, 125, 220, 40, "registration", registration_Click);

            connectionStatusLabel = MainProcess.CreateLabel(string.Empty, 10, 195, 230, MobileFontSize.Normal);
            wifiOffButton = MainProcess.CreateButton(string.Empty, 10, 220, 220, 40, "WifiOff", changeConnectionStatus);
            updateWifiOnOffButtonState(MainProcess.ConnectionAgent.WifiEnabled);

            MainProcess.CreateLabel("Синхронізація - F5", 25, 280, 230, MobileFontSize.Large);
            }

        private void changeConnectionStatus()
            {
            bool startStatus = MainProcess.ConnectionAgent.WifiEnabled;
            if (startStatus)
                {
                StopNetworkConnection();
                }
            else
                {
                MainProcess.StartConnectionAgent();
                }
            updateWifiOnOffButtonState(!startStatus);
            }

        private void updateWifiOnOffButtonState(bool wifiEnabled)
            {
            wifiOffButton.Text = wifiEnabled ? "ВІД'ЄДНАТИ" : "ПІД'ЄДНАТИ";
            connectionStatusLabel.Text = (wifiEnabled ? "" : "не ") + "під'єднан до мережі";
            }

        public override void OnBarcode(string barcode)
            {
          if (barcode.IsValidPositionBarcode())
                {
                tryPlacingLight(barcode);
                }
            else if (barcode.IsAccessoryBarcode())
                {
                var foundAccessory = Configuration.Current.Repository.FindAccessory(barcode.GetIntegerBarcode());
                var accessoryType = AccessoryHelper.GetAccessoryType(foundAccessory);
                if (accessoryType == TypeOfAccessories.Case && "Переместить светильник в цех?".Ask())
                    {
                    var _Case = foundAccessory as Case;
                    _Case.Map = 0;
                    _Case.Position = 0;
                    _Case.Register = 0;

                    if (!Configuration.Current.Repository.WriteCase(_Case))
                        {
                        "Не удалось переместить корпус в цех!".Warning();
                        return;
                        }
                    }
                }
            }

        private void tryPlacingLight(string barcode)
            {

            int map;
            Int16 register;
            byte position;
            if (barcode.GetPositionData(out map, out register, out position))
                {
                ClearControls();
                MainProcess.Process = new PlacingOnMap(map, register, position);
                }
            }

        public override void OnHotKey(KeyAction TypeOfAction)
            {
            switch (TypeOfAction)
                {
                //Синхронізація
                case KeyAction.Proceed:
                    new SynchronizerWithGreenhouse();
                    MainProcess.ClearControls();
                    MainProcess.Process = new StartProcess();
                    break;
                }
            }
        #endregion
        
        #region ButtonClick
        /// <summary>Перехід до процессу "Інформація"</summary>
        private void scannerMode_Click()
            {
            MainProcess.ClearControls();
            MainProcess.Process = new ScannerMode();
            }

        public bool SaveAccessoriesSet(Case _Case, Lamp lamp, Unit unit)
            {
            bool ok = true;

            var repository = Configuration.Current.Repository;

            if (unit != null)
                {
                if (unit.Id <= 0)
                    {
                    unit.Id = repository.GetNextUnitId();
                    ok = ok && repository.UpdateUnits(new List<Unit>() { unit }, true);
                    }
                else
                    {
                    ok = ok && repository.UpdateUnits(new List<Unit>() { unit }, false);
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
                    lamp.Id = repository.GetNextLampId();
                    ok = ok && repository.UpdateLamps(new List<Lamp>() { lamp }, true);
                    }
                else
                    {
                    ok = ok && repository.UpdateLamps(new List<Lamp>() { lamp }, false);
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
                ok = ok && repository.UpdateCases(new List<Case>() { _Case }, false);
                }

            return ok;
            }

        /// <summary>Регістрація/редагування</summary>
        private void registration_Click()
            {
            MainProcess.ClearControls();
            MainProcess.Process = new EditSelector();
            }
        #endregion

        #region Query
              
     

        #endregion
        }
    }

