namespace WMS_client.Enums
{
    public enum FilterSettings
    {
        /// <summary>Фильтр отсутствует</summary>
        None,
        /// <summary>Не удаленные</summary>
        NotMarkForDelete,
        /// <summary>Не проведенные</summary>
        NotPosted,
        /// <summary>Удаленные</summary>
        MarkForDelete,
        /// <summary>Проведенные</summary>
        Posted,
        /// <summary>Пригодные для синхронизации</summary>
        CanSynced
    }
}