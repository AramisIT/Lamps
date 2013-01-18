using WMS_client.Enums;
using System;

namespace WMS_client.db
{
    /// <summary>Переміщення</summary>
    public class Movement : dbObject, IBarcodeOwner
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
        public Movement(string barcode, OperationsWithLighters operation)
        {
            BarCode = barcode;
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
    }
}