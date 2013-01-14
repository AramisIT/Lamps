namespace WMS_client.db
{
    /// <summary>Синхронизируемый</summary>
    public interface ISynced
    {
        /// <summary>Статус синхронизации с сервером</summary>
        [dbAttributes(Description = "IsSynced", NotShowInForm = true)]
        bool IsSynced { get; set; }
    }
}