namespace WMS_client.db
{
    /// <summary>Таблиця ремонту. Отправка на ремонт</summary>
    public class SubSendingToRepairRepairTable : SubSending
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