using System;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace TestSQL
    {
    public class TestSqlClass : SqlCeResultSet
        {
        private Stopwatch stopWatch;
        public long TimeSec
            {
            get
                {
                return (long)(stopWatch.ElapsedMilliseconds * 0.001);
                }
            }

        public long TimeMiliSec
            {
            get
                {
                return (long)(stopWatch.ElapsedMilliseconds);
                }
            }

        private string fileName;

        public TestSqlClass(string fileName)
            {
            this.fileName = fileName;
            }

        public void Test()
            {
            // TestSQL();

            TestSQLResultSet();
            }

        private System.Data.SqlServerCe.ResultSetOptions resultSetOptions;

        protected void InitializeResultSetOptions()
            {
            resultSetOptions = System.Data.SqlServerCe.ResultSetOptions.Scrollable;
            resultSetOptions = (resultSetOptions | System.Data.SqlServerCe.ResultSetOptions.Sensitive);
            resultSetOptions = (resultSetOptions | System.Data.SqlServerCe.ResultSetOptions.Updatable);
            }

        private const int MaxId = 1000;
        DateTime now = DateTime.Now;

        private void TestSQLResultSet()
            {
            stopWatch = new Stopwatch();
            stopWatch.Start();

            DBEngine = new SqlCeEngine(String.Format("Data Source='{0}';", dbFilePath));

            InitializeResultSetOptions();

            using (var conn = new SqlCeConnection(DBEngine.LocalConnectionString))
                {
                conn.Open();

                using (var sqlCeSelectCommand = conn.CreateCommand())
                    {
                    sqlCeSelectCommand.CommandText = "Lamps";
                    sqlCeSelectCommand.CommandType = System.Data.CommandType.TableDirect;

                    sqlCeSelectCommand.ExecuteResultSet(resultSetOptions, this);
                    for (int id = MaxId; id > 0; id--)
                        {
                        System.Data.SqlServerCe.SqlCeUpdatableRecord newRecord = base.CreateRecord();

                        newRecord["Id"] = 1;
                        newRecord["Date"] = now;
                        newRecord["Comment"] = "dfgdfg";

                        try
                            {
                            base.Insert(newRecord);
                            }
                        catch (Exception exp)
                            {
                            Debug.WriteLine(exp.Message);
                            }
                        }

                    Close();
                    }




                }

            stopWatch.Stop();
            }

        private void TestSQL()
            {
            stopWatch = new Stopwatch();
            stopWatch.Start();

            DBEngine = new SqlCeEngine(String.Format("Data Source='{0}';", dbFilePath));

            for (int id = 1; id <= MaxId; id++)
                {
                InserRow(id);
                //if (id % 1000 == 0)
                //    {
                //    Trace.WriteLine(string.Format("current id = {0}; secs = {1}", id, (long)(stopWatch.ElapsedMilliseconds * 0.001)));
                //    }
                }


            }

        private void InserRow(int id)
            {
            //using (var sw = System.IO.File.CreateText(string.Format("{0}R_{1}.txt", newFilePath, id)))
            //    {
            //    sw.WriteLine(string.Format("{0} - {1} ; some text", id, DateTime.Now));
            //    sw.Close();
            //    }
            //return;
            const string sql = @"INSERT INTO Lamps (Id, [Date],[Comment])
                        VALUES(@Id, @now,' some text')";



            using (var z_dBConnection = new SqlCeConnection(DBEngine.LocalConnectionString))
                {
                z_dBConnection.Open();

                using (SqlCeCommand query = dBConnection.CreateCommand())
                    {
                    query.CommandText = sql;

                    query.Parameters.AddWithValue("@Id", id);
                    query.Parameters.AddWithValue("@now", now);

                    try
                        {
                        var rowsAffected = query.ExecuteNonQuery();
                        //var t = rowsAffected + 100;
                        }
                    catch (Exception exp)
                        {
                        Trace.WriteLine(string.Format("Insert exception: {0}", exp.Message));
                        }
                    }
                }

            }

        public SqlCeCommand NewCommand(string command)
            {
            SqlCeCommand SQLCommand = dBConnection.CreateCommand();
            SQLCommand.CommandText = command;

            return SQLCommand;
            }

        private SqlCeConnection dBConnection
            {
            get
                {
                if (z_dBConnection == null)
                    {
                    SqlCeEngine DBEngine = new SqlCeEngine(String.Format("Data Source='{0}';", dbFilePath));
                    z_dBConnection = new SqlCeConnection(DBEngine.LocalConnectionString);
                    z_dBConnection.Open();
                    }

                return z_dBConnection;
                }
            }
        private SqlCeConnection z_dBConnection;

        private string dbFilePath
            {
            get
                {
                if (string.IsNullOrEmpty(z_dbFilePath))
                    {
                    if (string.IsNullOrEmpty(fileName))
                        {
                        z_dbFilePath = //@"\SD-MMCard\SqlSpeedTest.sdf";
                        System.IO.Path.GetDirectoryName(
                        System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)
                        .Replace("file:\\", string.Empty) + @"\SqlSpeedTest.sdf";
                        }
                    else
                        {
                        z_dbFilePath = fileName;
                        }
                    }

                return z_dbFilePath;
                }
            }
        private string z_dbFilePath;

        private string newFilePath
            {
            get
                {
                if (string.IsNullOrEmpty(z_newFilePath))
                    {
                    z_newFilePath = System.IO.Path.GetDirectoryName(
                        System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", string.Empty) + @"\";
                    }

                return z_newFilePath;
                }
            }
        private string z_newFilePath;
        private SqlCeEngine DBEngine;
        }
    }
