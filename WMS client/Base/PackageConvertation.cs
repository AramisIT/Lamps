using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;

namespace WMS_client
{
    public class PackageConvertation
    {
        private const string PACKAGE_HEADER = "$T@RT";
        private const string PACKAGE_FOOTER = "#END>";
        private const string PACKAGE_SEPARATOR = "\t";

        private static string GetStrPatameter(object Parameter)
        {
            if (Parameter == null)
            {
                return "5";
            }

            Type ParamType = Parameter.GetType();

            if (ParamType == Type.GetType("System.Boolean"))
            {
                return "1" + ((bool)Parameter ? "1" : "0");
            }

            if (ParamType == typeof(UInt64) ||
                ParamType == typeof(Int64) ||
                ParamType == typeof(UInt32) ||
                ParamType == typeof(Int32) ||
                ParamType == typeof(UInt16) ||
                ParamType == typeof(Int16) ||
                ParamType == typeof(double))
            {
                return "2" + Parameter.ToString();
            }

            if (ParamType == Type.GetType("System.String"))
            {
                return "3" + Parameter.ToString();
            }

            if (Parameter is System.Data.DataTable)
            {
                return "4" + CreateTable(Parameter as DataTable);
            }

            return "3" + Parameter.ToString();
        }

        private static object GetPatameterFromStr(String Parameter)
        {
            if (Parameter[0] == '1')
            {
                return Parameter[1] == '1';
            }

            if (Parameter[0] == '2')
            {
                string value = Parameter.Substring(1, Parameter.Length - 1);
                if (value.IndexOf('.') == -1 && value.IndexOf(',') == -1)
                {
                    return Convert.ToInt32(value);
                }
                else
                {
                    return Convert.ToDouble(value);
                }
            }

            if (Parameter[0] == '3')
            {
                return Parameter.Substring(1, Parameter.Length - 1).Replace('$', '\r');
            }

            if (Parameter[0] == '4')
            {
                return GetTable(Parameter.Substring(1, Parameter.Length - 1));
            }

            return null;
        }

        private static string CreateTable(DataTable Table)
        {
            StringBuilder StringTable = new StringBuilder();

            foreach (DataColumn column in Table.Columns)
            {
                string StrType = "3";
                Type ParamType = column.DataType;

                if (ParamType == Type.GetType("System.Boolean"))
                {
                    StrType = "1";
                }

                if (ParamType == typeof(UInt64) ||
                    ParamType == typeof(Int64) ||
                    ParamType == typeof(UInt32) ||
                    ParamType == typeof(Int32) ||
                    ParamType == typeof(UInt16) ||
                    ParamType == typeof(Int16) ||
                    ParamType == typeof(double))
                {
                    StrType = "2";
                }


                //if (ParamType == Type.GetType("System.Int32") || ParamType == Type.GetType("System.Int16") || ParamType == Type.GetType("System.Double"))
                //{
                //    StrType = "2";
                //}

                if (StringTable.Length > 0)
                {
                    StringTable.Append("|" + StrType + column.ColumnName);
                }
                else
                {
                    StringTable.Append(StrType + column.ColumnName);
                }
            }

            foreach (DataRow row in Table.Rows)
            {
                StringTable.Append("$");

                for (int i = 0; i < Table.Columns.Count; i++)
                {
                    StringTable.Append((i == 0 ? "" : "|") + row[i]);
                }
            }


            return StringTable.ToString();
        }

        private static DataTable GetTable(string strTable)
        {
            string[] tableLines = SeparateStrings(strTable, "$");

            DataTable Table = new DataTable("Mobile");

            #region Creating columns

            string[] headers = SeparateStrings(tableLines[0], "|");

            foreach (string header in headers)
            {
                string HeaderName = header.Substring(1, header.Length - 1);
                if (header[0] == '1')
                {
                    Table.Columns.Add(HeaderName, Type.GetType("System.Boolean"));
                }
                if (header[0] == '2')
                {
                    Table.Columns.Add(HeaderName, Type.GetType("System.Double"));
                }
                if (header[0] == '3')
                {
                    Table.Columns.Add(HeaderName, Type.GetType("System.String"));
                }
            }

            #endregion

            #region Creating rows
            for (int i = 1; i < tableLines.Length; i++)
            {
                string[] fields = SeparateStrings(tableLines[i], "|");
                DataRow Row = Table.NewRow();
                string Value;
                for (int j = 0; j < fields.Length; j++)
                {
                    if (Table.Columns[j].DataType == Type.GetType("System.Double"))
                    {
                        Value = fields[j].Replace(',', '.');
                    }
                    else
                    {
                        Value = fields[j];
                    }

                    Row[j] = Value;
                }
                Table.Rows.Add(Row);
            }
            #endregion

            return Table;
        }

        private static string[] SeparateStrings(string str, string separator)
        {
            List<string> result = new List<string>();

            while (str.Length > 0)
            {
                int pos = str.IndexOf(separator);
                if (pos == -1)
                {
                    result.Add(str);
                    break;
                }
                else
                {
                    result.Add(str.Substring(0, pos));
                    str = str.Substring(pos + separator.Length, str.Length - pos - separator.Length);
                }
            }
            return result.ToArray();
        }

        public static object[] GetPatametersFromStr(String Parameters)
        {
            List<object> results = new List<object>();
            string[] parameters = SeparateStrings(Parameters, "\r");

            foreach (string param in parameters)
            {
                results.Add(GetPatameterFromStr(param));
            }

            return results.ToArray();
        }

        public static string GetStrPatametersFromArray(params object[] Parameters)
        {
            StringBuilder ParametersStr = new StringBuilder();
            bool FirstTime = true;
            foreach (object param in Parameters)
            {
                if (FirstTime)
                {
                    FirstTime = false;
                    ParametersStr.Append(GetStrPatameter(param));
                }
                else
                {
                    ParametersStr.Append("\r" + GetStrPatameter(param));
                }

            }
            return ParametersStr.ToString();
        }

        public static Byte[] getPackage(string QueryName, string UserCode, string Parameters)
        {
            string PackageID = generateID();

            string packageResult = PACKAGE_HEADER + "F" + PackageID + UserCode + PACKAGE_SEPARATOR +
                QueryName + PACKAGE_SEPARATOR + Parameters + PACKAGE_FOOTER;

            return Encoding.GetEncoding(1251).GetBytes(packageResult);

        }

        private static string generateID()
        {
            StringBuilder sb = new StringBuilder();
            Random rand = new Random();

            for (int i = 0; i < 8; i++)
            {
                int randValue = rand.Next(36);
                // Next string creates a symbol from this array - {"0","1",...,"9","A","B",..."Z"}
                byte[] bytes = new byte[] { (byte)(randValue + 55) };
                sb.Append((randValue < 10) ? randValue.ToString() : System.Text.Encoding.GetEncoding(1251).GetString(bytes, 0, bytes.Length));
            }
            return sb.ToString();
        }


    }
}
