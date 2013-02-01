using System;
using System.Data.SqlServerCe;
using WMS_client.Enums;

namespace WMS_client.db
{
    /// <summary>Штрих код рабочий</summary>
    public static class BarcodeWorker
    {
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
            SqlCeCommand query = dbWorker.NewQuery(string.Format(ACCESSORY_QUERY_COMMAND, "Id"));
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
            SqlCeCommand query = dbWorker.NewQuery(string.Format(DEFAULT_QUERY_COMMAND, "Id", type.Name));
            query.AddParameter("Barcode", barcode);
            object result = query.ExecuteScalar();

            return result ?? 0;
        }
    }
}