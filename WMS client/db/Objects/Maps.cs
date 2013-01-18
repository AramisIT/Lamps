namespace WMS_client.db
{
    /// <summary>�����</summary>
    public class Maps : CatalogObject, IBarcodeOwner
    {
        /// <summary>��������</summary>
        [dbFieldAtt(Description = "��������", NotShowInForm = true)]
        public string BarCode { get; set; }
        /// <summary>Id �������� (���� = 0 ������ ����� � �����)</summary>
        [dbFieldAtt(Description = "ParentId")]
        public long ParentId { get; set; }
        /// <summary>������� �..</summary>
        [dbFieldAtt(Description = "RegisterFrom")]
        public int RegisterFrom { get; set; }
        /// <summary>�������� ��..</summary>
        [dbFieldAtt(Description = "RegisterTo")]
        public int RegisterTo { get; set; }
        /// <summary>������ ������������� � ��������</summary>
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