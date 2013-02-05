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
        [dbFieldAtt(Description = "��������", NotShowInForm = true)]
        public string BarCode { get; set; }
        /// <summary>���� �����������</summary>
        [dbFieldAtt(Description = "���� �����������")]
        public DateTime DateOfActuality { get; set; }
        /// <summary>�����</summary>
        [dbFieldAtt(Description = "�����")]
        public DateTime DrawdownDate { get; set; }
        /// <summary>³�������� �����</summary>
        [dbFieldAtt(Description = "³�������� �����")]
        public double HoursOfWork { get; set; }
        /// <summary>����������</summary>
        [dbFieldAtt(Description = "����������")]
        public string Marking { get; set; }
        /// <summary>������</summary>
        [dbFieldAtt(Description = "������", dbObjectType = typeof(Models), NotShowInForm = true, ShowInEditForm = true)]
        public long Model { get; set; }
        /// <summary>�����</summary>
        [dbFieldAtt(Description = "�����", dbObjectType = typeof(Party), ShowInEditForm = true, ShowEmbadedInfo = true)]
        public long Party { get; set; }
        /// <summary>������</summary>
        [dbFieldAtt(Description = "������")]
        public TypesOfLampsStatus Status { get; set; }
        /// <summary>��� ������</summary>
        [dbFieldAtt(Description = "��� ������", ShowInEditForm = true)]
        public TypesOfLampsWarrantly TypeOfWarrantly { get; set; }
        /// <summary>���������� ������</summary>
        [dbFieldAtt(Description = "���������� ������", ShowInEditForm = true)]
        public DateTime DateOfWarrantyEnd { get; set; }
        /// <summary>������ ������������� � ��������</summary>
        [dbFieldAtt(Description = "��������������", NotShowInForm = true)]
        public bool IsSynced { get; set; }
        /// <summary>����� ����������</summary>
        [dbFieldAtt(Description = "����� ����������", dbObjectType = typeof(Contractors), NotShowInForm = true)]
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

        /// <summary>��������� ��� �� �������������� �� ����������</summary>
        /// <typeparam name="T">��� ��������������</typeparam>
        /// <param name="barcode">��������</param>
        /// <returns>������������</returns>
        public virtual T Read<T>(string barcode) where T : dbObject
        {
            return Read<T>(barcode, BARCODE_NAME);
        }

        #region Static
        /// <summary>�������� ��� �������� �� ������������</summary>
        /// <returns>���� ������������ (�� ��� ��)</returns>
        public Accessory CopyWithoutLinks()
        {
            dbObject copyObj = base.Copy();
            Cases caseObj = copyObj as Cases;

            if (caseObj != null)
            {
                caseObj.Lamp = 0;
                caseObj.ElectronicUnit = 0;

                return caseObj;
            }

            ElectronicUnits unitObj = copyObj as ElectronicUnits;

            if (unitObj != null)
            {
                unitObj.Case = 0;
                return unitObj;
            }

            Lamps lampObj = copyObj as Lamps;

            if (lampObj != null)
            {
                lampObj.Case = 0;
                return lampObj;
            }

            return (Accessory)copyObj;
        }

        /// <summary>�������� ������ ��������������</summary>
        /// <param name="accessory">��� ��������������</param>
        /// <param name="barcode">�������</param>
        /// <returns>������ ��������������</returns>
        public static TypesOfLampsStatus GetStatus(TypeOfAccessories accessory, string barcode)
        {
            string command = string.Format("SELECT Status FROM {0}s WHERE RTRIM({1})=RTRIM(@{1})",
                                           accessory, BARCODE_NAME);
            SqlCeCommand query = dbWorker.NewQuery(command);
            query.AddParameter(BARCODE_NAME, barcode);
            object statusObj = query.ExecuteScalar();
            int statusNumber = statusObj == null ? 0 : Convert.ToInt32(statusObj);

            return (TypesOfLampsStatus)statusNumber;
        }

        /// <summary>�������� ��������� ��������� ������ ������������� �������� ����</summary>
        /// <param name="accessoryType">���</param>
        /// <param name="accessory">��������� ��������� ������ �������������</param>
        /// <returns>������� �� ��������?</returns>
        public static bool GetLastAccesory(Type accessoryType, out Accessory accessory)
        {
            accessory = (Accessory)Activator.CreateInstance(accessoryType);
            string command = string.Format("SELECT ID FROM {0} ORDER BY Date,Id DESC", accessoryType.Name);
            SqlCeCommand query = dbWorker.NewQuery(command);
            object idOfLastAccesory = query.ExecuteScalar();

            if (idOfLastAccesory != null)
            {
                accessory.Read(accessoryType, idOfLastAccesory, IDENTIFIER_NAME);
                return true;
            }

            return false;
        } 
        #endregion
    }
}