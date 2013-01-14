using System;
using System.Data.SqlServerCe;
using WMS_client.Enums;

namespace WMS_client.db
{
    /// <summary>����� ��� �������</summary>
    public static class BarcodeWorker
    {
        /// <summary>������ ��� ����������� ���� �������������� � Id ��������� �� ��������� (��� ������� ������ ��������� � �������)</summary>
        private const string ACCESSORY_QUERY_COMMAND = @"
SELECT {0}
FROM(
    SELECT 1 Type, Id FROM Lamps WHERE RTRIM(BarCode)=RTRIM(@BarCode) AND MarkForDeleting=0
    UNION 
    SELECT 2 Type, Id FROM Cases WHERE RTRIM(BarCode)=RTRIM(@BarCode) AND MarkForDeleting=0
    UNION 
    SELECT 3 Type, Id FROM ElectronicUnits WHERE RTRIM(BarCode)=RTRIM(@BarCode) AND MarkForDeleting=0)t";
        private const string DEFAULT_QUERY_COMMAND = @"SELECT {0} FROM {1} WHERE RTRIM(BarCode)=RTRIM(@BarCode)";

        /// <summary>�������� ��� �������������� �� �����-����</summary>
        /// <param name="barcode">�����-���</param>
        /// <returns>��� ��������������</returns>
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

        /// <summary>�� ��� ���� ��������?</summary>
        /// <param name="barcode">��������</param>
        public static bool IsBarcodeExist(string barcode)
        {
            SqlCeCommand query = dbWorker.NewQuery(string.Format(ACCESSORY_QUERY_COMMAND, "1"));
            query.AddParameter("Barcode", barcode);
            object result = query.ExecuteScalar();

            return result != null;
        }

        /// <summary>�� ��� ���� ��������?</summary>
        /// <param name="barcode">��������</param>
        public static object GetIdByBarcode(object barcode)
        {
            return GetIdByBarcode(barcode.ToString());
        }

        /// <summary>�� ��� ���� ��������?</summary>
        /// <param name="barcode">��������</param>
        public static object GetIdByBarcode(string barcode)
        {
            SqlCeCommand query = dbWorker.NewQuery(string.Format(ACCESSORY_QUERY_COMMAND, "Id"));
            query.AddParameter("Barcode", barcode);
            object result = query.ExecuteScalar();

            return result ?? 0;
        }

        /// <summary>�� ��� ���� ��������?</summary>
        /// <param name="type"> </param>
        /// <param name="barcode">��������</param>
        public static object GetIdByBarcode(Type type, string barcode)
        {
            SqlCeCommand query = dbWorker.NewQuery(string.Format(DEFAULT_QUERY_COMMAND, "Id", type.Name));
            query.AddParameter("Barcode", barcode);
            object result = query.ExecuteScalar();

            return result ?? 0;
        }
    }
}