namespace WMS_client.Enums
{
    /// <summary>Фільтри</summary>
    public enum FilterSettings
    {
        /// <summary>Відсутній</summary>
        None,
        /// <summary>Не помічені на видалення</summary>
        NotMarkForDelete,
        /// <summary>Не проведені</summary>
        NotPosted,
        /// <summary>Помічені на видалення</summary>
        MarkForDelete,
        /// <summary>Проведені</summary>
        Posted,
        /// <summary>Пригодні для синхронізації</summary>
        CanSynced
    }
}