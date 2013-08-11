using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace WMS_client
    {
    static class StringParser
        {
        internal static int GetIntegerBarcode(this string barcodeStr)
            {
            if (string.IsNullOrEmpty(barcodeStr) || barcodeStr.Length < 2)
                {
                return 0;
                }

            try
                {
                int barcode = Convert.ToInt32(barcodeStr.Substring(1));
                return barcode;
                }
            catch
                {
                return 0;
                }
            }

        internal static object ParseDateTime(object value)
            {
            const char separator = '.';
            string[] parts = value.ToString().Substring(0, 10).Split(separator);

            DateTime result;

            if (parts.Length == 3)
                {
                result = Convert.ToDateTime(string.Concat(
                    parts[1],
                    separator,
                    parts[0],
                    separator,
                    parts[2]));
                }
            else
                {
                result = new DateTime();
                }

            return result;
            }
        }
    }
