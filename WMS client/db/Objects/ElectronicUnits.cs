using System.Data.SqlServerCe;
using System;

namespace WMS_client.db
{
    /// <summary>Электронный блок</summary>
    public class ElectronicUnits : Accessory
    {
        /// <summary>Корпус</summary>
        [dbFieldAtt(Description = "Корпус", dbObjectType = typeof(Cases), NeedDetailInfo = true)]
        public long Case { get; set; }

        /// <summary>Получить ID блока (без BC) по корпусу</summary>
        /// <param name="caseId">ID корпуса</param>
        /// <returns>ID блока</returns>
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