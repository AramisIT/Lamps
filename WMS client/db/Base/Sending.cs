using System;
using WMS_client.Enums;

namespace WMS_client.db
{
    public abstract class Sending : CatalogObject, IBarcodeOwner
    {
        /// <summary>Дата</summary>
        [dbAttributes(Description = "Дата", NotShowInForm = true)]
        public DateTime Date { get; set; }
        /// <summary>Штрихкод</summary>
        [dbAttributes(Description = "Штрихкод", NotShowInForm = true)]
        public string BarCode { get; set; }
        [dbAttributes(Description = "IsSynced", NotShowInForm = true)]
        public bool IsSynced { get; set; }
        /// <summary>Контрагент</summary>
        [dbAttributes(Description = "Контрагент", dbObjectType = typeof(Contractors))]
        public int Contractor { get; set; }
        /// <summary>Тип комплектующего</summary>
        [dbAttributes(Description = "Тип комплектующего")]
        public TypeOfAccessories TypeOfAccessory { get; set; }

        protected Sending()
        {
            BarCode = string.Empty;
        }
    }
}