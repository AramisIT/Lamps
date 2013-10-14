using System;
using System.Data;
using System.Data.SqlServerCe;
using WMS_client.Enums;

namespace WMS_client.db
    {
    /// <summary>������� ����� �������������</summary>
    public class AcceptanceOfNewComponents : DocumentObject
        {
        #region Properties
        /// <summary>����������</summary>
        [dbFieldAtt(Description = "Contractor", dbObjectType = typeof(Contractors))]
        public long Contractor { get; set; }
        /// <summary>���� ���������</summary>
        [dbFieldAtt(Description = "InvoiceDate")]
        public DateTime InvoiceDate { get; set; }
        /// <summary>����� ���������</summary>
        [dbFieldAtt(Description = "InvoiceNumber")]
        public long InvoiceNumber { get; set; }
        /// <summary>������ �������</summary>
        [dbFieldAtt(Description = "������ �������")]
        public long CaseModel { get; set; }
        /// <summary>������ �����</summary>
        [dbFieldAtt(Description = "������ �����")]
        public long LampModel { get; set; }
        /// <summary>������ ��.�����</summary>
        [dbFieldAtt(Description = "������ ��.�����")]
        public long UnitModel { get; set; }
        /// <summary>��� �������</summary>
        [dbFieldAtt(Description = "TypeOfAcceptance")]
        public WarrantyTypes TypesOfWarrantly { get; set; }
        /// <summary>��� ��������������</summary>
        [dbFieldAtt(Description = "TypeOfAccessories")]
        public TypeOfAccessories TypeOfAccessories { get; set; }
        /// <summary>�������� �����</summary>
        [dbFieldAtt(Description = "WarrantlyHours")]
        public int WarrantlyHours { get; set; }
        /// <summary>�������� ���</summary>
        [dbFieldAtt(Description = "WarrantlyYears")]
        public int WarrantlyYears { get; set; }
        /// <summary>������ ��������������</summary>
        [dbFieldAtt(Description = "������ ��������������")]
        public TypesOfLampsStatus State { get; set; }
        #endregion

        #region Query
        /// <summary>������: ID ���� ����������� �������</summary>
        private const string ACCEPTED_ID_QUERY = "SELECT Id FROM AcceptanceOfNewComponents WHERE Posted=1";
        /// <summary>������: ID ���� �� ����������� �������</summary>
        private const string NOT_ACCEPTED_ID_QUERY = "SELECT Id FROM AcceptanceOfNewComponents WHERE Posted=0";
        #endregion

        #region Static Methods

        /// <summary>�������� ����������� ���������</summary>
        public static void ClearAcceptedDocuments()
            {
            using (
                SqlCeCommand clearAccepted = dbWorker.NewQuery("DELETE FROM AcceptanceOfNewComponents WHERE Posted=1"))
                {
                clearAccepted.ExecuteNonQuery();
                }
            }

        /// <summary>�������� ��� ����������� ���������</summary>
        /// <returns>ID ���� ����������� �������</returns>
        public static DataTable GetAcceptedDocuments()
            {
            using (SqlCeCommand command = dbWorker.NewQuery(ACCEPTED_ID_QUERY))
                {
                DataTable acceptedDocuments = command.SelectToTable();

                return acceptedDocuments;
                }
            }

        /// <summary>�������� ��� �� ����������� ���������</summary>
        /// <returns>ID ���� �� ����������� �������</returns>
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