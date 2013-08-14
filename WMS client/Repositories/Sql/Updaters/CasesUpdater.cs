using System;
using System.Data.SqlServerCe;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using WMS_client.Models;

namespace WMS_client.Repositories.Sql.Updaters
    {
    class CasesUpdater : AccessoryUpdater<Case>
        {
        public CasesUpdater()
            : base("Cases", "PK_Cases")
        { }

        protected override void fillValues(SqlCeResultSet record, Case _Case)
            {
            record.SetInt16(CasesTable.Model, _Case.Model);
            record.SetInt32(CasesTable.Party, _Case.Party);
            record.SetByte(CasesTable.Status, _Case.Status);
            record.SetBoolean(CasesTable.RepairWarranty, _Case.RepairWarranty);
            record.SetValue(CasesTable.WarrantyExpiryDate, getSqlDateTime(_Case.WarrantyExpiryDate));

            record.SetInt32(CasesTable.Lamp, _Case.Lamp);
            record.SetInt32(CasesTable.Unit, _Case.Unit);

            record.SetInt32(CasesTable.Map, _Case.Map);
            record.SetInt16(CasesTable.Register, _Case.Register);
            record.SetByte(CasesTable.Position, _Case.Position);
            }

        //var logger = new AccessoryLogger<Case>("CasesUpdating", list, getOpenedConnection);
        //  return logger.Log();

        protected override void fillValues(SqlCeUpdatableRecord record, Case _Case)
            {
            record.SetInt32(0, _Case.Id);

            record.SetInt16(CasesTable.Model, _Case.Model);
            record.SetInt32(CasesTable.Party, _Case.Party);
            record.SetByte(CasesTable.Status, _Case.Status);
            record.SetBoolean(CasesTable.RepairWarranty, _Case.RepairWarranty);
            record.SetValue(CasesTable.WarrantyExpiryDate, getSqlDateTime(_Case.WarrantyExpiryDate));

            record.SetInt32(CasesTable.Lamp, _Case.Lamp);
            record.SetInt32(CasesTable.Unit, _Case.Unit);

            record.SetInt32(CasesTable.Map, _Case.Map);
            record.SetInt16(CasesTable.Register, _Case.Register);
            record.SetByte(CasesTable.Position, _Case.Position);
            }
        }
    }
