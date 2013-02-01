namespace WMS_client.db
{
    /// <summary>Таблица обмена. Отправка на списание</summary>
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