using System;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using WMS_client.Models;
using WMS_client.WinProcessesManagement;

namespace WMS_client.Repositories.Sql.Updaters
    {
    abstract class CatalogUpdater<T, ID> : TableUpdater<T> where T : ICatalog<ID>
        {
        public CatalogUpdater(string tableName, string tableIndexName)
            {
            this.tableName = tableName;
            this.tableIndexName = tableIndexName;
            }

        public void InitUpdater(List<T> catalogs, Func<SqlCeConnection> getSqlConnection)
            {
            this.itemsList = catalogs;
            this.getSqlConnection = getSqlConnection;
            }

        public bool Update()
            {
            using (var conn = getSqlConnection())
                {
                using (var cmd = conn.CreateCommand())
                    {
                    cmd.CommandType = System.Data.CommandType.TableDirect;
                    cmd.CommandText = tableName;
                    cmd.IndexName = tableIndexName;

                    using (var resultSet = cmd.ExecuteResultSet(SqlCeRepository.UPDATABLE_RESULT_SET_OPTIONS))
                        {
                        foreach (var catalog in itemsList)
                            {
                            bool recordFound = resultSet.Seek(DbSeekOptions.FirstEqual, catalog.Id);

                            if (catalog.Deleted)
                                {
                                if (recordFound)
                                    {
                                    resultSet.Read();
                                    resultSet.Delete();
                                    }
                                continue;
                                }

                            if (recordFound)
                                {
                                resultSet.Read();

                                fillValues(resultSet, catalog);

                                resultSet.Update();
                                }
                            else
                                {
                                var newRow = resultSet.CreateRecord();

                                fillValues(newRow, catalog);

                                try
                                    {
                                    resultSet.Insert(newRow);
                                    }
                                catch (Exception exp)
                                    {
                                    Trace.WriteLine(string.Format("Ошибка вставки записи: {0}", exp.Message));
                                    return false;
                                    }
                                }
                            }
                        }
                    }
                }

            return true;
            }

        }
    }
