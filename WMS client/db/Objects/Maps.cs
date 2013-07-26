using System.Data.SqlServerCe;
using System;

namespace WMS_client.db
    {
    /// <summary>�����</summary>
    public class Maps : CatalogObject, IBarcodeOwner
        {
        /// <summary>��������</summary>
        [dbFieldAtt(Description = "��������", NotShowInForm = true)]
        public string BarCode { get; set; }
        /// <summary>Id �������� (���� = 0 ������ ����� � �����)</summary>
        [dbFieldAtt(Description = "Id ��������")]
        public long ParentId { get; set; }
        /// <summary>������� �..</summary>
        [dbFieldAtt(Description = "������� �..")]
        public int RegisterFrom { get; set; }
        /// <summary>�������� ��..</summary>
        [dbFieldAtt(Description = "�������� ��..")]
        public int RegisterTo { get; set; }
        /// <summary>���-�� ������� �� ��������</summary>
        [dbFieldAtt(Description = "���-�� ������� �� ��������")]
        public int NumberOfPositions { get; set; }
        /// <summary>������ ������������� � ��������</summary>
        [dbFieldAtt(Description = "������ ������������� � ��������", NotShowInForm = true)]
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