using System;
using System.Data.SqlServerCe;
using WMS_client.Enums;

namespace WMS_client.db
    {
    /// <summary>Штрих код рабочий</summary>
    public static class BarcodeWorker
        {
        const char POSITION_SEPARATOR = '.';
        /// <summary>Запрос для определения типа комплектующего и Id документа по штрихкоду (или наличия такого документа в системе)</summary>
        private const string ACCESSORY_QUERY_COMMAND = @"
SELECT {0}
FROM(
    SELECT 1 Type, Id FROM Lamps WHERE RTRIM(BarCode)=RTRIM(@BarCode) AND MarkForDeleting=0
    UNION 
    SELECT 2 Type, Id FROM Cases WHERE RTRIM(BarCode)=RTRIM(@BarCode) AND MarkForDeleting=0
    UNION 
    SELECT 3 Type, Id FROM ElectronicUnits WHERE RTRIM(BarCode)=RTRIM(@BarCode) AND MarkForDeleting=0)t";
        private const string DEFAULT_QUERY_COMMAND = @"SELECT {0} FROM {1} WHERE RTRIM(BarCode)=RTRIM(@BarCode)";

        /// <summary>Чи являється строка валідним штрихкодом комплектуючого</summary>
        /// <param name="barcode">Строка</param>
        public static bool IsValidBarcode(this string barcode)
            {
            string trimBarcode = barcode.Trim();
            return trimBarcode.Length == 0 || trimBarcode[0] == 'L';
            }

        /// <summary>Чи являється строка валідним штрих-кодом позиції</summary>
        /// <param name="barcode">Штрих-код</param>
        public static bool IsValidPositionBarcode(this string barcode)
            {
            string trimBarcode = barcode.Trim();
            return trimBarcode.Length > 2
                    && trimBarcode[0] == 'P'
                    && trimBarcode[1] == '_'
                    && trimBarcode.Substring(2, trimBarcode.Length - 2).Split(POSITION_SEPARATOR).Length == 3;
            }

        /// <summary>Отримати дані позиції розміщення зі штрих-коду</summary>
        /// <param name="barcode">Штрих-код</param>
        /// <param name="map">Id карти</param>
        /// <param name="register">№ регістру</param>
        /// <param name="position">№ позиції</param>
        /// <returns>Чи були отримані данні з штрих-коду</returns>
        public static bool GetPositionData(this string barcode, out long map, out int register, out int position)
            {
            string[] parts = barcode.Substring(2, barcode.Length - 2).Split(POSITION_SEPARATOR);

            if (parts.Length == 3)
                {
                try
                    {
                    map = Convert.ToInt64(parts[0]);
                    register = Convert.ToInt32(parts[1]);
                    position = Convert.ToInt32(parts[2]);
                    return true;
                    }
                catch (Exception exc)
                    {
                    Console.Write(exc.Message);
                    }
                }

            map = 0;
            register = 0;
            position = 0;
            return false;
            }

        /// <summary>Получить тип комплектующего со штрих-кода</summary>
        /// <param name="barcode">Штрих-код</param>
        /// <returns>Тип комплектующего</returns>
        public static TypeOfAccessories GetTypeOfAccessoriesByBarcode(string barcode)
            {
            SqlCeCommand query = dbWorker.NewQuery(string.Format(ACCESSORY_QUERY_COMMAND, "Type"));
            query.AddParameter("Barcode", barcode);
            object result = query.ExecuteScalar();

            if (result != null)
                {
                TypeOfAccessories type = (TypeOfAccessories)Convert.ToInt32(result);

                return type;
                }

            return TypeOfAccessories.None;
            }

        /// <summary>Чи вже існує штрихкод?</summary>
        /// <param name="barcode">Штрихкод</param>
        public static bool IsBarcodeExist(string barcode)
            {
            SqlCeCommand query = dbWorker.NewQuery(string.Format(ACCESSORY_QUERY_COMMAND, "1"));
            query.AddParameter("Barcode", barcode);
            object result = query.ExecuteScalar();

            return result != null;
            }

        /// <summary>Чи існує посилання</summary>
        /// <param name="type">Тип об'єкту</param>
        /// <param name="syncRef">Посилання</param>
        public static bool IsRefExist(Type type, string syncRef)
            {
            string command = string.Format(@"SELECT 1 FROM {0} WHERE RTRIM({1})=RTRIM(@{2})",
                                           type.Name,
                                           dbObject.SYNCREF_NAME,
                                           dbSynchronizer.PARAMETER);
            SqlCeCommand query = dbWorker.NewQuery(command);
            query.AddParameter(dbSynchronizer.PARAMETER, syncRef);
            object result = query.ExecuteScalar();

            return result != null;
            }

        /// <summary>Чи вже існує штрихкод?</summary>
        /// <param name="barcode">Штрихкод</param>
        public static object GetIdByBarcode(object barcode)
            {
            return GetIdByBarcode(barcode.ToString());
            }

        /// <summary>Чи вже існує штрихкод?</summary>
        /// <param name="barcode">Штрихкод</param>
        public static object GetIdByBarcode(string barcode)
            {
            SqlCeCommand query = dbWorker.NewQuery(string.Format(ACCESSORY_QUERY_COMMAND, dbObject.IDENTIFIER_NAME));
            query.AddParameter("Barcode", barcode);
            object result = query.ExecuteScalar();

            return result ?? 0;
            }

        /// <summary>Чи вже існує штрихкод?</summary>
        /// <param name="type"> </param>
        /// <param name="syncRef">Штрихкод</param>
        public static object GetIdByRef(Type type, string syncRef)
            {
            string command = string.Format(@"SELECT {0} FROM {1} WHERE RTRIM({2})=RTRIM(@{3})",
                                           dbObject.IDENTIFIER_NAME,
                                           type.Name,
                                           dbObject.SYNCREF_NAME,
                                           dbSynchronizer.PARAMETER);
            SqlCeCommand query = dbWorker.NewQuery(command);
            query.AddParameter(dbSynchronizer.PARAMETER, syncRef);
            object result = query.ExecuteScalar();

            return result ?? 0;
            }

        /// <summary>Чи вже існує штрихкод?</summary>
        /// <param name="type"> </param>
        /// <param name="barcode">Штрихкод</param>
        public static object GetIdByBarcode(Type type, string barcode)
            {
            SqlCeCommand query = dbWorker.NewQuery(string.Format(DEFAULT_QUERY_COMMAND, dbObject.IDENTIFIER_NAME, type.Name));
            query.AddParameter("Barcode", barcode);
            object result = query.ExecuteScalar();

            return result ?? 0;
            }

        /// <summary>Чи вже існує штрихкод?</summary>
        /// <param name="type"> </param>
        /// <param name="barcode">Штрихкод</param>
        public static string GetRefByBarcode(Type type, string barcode)
            {
            return GetRefByBarcode(type.Name, barcode);
            }

        /// <summary>Чи вже існує штрихкод?</summary>
        /// <param name="tableName"> </param>
        /// <param name="barcode">Штрихкод</param>
        public static string GetRefByBarcode(string tableName, string barcode)
            {
            SqlCeCommand query = dbWorker.NewQuery(string.Format(DEFAULT_QUERY_COMMAND, dbObject.SYNCREF_NAME, tableName));
            query.AddParameter("Barcode", barcode);
            object result = query.ExecuteScalar();

            return result == null ? string.Empty : result.ToString();
            }
        }
    }