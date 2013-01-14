using System.Data.SqlServerCe;

namespace WMS_client.db
{
    /// <summary>��������� ��</summary>
    public static class dbArchitector
    {
        /// <summary>������� ������� Maps</summary>
        public static void CreateMapTable()
        {
            SqlCeCommand query = dbWorker.NewQuery(@"CREATE TABLE Maps(
Id bigint not null,
ParentId bigint not null,
Description nchar(50) not null,
RegisterFrom int not null,
RegisterTo int not null)");
            query.ExecuteNonQuery();
        }

        /// <summary>�������� ���� ������ � �������</summary>
        public static void ClearAllDataFromTable<T>()where T:dbObject
        {
            string tableName = typeof (T).Name;
            SqlCeCommand query = dbWorker.NewQuery(string.Concat(@"DELETE FROM ", tableName));
            query.ExecuteNonQuery();
        }

        /// <summary>�������� ���� ������ � �������</summary>
        /// <param name="tableName">��� �������</param>
        public static void ClearAllDataFromTable(string tableName)
        {
            SqlCeCommand query = dbWorker.NewQuery(string.Concat(@"DELETE FROM ", tableName));
            query.ExecuteNonQuery();
        }

        /// <summary>�������� ��� ���������</summary>
        public static void ClearAll()
        {
            ClearAllDataFromTable("Cases");
            ClearAllDataFromTable("ElectronicUnits");
            ClearAllDataFromTable("Lamps");
            ClearAllDataFromTable("Models");
            ClearAllDataFromTable("Party");
            ClearAllDataFromTable("AcceptanceOfNewComponents");
            ClearAllDataFromTable("SubAcceptanceOfNewComponentsMarkingInfo");
        }
    }
}