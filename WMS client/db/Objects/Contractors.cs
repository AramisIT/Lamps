namespace WMS_client.db
{
    /// <summary>����������</summary>
    public class Contractors : CatalogObject, IBarcodeOwner
    {
        /// <summary>������ ������������� � ��������</summary>
        [dbAttributes(Description = "IsSynced", NotShowInForm = true)]
        public bool IsSynced { get; set; }
        /// <summary>��������</summary>
        [dbAttributes(Description = "��������", NotShowInForm = true)]
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