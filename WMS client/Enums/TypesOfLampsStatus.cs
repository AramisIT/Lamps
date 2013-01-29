using WMS_client.db;

namespace WMS_client.Enums
{
    /// <summary>Тип статусів лампи</summary>
    public enum TypesOfLampsStatus
    {
        /// <summary>Хранение</summary>
        [dbFieldAtt(Description = "Хранение")]
        Storage,
        /// <summary>Ремонт</summary>
        [dbFieldAtt(Description = "Ремонт")]
        Repair,
        /// <summary>Работает</summary>
        [dbFieldAtt(Description = "Работает")]
        IsWorking,
        /// <summary>Списан</summary>
        [dbFieldAtt(Description = "Списан")]
        WrittenOff,
        /// <summary>На обмен</summary>
        [dbFieldAtt(Description = "На обмен")]
        ForExchange,
        /// <summary>Обменян</summary>
        [dbFieldAtt(Description = "Обменян")]
        Exchanged,
        /// <summary>На списание</summary>
        [dbFieldAtt(Description = "На списание")]
        ToCharge,
        /// <summary>На ремонт</summary>
        [dbFieldAtt(Description = "На ремонт")]
        ToRepair,
        /// <summary>В ремонте</summary>
        [dbFieldAtt(Description = "В ремонте")]
        UnderRepair,
        /// <summary>В списании</summary>
        [dbFieldAtt(Description = "В списании")]
        UnderCharge
    }
}