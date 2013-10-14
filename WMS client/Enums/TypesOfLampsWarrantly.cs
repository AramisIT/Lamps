using WMS_client.db;

namespace WMS_client.Enums
{
    /// <summary>Типи гарантії</summary>
    public enum WarrantyTypes
    {
        /// <summary>Не обрано</summary>
        [dbFieldAtt(Description = "Без гарантії")]
        Without,
        /// <summary>Заводська гарантія</summary>
        [dbFieldAtt(Description = "Заводська гарантія")]
        Factory,
        /// <summary>Ремонтна гарантія</summary>
        [dbFieldAtt(Description = "Ремонтна гарантія")]
        Repair
    }
}