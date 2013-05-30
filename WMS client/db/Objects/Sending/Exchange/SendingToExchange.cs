namespace WMS_client.db
{
    /// <summary>Відправка на списання</summary>
    public class SendingToExchange : Sending
    {
        public override object Write()
        {
            return base.Save<SendingToExchange>();
        }

        public override object Sync()
        {
            return base.Sync<SendingToExchange>();
        }
    }
}