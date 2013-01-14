namespace WMS_client.db
{
    /// <summary>Карта</summary>
    public class Maps : CatalogObject, IBarcodeOwner
    {
        /// <summary>Штрихкод</summary>
        [dbAttributes(Description = "Штрихкод", NotShowInForm = true)]
        public string BarCode { get; set; }
        /// <summary>Id родителя (если = 0 значит лажит в корне)</summary>
        [dbAttributes(Description = "ParentId")]
        public long ParentId { get; set; }
        /// <summary>Регистр с..</summary>
        [dbAttributes(Description = "RegisterFrom")]
        public int RegisterFrom { get; set; }
        /// <summary>Регистры по..</summary>
        [dbAttributes(Description = "RegisterTo")]
        public int RegisterTo { get; set; }
        /// <summary>Статус синхронизации с сервером</summary>
        [dbAttributes(Description = "IsSynced", NotShowInForm = true)]
        public bool IsSynced { get; set; }

        public override object Save()
        {
            return base.Save<Maps>();
        }

        public override object Sync()
        {
            return base.Sync<Maps>();
        }
    }
}