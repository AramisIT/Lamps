namespace WMS_client.Enums
{
    /// <summary>�������� � �������</summary>
    public enum OperationsWithLighters
    {
        /// <summary>��������� �����������\����� �� ������</summary>
        Installing,
        /// <summary>�������� �����������</summary>
        Removing,
        /// <summary>������� ����� �������������</summary>
        Acceptance,
        /// <summary>������� ������������� � �������</summary>
        AcceptanceFromRepair,
        /// <summary>������� ������������� � ������</summary>
        AcceptanceFromExchange,
        /// <summary>������� ������������� �� ��������</summary>
        AcceptanceFromCharge,
        /// <summary>�������� �� ������</summary>
        SendingToRepair,
        /// <summary>�������� �� �����</summary>
        SendingToExchange,
        /// <summary>�������� �� ��������</summary>
        SendingToCharge,
        /// <summary>�����������</summary>
        Registration
    }
}