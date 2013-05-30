namespace WMS_client.db
{
    /// <summary>Таблица обміну. Відправка на списання</summary>
    public class SubSendingToExchangeUploadTable : SubSending
    {
        public override object Write()
        {
            return base.Save<SubSendingToExchangeUploadTable>();
        }

        public override object Sync()
        {
            return base.Sync<SubSendingToExchangeUploadTable>();
        }
    }
}