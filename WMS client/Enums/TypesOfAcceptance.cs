using WMS_client.db;

namespace WMS_client.Enums
{
    /// <summary>��� �������</summary>
    public enum TypesOfAcceptance
    {
        /// <summary>����� �������������</summary>
        [dbFieldAtt(Description = "����� �������������")]
        IsNew,
        /// <summary>� �������</summary>
        [dbFieldAtt(Description = "� �������")]
        FromRepair
    }
}