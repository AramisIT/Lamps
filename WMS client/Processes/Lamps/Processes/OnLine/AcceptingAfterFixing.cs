using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace WMS_client.Processes.Lamps
    {
    class AcceptingAfterFixing : BusinessProcess
        {
        public AcceptingAfterFixing(WMSClient wmsClient)
            : base(wmsClient, 1)
            {

            }

        public override void DrawControls()
            {
            MainProcess.ToDoCommand = "Приймання з ремонту";
            }

        public override void OnBarcode(string barcode)
            {

            }

        public override void OnHotKey(KeyAction key)
            {

            }
        }
    }
