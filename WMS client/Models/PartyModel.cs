using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace WMS_client.Models
    {
    public sealed class PartyModel : ICatalog<int>
        {
        public int Id { get; set; }

        public string Description { get; set; }

        public string ContractorDescription { get; set; }

        public DateTime Date { get; set; }

        public DateTime DateOfActSet { get; set; }

        public Int16 WarrantyHours { get; set; }

        public Int16 WarrantyYears { get; set; }

        public bool Deleted { get; set; }
        }
    }
