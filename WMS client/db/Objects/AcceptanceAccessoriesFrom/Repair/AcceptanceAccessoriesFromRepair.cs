namespace WMS_client.db
{
    /// <summary>��������� �������������� � �������</summary>
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