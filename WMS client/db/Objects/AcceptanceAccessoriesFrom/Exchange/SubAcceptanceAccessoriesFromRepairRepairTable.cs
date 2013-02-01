namespace WMS_client.db
{
    /// <summary>Таблиця ремонту. Приймання комплектуючого з обміну</summary>
    public class SubAcceptanceAccessoriesFromExchangeExchange : SubSending
    {
        /// <summary>Номенклатура (Модель)</summary>
        [dbFieldAtt(Description = "Номенклатура", dbObjectType = typeof(Models))]
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