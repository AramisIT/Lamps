namespace WMS_client.db
{
    /// <summary>�������� �� ��������</summary>
    public class SendingToExchange : Sending
    {
        public override object Save()
        {
            return base.Save<SendingToExchange>();
        }

        public override object Sync()
        {
            return base.Sync<SendingToExchange>();
        }
    }
}