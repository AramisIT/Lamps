using System;
using WMS_client.Enums;
using System.Data.SqlTypes;
using System.Data.SqlServerCe;

namespace WMS_client.db
{
    /// <summary>Комплектующее</summary>
    public abstract class Accessory : DocumentObject, IBarcodeOwner
    {
        #region Properties
        /// <summary>Штрихкод</summary>
        [dbAttributes(Description = "Штрихкод", NotShowInForm = true)]
        public string BarCode { get; set; }
        /// <summary>Дата актуальності</summary>
        [dbAttributes(Description = "Дата актуальності")]
        public DateTime DateOfActuality { get; set; }
        /// <summary>Знято</summary>
        [dbAttributes(Description = "Знято")]
        public DateTime DrawdownDate { get; set; }
        /// <summary>Відроблено годин</summary>
        [dbAttributes(Description = "Відроблено годин")]
        public double HoursOfWork { get; set; }
        /// <summary>Маркування</summary>
        [dbAttributes(Description = "Маркування", ShowInEditForm = true)]
        public string Marking { get; set; }
        /// <summary>Модель</summary>
        [dbAttributes(Description = "Модель", dbObjectType = typeof(Models), NotShowInForm = true, ShowInEditForm = true)]
        public long Model { get; set; }
        /// <summary>Партія</summary>
        [dbAttributes(Description = "Партія", dbObjectType = typeof(Party), ShowInEditForm = true, ShowEmbadedInfo = true)]
        public long Party { get; set; }
        /// <summary>Статус</summary>
        [dbAttributes(Description = "Статус")]
        public TypesOfLampsStatus Status { get; set; }
        /// <summary>Тип гарантії</summary>
        [dbAttributes(Description = "Тип гарантії", ShowInEditForm = true)]
        public TypesOfLampsWarrantly TypeOfWarrantly { get; set; }
        /// <summary>Завершення гарантії</summary>
        [dbAttributes(Description = "Завершення гарантії", ShowInEditForm = true)]
        public DateTime DateOfWarrantyEnd { get; set; }
        /// <summary>Синхронизовано</summary>
        [dbAttributes(Description = "Синхронизовано", NotShowInForm = true)]
        public bool IsSynced { get; set; }
        /// <summary>Место нахождения</summary>
        [dbAttributes(Description = "Место нахождения", dbObjectType = typeof(Contractors), NotShowInForm = true)]
        public long Location { get; set; }
        #endregion

        /// <summary>Комплектующее</summary>
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

        /// <summary>Получить статус комплектующего</summary>
        /// <param name="accessory">Тип комплектующего</param>
        /// <param name="barcode">Штихкод</param>
        /// <returns>Статус комплектующего</returns>
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
                    throw new Exception("Не предусмотрена реализация!");
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