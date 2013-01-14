using WMS_client.Enums;

namespace WMS_client.db
{
    public abstract class SubSending : dbObject, ISynced
    {
        [dbAttributes(Description = "IsSynced", NotShowInForm = true)]
        public bool IsSynced { get; set; }
        /// <summary>Штрихкод документа</summary>
        [dbAttributes(Description = "Штрихкод документа", NotShowInForm = true)]
        public string Document { get; set; }
        /// <summary>Тип комплектующего</summary>
        [dbAttributes(Description = "Тип комплектующего")]
        public TypeOfAccessories TypeOfAccessory { get; set; }
    }
}