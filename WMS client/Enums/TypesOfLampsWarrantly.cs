using WMS_client.db;

namespace WMS_client.Enums
{
    /// <summary>���� ������</summary>
    public enum WarrantyTypes
    {
        /// <summary>�� ������</summary>
        [dbFieldAtt(Description = "��� ������")]
        Without,
        /// <summary>��������� �������</summary>
        [dbFieldAtt(Description = "��������� �������")]
        Factory,
        /// <summary>�������� �������</summary>
        [dbFieldAtt(Description = "�������� �������")]
        Repair
    }
}