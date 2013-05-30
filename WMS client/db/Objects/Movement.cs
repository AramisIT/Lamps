using WMS_client.Enums;
using System;

namespace WMS_client.db
{
    /// <summary>Переміщення</summary>
    public class Movement : dbObject, ISynced
    {
        #region Properties
        /// <summary>Штрихкод</summary>
        [dbFieldAtt(Description = "Штрихкод")]
        public string BarCode { get; set; }
        /// <summary>Дата</summary>
        [dbFieldAtt(Description = "Дата")]
        public DateTime Date { get; set; }
        /// <summary>Операція</summary>
        [dbFieldAtt(Description = "Операція")]
        public OperationsWithLighters Operation { get; set; }
        /// <summary>Статус синхронизации с сервером</summary>
        [dbFieldAtt(Description = "IsSynced")]
        public bool IsSynced { get; set; }
        /// <summary>Карта</summary>
        [dbFieldAtt(Description = "Карта")]
        public int Map { get; set; }
        /// <summary>Регістр</summary>
        [dbFieldAtt(Description = "Регістр")]
        public int Register { get; set; }
        /// <summary>Позиція</summary>
        [dbFieldAtt(Description = "Позиція")]
        public int Position { get; set; } 
        #endregion

        /// <summary>Переміщення</summary>
        public Movement(string barcode, string syncRef, OperationsWithLighters operation, int map, int register, int position)
        {
            BarCode = barcode;
            SyncRef = syncRef;
            Operation = operation;
            Date = DateTime.Now;
            Map = map;
            Register = register;
            Position = position;
        }

        #region Implemention of dbObject
        public override object Write()
        {
            return base.Save<Movement>();
        }

        public override object Sync()
        {
            return base.Sync<Movement>();
        } 
        #endregion

        #region Static
        /// <summary>Зарегистрировать светильник на перемещение</summary>
        /// <param name="barcode">Штрихкод светильника</param>
        /// <param name="syncRef">Ссылка синхронизации</param>
        /// <param name="operation">Операция</param>
        public static void RegisterLighter(string barcode, string syncRef, OperationsWithLighters operation)
        {
            RegisterLighter(barcode, syncRef, operation, 0, 0, 0);
        }

        /// <summary>Зарегистрировать светильник на перемещение</summary>
        /// <param name="barcode">Штрихкод светильника</param>
        /// <param name="syncRef">Ссылка синхронизации</param>
        /// <param name="operation">Операция</param>
        /// <param name="map">Карта</param>
        /// <param name="register">Регістр</param>
        /// <param name="position">Позиція</param>
        public static void RegisterLighter(string barcode, string syncRef, OperationsWithLighters operation, int map, int register, int position)
        {
            string lampBarcode;
            string lampRef;
            string unitBarcode;
            string unitRef;

            //Корпус
            Movement caseMovement = new Movement(barcode, syncRef, operation, map, register, position);
            caseMovement.Write();

            //Лампа
            if (Cases.GetMovementInfo(TypeOfAccessories.Lamp, barcode, out lampBarcode, out lampRef))
            {
                Movement lampMovement = new Movement(lampBarcode, lampRef, operation, map, register, position);
                lampMovement.Write();
            }

            //Эл.блок
            if (Cases.GetMovementInfo(TypeOfAccessories.ElectronicUnit, barcode, out unitBarcode, out unitRef))
            {
                Movement unitMovement = new Movement(unitBarcode, unitRef, operation, map, register, position);
                unitMovement.Write();
            }
        }
        #endregion
    }
}