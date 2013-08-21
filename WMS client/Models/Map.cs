using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace WMS_client.Models
    {
    public sealed class Map : ICatalog<int>
        {
        public int Id { get; set; }

        public string Description { get; set; }

        public bool Deleted { get; set; }
        }
    }
