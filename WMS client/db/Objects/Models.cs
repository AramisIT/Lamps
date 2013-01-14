namespace WMS_client.db
{
    /// <summary>Модели</summary>
    public class Models : CatalogObject, IBarcodeOwner
    {
        /// <summary>Штрихкод</summary>
        [dbAttributes(Description = "Штрихкод")]
        public string BarCode { get; set; }
        /// <summary>Статус синхронизации с сервером</summary>
        [dbAttributes(Description = "IsSynced", NotShowInForm = true)]
        public bool IsSynced { get; set; }

        /// <summary>Удалить все документы</summary>
        public static void ClearOldDocuments()
        {
            dbArchitector.ClearAllDataFromTable("Models");
        }

        public override object Save()
        {
            return base.Save<Models>();
        }

        public override object Sync()
        {
            return base.Sync<Models>();
        }
    }
}