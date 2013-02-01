using WMS_client.db;

namespace WMS_client.Enums
{
    /// <summary>��� ������� �����</summary>
    public enum TypesOfLampsStatus
    {
        /// <summary>���������</summary>
        [dbFieldAtt(Description = "���������")]
        Storage,
        /// <summary>������</summary>
        [dbFieldAtt(Description = "������")]
        Repair,
        /// <summary>������</summary>
        [dbFieldAtt(Description = "������")]
        IsWorking,
        /// <summary>������</summary>
        [dbFieldAtt(Description = "������")]
        WrittenOff,
        /// <summary>�� ����</summary>
        [dbFieldAtt(Description = "�� ����")]
        ForExchange,
        /// <summary>�������</summary>
        [dbFieldAtt(Description = "�������")]
        Exchanged,
        /// <summary>�� ��������</summary>
        [dbFieldAtt(Description = "�� ��������")]
        ToCharge,
        /// <summary>�� ������</summary>
        [dbFieldAtt(Description = "�� ������")]
        ToRepair,
        /// <summary>� ������</summary>
        [dbFieldAtt(Description = "� ������")]
        UnderRepair,
        /// <summary>� �������</summary>
        [dbFieldAtt(Description = "� �������")]
        UnderCharge
    }
}