namespace WMS_client.db
{
    /// <summary>Модели</summary>
    public class Models : CatalogObject, IBarcodeOwner
    {
        /// <summary>Штрихкод</summary>
        [dbFieldAtt(Description = "Штрихкод")]
        public string BarCode { get; set; }
        /// <summary>Статус синхронизации с сервером</summary>
        [dbFieldAtt(Description = "IsSynced", NotShowInForm = true)]
        public bool IsSynced { get; set; }

        public override object Write()
        {
            return base.Save<Models>();
        }

        public override object Sync()
        {
            return base.Sync<Models>();
        }
    }
}