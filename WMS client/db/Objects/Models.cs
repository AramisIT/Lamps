namespace WMS_client.db
{
    /// <summary>������</summary>
    public class Models : CatalogObject, IBarcodeOwner
    {
        /// <summary>��������</summary>
        [dbFieldAtt(Description = "��������")]
        public string BarCode { get; set; }
        /// <summary>������ ������������� � ��������</summary>
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