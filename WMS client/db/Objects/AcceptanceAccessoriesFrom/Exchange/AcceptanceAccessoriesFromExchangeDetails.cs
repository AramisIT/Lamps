namespace WMS_client.db
{
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
        public object Save(bool updId)
        {
            return SaveChanges<AcceptanceAccessoriesFromExchangeDetails>(false, updId);
        }

        public override object Save()
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