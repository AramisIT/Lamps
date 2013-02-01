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
        /// <summary>Працює</summary>
        [dbFieldAtt(Description = "Працює")]
        IsWorking,
        /// <summary>Списан</summary>
        [dbFieldAtt(Description = "Списан")]
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
        ToRepair,
        /// <summary>У ремонті</summary>
        [dbFieldAtt(Description = "У ремонті")]
        UnderRepair,
        /// <summary>В списанні</summary>
        [dbFieldAtt(Description = "В списанні")]
        UnderCharge
    }
}