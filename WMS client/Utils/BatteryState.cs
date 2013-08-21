﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsMobile.Status;

namespace WMS_client.Utils
    {
    class BatteryChargeStatus
        {

        public static bool Low
            {
            get
                {
                if (!Configuration.Current.ReleaseMode)
                    {
                    return false;
                    }
                return SystemState.PowerBatteryState != BatteryState.Charging &&
                       (SystemState.PowerBatteryStrength == BatteryLevel.Low || SystemState.PowerBatteryStrength == BatteryLevel.VeryLow);
                }
            }

        public static bool Critical
            {
            get
                {
                if (!Configuration.Current.ReleaseMode)
                    {
                    return false;
                    }
                return SystemState.PowerBatteryState != BatteryState.Charging &&
                       SystemState.PowerBatteryStrength == BatteryLevel.VeryLow;
                }
            }

        public static int ChargeValue
            {
            get
                {
                return ((int)SystemState.PowerBatteryStrength);
                }
            }
        }
    }