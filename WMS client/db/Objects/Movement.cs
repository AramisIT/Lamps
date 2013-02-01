using WMS_client.Enums;
using System;

namespace WMS_client.db
{
    /// <summary>����������</summary>
    public class Movement : dbObject, ISynced
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
        /// <summary>���������������� ���������� �� �����������</summary>
        /// <param name="barcode">�������� �����������</param>
        /// <param name="syncRef">������ �������������</param>
        /// <param name="operation">��������</param>
        public static void RegisterLighter(string barcode, string syncRef, OperationsWithLighters operation)
        {
            string lampBarcode;
            string lampRef;
            string unitBarcode;
            string unitRef;

            //������
            Movement caseMovement = new Movement(barcode, syncRef, operation);
            caseMovement.Save();

            //�����
            if (Cases.GetMovementInfo(TypeOfAccessories.Lamp, barcode, out lampBarcode, out lampRef))
            {
                Movement lampMovement = new Movement(lampBarcode, lampRef, operation);
                lampMovement.Save();
            }

            //��.����
            if (Cases.GetMovementInfo(TypeOfAccessories.ElectronicUnit, barcode, out unitBarcode, out unitRef))
            {
                Movement unitMovement = new Movement(unitBarcode, unitRef, operation);
                unitMovement.Save();
            }
        }
        #endregion
    }
}