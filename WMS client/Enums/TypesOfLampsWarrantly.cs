using WMS_client.db;

namespace WMS_client.Enums
{
    /// <summary>Типи гарантії</summary>
    public enum TypesOfLampsWarrantly
    {
        /// <summary>Не обрано</summary>
        [dbAttributes(Description = "Не обрано")]
        None,
        /// <summary>Заводська гарантія</summary>
        [dbAttributes(Description = "Заводська гарантія")]
        Factory,
        /// <summary>Ремонтна гарантія</summary>
        [dbAttributes(Description = "Ремонтна гарантія")]
        Repair,
        /// <summary>Без гарантії</summary>
        [dbAttributes(Description = "Без гарантії")]
        Without
    }
}