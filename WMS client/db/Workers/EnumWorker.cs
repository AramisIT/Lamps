using System;
using System.Reflection;
using System.Collections.Generic;

namespace WMS_client.db
{
    /// <summary>Enum �������</summary>
    public static class EnumWorker
    {
        /// <summary>�������� ������������</summary>
        /// <param name="enumType">��� ������������</param>
        /// <param name="value">��������</param>
        /// <returns>������������</returns>
        public static string GetDescription(Type enumType, int value)
        {
            FieldInfo[] fields = enumType.GetFields();
            value++;

            if (fields.Length > value)
            {
                Attribute[] attributes = Attribute.GetCustomAttributes(fields[value]);

                foreach (Attribute attribute in attributes)
                {
                    dbFieldAtt enumAttributes = attribute as dbFieldAtt;

                    if (enumAttributes != null)
                    {
                        return enumAttributes.Description;
                    }
                }
            }

            return "�������: ��� �� �������!";
        }

        /// <summary>�������� ������ (��������; ������������)</summary>
        /// <param name="enumType">��� ������������</param>
        /// <returns>������ (��������; ������������)</returns>
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
                dbFieldAtt attribute = Attribute.GetCustomAttribute(inf, typeof(dbFieldAtt)) as dbFieldAtt;

                if (attribute != null)
                {
                    list.Add(index++, attribute.Description);
                }
            }

            return list;
        }
    }
}