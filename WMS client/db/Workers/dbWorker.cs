using System;
using System.Data;
using System.Data.SqlServerCe;
using System.Collections.Generic;
using System.Diagnostics;
using WMS_client.Enums;
using System.Data.SqlTypes;

namespace WMS_client.db
    {
    /// <summary>Работник БД</summary>
    public static class dbWorker
        {
        #region Properties
        /// <summary>Путь к файлу БД</summary>
        private static string dbFilePath
            {
            get
                {
                if (string.IsNullOrEmpty(z_dbFilePath))
                    {
                    z_dbFilePath = System.IO.Path.GetDirectoryName(
                        System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", string.Empty) + @"\aramis_wms.sdf";
                    }

                return z_dbFilePath;
                }
            }
        private static string z_dbFilePath;

        /// <summary>Строка подключения</summary>
        private static string connString
            {
            get
                {
                if (string.IsNullOrEmpty(z_connString))
                    {
                    string filePath = dbFilePath;
                    string alternativeFilePath = @"\Storage Card\aramis_wms.sdf";
                    if (System.IO.File.Exists(alternativeFilePath))
                        {
                        //   filePath = alternativeFilePath;
                        }
                    z_connString = String.Format("Data Source='{0}';", filePath);
                    }

                return z_connString;
                }
            }
        private static string z_connString;

        /// <summary>Подключение к БД</summary>
        private static SqlCeConnection dBConnection
            {
            get
                {
                if (z_dBConnection == null)
                    {
                    SqlCeEngine DBEngine = new SqlCeEngine(connString);
                    z_dBConnection = new SqlCeConnection(DBEngine.LocalConnectionString);
                    try
                        {
                        z_dBConnection.Open();
                        }
                    catch (Exception exp)
                        {
                        Trace.WriteLine(exp.Message);
                        }
                    }

                return z_dBConnection;
                }
            }
        private static SqlCeConnection z_dBConnection;
        #endregion

        public static void Dispose()
            {
            if (dBConnection != null && dBConnection.State != ConnectionState.Closed)
                {
                dBConnection.Close();
                }
            }

        /// <summary>Новый запрос к БД</summary>
        /// <param name="command">Комманда</param>
        public static SqlCeCommand NewQuery(string command)
            {
            SqlCeCommand SQLCommand = dBConnection.CreateCommand();
            SQLCommand.CommandText = command;

            return SQLCommand;
            }

        #region AddParameters
        public static void AddParameters(this SqlCeCommand command, Dictionary<string, object> values)
            {
            if (values != null)
                {
                foreach (KeyValuePair<string, object> value in values)
                    {
                    if (value.Value is DateTime)
                        {
                        DateTime dateValue = (DateTime)value.Value;

                        if (dateValue < SqlDateTime.MinValue.Value)
                            {
                            dateValue = SqlDateTime.MinValue.Value;
                            }
                        else if (dateValue > SqlDateTime.MaxValue.Value)
                            {
                            dateValue = SqlDateTime.MaxValue.Value;
                            }

                        AddParameter(command, value.Key, dateValue);
                        }
                    else
                        {
                        AddParameter(command, value.Key, value.Value);
                        }
                    }
                }
            }

        public static void AddParameter(this SqlCeCommand command, string name, object value)
            {
            command.Parameters.Add(name, value);
            }

        public static void AddParameter(this SqlCeCommand command, string name, string value)
            {
            command.Parameters.Add(name, value ?? string.Empty);
            }
        #endregion

        #region Select
        /// <summary>Выбрать в таблицу</summary>
        /// <param name="command">Комманда</param>
        /// <returns>Таблица данных</returns>
        public static DataTable SelectToTable(this SqlCeCommand command)
            {
            return SelectToTable(command, new Dictionary<string, Enum>());
            }

        /// <summary>Выбрать в таблицу</summary>
        /// <param name="command">Комманда</param>
        /// <param name="formatDic">Словать форматирования данных</param>
        /// <returns>Таблица данных</returns>
        public static DataTable SelectToTable(this SqlCeCommand command, Dictionary<string, Enum> formatDic)
            {
            SqlCeDataReader reader = command.ExecuteReader();
            DataTable schemaTable = reader.GetSchemaTable();
            DataTable table = new DataTable();
            int index = 0;

            if (schemaTable != null)
                {
                foreach (DataRow row in schemaTable.Rows)
                    {
                    Type type = reader.GetFieldType(index++);

                    if (type == typeof(DateTime))
                        {
                        type = typeof(string);
                        }

                    DataColumn column = new DataColumn(row["ColumnName"].ToString(), type);
                    table.Columns.Add(column);
                    }

                while (reader.Read())
                    {
                    DataRow row = table.NewRow();

                    for (int i = 0; i < reader.FieldCount; i++)
                        {
                        Type type = reader[i].GetType();

                        switch (type.FullName)
                            {
                            case BaseFormatName.DBNull:
                                row[i] = string.Empty;
                                break;
                            case BaseFormatName.DateTime:
                                row[i] = GetDateTimeInFormat(reader.GetDateTime(i), formatDic);
                                break;
                            default:
                                row[i] = reader[i];
                                break;
                            }
                        }

                    table.Rows.Add(row);
                    }
                }

            return table;
            }

        /// <summary>Выбрать в список</summary>
        /// <param name="command">Комманда</param>
        /// <returns>Список</returns>
        public static List<object> SelectToList(this SqlCeCommand command)
            {
            return SelectToList(command, new Dictionary<string, Enum> { { BaseFormatName.DateTime, DateTimeFormat.None } });
            }

        /// <summary>Выбрать в список</summary>
        /// <param name="command">Комманда</param>
        /// <param name="formatDic">Словать форматирования данных</param>
        /// <returns>Список</returns>
        public static List<object> SelectToList(this SqlCeCommand command, Dictionary<string, Enum> formatDic)
            {
            List<object> list = new List<object>();
            SqlCeDataReader reader = command.ExecuteReader();

            while (reader.Read())
                {
                for (int i = 0; i < reader.FieldCount; i++)
                    {
                    Type type = reader[i].GetType();

                    switch (type.FullName)
                        {
                        case BaseFormatName.DBNull:
                            list.Add(string.Empty);
                            break;
                        case BaseFormatName.DateTime:
                            list.Add(GetDateTimeInFormat(reader.GetDateTime(i), formatDic));
                            break;
                        default:
                            list.Add(reader[i]);
                            break;
                        }
                    }
                }

            return list;
            }

        /// <summary>Выбрать в масив</summary>
        /// <param name="command">Комманда</param>
        /// <returns>Масив данных</returns>
        public static object[] SelectArray(this SqlCeCommand command)
            {
            return SelectArray(command, new Dictionary<string, Enum>());
            }

        /// <summary>Выбрать в масив</summary>
        /// <param name="command">Комманда</param>
        /// <param name="formatDic">Словать форматирования данных</param>
        /// <returns>Масив данных</returns>
        public static object[] SelectArray(this SqlCeCommand command, Dictionary<string, Enum> formatDic)
            {
            return SelectToList(command, formatDic).ToArray();
            }
        #endregion

        /// <summary>Получить Дата+Время в зданном формате</summary>
        /// <param name="dateTime"></param>
        /// <param name="formatDic"></param>
        /// <returns></returns>
        public static string GetDateTimeInFormat(DateTime dateTime, Dictionary<string, Enum> formatDic)
            {
            DateTimeFormat format = DateTimeFormat.None;

            if (formatDic.ContainsKey(BaseFormatName.DateTime))
                {
                format = (DateTimeFormat)formatDic[BaseFormatName.DateTime];
                }

            switch (format)
                {
                case DateTimeFormat.OnlyDate:
                    if (dateTime == DateTime.MinValue)
                        {
                        return "'Дата не вказана'";
                        }

                    return string.Format("{0:00}.{1:00}.{2}", dateTime.Day, dateTime.Month, dateTime.Year);
                case DateTimeFormat.OnlyTime:
                    return dateTime.ToShortTimeString();
                case DateTimeFormat.All:
                    return string.Format("{0:00}.{1:00}.{2} {3:00}:{4:00}:{5:00}",
                        dateTime.Day, dateTime.Month, dateTime.Year,
                        dateTime.Hour, dateTime.Minute, dateTime.Second);
                default:
                    return dateTime.ToString();
                }
            }
        }
    }