using System;
using WMS_client.Enums;

namespace WMS_client.db
{
    /// <summary>Поля для документів "Відправка на.."/"Приймання з .."</summary>
    public abstract class Sending : CatalogObject, IBarcodeOwner
    {
        /// <summary>Дата</summary>
        [dbFieldAtt(Description = "Дата", NotShowInForm = true)]
        public DateTime Date { get; set; }
        /// <summary>Штрихкод</summary>
        [dbFieldAtt(Description = "Штрихкод", NotShowInForm = true)]
        public string BarCode { get; set; }
        [dbFieldAtt(Description = "IsSynced", NotShowInForm = true)]
        public bool IsSynced { get; set; }
        /// <summary>Контрагент</summary>
        [dbFieldAtt(Description = "Контрагент", dbObjectType = typeof(Contractors))]
        public int Contractor { get; set; }
        /// <summary>Тип комплектующего</summary>
        [dbFieldAtt(Description = "Тип комплектующего")]
        public TypeOfAccessories TypeOfAccessory { get; set; }

        protected Sending()
        {
            BarCode = string.Empty;
        }
    }
}