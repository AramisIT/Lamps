namespace WMS_client.db
{
    /// <summary>Владелец штрихкода</summary>
    public interface IBarcodeOwner : ISynced
    {
        /// <summary>Штихкод</summary>
        [dbFieldAtt(Description = "Штрихкод")]
        string BarCode { get; set; }
    }
}