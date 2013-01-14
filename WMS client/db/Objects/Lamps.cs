namespace WMS_client.db
{
    /// <summary>Лампа</summary>
    public class Lamps : Accessory
    {
        /// <summary>Корпус</summary>
        [dbAttributes(Description = "Корпус", dbObjectType = typeof(Cases), NeedDetailInfo = true)]
        public long Case { get; set; }

        public override object Save()
        {
            return base.Save<Lamps>();
        }

        public override object Sync()
        {
            return base.Sync<Lamps>();
        }
    }
}