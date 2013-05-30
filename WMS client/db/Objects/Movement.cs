using WMS_client.Enums;
using System;

namespace WMS_client.db
{
    /// <summary>����������</summary>
    public class Movement : dbObject, ISynced
    {
        #region Properties
        /// <summary>��������</summary>
        [dbFieldAtt(Description = "��������")]
        public string BarCode { get; set; }
        /// <summary>����</summary>
        [dbFieldAtt(Description = "����")]
        public DateTime Date { get; set; }
        /// <summary>��������</summary>
        [dbFieldAtt(Description = "��������")]
        public OperationsWithLighters Operation { get; set; }
        /// <summary>������ ������������� � ��������</summary>
        [dbFieldAtt(Description = "IsSynced")]
        public bool IsSynced { get; set; }
        /// <summary>�����</summary>
        [dbFieldAtt(Description = "�����")]
        public int Map { get; set; }
        /// <summary>������</summary>
        [dbFieldAtt(Description = "������")]
        public int Register { get; set; }
        /// <summary>�������</summary>
        [dbFieldAtt(Description = "�������")]
        public int Position { get; set; } 
        #endregion

        /// <summary>����������</summary>
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
        /// <summary>���������������� ���������� �� �����������</summary>
        /// <param name="barcode">�������� �����������</param>
        /// <param name="syncRef">������ �������������</param>
        /// <param name="operation">��������</param>
        public static void RegisterLighter(string barcode, string syncRef, OperationsWithLighters operation)
        {
            RegisterLighter(barcode, syncRef, operation, 0, 0, 0);
        }

        /// <summary>���������������� ���������� �� �����������</summary>
        /// <param name="barcode">�������� �����������</param>
        /// <param name="syncRef">������ �������������</param>
        /// <param name="operation">��������</param>
        /// <param name="map">�����</param>
        /// <param name="register">������</param>
        /// <param name="position">�������</param>
        public static void RegisterLighter(string barcode, string syncRef, OperationsWithLighters operation, int map, int register, int position)
        {
            string lampBarcode;
            string lampRef;
            string unitBarcode;
            string unitRef;

            //������
            Movement caseMovement = new Movement(barcode, syncRef, operation, map, register, position);
            caseMovement.Write();

            //�����
            if (Cases.GetMovementInfo(TypeOfAccessories.Lamp, barcode, out lampBarcode, out lampRef))
            {
                Movement lampMovement = new Movement(lampBarcode, lampRef, operation, map, register, position);
                lampMovement.Write();
            }

            //��.����
            if (Cases.GetMovementInfo(TypeOfAccessories.ElectronicUnit, barcode, out unitBarcode, out unitRef))
            {
                Movement unitMovement = new Movement(unitBarcode, unitRef, operation, map, register, position);
                unitMovement.Write();
            }
        }
        #endregion
    }
}