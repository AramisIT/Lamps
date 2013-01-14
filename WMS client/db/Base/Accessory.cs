using System;
using WMS_client.Enums;
using System.Data.SqlTypes;
using System.Data.SqlServerCe;

namespace WMS_client.db
{
    /// <summary>�������������</summary>
    public abstract class Accessory : DocumentObject, IBarcodeOwner
    {
        #region Properties
        /// <summary>��������</summary>
        [dbAttributes(Description = "��������", NotShowInForm = true)]
        public string BarCode { get; set; }
        /// <summary>���� �����������</summary>
        [dbAttributes(Description = "���� �����������")]
        public DateTime DateOfActuality { get; set; }
        /// <summary>�����</summary>
        [dbAttributes(Description = "�����")]
        public DateTime DrawdownDate { get; set; }
        /// <summary>³�������� �����</summary>
        [dbAttributes(Description = "³�������� �����")]
        public double HoursOfWork { get; set; }
        /// <summary>����������</summary>
        [dbAttributes(Description = "����������", ShowInEditForm = true)]
        public string Marking { get; set; }
        /// <summary>������</summary>
        [dbAttributes(Description = "������", dbObjectType = typeof(Models), NotShowInForm = true, ShowInEditForm = true)]
        public long Model { get; set; }
        /// <summary>�����</summary>
        [dbAttributes(Description = "�����", dbObjectType = typeof(Party), ShowInEditForm = true, ShowEmbadedInfo = true)]
        public long Party { get; set; }
        /// <summary>������</summary>
        [dbAttributes(Description = "������")]
        public TypesOfLampsStatus Status { get; set; }
        /// <summary>��� ������</summary>
        [dbAttributes(Description = "��� ������", ShowInEditForm = true)]
        public TypesOfLampsWarrantly TypeOfWarrantly { get; set; }
        /// <summary>���������� ������</summary>
        [dbAttributes(Description = "���������� ������", ShowInEditForm = true)]
        public DateTime DateOfWarrantyEnd { get; set; }
        /// <summary>��������������</summary>
        [dbAttributes(Description = "��������������", NotShowInForm = true)]
        public bool IsSynced { get; set; }
        /// <summary>����� ����������</summary>
        [dbAttributes(Description = "����� ����������", dbObjectType = typeof(Contractors), NotShowInForm = true)]
        public long Location { get; set; }
        #endregion

        /// <summary>�������������</summary>
        protected Accessory()
        {
            BarCode = string.Empty;
            Marking = string.Empty;
            DateOfActuality = DateTime.Now;
            DrawdownDate = SqlDateTime.MinValue.Value;
            DateOfWarrantyEnd = SqlDateTime.MinValue.Value;
        }

        public virtual T Read<T>(string barcode) where T : dbObject
        {
            return Read<T>(barcode, BARCODE_NAME);
        }

        /// <summary>�������� ������ ��������������</summary>
        /// <param name="accessory">��� ��������������</param>
        /// <param name="barcode">�������</param>
        /// <returns>������ ��������������</returns>
        public static TypesOfLampsStatus GetStatus(TypeOfAccessories accessory, string barcode)
        {
            string tableName;

            switch (accessory)
            {
                    case TypeOfAccessories.Lamp:
                    tableName = "Lamps";
                    break;
                    case TypeOfAccessories.ElectronicUnit:
                    tableName = "ElectronicUnits";
                    break;
                    case TypeOfAccessories.Case:
                    tableName = "Cases";
                    break;
                default:
                    throw new Exception("�� ������������� ����������!");
            }

            string command = string.Format("SELECT Status FROM {0} WHERE RTRIM({1})=RTRIM(@Barcode)",
                                           tableName, BARCODE_NAME);
            SqlCeCommand query = dbWorker.NewQuery(command);
            query.AddParameter("barcode", barcode);
            object statusObj = query.ExecuteScalar();
            int statusNumber = statusObj == null ? 0 : Convert.ToInt32(statusObj);

            return (TypesOfLampsStatus) statusNumber;
        }
    }
}