using System;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using WMS_client.Models;
using WMS_client.WinProcessesManagement;

namespace WMS_client.Repositories.Sql
    {
    public abstract class AccessoryUpdater<T> where T : IAccessory
        {
        private List<T> accessoriesList;
        private Func<SqlCeConnection> getSqlConnection;
        private readonly string tableName;
        private bool justInsert;
        private string tableIndexName;
        protected abstract void fillValues(SqlCeResultSet record, T accessory);
        protected abstract void fillValues(SqlCeUpdatableRecord record, T accessory);
        
        protected object getSqlDateTime(DateTime dateTime)
            {
            object result = DBNull.Value;
            if (!DateTime.MinValue.Equals(dateTime))
                {
                result = dateTime;
                }
            return result;
            }

        public AccessoryUpdater(string tableName, string tableIndexName)
            {
            this.tableName = tableName;
            this.tableIndexName = tableIndexName;
            }

        public bool JustInsert
            {
            get { return justInsert; }
            set { justInsert = value; }
            }

        public void InitUpdater(List<T> accessories, Func<SqlCeConnection> getSqlConnection)
            {
            this.accessoriesList = accessories;
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
                        foreach (var accessory in accessoriesList)
                            {
                            if (!justInsert && resultSet.Seek(DbSeekOptions.FirstEqual, accessory.Id))
                                {
                                resultSet.Read();
                                
                                fillValues(resultSet, accessory);

                                resultSet.Update();
                                }
                            else
                                {
                                var newRow = resultSet.CreateRecord();
                                
                                fillValues(newRow, accessory);

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
