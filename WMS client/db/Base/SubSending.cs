using WMS_client.Enums;

namespace WMS_client.db
{
    /// <summary>Поля для такбличної частини документів "Відправка на.."/"Приймання з .."</summary>
    public abstract class SubSending : dbObject, ISynced
    {
        [dbFieldAtt(Description = "IsSynced", NotShowInForm = true)]
        public bool IsSynced { get; set; }
        /// <summary>Штрихкод документа</summary>
        [dbFieldAtt(Description = "Штрихкод документа", NotShowInForm = true)]
        public string Document { get; set; }
        /// <summary>Тип комплектующего</summary>
        [dbFieldAtt(Description = "Тип комплектующего")]
        public TypeOfAccessories TypeOfAccessory { get; set; }
    }
}