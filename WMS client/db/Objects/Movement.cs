using WMS_client.Enums;
using System;

namespace WMS_client.db
{
    /// <summary>����������</summary>
    public class Movement : dbObject, IBarcodeOwner
    {
        #region Properties
        /// <summary>��������</summary>
        [dbFieldAtt(Description = "��������", NotShowInForm = true)]
        public string BarCode { get; set; }
        /// <summary>����</summary>
        [dbFieldAtt(Description = "����", NotShowInForm = true)]
        public DateTime Date { get; set; }
        /// <summary>��������</summary>
        [dbFieldAtt(Description = "��������", ShowInEditForm = true, NotShowInForm = true)]
        public OperationsWithLighters Operation { get; set; }
        /// <summary>������ ������������� � ��������</summary>
        [dbFieldAtt(Description = "IsSynced", NotShowInForm = true)]
        public bool IsSynced { get; set; } 
        #endregion

        /// <summary>����������</summary>
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