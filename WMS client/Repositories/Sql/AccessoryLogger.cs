using System;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using WMS_client.Models;
using WMS_client.WinProcessesManagement;

namespace WMS_client.Repositories
    {
    class AccessoryLogger<T> : SqlCeResultSet where T : IAccessory
        {
        private Func<SqlCeConnection> getSqlConnection;
        private List<T> accessotyList;
        private string tableName;

        public AccessoryLogger(string tableName, List<T> accessotyList, Func<SqlCeConnection> getSqlConnection)
            {
            this.tableName = tableName;
            this.accessotyList = accessotyList;
            this.getSqlConnection = getSqlConnection;
            }

        internal bool Log()
            {
            if (accessotyList.Count == 0)
                {
                return true;
                }

            bool isCase = accessotyList[0] is Case;

            try
                {
                using (var conn = getSqlConnection())
                    {
                    using (var sqlCeSelectCommand = conn.CreateCommand())
                        {
                        sqlCeSelectCommand.CommandText = tableName;
                        sqlCeSelectCommand.CommandType = System.Data.CommandType.TableDirect;
                        sqlCeSelectCommand.ExecuteResultSet(SqlCeRepository.RESULT_SET_OPTIONS, this);

                        foreach (var accessory in accessotyList)
                            {
                            var newRow = CreateRecord();

                            newRow["Id"] = accessory.Id;

                            if (isCase)
                                {
                                newRow["New"] = true;
                                }

                            Insert(newRow);
                            }
                        }
                    }
                }
            catch (Exception exp)
                {
                Debug.WriteLine(string.Format("Ошибка вставки в базу - {0}", exp.Message));
                return false;
                }
            finally
                {
                Close();
                }
            return true;
            }
        }
    }
