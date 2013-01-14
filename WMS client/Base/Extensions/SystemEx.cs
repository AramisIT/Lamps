using System.Collections.Generic;
using System.Text;

namespace System
{
    public static class Number
    {
        public static bool IsNumber(string str)
        {
            if (str == "") return false;

            for (int i = 0; i < str.Length; i++)
            {
                if (!Char.IsNumber(str, i)) return false;
            }

            return true;
        }
    }
}
