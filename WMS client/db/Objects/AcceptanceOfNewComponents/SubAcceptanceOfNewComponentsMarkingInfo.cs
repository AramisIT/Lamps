//namespace WMS_client.db
//{
//    /// <summary>������� ����� �������������. ��������� ����� "����������"</summary>
//    public class SubAcceptanceOfNewComponentsMarkingInfo : dbObject
//    {
//        /// <summary>����������</summary>
//        [dbFieldAtt(Description = "Marking", dbObjectType = typeof(Models))]
//        public long Marking { get; set; }
//        /// <summary>����</summary>
//        [dbFieldAtt(Description = "Plan")]
//        public int Plan { get; set; }
//        /// <summary>����</summary>
//        [dbFieldAtt(Description = "Fact")]
//        public int Fact { get; set; }

//        public override object Save()
//        {
//            return base.Save<SubAcceptanceOfNewComponentsMarkingInfo>();
//        }

//        public override object Sync()
//        {
//            return base.Sync<SubAcceptanceOfNewComponentsMarkingInfo>();
//        }
//    }
//}