namespace WMS_client.db
{
    /// <summary>������</summary>
    public class Models : CatalogObject, IBarcodeOwner
    {
        /// <summary>��������</summary>
        [dbAttributes(Description = "��������")]
        public string BarCode { get; set; }
        /// <summary>������ ������������� � ��������</summary>
        [dbAttributes(Description = "IsSynced", NotShowInForm = true)]
        public bool IsSynced { get; set; }

        /// <summary>������� ��� ���������</summary>
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