using System.Data.SqlServerCe;
using System;
using WMS_client.Enums;
using System.Collections.Generic;

namespace WMS_client.db
{
    /// <summary>Корпус</summary>
    public class Cases : Accessory
    {
        /// <summary>Ел. блок</summary>
        [dbFieldAtt(Description = "Ел. блок", dbObjectType = typeof(ElectronicUnits), NeedDetailInfo = true)]
        public long ElectronicUnit { get; set; }
        /// <summary>Лампа</summary>
        [dbFieldAtt(Description = "Лампа", dbObjectType = typeof(Lamps), NeedDetailInfo = true)]
        public long Lamp { get; set; }
        /// <summary>Карта</summary>
        [dbFieldAtt(Description = "Карта", dbObjectType = typeof(Maps))]
        public long Map { get; set; }
        /// <summary>Позиція</summary>
        [dbFieldAtt(Description = "Позиція")]
        public int Position { get; set; }
        /// <summary>Регістр</summary>
        [dbFieldAtt(Description = "Регістр")]
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
        #region Accessory
        /// <summary>Отримати назву колонки по типу</summary>
        /// <param name="type">Тип комплектуючого</param>
        /// <returns>Назва колонки комплектуючого</returns>
        public static string GetColumnOfAccessory(TypeOfAccessories type)
        {
            return type.ToString();
        }

        /// <summary>Отримати опис комплектуючого за типом</summary>
        /// <param name="type">Тип комплектуючого</param>
        /// <returns>Опис комплектуючого</returns>
        public static string GetDescriptionOfAccessory(TypeOfAccessories type)
        {
            return EnumWorker.GetDescription(typeof(TypeOfAccessories), (int)type);
        }

        /// <summary>Отримати назву таблиці комплектуючого за типом</summary>
        /// <param name="type">Тип комплектуючого</param>
        /// <returns>Назва таблиці комплектуючого</returns>
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
                throw new Exception("Не знайдно тип комплектуючого!");
            }

            return accessory.GetType().Name;
        }

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
        /// <summary>Содержит ли корпус электроблок</summary>
        /// <param name="caseBarcode">Штрихкод корпуса</param>
        public static bool IsHaveUnit(string caseBarcode)
        {
            return GetUnitInCase(caseBarcode) != 0;
        }

        /// <summary>ID электроблока в корпусе</summary>
        /// <param name="barcode">Штрихкод корпуса</param>
        public static long GetUnitInCase(string barcode)
        {
            SqlCeCommand query = dbWorker.NewQuery(@"SELECT c.ElectronicUnit FROM Cases c WHERE RTRIM(c.Barcode)=@Barcode");
            query.AddParameter("Barcode", barcode);
            object idObj = query.ExecuteScalar();

            return idObj == null ? 0 : Convert.ToInt64(idObj);
        } 
        #endregion

        #region Lamp

        public static bool IsHaveLamp(string caseBarcode)
        {
            return GetLampInCase(caseBarcode) != 0;
        }

        public static long GetLampInCase(string barcode)
        {
            SqlCeCommand query = dbWorker.NewQuery(@"SELECT c.Lamp FROM Cases c WHERE RTRIM(c.Barcode)=@Barcode");
            query.AddParameter("Barcode", barcode);
            object idObj = query.ExecuteScalar();

            return idObj == null ? 0 : Convert.ToInt64(idObj);
        } 
        #endregion

        #region Lighter
        /// <summary>Информация по светильнику</summary>
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

        /// <summary>Изменить статус корпуса</summary>
        /// <param name="lighterBarcode">Штрихкод корпуса</param>
        /// <param name="status">Новый статус</param>
        /// <param name="remove">Процесс демонтажа?</param>
        public static void ChangeLighterStatus(string lighterBarcode, TypesOfLampsStatus status, bool remove)
        {
            //Корпус
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

            //Эл блок
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

            //Лампа
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

        /// <summary>Находится ли светильник на гарантии</summary>
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

        /// <summary>Чи містить корпус вказаний тип комплектуючого</summary>
        /// <param name="caseBarcode">Штрихкод корпусу</param>
        /// <param name="type">Тип комплектуючого</param>
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
                @"SELECT {0} FROM {1} m JOIN {0}s s ON m.{0}=s.{2} WHERE m.{3}=@{3} AND RTRIM(s.{3})=@{4}",
                accessory, typeof(Cases).Name, IDENTIFIER_NAME, BARCODE_NAME, dbSynchronizer.PARAMETER);
            SqlCeCommand query = dbWorker.NewQuery(command);
            query.AddParameter(BARCODE_NAME, lightBarcode);
            query.AddParameter(dbSynchronizer.PARAMETER, string.Empty);
            object idObj = query.ExecuteScalar();

            if (idObj != null)
            {
                lampId = Convert.ToInt64(idObj);
                return true;
            }

            lampId = 0;
            return false;
        } 
        #endregion
        #endregion
    }
}