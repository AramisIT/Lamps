using System;
using System.Data;
using System.Data.SqlServerCe;
using WMS_client.Enums;

namespace WMS_client.db
{
    /// <summary>Приемка новых комплектующих</summary>
    public class AcceptanceOfNewComponents : DocumentObject
    {
        /// <summary>Контрагент</summary>
        [dbFieldAtt(Description = "Contractor", dbObjectType = typeof(Contractors))]
        public long Contractor { get; set; }
        /// <summary>Дата накладной</summary>
        [dbFieldAtt(Description = "InvoiceDate")]
        public DateTime InvoiceDate { get; set; }
        /// <summary>Номер накладной</summary>
        [dbFieldAtt(Description = "InvoiceNumber")]
        public long InvoiceNumber { get; set; }
        /// <summary>Модель корпусу</summary>
        [dbFieldAtt(Description = "Модель корпусу")]
        public long CaseModel { get; set; }
        /// <summary>Модель лампи</summary>
        [dbFieldAtt(Description = "Модель лампи")]
        public long LampModel { get; set; }
        /// <summary>Модель ел.блоку</summary>
        [dbFieldAtt(Description = "Модель ел.блоку")]
        public long UnitModel { get; set; }
        /// <summary>Тип приемки</summary>
        [dbFieldAtt(Description = "TypeOfAcceptance")]
        public TypesOfLampsWarrantly TypesOfWarrantly { get; set; }
        /// <summary>Тип комплектующего</summary>
        [dbFieldAtt(Description = "TypeOfAccessories")]
        public TypeOfAccessories TypeOfAccessories { get; set; }
        /// <summary>Гарантия часов</summary>
        [dbFieldAtt(Description = "WarrantlyHours")]
        public int WarrantlyHours { get; set; }
        /// <summary>Гарантия лет</summary>
        [dbFieldAtt(Description = "WarrantlyYears")]
        public int WarrantlyYears { get; set; }

        /// <summary>Запрос: ID всех проведенных приймок</summary>
        private const string ACCEPTED_ID_QUERY = "SELECT Id FROM AcceptanceOfNewComponents WHERE Posted=1";
        /// <summary>Запрос: ID всех НЕ проведенных приймок</summary>
        private const string NOT_ACCEPTED_ID_QUERY = "SELECT Id FROM AcceptanceOfNewComponents WHERE Posted=0";

        /// <summary>Очистить все документы</summary>
        public static void ClearOldDocuments()
        {
            dbArchitector.ClearAllDataFromTable("AcceptanceOfNewComponents");
            //dbArchitector.ClearAllDataFromTable("SubAcceptanceOfNewComponentsMarkingInfo");
        }

        /// <summary>Очистить проведенные документы</summary>
        public static void ClearAcceptedDocuments()
        {
            //SqlCeCommand command = dbWorker.NewQuery(ACCEPTED_ID_QUERY);
            //List<object> acceptedDocuments = command.SelectToList();

            //if (acceptedDocuments.Count > 0)
            //{
            //    StringBuilder subDocCommand =
            //        new StringBuilder("DELETE FROM SubAcceptanceOfNewComponentsMarkingInfo WHERE ");
            //    Dictionary<string, object> parameters = new Dictionary<string, object>();
            //    int index = 0;

            //    foreach (object acceptedDocument in acceptedDocuments)
            //    {
            //        subDocCommand.AppendFormat("{0}=@{1}{2} OR ", IDENTIFIER_NAME, dbSynchronizer.PARAMETER, index);
            //        parameters.Add(dbSynchronizer.PARAMETER + index.ToString(), acceptedDocument);
            //        index++;
            //    }

            //    SqlCeCommand subDocQuery = dbWorker.NewQuery(subDocCommand.ToString(0, subDocCommand.Length - 3));
            //    subDocQuery.AddParameters(parameters);
            //    subDocQuery.ExecuteNonQuery();

            //    SqlCeCommand acceptedDocQuery = dbWorker.NewQuery("DELETE FROM AcceptanceOfNewComponents WHERE Posted=1");
            //    acceptedDocQuery.ExecuteNonQuery();
            //}

            SqlCeCommand acceptedDocQuery = dbWorker.NewQuery("DELETE FROM AcceptanceOfNewComponents WHERE Posted=1");
            acceptedDocQuery.ExecuteNonQuery();
        }

        /// <summary>Получить все проведенные документы</summary>
        /// <returns>ID всех проведенных приймок</returns>
        public static DataTable GetAcceptedDocuments()
        {
            SqlCeCommand command = dbWorker.NewQuery(ACCEPTED_ID_QUERY);
            DataTable acceptedDocuments = command.SelectToTable();

            return acceptedDocuments;
        }

        /// <summary>Получить все НЕ проведенные документы</summary>
        /// <returns>ID всех НЕ проведенных приймок</returns>
        public static DataTable GetNotAcceptedDocuments()
        {
            SqlCeCommand command = dbWorker.NewQuery(NOT_ACCEPTED_ID_QUERY);
            DataTable acceptedDocuments = command.SelectToTable();

            return acceptedDocuments;
        }

        public override object Save()
        {
            return base.Save<AcceptanceOfNewComponents>();
        }
        
        public override object Sync()
        {
            return base.Sync<AcceptanceOfNewComponents>();
        }
    }
}