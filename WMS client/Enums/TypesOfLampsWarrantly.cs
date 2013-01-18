using WMS_client.db;

namespace WMS_client.Enums
{
    /// <summary>���� ������</summary>
    public enum TypesOfLampsWarrantly
    {
        /// <summary>�� ������</summary>
        [dbFieldAtt(Description = "�� ������")]
        None,
        /// <summary>��������� �������</summary>
        [dbFieldAtt(Description = "��������� �������")]
        Factory,
        /// <summary>�������� �������</summary>
        [dbFieldAtt(Description = "�������� �������")]
        Repair,
        /// <summary>��� ������</summary>
        [dbFieldAtt(Description = "��� ������")]
        Without
    }
}