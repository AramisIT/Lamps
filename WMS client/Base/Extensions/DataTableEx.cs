using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace System
{
    static public class DataTableEx
    {
        public static DataRow FindRowInTable(DataTable Table, DataRow Row, string ColumnsNames, ref int StartIndex)
        {
            if (Table == null || Row == null) return null;
                        
            List<string> Columns = GetColumnsList(ColumnsNames, Row.Table);
            
            #region Поиск строки которая равна по указанным полям строке "Row"
            
            for (int i = StartIndex; i < Table.Rows.Count; i++)
            {
                DataRow dr = Table.Rows[i];
            
                bool equal = true;

                foreach (string ColName in Columns)
                {
                    equal = equal && IsFieldsEqual(Row, dr, ColName);

                    if (!equal)
                    {   // Нет смысла далее перебирать столбцы уже есть различия
                        break;
                    }
                }

                if (equal)
                {   // Найдено совпадение
                    StartIndex = i;
                    return dr;
                }
            } 
            
            #endregion

            return null;
        }

        public static DataRow FindRowInTable(DataTable table, object value, string columnName)
        {
            foreach (DataRow dr in table.Rows)
            {
                if (Convert.ToString(dr[columnName]) == value.ToString())
                    return dr;
            }

            return null;
        }

        private static List<string> GetColumnsList(string ColumnsNames, DataTable Table)
        {
            // Создание списка имен столбцов

            List<string> Columns = new List<string>();

            if (ColumnsNames.Trim() == "")
            {
                foreach (DataColumn dc in Table.Columns)
                {
                    Columns.Add(dc.ColumnName);
                }
            }
            else
            {
                int index = -1;
                do
                {
                    int startIndex = index + 1;
                    index = ColumnsNames.IndexOf(',', startIndex);
                    string ColumnName = (index == -1 ? ColumnsNames.Substring(startIndex) : ColumnsNames.Substring(startIndex, index - startIndex)).Trim();
                    Columns.Add(ColumnName);
                    if (Table.Columns.IndexOf(ColumnName) == -1)
                    {
                        throw new Exception(string.Format("Accepted Row isn't contain field \"{0}\"", ColumnName));
                    }
                } while (index != -1);
            } 

            return Columns;
        }

        private static bool IsFieldsEqual(DataRow Row1, DataRow Row2, string ColName)
        {
            Type FType = Row1.Table.Columns[ColName].DataType;
            if (FType != Row2.Table.Columns[ColName].DataType)
            {
                return false;
            }

            if (FType == typeof(double))
                return ((double)(Row1[ColName])) == ((double)(Row2[ColName]));

            if (FType == typeof(string))
                return (Row1[ColName] as string) == (Row2[ColName] as string);
            
            if (FType == typeof(bool))
                return ((bool)(Row1[ColName])) == ((bool)(Row2[ColName]));

            if (FType == typeof(Int32))
                return ((Int32)(Row1[ColName])) == ((Int32)(Row2[ColName]));

            if (FType == typeof(Int64))
                return ((Int64)(Row1[ColName])) == ((Int64)(Row2[ColName]));            
           
            return Row1[ColName] == Row2[ColName];
        }

        public static void RemoveColumns(DataTable Table, string ColumnsNames)
        {
            List<string> Columns = GetColumnsList(ColumnsNames, Table);
            foreach (string ColName in Columns)
            {
                Table.Columns.Remove(ColName);
            }
        }
    
        public static void CopyRow(DataRow WritableRow, DataRow SourceRow)
        {
            foreach (DataColumn Column in SourceRow.Table.Columns)
            {
                WritableRow[Column.ColumnName] = SourceRow[Column.ColumnName];
            }
        }

        public static void AddRowCopy(DataTable WritableTable, DataRow SourceRow)
        {
            var NewRow = WritableTable.NewRow();
            DataTableEx.CopyRow(NewRow, SourceRow);
            WritableTable.Rows.Add(NewRow);
        }
    }
}
