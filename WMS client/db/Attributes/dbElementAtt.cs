using System;

namespace WMS_client.db
{
    /// <summary>Аттребуты элементов</summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Class)]
    public class dbElementAtt : Attribute
    {
        /// <summary>Длина наименования по умолчанию</summary>
        public const int DEFAULT_DES_LENGTH = 25;

        /// <summary>Длина поля наименования</summary>
        public int DescriptionLength { get; set; }
    }
}