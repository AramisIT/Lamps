namespace WMS_client.db
{
    /// <summary>�������� �� ��������</summary>
    public class SendingToCharge : Sending
    {
        public override object Write()
        {
            return base.Save<SendingToCharge>();
        }

        public override object Sync()
        {
            return base.Sync<SendingToCharge>();
        }
    }
}