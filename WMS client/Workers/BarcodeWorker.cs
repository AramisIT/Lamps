using System;
using System.Data.SqlServerCe;
using WMS_client.Enums;

namespace WMS_client.db
    {
    /// <summary>����� ��� �������</summary>
    public static class BarcodeWorker
        {
        const char POSITION_SEPARATOR = '_';

        /// <summary>�� ��������� ������ ������� ���������� ��������������</summary>
        /// <param name="barcode">������</param>
        public static bool IsAccessoryBarcode(this string barcode)
            {
            string trimBarcode = barcode.Trim();

            if (trimBarcode.Length == 0)
                {
                return true;
                }
            else if (trimBarcode[0] != 'L')
                {
                return false;
                }

            return barcode.GetIntegerBarcode() > 0;
            }

        /// <summary>�� ��������� ������ ������� �����-����� �������</summary>
        /// <param name="barcode">�����-���</param>
        public static bool IsValidPositionBarcode(this string barcode)
            {
            string trimBarcode = barcode.Trim();
            return trimBarcode.Length > 2
                    && trimBarcode[0] == 'P'
                    && trimBarcode[1] == '_'
                    && trimBarcode.Substring(2, trimBarcode.Length - 2).Split(POSITION_SEPARATOR).Length == 3;
            }

        /// <summary>�������� ��� ������� ��������� � �����-����</summary>
        /// <param name="barcode">�����-���</param>
        /// <param name="map">Id �����</param>
        /// <param name="register">� �������</param>
        /// <param name="position">� �������</param>
        /// <returns>�� ���� ������� ���� � �����-����</returns>
        public static bool GetPositionData(this string barcode, out int map, out Int16 register, out byte position)
            {
            string[] parts = barcode.Substring(2, barcode.Length - 2).Split(POSITION_SEPARATOR);

            if (parts.Length == 3)
                {
                try
                    {
                    map = Convert.ToInt32(parts[0]);
                    register = Convert.ToInt16(parts[1]);
                    position = Convert.ToByte(parts[2]);
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


        }
    }