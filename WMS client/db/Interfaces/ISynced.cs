namespace WMS_client.db
{
    /// <summary>����������������</summary>
    public interface ISynced
    {
        /// <summary>������ ������������� � ��������</summary>
        [dbFieldAtt(Description = "IsSynced", NotShowInForm = true)]
        bool IsSynced { get; set; }
    }
}