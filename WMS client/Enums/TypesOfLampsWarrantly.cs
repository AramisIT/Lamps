using WMS_client.db;

namespace WMS_client.Enums
{
    /// <summary>���� ������</summary>
    public enum TypesOfLampsWarrantly
    {
        /// <summary>�� ������</summary>
        [dbAttributes(Description = "�� ������")]
        None,
        /// <summary>��������� �������</summary>
        [dbAttributes(Description = "��������� �������")]
        Factory,
        /// <summary>�������� �������</summary>
        [dbAttributes(Description = "�������� �������")]
        Repair,
        /// <summary>��� ������</summary>
        [dbAttributes(Description = "��� ������")]
        Without
    }
}