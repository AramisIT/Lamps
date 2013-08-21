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
    abstract class AccessoryUpdater<T> : TableUpdater<T> where T : IAccessory
        {
        private readonly string logTableIndexName;
        private readonly string logTableName;

        private bool justInsert;
        private bool don_tAddNewToLog;
       
        public AccessoryUpdater(string tableName, string tableIndexName, string logTableName,
            string logTableIndexName)
            {
            this.tableName = tableName;
            this.tableIndexName = tableIndexName;
            this.logTableName = logTableName;
            this.logTableIndexName = logTableIndexName;
            }

        public bool JustInsert
            {
            get { return justInsert; }
            set { justInsert = value; }
            }

        public bool Don_tAddNewToLog
            {
            get { return don_tAddNewToLog; }
            set { don_tAddNewToLog = value; }
            }


        private int lastUploadedToGreenhouseId;
        private int minAccessoryIdForCurrentPdt;
        private int maxAccessoryIdForCurrentPdt;

        public int LastUploadedToGreenhouseId
            {
            get { return lastUploadedToGreenhouseId; }
            set { lastUploadedToGreenhouseId = value; }
            }

        public int MinAccessoryIdForCurrentPdt
            {
            get { return minAccessoryIdForCurrentPdt; }
            set { minAccessoryIdForCurrentPdt = value; }
            }

        public int MaxAccessoryIdForCurrentPdt
            {
            get { return maxAccessoryIdForCurrentPdt; }
            set { maxAccessoryIdForCurrentPdt = value; }
            }

        public bool LoadingDataFromGreenhouse { get; set; }

        public void InitUpdater(List<T> accessories, Func<SqlCeConnection> getSqlConnection)
            {
            this.itemsList = accessories;
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
                        foreach (var accessory in itemsList)
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

            return LoadingDataFromGreenhouse ? true : writeToUpdateLog();
            }

        private bool writeToUpdateLog()
            {
            try
                {
                using (var conn = getSqlConnection())
                    {
                    using (var cmd = conn.CreateCommand())
                        {
                        cmd.CommandType = System.Data.CommandType.TableDirect;
                        cmd.CommandText = logTableName;
                        cmd.IndexName = logTableIndexName;

                        using (var resultSet = cmd.ExecuteResultSet(SqlCeRepository.UPDATABLE_RESULT_SET_OPTIONS))
                            {
                            foreach (var accessory in itemsList)
                                {
                                int currentId = accessory.Id;

                                if (don_tAddNewToLog)
                                    {
                                    bool idCreatedOnThisPDT = minAccessoryIdForCurrentPdt <= currentId &&
                                                              currentId <= maxAccessoryIdForCurrentPdt;

                                    bool accessoryIsNotExistsInGreenhouse = idCreatedOnThisPDT &&
                                                                            currentId > lastUploadedToGreenhouseId;
                                    if (accessoryIsNotExistsInGreenhouse)
                                        {
                                        // this accessory will be uploaded even without log
                                        continue;
                                        }
                                    }

                                if (justInsert || !resultSet.Seek(DbSeekOptions.FirstEqual, currentId))
                                    {
                                    var newRow = resultSet.CreateRecord();
                                    newRow.SetInt32(0, currentId);
                                    resultSet.Insert(newRow);
                                    }
                                }
                            }
                        }
                    }
                }
            catch (Exception exp)
                {
                Trace.WriteLine(string.Format("Ошибка при записи в лог: {0}", exp.Message));
                return false;
                }

            return true;
            }
        }
    }
