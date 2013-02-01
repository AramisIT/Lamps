namespace WMS_client.db
{
    /// <summary>������� �������. ��������� �������������� � �����</summary>
    public class SubAcceptanceAccessoriesFromExchangeExchange : SubSending
    {
        /// <summary>������������ (������)</summary>
        [dbFieldAtt(Description = "������������", dbObjectType = typeof(Models))]
        public int Nomenclature { get; set; }

        public SubAcceptanceAccessoriesFromExchangeExchange()
        {
            Document = string.Empty;
        }

        public override object Save()
        {
            return base.Save<SubAcceptanceAccessoriesFromExchangeExchange>();
        }

        public override object Sync()
        {
            return base.Sync<SubAcceptanceAccessoriesFromExchangeExchange>();
        }
    }
}