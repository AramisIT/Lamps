//using System;
//using WMS_client.Enums;

//namespace WMS_client.db
//{
//    public abstract class AcceptanceAccessories : CatalogObject, IBarcodeOwner
//    {
//        /// <summary>����</summary>
//        [dbFieldAtt(Description = "����", NotShowInForm = true)]
//        public DateTime Date { get; set; }
//        /// <summary>��������</summary>
//        [dbFieldAtt(Description = "��������", NotShowInForm = true)]
//        public string BarCode { get; set; }
//        [dbFieldAtt(Description = "IsSynced", NotShowInForm = true)]
//        public bool IsSynced { get; set; }
//        /// <summary>����������</summary>
//        [dbFieldAtt(Description = "����������", dbObjectType = typeof(Contractors))]
//        public int Contractor { get; set; }
//        /// <summary>��� ��������������</summary>
//        [dbFieldAtt(Description = "��� ��������������")]
//        public TypeOfAccessories TypeOfAccessory { get; set; }

//        protected AcceptanceAccessories()
//        {
//            BarCode = string.Empty;
//        }
//    }
//    public abstract class SubAcceptanceAccessoriesFrom : dbObject, ISynced
//    {
//        [dbFieldAtt(Description = "IsSynced", NotShowInForm = true)]
//        public bool IsSynced { get; set; }
//        /// <summary>�������� ���������</summary>
//        [dbFieldAtt(Description = "�������� ���������", NotShowInForm = true)]
//        public string Document { get; set; }
//        /// <summary>��� ��������������</summary>
//        [dbFieldAtt(Description = "��� ��������������")]
//        public TypeOfAccessories TypeOfAccessory { get; set; }
//    }
//}