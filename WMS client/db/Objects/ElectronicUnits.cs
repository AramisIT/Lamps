using System.Data.SqlServerCe;
using System;
using WMS_client.Enums;
using WMS_client.Utils;

namespace WMS_client.db
    {
    /// <summary>����������� ����</summary>
    public class ElectronicUnits : Accessory
        {
        private long _Case = -1;

        /// <summary>������</summary>
        public long Case
            {
            get
                {
                //if (_case < 0)
                    {
                    _Case = CatalogHelper.FindCaseId(Id, TypeOfAccessories.ElectronicUnit);
                    }
                    return _Case;
                }
            set { _Case = value; }
            }

        /// <summary>�������� ID ����� (��� BC) �� �������</summary>
        /// <param name="caseId">ID �������</param>
        /// <returns>ID �����</returns>
        public static long GetIdByEmptyBarcode(long caseId)
            {
            string command = string.Format("SELECT {0} FROM {1} WHERE [Case]=@Id",
                                           IDENTIFIER_NAME,
                                           typeof(ElectronicUnits).Name);
            using (SqlCeCommand query = dbWorker.NewQuery(command))
                {
                query.AddParameter("Id", caseId);

                object idObj = query.ExecuteScalar();
                long id = idObj == null ? 0 : Convert.ToInt64(idObj);

                return id;
                }
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