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
        [dbFieldAtt(Description = "Штрихкод", NotShowInForm = true)]
        public string BarCode { get; set; }
        /// <summary>Дата актуальності</summary>
        [dbFieldAtt(Description = "Дата актуальності")]
        public DateTime DateOfActuality { get; set; }
        /// <summary>Знято</summary>
        [dbFieldAtt(Description = "Знято")]
        public DateTime DrawdownDate { get; set; }
        /// <summary>Відроблено годин</summary>
        [dbFieldAtt(Description = "Відроблено годин")]
        public double HoursOfWork { get; set; }
        /// <summary>Маркування</summary>
        [dbFieldAtt(Description = "Маркування")]
        public string Marking { get; set; }
        /// <summary>Модель</summary>
        [dbFieldAtt(Description = "Модель", dbObjectType = typeof(Models), NotShowInForm = true, ShowInEditForm = true)]
        public long Model { get; set; }
        /// <summary>Партія</summary>
        [dbFieldAtt(Description = "Партія", dbObjectType = typeof(Party), ShowInEditForm = true, ShowEmbadedInfo = true)]
        public long Party { get; set; }
        /// <summary>Статус</summary>
        [dbFieldAtt(Description = "Статус", ShowInEditForm = true)]
        public TypesOfLampsStatus Status { get; set; }
        /// <summary>Тип гарантії</summary>
        [dbFieldAtt(Description = "Тип гарантії", ShowInEditForm = true)]
        public TypesOfLampsWarrantly TypeOfWarrantly { get; set; }
        /// <summary>Завершення гарантії</summary>
        [dbFieldAtt(Description = "Завершення гарантії", ShowInEditForm = true)]
        public DateTime DateOfWarrantyEnd { get; set; }
        /// <summary>Статус синхронизации с сервером</summary>
        [dbFieldAtt(Description = "Синхронизовано", NotShowInForm = true)]
        public bool IsSynced { get; set; }
        /// <summary>Место нахождения</summary>
        [dbFieldAtt(Description = "Место нахождения", dbObjectType = typeof(Contractors), NotShowInForm = true)]
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

        /// <summary>Прочитати дані по комплектуючому за штрихкодом</summary>
        /// <typeparam name="T">Тип комплектуючого</typeparam>
        /// <param name="barcode">Штрихкод</param>
        /// <returns>Комплектуюче</returns>
        public virtual T Read<T>(string barcode) where T : dbObject
            {
            return Read<T>(barcode, BARCODE_NAME);
            }

        public void ClearPosition()
            {
            Cases caseAccessory = this as Cases;

            if (caseAccessory != null)
                {
                Status = TypesOfLampsStatus.Storage;
                caseAccessory.Position = 0;
                caseAccessory.Map = 0;
                caseAccessory.Register = 0;
                }
            }

        #region Static
        /// <summary>Копіювати без посилань на комплектуюче</summary>
        /// <returns>Нове комплектуюче (ще без ІД)</returns>
        public Accessory CopyWithoutLinks()
            {
            dbObject copyObj = base.Copy();

            Accessory accessoryCopy = (copyObj as Accessory);
            if (accessoryCopy != null)
                {
                accessoryCopy.Id = 0;
                accessoryCopy.BarCode = string.Empty;
                }

            Cases caseObj = copyObj as Cases;
            //BarCode = string.Empty;

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

        /// <summary>ВСтановити статус комплектующего</summary>
        /// <param name="accessory">Тип комплектуючого</param>
        /// <param name="barcode">Штихкод</param>
        /// <param name="state">Новий статус</param>
        public static void SetNewState(TypeOfAccessories accessory, string barcode, TypesOfLampsStatus state)
            {
            if (accessory == TypeOfAccessories.Case)
                {
                Cases.ChangeLighterState(barcode, state, true);
                }

            string command = string.Format(
                "UPDATE {0}s SET Status=@{1} WHERE RTRIM({2})=RTRIM(@{2})",
                accessory, SynchronizerWithGreenhouse.PARAMETER, BARCODE_NAME);
            using (SqlCeCommand query = dbWorker.NewQuery(command))
                {
                query.AddParameter(BARCODE_NAME, barcode);
                query.AddParameter(SynchronizerWithGreenhouse.PARAMETER, state);
                query.ExecuteNonQuery();
                }
            }

        /// <summary>Получить статус комплектующего</summary>
        /// <param name="accessory">Тип комплектующего</param>
        /// <param name="barcode">Штихкод</param>
        /// <returns>Статус комплектующего</returns>
        public static TypesOfLampsStatus GetState(TypeOfAccessories accessory, string barcode)
            {
            string command = string.Format("SELECT Status FROM {0}s WHERE RTRIM({1})=RTRIM(@{1})",
                                           accessory, BARCODE_NAME);
            using (SqlCeCommand query = dbWorker.NewQuery(command))
                {
                query.AddParameter(BARCODE_NAME, barcode);
                object statusObj = query.ExecuteScalar();
                int statusNumber = statusObj == null ? 0 : Convert.ToInt32(statusObj);

                return (TypesOfLampsStatus)statusNumber;
                }
            }

        /// <summary>Встановити новий статус</summary>
        /// <param name="accessory">Тип комплектуючого</param>
        /// <param name="newState">Новий статус</param>
        /// <param name="barcode">Штрихкод комплектуючого</param>
        public static void SetState(TypeOfAccessories accessory, TypesOfLampsStatus newState, string barcode)
            {
            string command = string.Format("UPDATE {0}s SET Status=@State WHERE RTRIM({1})=RTRIM(@{1})",
                                           accessory, BARCODE_NAME);
            using (SqlCeCommand query = dbWorker.NewQuery(command))
                {
                query.AddParameter("State", newState);
                query.AddParameter(BARCODE_NAME, barcode);
                query.ExecuteNonQuery();
                }
            }

        /// <summary>Получить последний созданный объект комлектующего заданого типа</summary>
        /// <param name="accessoryType">Тип</param>
        /// <param name="accessory">Последний созданный объект комлектующего</param>
        /// <returns>Вернули ли значение?</returns>
        public static bool GetLastAccesory(Type accessoryType, out Accessory accessory)
            {
            accessory = (Accessory)Activator.CreateInstance(accessoryType);
            string command = string.Format("SELECT ID FROM {0} ORDER BY Date,Id DESC", accessoryType.Name);
            using (SqlCeCommand query = dbWorker.NewQuery(command))
                {
                object idOfLastAccesory = query.ExecuteScalar();

                if (idOfLastAccesory != null)
                    {
                    accessory.Read(accessoryType, idOfLastAccesory, IDENTIFIER_NAME);
                    return true;
                    }
                }

            return false;

            }

        #endregion


        }
    }