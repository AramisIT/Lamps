using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WMS_client.Models
    {
    interface ICatalog<ID>
        {
        ID Id { get; set; }
        }
    }
