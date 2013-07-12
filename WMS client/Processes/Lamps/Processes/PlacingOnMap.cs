using System;
using System.Data.SqlServerCe;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using WMS_client.Enums;
using WMS_client.db;

namespace WMS_client.Processes.Lamps
    {
    public class PlacingOnMap : BusinessProcess
        {
        private static SortedList<long, string> maps = new SortedList<long, string>();

        private static string getMapDescription(long id)
            {
            string mapDescription;
            if (!maps.TryGetValue(id, out mapDescription))
                {
                mapDescription = Accessory.GetDescription(typeof(Maps).Name, id);
                maps.Add(id, mapDescription);
                }

            return mapDescription;
            }

        private readonly long map;
        private string mapDescription;
        private readonly int register;
        private readonly int position;

        public PlacingOnMap(WMSClient wmsClient, long map, int register, int position)
            : base(wmsClient, 1)
            {
#if Release
            StopNetworkConnection();
#endif
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
            mapDescription = getMapDescription(map);
            if (string.IsNullOrEmpty(mapDescription))
                {
                MainProcess.Process = new SelectingLampProcess(MainProcess);
                return;
                }

            MainProcess.CreateLabel(string.Format("Карта {0}", mapDescription), 10, 70, 160, ControlsStyle.LabelLarge);
            MainProcess.CreateLabel(string.Format("Регістр {0}", register), 10, 105, 160, ControlsStyle.LabelLarge);
            MainProcess.CreateLabel(string.Format("Позиція {0}", position), 10, 140, 160, ControlsStyle.LabelLarge);

            MainProcess.CreateLabel("Скануйте світильник", 10, 220, 230, ControlsStyle.LabelLarge);
            }

        public override void OnBarcode(string barcode)
            {
            if (barcode.IsValidBarcode())
                {
                long id = Cases.GetIdByBarcode(barcode);
                if (id == 0)
                    {
                    ShowMessage("Не знайдено корпусу з таким штрих-кодом!");
                    return;
                    }

                Cases _case = new Cases();
                _case.Read(id);
                _case.Map = map;
                _case.Register = register;
                _case.Position = position;
                _case.IsSynced = false;
                _case.Status = TypesOfLampsStatus.IsWorking;
                _case.Write();

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
            MainProcess.Process = new SelectingLampProcess(MainProcess);
            }
        }
    }
