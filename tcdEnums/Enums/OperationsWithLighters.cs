namespace WMS_client.Enums
{
    /// <summary>Операции с лампами</summary>
    public enum OperationsWithLighters
    {
        /// <summary>Установка светильника\лампы на гектар</summary>
        Installing,
        /// <summary>Демонтаж светильника</summary>
        Removing,
        /// <summary>Приемка новых комплектующих</summary>
        Acceptance,
        /// <summary>Приемка комплектующих с ремонта</summary>
        AcceptanceFromRepair,
        /// <summary>Приемка комплектующих с обмена</summary>
        AcceptanceFromExchange,
        /// <summary>Приемка комплектующих со списания</summary>
        AcceptanceFromCharge,
        /// <summary>Отправка на ремонт</summary>
        SendingToRepair,
        /// <summary>Отправка на обмен</summary>
        SendingToExchange,
        /// <summary>Отправка на списание</summary>
        SendingToCharge,
        /// <summary>Регистрация</summary>
        Registration
    }
}