namespace WMS_client.db
{
    /// <summary>Таблиця ремонту. Приймання комплектуючого з ремонту</summary>
    public class SubAcceptanceAccessoriesFromRepairRepairTable : SubSending
    {
        public override object Write()
        {
            return base.Save<SubAcceptanceAccessoriesFromRepairRepairTable>();
        }

        public override object Sync()
        {
            return base.Sync<SubAcceptanceAccessoriesFromRepairRepairTable>();
        }
    }
}