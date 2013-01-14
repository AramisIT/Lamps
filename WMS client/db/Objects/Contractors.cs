namespace WMS_client.db
{
    /// <summary>Контрагент</summary>
    public class Contractors : CatalogObject, IBarcodeOwner
    {
        /// <summary>Статус синхронизации с сервером</summary>
        [dbAttributes(Description = "IsSynced", NotShowInForm = true)]
        public bool IsSynced { get; set; }
        /// <summary>Штрихкод</summary>
        [dbAttributes(Description = "Штрихкод", NotShowInForm = true)]
        public string BarCode { get; set; }

        public override object Save()
        {
            return base.Save<Contractors>();
        }

        public override object Sync()
        {
            return base.Sync<Contractors>();
        }
    }
}