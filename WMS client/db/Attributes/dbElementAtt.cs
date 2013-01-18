using System;

namespace WMS_client.db
{
    /// <summary>��������� ���������</summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Class)]
    public class dbElementAtt : Attribute
    {
        /// <summary>����� ������������ �� ���������</summary>
        public const int DEFAULT_DES_LENGTH = 25;

        /// <summary>����� ���� ������������</summary>
        public int DescriptionLength { get; set; }
    }
}