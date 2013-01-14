using System.Data.SqlServerCe;
using System;
using WMS_client.Enums;

namespace WMS_client.db
{
    /// <summary>������</summary>
    public class Cases : Accessory
    {
        /// <summary>��. ����</summary>
        [dbAttributes(Description = "��. ����", dbObjectType = typeof(ElectronicUnits), NeedDetailInfo = true)]
        public long ElectronicUnit { get; set; }
        /// <summary>�����</summary>
        [dbAttributes(Description = "�����", dbObjectType = typeof(Lamps), NeedDetailInfo = true)]
        public long Lamp { get; set; }
        /// <summary>�����</summary>
        [dbAttributes(Description = "�����", dbObjectType = typeof(Maps))]
        public long Map { get; set; }
        /// <summary>�������</summary>
        [dbAttributes(Description = "�������")]
        public int Position { get; set; }
        /// <summary>������</summary>
        [dbAttributes(Description = "������")]
        public int Register { get; set; }

        public override object Save()
        {
            return base.Save<Cases>();
        }

        public override object Sync()
        {
            return base.Sync<Cases>();
        }

        #region Static
        /// <summary>�� ������ ������ �������� ��� ��������������</summary>
        /// <param name="caseBarcode">�������� �������</param>
        /// <param name="type">��� ��������������</param>
        public static bool IsCaseHaveAccessory(string caseBarcode, TypeOfAccessories type)
        {
            string column = GetColumnOfAccessory(type);
            SqlCeCommand query = dbWorker.NewQuery(string.Format("SELECT {0} FROM Cases WHERE BarCode=@BarCode", column));
            query.AddParameter("BarCode", caseBarcode);
            object result = query.ExecuteScalar();

            return result != null && Convert.ToInt64(result) != 0;
        }

        /// <summary>�������� ����� ������� �� ����</summary>
        /// <param name="type">��� ��������������</param>
        /// <returns>����� ������� ��������������</returns>
        public static string GetColumnOfAccessory(TypeOfAccessories type)
        {
            return type.ToString();
        }

        /// <summary>�������� ���� �������������� �� �����</summary>
        /// <param name="type">��� ��������������</param>
        /// <returns>���� ��������������</returns>
        public static string GetDescriptionOfAccessory(TypeOfAccessories type)
        {
            return EnumWorker.GetDescription(typeof(TypeOfAccessories), (int) type);
        }

        /// <summary>�������� ����� ������� �������������� �� �����</summary>
        /// <param name="type">��� ��������������</param>
        /// <returns>����� ������� ��������������</returns>
        public static string GetTableNameForAccessory(TypeOfAccessories type)
        {
            Accessory accessory = null;

            switch (type)
            {
                case TypeOfAccessories.Lamp:
                    accessory = new Lamps();
                    break;
                case TypeOfAccessories.ElectronicUnit:
                    accessory = new ElectronicUnits();
                    break;
                case TypeOfAccessories.Case:
                    accessory = new Cases();
                    break;
            }

            if (accessory == null)
            {
                throw new Exception("�� ������� ��� ��������������!");
            }

            return accessory.GetType().Name;
        }

        /// <summary>�������� �� ������ �����������</summary>
        /// <param name="caseBarcode">�������� �������</param>
        public static bool IsHaveUnit(string caseBarcode)
        {
            return GetUnitInCase(caseBarcode) != 0;
        }

        /// <summary>ID ������������ � �������</summary>
        /// <param name="barcode">�������� �������</param>
        public static long GetUnitInCase(string barcode)
        {
            SqlCeCommand query = dbWorker.NewQuery(@"SELECT c.ElectronicUnit FROM Cases c WHERE RTRIM(c.Barcode)=@Barcode");
            query.AddParameter("Barcode", barcode);
            object idObj = query.ExecuteScalar();

            return idObj==null? 0: Convert.ToInt64(idObj);
        }

        /// <summary>�������� ������ �������</summary>
        /// <param name="lighterBarcode">�������� �������</param>
        /// <param name="status">����� ������</param>
        /// <param name="remove">������� ���������?</param>
        public static void ChangeLighterStatus(string lighterBarcode, TypesOfLampsStatus status, bool remove)
        {
            //������
            string command = string.Format(
                "UPDATE Cases SET Map=0,Register=0,Position=0,Status=@Status,{0}=0,DateOfActuality=@Date{1} WHERE RTRIM(Barcode)=@Barcode",
                IS_SYNCED,
                remove ? ",DrawdownDate=@DrawdownDate" : string.Empty);
            SqlCeCommand query = dbWorker.NewQuery(command);
            query.AddParameter("Barcode", lighterBarcode);
            query.AddParameter("Status", status);
            query.AddParameter("Date", DateTime.Now);
            query.AddParameter("DrawdownDate", DateTime.Now);
            query.ExecuteNonQuery();

            //�� ����
            object caseId = BarcodeWorker.GetIdByBarcode(lighterBarcode);
            command = string.Format(
                "UPDATE ElectronicUnits SET Status=@Status,{0}=0,DateOfActuality=@Date{1} WHERE [Case]=@Id",
                IS_SYNCED,
                remove ? ",DrawdownDate=@DrawdownDate" : string.Empty);
            query = dbWorker.NewQuery(command);
            query.AddParameter("Status", status);
            query.AddParameter("Id", caseId);
            query.AddParameter("Date", DateTime.Now);
            query.AddParameter("DrawdownDate", DateTime.Now);
            query.ExecuteNonQuery();

            //�����
            command = string.Format(
                "UPDATE Lamps SET Status=@Status,{0}=0,DateOfActuality=@Date{1} WHERE [Case]=@Id",
                IS_SYNCED,
                remove ? ",DrawdownDate=@DrawdownDate" : string.Empty);
            query = dbWorker.NewQuery(command);
            query.AddParameter("Status", status);
            query.AddParameter("Id", caseId);
            query.AddParameter("Date", DateTime.Now);
            query.AddParameter("DrawdownDate", DateTime.Now);
            query.ExecuteNonQuery();
        }
        #endregion
    }
}