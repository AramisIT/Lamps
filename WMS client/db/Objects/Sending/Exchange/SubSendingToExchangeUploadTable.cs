namespace WMS_client.db
{
    /// <summary>������� �����. ³������� �� ��������</summary>
    public class SubSendingToExchangeUploadTable : SubSending
    {
        public override object Save()
        {
            return base.Save<SubSendingToExchangeUploadTable>();
        }

        public override object Sync()
        {
            return base.Sync<SubSendingToExchangeUploadTable>();
        }
    }
}