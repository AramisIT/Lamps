using WMS_client.db;

namespace WMS_client.Enums
{
    /// <summary>��� �������</summary>
    public enum TypesOfAcceptance
    {
        /// <summary>����� �������������</summary>
        [dbAttributes(Description = "����� �������������")]
        IsNew,
        /// <summary>� �������</summary>
        [dbAttributes(Description = "� �������")]
        FromRepair
    }
}