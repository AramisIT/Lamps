using System.Data.SqlServerCe;

namespace WMS_client.db
{
    /// <summary>Создатель БД</summary>
    public static class dbArchitector
    {
        /// <summary>Создать таблицу Maps</summary>
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

        /// <summary>Удаление всех данных с таблицы</summary>
        public static void ClearAllDataFromTable<T>()where T:dbObject
        {
            string tableName = typeof (T).Name;
            SqlCeCommand query = dbWorker.NewQuery(string.Concat(@"DELETE FROM ", tableName));
            query.ExecuteNonQuery();
        }

        /// <summary>Удаление всех данных с таблицы</summary>
        /// <param name="tableName">Имя таблицы</param>
        public static void ClearAllDataFromTable(string tableName)
        {
            SqlCeCommand query = dbWorker.NewQuery(string.Concat(@"DELETE FROM ", tableName));
            query.ExecuteNonQuery();
        }

        /// <summary>Очистить все документы</summary>
        public static void ClearAll()
        {
            //Документы
            ClearAllDataFromTable(typeof(Maps).Name);
            ClearAllDataFromTable(typeof(Cases).Name);
            ClearAllDataFromTable(typeof(ElectronicUnits).Name);
            ClearAllDataFromTable(typeof(Lamps).Name);
            ClearAllDataFromTable(typeof(Models).Name);
            ClearAllDataFromTable(typeof(Party).Name);
            //Прийомка
            ClearAllDataFromTable(typeof(AcceptanceOfNewComponents).Name);
            ClearAllDataFromTable(typeof(SubAcceptanceOfNewComponentsMarkingInfo).Name);
            //Відправка на ...
            ClearAllDataFromTable(typeof(SendingToCharge).Name);
            ClearAllDataFromTable(typeof(SubSendingToChargeChargeTable).Name);
            ClearAllDataFromTable(typeof(SendingToExchange).Name);
            ClearAllDataFromTable(typeof(SubSendingToExchangeUploadTable).Name);
            ClearAllDataFromTable(typeof(SendingToRepair).Name);
            ClearAllDataFromTable(typeof(SubSendingToRepairRepairTable).Name);
            //Прийомка з ...
            ClearAllDataFromTable(typeof(AcceptanceAccessoriesFromRepair).Name);
            ClearAllDataFromTable(typeof(SubAcceptanceAccessoriesFromRepairRepairTable).Name);
            ClearAllDataFromTable(typeof(AcceptanceAccessoriesFromExchange).Name);
            ClearAllDataFromTable(typeof(AcceptanceAccessoriesFromExchangeDetails).Name);
            ClearAllDataFromTable(typeof(SubAcceptanceAccessoriesFromExchangeExchange).Name);
            //Переміщення
            ClearAllDataFromTable(typeof(Movement).Name);
        }
    }
}