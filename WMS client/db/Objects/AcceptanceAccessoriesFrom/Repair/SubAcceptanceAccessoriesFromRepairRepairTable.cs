namespace WMS_client.db
{
    /// <summary>������� �������. ��������� �������������� � �������</summary>
    public class SubAcceptanceAccessoriesFromRepairRepairTable : SubSending
    {
        public override object Save()
        {
            return base.Save<SubAcceptanceAccessoriesFromRepairRepairTable>();
        }

        public override object Sync()
        {
            return base.Sync<SubAcceptanceAccessoriesFromRepairRepairTable>();
        }
    }
}