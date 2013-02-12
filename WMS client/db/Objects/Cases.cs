using System.Data.SqlServerCe;
using System;
using WMS_client.Enums;
using System.Collections.Generic;

namespace WMS_client.db
{
    /// <summary>������</summary>
    public class Cases : Accessory
    {
        #region Properties
        /// <summary>��. ����</summary>
        [dbFieldAtt(Description = "��. ����", dbObjectType = typeof(ElectronicUnits), NeedDetailInfo = true)]
        public long ElectronicUnit { get; set; }
        /// <summary>�����</summary>
        [dbFieldAtt(Description = "�����", dbObjectType = typeof(Lamps), NeedDetailInfo = true)]
        public long Lamp { get; set; }
        /// <summary>�����</summary>
        [dbFieldAtt(Description = "�����", dbObjectType = typeof(Maps))]
        public long Map { get; set; }
        /// <summary>�������</summary>
        [dbFieldAtt(Description = "�������")]
        public int Position { get; set; }
        /// <summary>������</summary>
        [dbFieldAtt(Description = "������")]
        public int Register { get; set; } 
        #endregion

        #region Save&Sync
        public override object Save()
        {
            //old:
            return Save<Cases>();
            //new:
            //return saveChanges(false, true);
        }

        public override object Sync()
        {
            return Sync<Cases>();
            //return saveChanges(true, true);
        }

//        public override object Sync<T>(bool updId)
//        {
//            return saveChanges(true, updId);
//        } 

//        private object saveChanges(bool sync, bool updId)
//        {
//            object idValue;
//            LastModified = DateTime.Now;
//            IsSynced = sync;

//            if (IsNew)
//            {
//                if (string.IsNullOrEmpty(SyncRef))
//                {
//                    SyncRef = GenerateSyncRef();
//                }

//                if (Date == SqlDateTime.MinValue.Value)
//                {
//                    Date = DateTime.Now;
//                }

//                idValue = save(updId);
//            }
//            else
//            {
//                idValue = update();
//            }

//            return idValue;
//        }

//        private object save(bool updId)
//        {
//            string query = string.Format(@"
//INSERT INTO {0}(
//{1},{2},Date,DateOfActuality,DateOfWarrantyEnd,DrawdownDate,
//ElectronicUnit,HoursOfWork,Lamp,Map,{3},Marking,Model,Number,Party,Position,
//Posted,Register,Responsible,Status,TypeOfWarrantly,{4},LastModified,Location,{5},{6})
//VALUES(
//@{1},@{2},@Date,@DateOfActuality,@DateOfWarrantyEnd,@DrawdownDate,
//@ElectronicUnit,@HoursOfWork,@Lamp,@Map,@{3},@Marking,@Model,@Number,@Party,@Position,
//@Posted,@Register,@Responsible,@Status,@TypeOfWarrantly,@{4},@LastModified,@Location,@{5},@{6})",
//                                         GetType().Name, IDENTIFIER_NAME, BARCODE_NAME, MARK_FOR_DELETING, DESCRIPTION,
//                                         IS_SYNCED, SYNCREF_NAME);
//            SqlCeCommand command = dbWorker.NewQuery(query);

//            if(updId)
//            {
//                Id = Convert.ToInt64(GetNewId());
//            }

//            command.AddParameter(IDENTIFIER_NAME, Id);
//            command.AddParameter(BARCODE_NAME, BarCode);
//            command.AddParameter("Date", Date);
//            command.AddParameter("DateOfActuality", DateOfActuality);
//            command.AddParameter("DateOfWarrantyEnd", DateOfWarrantyEnd);
//            command.AddParameter("DrawdownDate", DrawdownDate);
//            command.AddParameter("ElectronicUnit", ElectronicUnit);
//            command.AddParameter("HoursOfWork", HoursOfWork);
//            command.AddParameter("Lamp", Lamp);
//            command.AddParameter("Map", Map);
//            command.AddParameter(MARK_FOR_DELETING, MarkForDeleting);
//            command.AddParameter("Marking", Marking);
//            command.AddParameter("Model", Model);
//            command.AddParameter("Number", Number);
//            command.AddParameter("Party", Party);
//            command.AddParameter("Position", Position);
//            command.AddParameter("Posted", Posted);
//            command.AddParameter("Register", Register);
//            command.AddParameter("Responsible", Responsible);
//            command.AddParameter("Status", Status);
//            command.AddParameter("TypeOfWarrantly", TypeOfWarrantly);
//            command.AddParameter(DESCRIPTION, Description);
//            command.AddParameter("LastModified", LastModified);
//            command.AddParameter("Location", Location);
//            command.AddParameter(IS_SYNCED, Location);
//            command.AddParameter(SYNCREF_NAME, SyncRef);
//            command.ExecuteNonQuery();

//            return Id;
//        }

//        private object update()
//        {
//            string query = string.Format(@"
//UPDATE {0} SET 
//{2}=@{2},Date=@Date,DateOfActuality=@DateOfActuality,DateOfWarrantyEnd=@DateOfWarrantyEnd,
//DrawdownDate=@DrawdownDate,ElectronicUnit=@ElectronicUnit,HoursOfWork=@HoursOfWork,Lamp=@Lamp,
//Map=@Map,{3}=@{3},Marking=@Marking,Model=@Model,Number=@Number,Party=@Party,Position=@Position,
//Posted=@Posted,Register=@Register,Responsible=@Responsible,Status=@Status,
//TypeOfWarrantly=@TypeOfWarrantly,{4}=@{4},LastModified=@LastModified,Location=@Location,{5}=@{5},{6}=@{6}
//WHERE {1}=@{1}",
//                                            GetType().Name, IDENTIFIER_NAME, BARCODE_NAME, MARK_FOR_DELETING, DESCRIPTION,
//                                            IS_SYNCED, SYNCREF_NAME);
//            SqlCeCommand command = dbWorker.NewQuery(query);

//            command.AddParameter(IDENTIFIER_NAME, Id);
//            command.AddParameter(BARCODE_NAME, BarCode);
//            command.AddParameter("Date", Date);
//            command.AddParameter("DateOfActuality", DateOfActuality);
//            command.AddParameter("DateOfWarrantyEnd", DateOfWarrantyEnd);
//            command.AddParameter("DrawdownDate", DrawdownDate);
//            command.AddParameter("ElectronicUnit", ElectronicUnit);
//            command.AddParameter("HoursOfWork", HoursOfWork);
//            command.AddParameter("Lamp", Lamp);
//            command.AddParameter("Map", Map);
//            command.AddParameter(MARK_FOR_DELETING, MARK_FOR_DELETING);
//            command.AddParameter("Marking", Marking);
//            command.AddParameter("Model", Model);
//            command.AddParameter("Number", Number);
//            command.AddParameter("Party", Party);
//            command.AddParameter("Position", Position);
//            command.AddParameter("Posted", Posted);
//            command.AddParameter("Register", Register);
//            command.AddParameter("Responsible", Responsible);
//            command.AddParameter("Status", Status);
//            command.AddParameter("TypeOfWarrantly", TypeOfWarrantly);
//            command.AddParameter(DESCRIPTION, Description);
//            command.AddParameter("LastModified", LastModified);
//            command.AddParameter("Location", Location);
//            command.AddParameter(IS_SYNCED, Location);
//            command.AddParameter(SYNCREF_NAME, SyncRef);
//            command.ExecuteNonQuery();

//            return Id;
//        }
        #endregion

        #region Static
        #region Accessory
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
            return EnumWorker.GetDescription(typeof(TypeOfAccessories), (int)type);
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
                throw new Exception("�� �������� ��� ��������������!");
            }

            return accessory.GetType().Name;
        }

        /// <summary>���������� ��� ��������� ������ � "����������"</summary>
        /// <param name="accessory">��� ��������������</param>
        /// <param name="caseBarcode">�������� �������</param>
        /// <param name="barcode">�������� ��������������</param>
        /// <param name="syncRef">SyncRef ��������������</param>
        /// <returns>�� ������� ����� ��� �������������� � ������ ������</returns>
        public static bool GetMovementInfo(TypeOfAccessories accessory, string caseBarcode, out string barcode, out string syncRef)
        {
            string command = string.Format(@"
SELECT s.{0}, s.{1}
FROM Cases c 
JOIN {2}s s ON s.Id=c.{2}
WHERE RTRIM(c.{0})=RTRIM(@{0})",
                 BARCODE_NAME,
                 SYNCREF_NAME,
                 accessory.ToString());
            SqlCeCommand query = dbWorker.NewQuery(command);
            query.AddParameter(BARCODE_NAME, caseBarcode);
            SqlCeDataReader reader = query.ExecuteReader();

            if (reader != null && reader.Read())
            {
                barcode = reader[BARCODE_NAME].ToString();
                syncRef = reader[SYNCREF_NAME].ToString();
                return true;
            }

            barcode = string.Empty;
            syncRef = string.Empty;
            return false;
        }
        #endregion

        #region Unit
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

            return idObj == null ? 0 : Convert.ToInt64(idObj);
        } 
        #endregion

        #region Lamp
        /// <summary>�� ������ ������ �����?</summary>
        /// <param name="caseBarcode">�������� �������</param>
        public static bool IsHaveLamp(string caseBarcode)
        {
            return GetLampInCase(caseBarcode) != 0;
        }

        /// <summary>�������� Id ����� � �������</summary>
        /// <param name="caseBarcode">�������� �������</param>
        /// <returns>Id �����</returns>
        public static long GetLampInCase(string caseBarcode)
        {
            SqlCeCommand query = dbWorker.NewQuery(@"SELECT c.Lamp FROM Cases c WHERE RTRIM(c.Barcode)=@Barcode");
            query.AddParameter("Barcode", caseBarcode);
            object idObj = query.ExecuteScalar();

            return idObj == null ? 0 : Convert.ToInt64(idObj);
        } 
        #endregion

        #region Lighter
        /// <summary>���������� �� �����������</summary>
        /// <returns>Model, Party, DateOfWarrantyEnd, Contractor</returns>
        public static object[] GetLightInfo(string lightBarcode)
        {
            SqlCeCommand query = dbWorker.NewQuery(@"SELECT 
	m.Description Model
	, p.Description Party
	, c.DateOfWarrantyEnd
	, cc.Description Contractor
FROM Cases c
LEFT JOIN Models m ON m.Id=c.Model
LEFT JOIN Party p ON p.Id=c.Party
LEFT JOIN Contractors cc ON cc.Id=p.Contractor
WHERE RTRIM(c.Barcode)=RTRIM(@BarCode)");
            query.AddParameter("Barcode", lightBarcode);

            return query.SelectArray(new Dictionary<string, Enum> { { BaseFormatName.DateTime, DateTimeFormat.OnlyDate } });
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

        /// <summary>��������� �� ���������� �� ��������</summary>
        public static bool UnderWarranty(string lightBarcode)
        {
            SqlCeCommand query = dbWorker.NewQuery(@"SELECT 
	CASE WHEN c.DateOfWarrantyEnd>=@EndOfDay THEN 1 ELSE 0 END UnderWarranty
FROM Cases c 
LEFT JOIN Models t ON t.Id=c.Model
LEFT JOIN Party p ON p.Id=c.Party
WHERE RTRIM(c.BarCode)=RTRIM(@BarCode)");
            query.AddParameter("BarCode", lightBarcode);
            query.AddParameter("EndOfDay", DateTime.Now.Date.AddDays(1));
            object result = query.ExecuteScalar();

            return result != null && Convert.ToBoolean(result);
        }

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
        #endregion

        #region Try get accessoryId without barcode from case
        public static bool TryGetLampIdWithoutBarcode(string lightBarcode, out long lampId)
        {
            return tryGetAccessoryWithoutBarcodeFromCase(lightBarcode, "Lamp", out lampId);
        }

        public static bool TryGetUnitIdWithoutBarcode(string lightBarcode, out long unitId)
        {
            return tryGetAccessoryWithoutBarcodeFromCase(lightBarcode, "ElectronicUnit", out unitId);
        }

        private static bool tryGetAccessoryWithoutBarcodeFromCase(string lightBarcode, string accessory, out long lampId)
        {
            string command = string.Format(
                @"SELECT {0} {2},CASE WHEN RTRIM(s.{3})=@{4} THEN 1 ELSE 0 END IsEmpty FROM {1} m JOIN {0}s s ON m.{0}=s.{2} WHERE m.{3}=@{3}",
                accessory, typeof(Cases).Name, IDENTIFIER_NAME, BARCODE_NAME, dbSynchronizer.PARAMETER);
            SqlCeCommand query = dbWorker.NewQuery(command);
            query.AddParameter(BARCODE_NAME, lightBarcode);
            query.AddParameter(dbSynchronizer.PARAMETER, string.Empty);
            SqlCeDataReader reader = query.ExecuteReader();

            if(reader!=null && reader.Read())
            {
                lampId = Convert.ToInt64(reader[IDENTIFIER_NAME]);
                return Convert.ToBoolean(reader["IsEmpty"]);
            }

            lampId = 0;
            return false;
        } 
        #endregion
        #endregion
    }
}