using System;
using System.Data;
using System.Data.SqlServerCe;
using WMS_client.Enums;

namespace WMS_client.db
    {
    /// <summary>Приемка новых комплектующих</summary>
    public class AcceptanceOfNewComponents : DocumentObject
        {
        #region Properties
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
        public WarrantyTypes TypesOfWarrantly { get; set; }
        /// <summary>Тип комплектующего</summary>
        [dbFieldAtt(Description = "TypeOfAccessories")]
        public TypeOfAccessories TypeOfAccessories { get; set; }
        /// <summary>Гарантия часов</summary>
        [dbFieldAtt(Description = "WarrantlyHours")]
        public int WarrantlyHours { get; set; }
        /// <summary>Гарантия лет</summary>
        [dbFieldAtt(Description = "WarrantlyYears")]
        public int WarrantlyYears { get; set; }
        /// <summary>Статус комплектуючого</summary>
        [dbFieldAtt(Description = "Статус комплектуючого")]
        public TypesOfLampsStatus State { get; set; }
        #endregion

        #region Query
        /// <summary>Запрос: ID всех проведенных приймок</summary>
        private const string ACCEPTED_ID_QUERY = "SELECT Id FROM AcceptanceOfNewComponents WHERE Posted=1";
        /// <summary>Запрос: ID всех НЕ проведенных приймок</summary>
        private const string NOT_ACCEPTED_ID_QUERY = "SELECT Id FROM AcceptanceOfNewComponents WHERE Posted=0";
        #endregion

        #region Static Methods

        /// <summary>Очистить проведенные документы</summary>
        public static void ClearAcceptedDocuments()
            {
            using (
                SqlCeCommand clearAccepted = dbWorker.NewQuery("DELETE FROM AcceptanceOfNewComponents WHERE Posted=1"))
                {
                clearAccepted.ExecuteNonQuery();
                }
            }

        /// <summary>Получить все проведенные документы</summary>
        /// <returns>ID всех проведенных приймок</returns>
        public static DataTable GetAcceptedDocuments()
            {
            using (SqlCeCommand command = dbWorker.NewQuery(ACCEPTED_ID_QUERY))
                {
                DataTable acceptedDocuments = command.SelectToTable();

                return acceptedDocuments;
                }
            }

        /// <summary>Получить все НЕ проведенные документы</summary>
        /// <returns>ID всех НЕ проведенных приймок</returns>
        public static DataTable GetNotAcceptedDocuments()
            {
            using (SqlCeCommand command = dbWorker.NewQuery(NOT_ACCEPTED_ID_QUERY))
                {
                DataTable acceptedDocuments = command.SelectToTable();

                return acceptedDocuments;
                }
            }

        #endregion

        #region Implemention
        public override object Write()
            {
            return base.Save<AcceptanceOfNewComponents>();
            }

        public override object Sync()
            {
            return base.Sync<AcceptanceOfNewComponents>();
            }
        #endregion
        }
    }