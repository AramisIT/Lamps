using WMS_client.Enums;

namespace WMS_client.db
{
    /// <summary>���� ��� ��������� ������� ��������� "³������� ��.."/"��������� � .."</summary>
    public abstract class SubSending : dbObject, ISynced
    {
        [dbFieldAtt(Description = "IsSynced", NotShowInForm = true)]
        public bool IsSynced { get; set; }
        /// <summary>�������� ���������</summary>
        [dbFieldAtt(Description = "�������� ���������", NotShowInForm = true)]
        public string Document { get; set; }
        /// <summary>��� ��������������</summary>
        [dbFieldAtt(Description = "��� ��������������")]
        public TypeOfAccessories TypeOfAccessory { get; set; }
    }
}