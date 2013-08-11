using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlServerCe;
using WMS_client.Utils;
using WMS_client.db;

namespace WMS_client.Processes.Lamps.Sync
    {
    internal class AccessorySynchronizer : CatalogSynchronizer
        {
        protected string tableName;

        public AccessorySynchronizer(string tableName)
            {
            this.tableName = tableName;
            }

        protected override string TableName
            {
            get { return tableName; }
            }

        protected override SqlCeCommand GetUpdateQuery(DataRow row)
            {
            string sql = string.Format(@"UPDATE {0} 
SET 
[BarCode]=@BarCode,
[DateOfActuality]=@DateOfActuality,
[DrawdownDate]=@DrawdownDate,
[HoursOfWork]=@HoursOfWork,
[Marking]=@Marking,
[Model]=@Model,
[Party]=@Party,
[Status]=@Status,
[TypeOfWarrantly]=@TypeOfWarrantly,
[DateOfWarrantyEnd]=@DateOfWarrantyEnd,
[IsSynced]=@IsSynced,
[Location]=@Location,
[Posted]=@Posted,
[Number]=@Number,
[Responsible]=@Responsible,
[Date]=@Date,
[MarkForDeleting]=@MarkForDeleting,
[Description]=@Description,
[SyncRef]=@SyncRef,
[LastModified]=@LastModified 
WHERE [Id]=@Id", tableName);

            SqlCeCommand query = dbWorker.NewQuery(sql);

            return query;
            }

        protected override SqlCeCommand GetInsertQuery(DataRow row)
            {
            string sql =
                string.Format(
                    @"INSERT INTO {0}([BarCode],[DateOfActuality],[DrawdownDate],[HoursOfWork],[Marking],[Model],[Party],
                        [Status],[TypeOfWarrantly],[DateOfWarrantyEnd],[IsSynced],[Location],[Posted],[Number],[Responsible],[Date],
                        [MarkForDeleting],[Description],[SyncRef],[Id],[LastModified])
 
                        VALUES(@BarCode,@DateOfActuality,@DrawdownDate,@HoursOfWork,@Marking,@Model,
                        @Party,@Status,@TypeOfWarrantly,@DateOfWarrantyEnd,@IsSynced,@Location,@Posted,@Number,
                        @Responsible,@Date,@MarkForDeleting,@Description,@SyncRef, @Id, @LastModified)",
                    tableName);

            SqlCeCommand query = dbWorker.NewQuery(sql);

            return query;
            }
        }

    internal class CasesSynchronizer : CatalogSynchronizer
        {
        protected override string TableName
            {
            get { return "Cases"; }
            }


        protected override SqlCeCommand GetUpdateQuery(DataRow row)
            {
            const string sql = @"UPDATE Cases 
SET 
[BarCode]=@BarCode,
[DateOfActuality]=@DateOfActuality,
[DrawdownDate]=@DrawdownDate,
[HoursOfWork]=@HoursOfWork,
[Marking]=@Marking,
[Model]=@Model,
[Party]=@Party,
[Status]=@Status,
[TypeOfWarrantly]=@TypeOfWarrantly,
[DateOfWarrantyEnd]=@DateOfWarrantyEnd,
[IsSynced]=@IsSynced,
[Location]=@Location,
[Posted]=@Posted,
[Number]=@Number,
[Responsible]=@Responsible,
[Date]=@Date,
[MarkForDeleting]=@MarkForDeleting,
[Description]=@Description,
[SyncRef]=@SyncRef,
[LastModified]=@LastModified,
[ElectronicUnit]=@ElectronicUnit, 
[Lamp]=@Lamp, 
[Map]=@Map, 
[Position]=@Position,
[Register]=@Register

WHERE [Id]=@Id";

            SqlCeCommand query = dbWorker.NewQuery(sql);
            addCaseParameters(query, row);

            return query;
            }

        protected override SqlCeCommand GetInsertQuery(DataRow row)
            {
            const string sql = @"
            INSERT INTO Cases ([BarCode],[DateOfActuality],[DrawdownDate],[HoursOfWork],[Marking],[Model],[Party],
                        [Status],[TypeOfWarrantly],[DateOfWarrantyEnd],[IsSynced],[Location],[Posted],[Number],[Responsible],[Date],
                        [MarkForDeleting],[Description],[SyncRef],[Id], [LastModified], [ElectronicUnit], [Lamp], [Map], [Position], [Register])
 
                        VALUES(@BarCode,@DateOfActuality,@DrawdownDate,@HoursOfWork,@Marking,@Model,
                        @Party,@Status,@TypeOfWarrantly,@DateOfWarrantyEnd,@IsSynced,@Location,@Posted,@Number,
                        @Responsible,@Date,@MarkForDeleting,@Description,@SyncRef, @Id, @LastModified, @ElectronicUnit, @Lamp, @Map, @Position, @Register)";

            SqlCeCommand query = dbWorker.NewQuery(sql);
            addCaseParameters(query, row);
            return query;
            }

        private void addCaseParameters(SqlCeCommand query, DataRow row)
            {
            query.AddParameter("ElectronicUnit", BarcodeWorker.GetIdByRef(typeof(ElectronicUnits), row["ElectronicUnit"] as string));
            query.AddParameter("Lamp", BarcodeWorker.GetIdByRef(typeof(db.Lamps), row["Lamp"] as string));
            query.AddParameter("Map", CatalogHelper.GetModelId<Maps>(row["Map"]));
            query.AddParameter("Position", Convert.ToInt32(row["Position"]));
            query.AddParameter("Register", Convert.ToInt32(row["Register"]));
            }

        }


    internal abstract class CatalogSynchronizer
        {
        private static readonly string SYNCREF_NAME = dbObject.SYNCREF_NAME;

        protected abstract string TableName { get; }

        public void Merge(DataRow row)
            {
            string syncRef = row[SYNCREF_NAME] as string;
            if (string.IsNullOrEmpty(syncRef))
                {
                return;
                }

            string sql = string.Format("select Id from {0} where SyncRef=@SyncRef", TableName);

            object statusObj = null;

            using (SqlCeCommand selectQuery = dbWorker.NewQuery(sql))
                {
                selectQuery.AddParameter(SYNCREF_NAME, syncRef);

                statusObj = selectQuery.ExecuteScalar();
                }

            using (SqlCeCommand query = getMergeQuery(row, statusObj))
                {
                try
                    {
                    addDefaultParameters(query, row);
                    }
                catch (Exception exp)
                    {
                    SqlCeCommand newQuery = getMergeQuery(row, statusObj);
                    addDefaultParameters(newQuery, row);
                    Trace.Write(exp.Message);
                    }

                try
                    {
                    query.ExecuteNonQuery();
                    }
                catch (Exception exp)
                    {
                    string errorMessage = exp.Message;
                    Trace.WriteLine(errorMessage);
                    }
                }
            }

        private SqlCeCommand getMergeQuery(DataRow row, object statusObj)
            {
            SqlCeCommand query;
            if (statusObj == null)
                {
                query = GetInsertQuery(row);
                query.AddParameter("Id", dbObject.GetNewId(TableName));
                }
            else
                {
                query = GetUpdateQuery(row);
                query.AddParameter("Id", Convert.ToInt64(statusObj));
                }
            return query;
            }

        private void addDefaultParameters(SqlCeCommand query, DataRow row)
            {
            query.AddParameter(SYNCREF_NAME, row[SYNCREF_NAME]);
            query.AddParameter("BarCode", row["Barcode"]);


            query.AddParameter("DateOfActuality", StringParser.ParseDateTime(row["DateOfActuality"]));
            query.AddParameter("DrawdownDate", StringParser.ParseDateTime(row["DrawdownDate"]));
            query.AddParameter("DateOfWarrantyEnd", StringParser.ParseDateTime(row["DateOfWarrantyEnd"]));
            query.AddParameter("Date", StringParser.ParseDateTime(row["Date"]));
            query.AddParameter("LastModified", StringParser.ParseDateTime(row["LastModified"]));

            query.AddParameter("IsSynced", true);
            query.AddParameter("Description", string.Empty);

            query.AddParameter("HoursOfWork", Convert.ToDouble(row["HoursOfWork"]));
            query.AddParameter("Marking", row["Marking"] ?? string.Empty);
            query.AddParameter("Model", CatalogHelper.GetModelId<Models>(row["Model"]));
            query.AddParameter("Party", CatalogHelper.GetModelId<Party>(row["Party"]));
            query.AddParameter("Status", row["Status"]);
            query.AddParameter("TypeOfWarrantly", row["TypeOfWarrantly"]);


            query.AddParameter("Location", CatalogHelper.GetModelId<Contractors>(row["Location"]));
            query.AddParameter("Posted", Convert.ToBoolean(row["Posted"]));
            query.AddParameter("Number", Convert.ToInt32(row["Number"]));
            query.AddParameter("Responsible", Convert.ToInt64(row["Responsible"]));

            query.AddParameter("MarkForDeleting", row["MarkForDeleting"]);

            }

        protected abstract SqlCeCommand GetUpdateQuery(DataRow row);
        protected abstract SqlCeCommand GetInsertQuery(DataRow row);
        }
    }
