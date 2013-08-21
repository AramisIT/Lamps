using System;
using System.Data.SqlServerCe;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using WMS_client.Models;

namespace WMS_client.Repositories.Sql.Updaters
    {
    sealed class PartiesUpdater : CatalogUpdater<PartyModel, Int32>
        {
        public PartiesUpdater()
            : base("Parties", "PK_Parties")
        { }

        protected override void fillValues(SqlCeResultSet record, PartyModel item)
            {
            record.SetInt32(PartiesTable.Id, item.Id);
            record.SetString(PartiesTable.Description, item.Description);
            record.SetString(PartiesTable.ContractorDescription, item.ContractorDescription);
            record.SetValue(PartiesTable.DateOfActSet, item.DateOfActSet);
            record.SetInt16(PartiesTable.WarrantyHours, item.WarrantyHours);
            record.SetInt16(PartiesTable.WarrantyYears, item.WarrantyYears);
            record.SetDateTime(PartiesTable.Date, item.Date);
            }

        protected override void fillValues(SqlCeUpdatableRecord record, PartyModel item)
            {
            record.SetInt32(PartiesTable.Id, item.Id);
            record.SetString(PartiesTable.Description, item.Description);
            record.SetString(PartiesTable.ContractorDescription, item.ContractorDescription);
            record.SetValue(PartiesTable.DateOfActSet, item.DateOfActSet);
            record.SetInt16(PartiesTable.WarrantyHours, item.WarrantyHours);
            record.SetInt16(PartiesTable.WarrantyYears, item.WarrantyYears);
            record.SetDateTime(PartiesTable.Date, item.Date);
            }
        }
    }
