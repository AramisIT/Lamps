using System;
using System.Data.SqlServerCe;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using WMS_client.Enums;
using WMS_client.db;

namespace WMS_client.Processes.Lamps
    {
    public class ScannerMode : BusinessProcess
        {
        private MobileLabel serverReplyLabel;
        private MobileLabel barcodeDataLabel;

        public ScannerMode(WMSClient wmsClient)
            : base(wmsClient, 1)
            {

            }

        public override void DrawControls()
            {
            MainProcess.ToDoCommand = "Режим сканеру";

            barcodeDataLabel = MainProcess.CreateLabel("<нема штрих-коду>", 8, 70, 224, ControlsStyle.LabelLarge);
            serverReplyLabel = MainProcess.CreateLabel("", 8, 120, 224, ControlsStyle.LabelMultiline);
            }

        public override void OnBarcode(string barcode)
            {
            barcodeDataLabel.Text = barcode;
            PerformQuery("PerformeBarcodeAction", barcode);
            serverReplyLabel.Text = SuccessQueryResult ? ResultParameters[1].ToString() : "помилка";
            }

        private void leaveProcess()
            {
            ClearControls();
            MainProcess.Process = new SelectingLampProcess(MainProcess);
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
        }
    }
