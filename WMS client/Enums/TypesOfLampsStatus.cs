using WMS_client.db;

namespace WMS_client.Enums
{
    /// <summary>��� ������� �����</summary>
    public enum TypesOfLampsStatus
    {
        /// <summary>���������</summary>
        [dbAttributes(Description = "���������")]
        Storage,
        /// <summary>������</summary>
        [dbAttributes(Description = "������")]
        Repair,
        /// <summary>� �����</summary>
        [dbAttributes(Description = "� �����")]
        IsWorking,
        /// <summary>�������</summary>
        [dbAttributes(Description = "�������")]
        WrittenOff,
        /// <summary>�� ����</summary>
        [dbAttributes(Description = "�� ����")]
        ForExchange,
        /// <summary>�������</summary>
        [dbAttributes(Description = "�������")]
        Exchanged,
        /// <summary>�� ��������</summary>
        [dbAttributes(Description = "�� ��������")]
        ToCharge,
        /// <summary>�� ������</summary>
        [dbAttributes(Description = "�� ������")]
        ToRepair
    }
}