using WMS_client.db;

namespace WMS_client.Enums
{
    /// <summary>Тип приймки</summary>
    public enum TypesOfAcceptance
    {
        /// <summary>Нове комплектуюче</summary>
        [dbFieldAtt(Description = "Нове комплектуюче")]
        IsNew,
        /// <summary>З ремонту</summary>
        [dbFieldAtt(Description = "З ремонту")]
        FromRepair
    }
}