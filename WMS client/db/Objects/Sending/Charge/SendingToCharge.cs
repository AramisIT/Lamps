namespace WMS_client.db
{
    /// <summary>Отправка на списание</summary>
    public class SendingToCharge : Sending
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