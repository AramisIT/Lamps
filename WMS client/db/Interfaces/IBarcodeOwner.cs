namespace WMS_client.db
{
    /// <summary>�������� ���������</summary>
    public interface IBarcodeOwner : ISynced
    {
        /// <summary>�������</summary>
        [dbAttributes(Description = "��������")]
        string BarCode { get; set; }
    }
}