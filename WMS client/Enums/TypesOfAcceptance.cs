using WMS_client.db;

namespace WMS_client.Enums
{
    /// <summary>Тип приймки</summary>
    public enum TypesOfAcceptance
    {
        /// <summary>Новые комплектующие</summary>
        [dbAttributes(Description = "Новые комплектующие")]
        IsNew,
        /// <summary>С ремонта</summary>
        [dbAttributes(Description = "С ремонта")]
        FromRepair
    }
}