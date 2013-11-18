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
    public class DamagedLightsRegistration : BusinessProcess
        {
        private int mapId;
        private Int16 register;
        private byte amount;

        private MobileButton mapButton;
        private MobileButton proceedButton;
        private MobileControl registerTextBox;
        private MobileControl amountTextBox;

        public DamagedLightsRegistration()
            : base(1)
            {
            if (applicationIsClosing)
                {
                return;
                }
            StopNetworkConnection();
            }

        public override void DrawControls()
            {
            MainProcess.ToDoCommand = "Реєстрація непраціючих";

            var top = 5;
            const int delta = 55;

            top += delta;
            mapButton = MainProcess.CreateButton("Карта", 5, top, 230, 35, string.Empty, chooseMap);

            top += delta;
            MainProcess.CreateLabel("Регістр", 5, top, 160, ControlsStyle.LabelNormal);
            registerTextBox = MainProcess.CreateTextBox(190, top, 40, string.Empty, ControlsStyle.LabelLarge, false);

            top += delta;
            MainProcess.CreateLabel("Кількість непраціючих", 5, top, 180, ControlsStyle.LabelNormal);
            amountTextBox = MainProcess.CreateTextBox(190, top, 40, string.Empty, ControlsStyle.LabelLarge, false);

            top += delta;
            top += delta;
            proceedButton = MainProcess.CreateButton("Далі", 5, top, 230, 35, string.Empty, proceedClick);

            updateMapDescription();
            }

        private void updateMapDescription()
            {
            mapButton.Text = mapId == 0 ? "<виберіть карту>"
                : (Configuration.Current.Repository.GetMap(mapId) ?? new Map()).Description;
            }

        private void chooseMap()
            {
            CatalogItem selectedMap;

            if (SelectFromList(Configuration.Current.Repository.MapsList, out selectedMap))
                {
                mapId = (int)selectedMap.Id;
                updateMapDescription();
                }
            }

        private void proceedClick()
            {

            }

        public override void OnBarcode(string barcode)
            {

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
            MainProcess.Process = new StartProcess();
            }
        }
    }
