namespace WMS_client.db
{
    /// <summary>������� ������. �������� �� ��������</summary>
    public class SubSendingToChargeChargeTable : SubSending
    {
        public override object Save()
        {
            return base.Save<SubSendingToChargeChargeTable>();
        }

        public override object Sync()
        {
            return base.Sync<SubSendingToChargeChargeTable>();
        }
    }
}