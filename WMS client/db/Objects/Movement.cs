using WMS_client.Enums;
using System;

namespace WMS_client.db
{
    /// <summary>Переміщення</summary>
    public class Movement : dbObject, ISynced
    {
        #region Properties
        /// <summary>Штрихкод</summary>
        [dbFieldAtt(Description = "Штрихкод", NotShowInForm = true)]
        public string BarCode { get; set; }
        /// <summary>Дата</summary>
        [dbFieldAtt(Description = "Дата", NotShowInForm = true)]
        public DateTime Date { get; set; }
        /// <summary>Операція</summary>
        [dbFieldAtt(Description = "Операція", ShowInEditForm = true, NotShowInForm = true)]
        public OperationsWithLighters Operation { get; set; }
        /// <summary>Статус синхронизации с сервером</summary>
        [dbFieldAtt(Description = "IsSynced", NotShowInForm = true)]
        public bool IsSynced { get; set; } 
        #endregion
        
        /// <summary>Переміщення</summary>
        public Movement(string barcode, string syncRef, OperationsWithLighters operation)
        {
            BarCode = barcode;
            SyncRef = syncRef;
            Operation = operation;
            Date = DateTime.Now;
        }

        #region Implemention of dbObject
        public override object Save()
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
            string lampBarcode;
            string lampRef;
            string unitBarcode;
            string unitRef;

            //Корпус
            Movement caseMovement = new Movement(barcode, syncRef, operation);
            caseMovement.Save();

            //Лампа
            if (Cases.GetMovementInfo(TypeOfAccessories.Lamp, barcode, out lampBarcode, out lampRef))
            {
                Movement lampMovement = new Movement(lampBarcode, lampRef, operation);
                lampMovement.Save();
            }

            //Эл.блок
            if (Cases.GetMovementInfo(TypeOfAccessories.ElectronicUnit, barcode, out unitBarcode, out unitRef))
            {
                Movement unitMovement = new Movement(unitBarcode, unitRef, operation);
                unitMovement.Save();
            }
        }
        #endregion
    }
}