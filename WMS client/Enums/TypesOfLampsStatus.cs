using WMS_client.db;

namespace WMS_client.Enums
{
    /// <summary>Тип статусів лампи</summary>
    public enum TypesOfLampsStatus
    {
        /// <summary>Зберігання</summary>
        [dbAttributes(Description = "Зберігання")]
        Storage,
        /// <summary>Ремонт</summary>
        [dbAttributes(Description = "Ремонт")]
        Repair,
        /// <summary>В роботі</summary>
        [dbAttributes(Description = "В роботі")]
        IsWorking,
        /// <summary>Списано</summary>
        [dbAttributes(Description = "Списано")]
        WrittenOff,
        /// <summary>На обмін</summary>
        [dbAttributes(Description = "На обмін")]
        ForExchange,
        /// <summary>Обміняно</summary>
        [dbAttributes(Description = "Обміняно")]
        Exchanged,
        /// <summary>На списання</summary>
        [dbAttributes(Description = "На списання")]
        ToCharge,
        /// <summary>На ремонт</summary>
        [dbAttributes(Description = "На ремонт")]
        ToRepair
    }
}