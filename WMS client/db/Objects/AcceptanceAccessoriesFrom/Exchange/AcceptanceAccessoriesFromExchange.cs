namespace WMS_client.db
{
    /// <summary>Приймання комплектуючого з обміну</summary>
    public class AcceptanceAccessoriesFromExchange : Sending
    {
        public override object Write()
        {
            return base.Save<AcceptanceAccessoriesFromExchange>();
        }

        public override object Sync()
        {
            return base.Sync<AcceptanceAccessoriesFromExchange>();
        }
    }
}