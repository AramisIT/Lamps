namespace WMS_client.db
{
    /// <summary>����������</summary>
    public class Contractors : CatalogObject, IBarcodeOwner
    {
        /// <summary>������ ������������� � ��������</summary>
        [dbFieldAtt(Description = "IsSynced", NotShowInForm = true)]
        public bool IsSynced { get; set; }
        /// <summary>��������</summary>
        [dbFieldAtt(Description = "��������", NotShowInForm = true)]
        public string BarCode { get; set; }

        public override object Write()
        {
            return base.Save<Contractors>();
        }

        public override object Sync()
        {
            return base.Sync<Contractors>();
        }
    }
}