namespace WMS_client.db
{
    /// <summary>Таблица обмена. Отправка на списание</summary>
    public class SubSendingToChargeChargeTable : SubSending
    {
        public override object Write()
        {
            return base.Save<SubSendingToChargeChargeTable>();
        }

        public override object Sync()
        {
            return base.Sync<SubSendingToChargeChargeTable>();
        }
    }
}