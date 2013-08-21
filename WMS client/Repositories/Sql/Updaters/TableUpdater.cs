using System;
using System.Data.SqlServerCe;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace WMS_client.Repositories.Sql.Updaters
    {
    abstract class TableUpdater<T>
        {
        protected List<T> itemsList;
        protected Func<SqlCeConnection> getSqlConnection;

        protected string tableName;
        protected string tableIndexName;

        protected abstract void fillValues(SqlCeResultSet record, T item);
        protected abstract void fillValues(SqlCeUpdatableRecord record, T item);

        protected object getSqlDateTime(DateTime dateTime)
            {
            object result = DBNull.Value;
            if (!DateTime.MinValue.Equals(dateTime))
                {
                result = dateTime;
                }
            return result;
            }
        }
    }
