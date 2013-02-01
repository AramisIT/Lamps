//using System;
//using WMS_client.Enums;

//namespace WMS_client.db
//{
//    public abstract class AcceptanceAccessories : CatalogObject, IBarcodeOwner
//    {
//        /// <summary>Дата</summary>
//        [dbFieldAtt(Description = "Дата", NotShowInForm = true)]
//        public DateTime Date { get; set; }
//        /// <summary>Штрихкод</summary>
//        [dbFieldAtt(Description = "Штрихкод", NotShowInForm = true)]
//        public string BarCode { get; set; }
//        [dbFieldAtt(Description = "IsSynced", NotShowInForm = true)]
//        public bool IsSynced { get; set; }
//        /// <summary>Контрагент</summary>
//        [dbFieldAtt(Description = "Контрагент", dbObjectType = typeof(Contractors))]
//        public int Contractor { get; set; }
//        /// <summary>Тип комплектующего</summary>
//        [dbFieldAtt(Description = "Тип комплектующего")]
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
//        /// <summary>Штрихкод документа</summary>
//        [dbFieldAtt(Description = "Штрихкод документа", NotShowInForm = true)]
//        public string Document { get; set; }
//        /// <summary>Тип комплектующего</summary>
//        [dbFieldAtt(Description = "Тип комплектующего")]
//        public TypeOfAccessories TypeOfAccessory { get; set; }
//    }
//}