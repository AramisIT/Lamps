namespace WMS_client.db
{
    /// <summary>��������� ������� ��� ������������ ���������� ����� ������� "��������� � �����"</summary>
    public class AcceptanceAccessoriesFromExchangeDetails:dbObject, IBarcodeOwner
    {
        /// <summary>IsSynced</summary>
        [dbFieldAtt(Description = "IsSynced")]
        public bool IsSynced { get; set; }
        /// <summary>������������ (������)</summary>
        [dbFieldAtt(Description = "������������ (������)", dbObjectType = typeof(Models))]
        public int Nomenclature { get; set; }
        /// <summary>��������</summary>
        [dbFieldAtt(Description = "��������")]
        public string BarCode { get; set; }

        #region Overrides of dbObject
        /// <summary>�������� ���</summary>
        /// <param name="updId">������� Id ��� ������ ��'����?</param>
        /// <returns>���������� ��'���</returns>
        public object Save(bool updId)
        {
            return SaveChanges<AcceptanceAccessoriesFromExchangeDetails>(false, updId);
        }

        public override object Write()
        {
            return base.Save<AcceptanceAccessoriesFromExchangeDetails>();
        }

        public override object Sync()
        {
            return base.Sync<AcceptanceAccessoriesFromExchangeDetails>();
        }
        #endregion
    }
}