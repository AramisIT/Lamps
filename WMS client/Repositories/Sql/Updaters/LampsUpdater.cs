using System;
using System.Data.SqlServerCe;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using WMS_client.Models;

namespace WMS_client.Repositories.Sql.Updaters
    {
    sealed class LampsUpdater : AccessoryUpdater<Lamp>
        {
        public LampsUpdater()
            : base("Lamps", "PK_Lamps", "LampsUpdating", "PK_LampsUpdating")
        { }

        protected override void fillValues(SqlCeResultSet record, Lamp lamp)
            {
            record.SetInt16(LampsTable.Model, lamp.Model);
            record.SetInt32(LampsTable.Party, lamp.Party);
            record.SetByte(LampsTable.Status, lamp.Status);
            record.SetValue(LampsTable.WarrantyExpiryDate, getSqlDateTime(lamp.WarrantyExpiryDate));
            record.SetInt32(LampsTable.Barcode, lamp.Barcode);
            }

        protected override void fillValues(SqlCeUpdatableRecord record, Lamp lamp)
            {
            record.SetInt32(0, lamp.Id);

            record.SetInt16(LampsTable.Model, lamp.Model);
            record.SetInt32(LampsTable.Party, lamp.Party);
            record.SetByte(LampsTable.Status, lamp.Status);
            record.SetValue(LampsTable.WarrantyExpiryDate, getSqlDateTime(lamp.WarrantyExpiryDate));
            record.SetInt32(LampsTable.Barcode, lamp.Barcode);
            }
        }
    }
