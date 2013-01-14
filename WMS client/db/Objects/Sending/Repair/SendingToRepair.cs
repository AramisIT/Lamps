namespace WMS_client.db
{
    /// <summary>Отправка на ремонт</summary>
    public class SendingToRepair : Sending
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