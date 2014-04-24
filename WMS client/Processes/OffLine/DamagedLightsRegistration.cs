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

        private Int16 registerNumber
            {
            get
                {
                if (string.IsNullOrEmpty(registerTextBox.Text))
                    {
                    return 0;
                    }
                try
                    {
                    return Convert.ToInt16(registerTextBox.Text);
                    }
                catch
                    {
                    return 0;
                    }
                }
            set
                {
                registerTextBox.Text = (value == 0) ? string.Empty : value.ToString();
                }
            }
        private byte amount
            {
            get
                {
                if (string.IsNullOrEmpty(amountTextBox.Text))
                    {
                    return 0;
                    }
                try
                    {
                    return Convert.ToByte(amountTextBox.Text);
                    }
                catch
                    {
                    return 0;
                    }
                }
            set
                {
                amountTextBox.Text = (value == 0) ? string.Empty : value.ToString();
                }
            }

        private MobileButton mapButton;
        private MobileButton proceedButton;
        private MobileTextBox registerTextBox;
        private MobileTextBox amountTextBox;

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
            registerTextBox = MainProcess.CreateTextBox(190, top, 40, string.Empty, ControlsStyle.LabelLarge, null, false);

            top += delta;
            MainProcess.CreateLabel("Кількість непраціючих", 5, top, 180, ControlsStyle.LabelNormal);
            amountTextBox = MainProcess.CreateTextBox(190, top, 40, string.Empty, ControlsStyle.LabelLarge, null, false);

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
            if (!writeData()) return;

            registerNumber = 0;
            amount = 0;
            }

        private bool writeData()
            {
            if (mapId == 0)
                {
                "Виберіть мапу!".Warning();
                return false;
                }

            if (registerNumber == 0)
                {
                "Вкажіть регістр!".Warning();
                return false;
                }

            var brokenLightsRecord = new BrokenLightsRecord() { Map = mapId, RegisterNumber = registerNumber, Amount = amount };
            if (!Configuration.Current.Repository.UpdateBrokenLightsRecord(brokenLightsRecord))
                {
                "Помилка при збереженні даних".Warning();
                return false;
                }

            return true;
            }

        protected override void OnBarcode(string barcode)
            {

            }

        protected override void OnHotKey(KeyAction key)
            {
            switch (key)
                {
                case KeyAction.Esc:
                    if ("Вийти?".Ask())
                        {
                        leaveProcess();
                        }
                    break;

                case KeyAction.Complate:
                    if (writeData())
                        {
                        leaveProcess();
                        }
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
