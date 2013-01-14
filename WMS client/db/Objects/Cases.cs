using System.Data.SqlServerCe;
using System;
using WMS_client.Enums;

namespace WMS_client.db
{
    /// <summary>Корпус</summary>
    public class Cases : Accessory
    {
        /// <summary>Ел. блок</summary>
        [dbAttributes(Description = "Ел. блок", dbObjectType = typeof(ElectronicUnits), NeedDetailInfo = true)]
        public long ElectronicUnit { get; set; }
        /// <summary>Лампа</summary>
        [dbAttributes(Description = "Лампа", dbObjectType = typeof(Lamps), NeedDetailInfo = true)]
        public long Lamp { get; set; }
        /// <summary>Карта</summary>
        [dbAttributes(Description = "Карта", dbObjectType = typeof(Maps))]
        public long Map { get; set; }
        /// <summary>Позиція</summary>
        [dbAttributes(Description = "Позиція")]
        public int Position { get; set; }
        /// <summary>Регістр</summary>
        [dbAttributes(Description = "Регістр")]
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
            return EnumWorker.GetDescription(typeof(TypeOfAccessories), (int) type);
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

            return idObj==null? 0: Convert.ToInt64(idObj);
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
        #endregion
    }
}