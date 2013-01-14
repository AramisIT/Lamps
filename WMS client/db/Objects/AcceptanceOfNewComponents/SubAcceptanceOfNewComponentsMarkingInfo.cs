namespace WMS_client.db
{
    /// <summary>������� ����� �������������. ��������� ����� "����������"</summary>
    public class SubAcceptanceOfNewComponentsMarkingInfo : dbObject
    {
        /// <summary>����������</summary>
        [dbAttributes(Description = "Marking", dbObjectType = typeof(Models))]
        public long Marking { get; set; }
        /// <summary>����</summary>
        [dbAttributes(Description = "Plan")]
        public int Plan { get; set; }
        /// <summary>����</summary>
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