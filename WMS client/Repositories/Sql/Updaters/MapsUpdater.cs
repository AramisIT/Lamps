using System;
using System.Data.SqlServerCe;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using WMS_client.Models;

namespace WMS_client.Repositories.Sql.Updaters
    {
    sealed class MapsUpdater : CatalogUpdater<Map, Int32>
        {
        public MapsUpdater()
            : base("Maps", "PK_Maps")
        { }

        protected override void fillValues(SqlCeResultSet record, Map item)
            {
            record.SetInt32(MapsTable.Id, item.Id);
            record.SetString(MapsTable.Description, item.Description);
            }

        protected override void fillValues(SqlCeUpdatableRecord record, Map item)
            {
            record.SetInt32(MapsTable.Id, item.Id);
            record.SetString(MapsTable.Description, item.Description);
            }
        }
    }
