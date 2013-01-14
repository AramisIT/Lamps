namespace WMS_client.db
{
    /// <summary>Владелец штрихкода</summary>
    public interface IBarcodeOwner : ISynced
    {
        /// <summary>Штихкод</summary>
        [dbAttributes(Description = "Штрихкод")]
        string BarCode { get; set; }
    }
}