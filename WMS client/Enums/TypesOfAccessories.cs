using WMS_client.db;

namespace WMS_client.Enums
{
    /// <summary>Тип комплектуючого</summary>
    public enum TypeOfAccessories
    {
        /// <summary>Не выбрано</summary>
        [dbFieldAtt(Description = "Не обрано")]
        None,
        /// <summary>Лампа</summary>
        [dbFieldAtt(Description = "Лампа")]
        Lamp,
        /// <summary>Корпус</summary>
        [dbFieldAtt(Description = "Корпус")]
        Case,
        /// <summary>Эл.блок</summary>
        [dbFieldAtt(Description = "Електроний блок")]
        ElectronicUnit
    }
}
