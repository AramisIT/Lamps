using System.Data.SqlServerCe;
using System;

namespace WMS_client.db
{
    /// <summary>����������� ����</summary>
    public class ElectronicUnits : Accessory
    {
        /// <summary>������</summary>
        [dbFieldAtt(Description = "������", dbObjectType = typeof(Cases), NeedDetailInfo = true)]
        public long Case { get; set; }

        /// <summary>�������� ID ����� (��� BC) �� �������</summary>
        /// <param name="caseId">ID �������</param>
        /// <returns>ID �����</returns>
        public static long GetIdByEmptyBarcode(long caseId)
        {
            string command = string.Format("SELECT {0} FROM {1} WHERE [Case]=@Id",
                                           IDENTIFIER_NAME,
                                           typeof (ElectronicUnits).Name);
            SqlCeCommand query = dbWorker.NewQuery(command);
            query.AddParameter("Id", caseId);

            object idObj = query.ExecuteScalar();
            long id = idObj == null ? 0 : Convert.ToInt64(idObj);

            return id;
        }

        #region Implemention of dbObject
        public override object Write()
        {
            return base.Save<ElectronicUnits>();
        }

        public override object Sync()
        {
            return base.Sync<ElectronicUnits>();
        } 
        #endregion
    }
}