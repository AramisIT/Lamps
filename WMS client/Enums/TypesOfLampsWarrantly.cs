using WMS_client.db;

namespace WMS_client.Enums
{
    /// <summary>Типи гарантії</summary>
    public enum TypesOfLampsWarrantly
    {
        /// <summary>Не обрано</summary>
        [dbFieldAtt(Description = "Не обрано")]
        None,
        /// <summary>Заводська гарантія</summary>
        [dbFieldAtt(Description = "Заводська гарантія")]
        Factory,
        /// <summary>Ремонтна гарантія</summary>
        [dbFieldAtt(Description = "Ремонтна гарантія")]
        Repair,
        /// <summary>Без гарантії</summary>
        [dbFieldAtt(Description = "Без гарантії")]
        Without
    }
}