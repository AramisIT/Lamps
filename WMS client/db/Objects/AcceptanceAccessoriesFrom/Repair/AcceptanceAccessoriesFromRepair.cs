namespace WMS_client.db
{
    /// <summary>Приймання комплектуючого з ремонту</summary>
    public class AcceptanceAccessoriesFromRepair : Sending
    {
        public override object Save()
        {
            return base.Save<SendingToCharge>();
        }

        public override object Sync()
        {
            return base.Sync<SendingToCharge>();
        }
    }
}