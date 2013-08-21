using System;
using System.Data.SqlServerCe;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using WMS_client.Models;

namespace WMS_client.Repositories.Sql.Updaters
    {
    sealed class UnitsUpdater : AccessoryUpdater<Unit>
        {
        public UnitsUpdater()
            : base("Units", "PK_Units", "UnitsUpdating", "PK_UnitsUpdating")
        { }

        protected override void fillValues(SqlCeResultSet record, Unit unit)
            {
            record.SetInt16(UnitsTable.Model, unit.Model);
            record.SetInt32(UnitsTable.Party, unit.Party);
            record.SetByte(UnitsTable.Status, unit.Status);
            record.SetBoolean(UnitsTable.RepairWarranty, unit.RepairWarranty);
            record.SetValue(UnitsTable.WarrantyExpiryDate, getSqlDateTime(unit.WarrantyExpiryDate));
            record.SetInt32(UnitsTable.Barcode, unit.Barcode);
            }

        protected override void fillValues(SqlCeUpdatableRecord record, Unit unit)
            {
            record.SetInt32(0, unit.Id);

            record.SetInt16(UnitsTable.Model, unit.Model);
            record.SetInt32(UnitsTable.Party, unit.Party);
            record.SetByte(UnitsTable.Status, unit.Status);
            record.SetBoolean(UnitsTable.RepairWarranty, unit.RepairWarranty);
            record.SetValue(UnitsTable.WarrantyExpiryDate, getSqlDateTime(unit.WarrantyExpiryDate));
            record.SetInt32(UnitsTable.Barcode, unit.Barcode);
            }
        }
    }
