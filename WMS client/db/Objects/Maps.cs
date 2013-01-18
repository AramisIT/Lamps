namespace WMS_client.db
{
    /// <summary>Карта</summary>
    public class Maps : CatalogObject, IBarcodeOwner
    {
        /// <summary>Штрихкод</summary>
        [dbFieldAtt(Description = "Штрихкод", NotShowInForm = true)]
        public string BarCode { get; set; }
        /// <summary>Id родителя (если = 0 значит лажит в корне)</summary>
        [dbFieldAtt(Description = "ParentId")]
        public long ParentId { get; set; }
        /// <summary>Регистр с..</summary>
        [dbFieldAtt(Description = "RegisterFrom")]
        public int RegisterFrom { get; set; }
        /// <summary>Регистры по..</summary>
        [dbFieldAtt(Description = "RegisterTo")]
        public int RegisterTo { get; set; }
        /// <summary>Статус синхронизации с сервером</summary>
        [dbFieldAtt(Description = "IsSynced", NotShowInForm = true)]
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