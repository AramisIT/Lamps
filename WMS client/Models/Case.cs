using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace WMS_client.Models
    {
    public sealed class Case : IAccessory, IFixableAccessory
        {
        public int Id { get; set; }

        public int Lamp { get; set; }

        public int Unit { get; set; }

        public Int16 Model { get; set; }

        public int Party { get; set; }

        public DateTime WarrantyExpiryDate { get; set; }

        public Byte Status { get; set; }

        public bool RepairWarranty { get; set; }

        public int Map { get; set; }

        public Int16 Register { get; set; }

        public byte Position { get; set; }
        }
    }
