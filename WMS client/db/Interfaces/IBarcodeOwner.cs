namespace WMS_client.db
{
    /// <summary>�������� ���������</summary>
    public interface IBarcodeOwner : ISynced
    {
        /// <summary>�������</summary>
        [dbFieldAtt(Description = "��������")]
        string BarCode { get; set; }
    }
}