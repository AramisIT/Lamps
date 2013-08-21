using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WMS_client.Models
    {
    public interface ICatalog<ID>
        {
        ID Id { get; set; }

        string Description { get; set; }

        bool Deleted { get; set; }
        }
    }
