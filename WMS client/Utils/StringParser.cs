using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace WMS_client.Utils
    {
    class StringParser
        {
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
