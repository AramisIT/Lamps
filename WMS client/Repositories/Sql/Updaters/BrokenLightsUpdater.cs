using System;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using WMS_client.Models;

namespace WMS_client.Repositories.Sql.Updaters
    {
    class BrokenLightsUpdater : TableUpdater<BrokenLightsRecord>
        {
        public BrokenLightsUpdater(BrokenLightsRecord brokenLightsRecord, Func<SqlCeConnection> getSqlConnection)
            {
            this.itemsList = new List<BrokenLightsRecord>() { brokenLightsRecord };
            this.getSqlConnection = getSqlConnection;

            tableName = "BrokenLights";
            tableIndexName = "BrokenLights_Register";
            }

        protected override void fillValues(System.Data.SqlServerCe.SqlCeResultSet record, BrokenLightsRecord item)
            {
            record.SetInt32(BrokenLightsTable.Map, item.Map);
            record.SetInt16(BrokenLightsTable.Register, item.RegisterNumber);
            record.SetByte(BrokenLightsTable.Amount, item.Amount);
            }

        protected override void fillValues(System.Data.SqlServerCe.SqlCeUpdatableRecord record, BrokenLightsRecord item)
            {
            record.SetInt32(BrokenLightsTable.Map, item.Map);
            record.SetInt16(BrokenLightsTable.Register, item.RegisterNumber);
            record.SetByte(BrokenLightsTable.Amount, item.Amount);
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
                        foreach (var item in itemsList)
                            {
                            if (!updateItem(resultSet, item)) return false;
                            }
                        }
                    }
                }

            return true;
            }

        private bool updateItem(SqlCeResultSet resultSet, BrokenLightsRecord item)
            {
            bool recordFound = resultSet.Seek(DbSeekOptions.FirstEqual, item.RegisterNumber, item.Map);
            bool recordMustExist = item.Amount > 0;

            if (recordMustExist)
                {
                if (recordFound)
                    {
                    fillValues(resultSet, item);
                    resultSet.Read();
                    resultSet.Update();
                    }
                else
                    {
                    var newRow = resultSet.CreateRecord();

                    fillValues(newRow, item);

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
            else
                {
                if (recordFound)
                    {
                    resultSet.Read();
                    resultSet.Delete();
                    }
                }

            return true;
            }

        //private bool seekToRecord(SqlCeResultSet resultSet, BrokenLightsRecord item)
        //    {

        //    resultSet.Seek(DbSeekOptions.FirstEqual, item.RegisterNumber, item.Map );
        //    }
        }
    }
