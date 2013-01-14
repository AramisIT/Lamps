using System;
using System.Reflection;
using System.Collections.Generic;

namespace WMS_client.db
{
    /// <summary>Enum рабочий</summary>
    public static class EnumWorker
    {
        /// <summary>Получить наименование</summary>
        /// <param name="enumType">Тип перечисления</param>
        /// <param name="value">Значение</param>
        /// <returns>Наименование</returns>
        public static string GetDescription(Type enumType, int value)
        {
            FieldInfo[] fields = enumType.GetFields();
            Attribute[] attributes = Attribute.GetCustomAttributes(fields[value + 1]);

            foreach (Attribute attribute in attributes)
            {
                dbAttributes enumAttributes = attribute as dbAttributes;

                if (enumAttributes != null)
                {
                    return enumAttributes.Description;
                }
            }

            throw new Exception("Тип не знайдно!");
        }

        /// <summary>Получить список (значение; наименование)</summary>
        /// <param name="enumType">Тип перечисления</param>
        /// <returns>Список (значение; наименование)</returns>
        public static Dictionary<int,string> GetList(Type enumType)
        {
            int index = 0;
            Dictionary<int,string> list = new Dictionary<int, string>();

            while (true)
            {
                string numberStr = index.ToString();
                object valueDescription = Enum.Parse(enumType, index.ToString(), true);
                string valueStr = valueDescription.ToString();

                if (numberStr == valueStr)
                {
                    break;
                }

                MemberInfo inf = enumType.GetMembers()[10 + index];
                dbAttributes attribute = Attribute.GetCustomAttribute(inf, typeof(dbAttributes)) as dbAttributes;

                if (attribute != null)
                {
                    list.Add(index++, attribute.Description);
                }
            }

            return list;
        }
    }
}