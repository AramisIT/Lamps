using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WMS_client.Models
    {
    public interface IAccessory
        {
        int Id { get; set; }

        Int16 Model { get; set; }

        int Party { get; set; }

        DateTime WarrantyExpiryDate { get; set; }

        Byte Status { get; set; }
        }
    }
