using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using WMS_client.Models;

namespace WMS_client.Processes
    {
    public class AccessoriesSet
        {
        public Case Case { get; set; }
        public Lamp Lamp { get; set; }
        public Unit Unit { get; set; }

        public IAccessory CurrentAccessory { get; set; }
        }
    }
