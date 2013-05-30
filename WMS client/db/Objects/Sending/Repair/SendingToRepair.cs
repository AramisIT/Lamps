namespace WMS_client.db
{
    /// <summary>�������� �� ������</summary>
    public class SendingToRepair : Sending
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