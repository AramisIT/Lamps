namespace WMS_client.db
{
    /// <summary>Приемка новых комплектующих. Табличная часть "Маркировка"</summary>
    public class SubAcceptanceOfNewComponentsMarkingInfo : dbObject
    {
        /// <summary>Маркировка</summary>
        [dbAttributes(Description = "Marking", dbObjectType = typeof(Models))]
        public long Marking { get; set; }
        /// <summary>План</summary>
        [dbAttributes(Description = "Plan")]
        public int Plan { get; set; }
        /// <summary>Факт</summary>
        [dbAttributes(Description = "Fact")]
        public int Fact { get; set; }

        public override object Save()
        {
            return base.Save<SubAcceptanceOfNewComponentsMarkingInfo>();
        }

        public override object Sync()
        {
            return base.Sync<SubAcceptanceOfNewComponentsMarkingInfo>();
        }
    }
}