using WMS_client.Enums;

namespace WMS_client.db
{
    public abstract class SubSending : dbObject, ISynced
    {
        [dbAttributes(Description = "IsSynced", NotShowInForm = true)]
        public bool IsSynced { get; set; }
        /// <summary>�������� ���������</summary>
        [dbAttributes(Description = "�������� ���������", NotShowInForm = true)]
        public string Document { get; set; }
        /// <summary>��� ��������������</summary>
        [dbAttributes(Description = "��� ��������������")]
        public TypeOfAccessories TypeOfAccessory { get; set; }
    }
}