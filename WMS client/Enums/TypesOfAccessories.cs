using WMS_client.db;

namespace WMS_client.Enums
{
    public enum TypeOfAccessories
    {
        /// <summary>Не выбрано</summary>
        [dbAttributes(Description = "Не обрано")]
        None,
        /// <summary>Лампа</summary>
        [dbAttributes(Description = "Лампа")]
        Lamp,
        /// <summary>Корпус</summary>
        [dbAttributes(Description = "Корпус")]
        Case,
        /// <summary>Эл.блок</summary>
        [dbAttributes(Description = "Електроний блок")]
        ElectronicUnit
    }
}
