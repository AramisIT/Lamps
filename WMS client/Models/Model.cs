using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace WMS_client.Models
    {
    public sealed class Model : ICatalog<Int16>
        {
        public Int16 Id { get; set; }

        public string Description { get; set; }

        public bool Deleted { get; set; }
        }
    }
