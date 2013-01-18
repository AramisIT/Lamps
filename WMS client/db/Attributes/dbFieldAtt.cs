using System;

namespace WMS_client.db
{
    /// <summary>��������� �������</summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class dbFieldAtt : Attribute
    {
        /// <summary>����� ������ �� ���������</summary>
        public const int DEFAULT_STR_LENGTH = 25;

        /// <summary>����� ���������� ����</summary>
        public int StrLength { get; set; }
        /// <summary>������������� ����</summary>
        public string Description { get; set; }
        /// <summary>�� ���������� �� ����</summary>
        public bool NotShowInForm { get; set; }
        /// <summary>³��������� �� ���� �����������</summary>
        public bool ShowInEditForm { get; set; }
        /// <summary></summary>
        public Type dbObjectType { get; set; }
        /// <summary>³��������� �������� ����������</summary>
        public bool NeedDetailInfo { get; set; }
        /// <summary>³��������� �������� ����������</summary>
        public bool ShowEmbadedInfo { get; set; }
    }
}