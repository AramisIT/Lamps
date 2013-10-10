using System;
using System.Data.SqlServerCe;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using WMS_client.Enums;
using WMS_client.db;
using WMS_client.Models;

namespace WMS_client.Processes.Lamps
    {
    public class PlacingOnMap : BusinessProcess
        {
        private readonly int map;
        private string mapDescription;
        private readonly Int16 register;
        private readonly byte position;

        public PlacingOnMap(WMSClient wmsClient, int map, Int16 register, byte position)
            : base(wmsClient, 1)
            {
            if (applicationIsClosing)
                {
                return;
                }
            StopNetworkConnection();

            this.map = map;
            this.register = register;
            this.position = position;

            DrawForm1Controls();
            }

        public override void DrawControls()
            {
            MainProcess.ToDoCommand = "Установка світильника";
            }

        private void DrawForm1Controls()
            {
            mapDescription = (Configuration.Current.Repository.GetMap(map) ?? new Map()).Description;
            if (string.IsNullOrEmpty(mapDescription))
                {
                MainProcess.Process = new StartProcess(MainProcess);
                return;
                }

            MainProcess.CreateLabel(string.Format("Карта {0}", mapDescription), 10, 70, 160, ControlsStyle.LabelLarge);
            MainProcess.CreateLabel(string.Format("Регістр {0}", register), 10, 105, 160, ControlsStyle.LabelLarge);
            MainProcess.CreateLabel(string.Format("Позиція {0}", position), 10, 140, 160, ControlsStyle.LabelLarge);

            MainProcess.CreateLabel("Скануйте світильник", 10, 220, 230, ControlsStyle.LabelLarge);
            }

        public override void OnBarcode(string barcode)
            {
            if (barcode.IsAccessoryBarcode())
                {
                Case _Case = Configuration.Current.Repository.ReadCase(barcode.GetIntegerBarcode());
                if (_Case == null)
                    {
                    ShowMessage("Не знайдено корпусу з таким штрих-кодом!");
                    return;
                    }

                if (_Case.Lamp == 0)
                    {
                    ShowMessage("Лампа не вказана! Операція відмінена!");
                    return;
                    }

                if (_Case.Unit == 0)
                    {
                    ShowMessage("Електронний блок не вказаний! Операція відмінена!");
                    return;
                    }

                _Case.Map = map;
                _Case.Register = register;
                _Case.Position = position;
                _Case.Status = (int)TypesOfLampsStatus.IsWorking;

                if (!Configuration.Current.Repository.UpdateCases(new List<Case> { _Case }, false))
                    {
                    ShowMessage("Не вдалося оновити світильник!");
                    return;
                    }

                leaveProcess();
                }
            }

        public override void OnHotKey(KeyAction key)
            {
            switch (key)
                {
                case KeyAction.Esc:
                    leaveProcess();
                    break;
                }
            }

        private void leaveProcess()
            {
            ClearControls();
            MainProcess.Process = new StartProcess(MainProcess);
            }
        }
    }
