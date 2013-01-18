using WMS_client.db;

namespace WMS_client.Enums
{
    /// <summary>Тип статусів лампи</summary>
    public enum TypesOfLampsStatus
    {
        /// <summary>Зберігання</summary>
        [dbFieldAtt(Description = "Зберігання")]
        Storage,
        /// <summary>Ремонт</summary>
        [dbFieldAtt(Description = "Ремонт")]
        Repair,
        /// <summary>В роботі</summary>
        [dbFieldAtt(Description = "В роботі")]
        IsWorking,
        /// <summary>Списано</summary>
        [dbFieldAtt(Description = "Списано")]
        WrittenOff,
        /// <summary>На обмін</summary>
        [dbFieldAtt(Description = "На обмін")]
        ForExchange,
        /// <summary>Обміняно</summary>
        [dbFieldAtt(Description = "Обміняно")]
        Exchanged,
        /// <summary>На списання</summary>
        [dbFieldAtt(Description = "На списання")]
        ToCharge,
        /// <summary>На ремонт</summary>
        [dbFieldAtt(Description = "На ремонт")]
        ToRepair
    }
}