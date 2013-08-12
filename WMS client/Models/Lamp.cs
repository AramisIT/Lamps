﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace WMS_client.Models
    {
    public sealed class Lamp : IAccessory, IBarcodeAccessory
        {
        public int Id { get; set; }

        public Int16 Model { get; set; }

        public int Party { get; set; }

        public DateTime WarrantyExpiryDate { get; set; }

        public Byte Status { get; set; }

        public int Barcode { get; set; }
        }
    }