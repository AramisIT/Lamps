using System.Data.SqlServerCe;
using System;

namespace WMS_client.db
    {
    /// <summary>Карта</summary>
    public class Maps : CatalogObject, IBarcodeOwner
        {
        /// <summary>Штрихкод</summary>
        [dbFieldAtt(Description = "Штрихкод", NotShowInForm = true)]
        public string BarCode { get; set; }
        /// <summary>Id родителя (если = 0 значит лажит в корне)</summary>
        [dbFieldAtt(Description = "Id родителя")]
        public long ParentId { get; set; }
        /// <summary>Регистр с..</summary>
        [dbFieldAtt(Description = "Регистр с..")]
        public int RegisterFrom { get; set; }
        /// <summary>Регистры по..</summary>
        [dbFieldAtt(Description = "Регистры по..")]
        public int RegisterTo { get; set; }
        /// <summary>Кол-во позиций на регистре</summary>
        [dbFieldAtt(Description = "Кол-во позиций на регистре")]
        public int NumberOfPositions { get; set; }
        /// <summary>Статус синхронизации с сервером</summary>
        [dbFieldAtt(Description = "Статус синхронизации с сервером", NotShowInForm = true)]
        public bool IsSynced { get; set; }

        public override object Write()
            {
            return base.Save<Maps>();
            }

        public override object Sync()
            {
            return base.Sync<Maps>();
            }

        public static int GetMaxPositionNumber(object mapId)
            {
            string query = string.Format("SELECT NumberOfPositions FROM {0} WHERE {1}=@{1}",
                                         typeof(Maps).Name, IDENTIFIER_NAME);
            using (SqlCeCommand command = dbWorker.NewQuery(query))
                {
                command.AddParameter(IDENTIFIER_NAME, mapId);
                object result = command.ExecuteScalar();

                return result == null ? 0 : Convert.ToInt32(result);
                }
            }
        }
    }