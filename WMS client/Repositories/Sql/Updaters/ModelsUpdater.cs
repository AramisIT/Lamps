using System;
using System.Data.SqlServerCe;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using WMS_client.Models;

namespace WMS_client.Repositories.Sql.Updaters
    {
    sealed class ModelsUpdater : CatalogUpdater<Model, Int16>
        {
        public ModelsUpdater()
            : base("Models", "PK__Models")
        { }

        protected override void fillValues(SqlCeResultSet record, Model item)
            {
            record.SetInt16(ModelsTable.Id, item.Id);
            record.SetString(ModelsTable.Description, item.Description);
            }

        protected override void fillValues(SqlCeUpdatableRecord record, Model item)
            {
            record.SetInt16(ModelsTable.Id, item.Id);
            record.SetString(ModelsTable.Description, item.Description);
            }
        }
    }
