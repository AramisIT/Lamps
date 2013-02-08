using System;
using WMS_client.db;

namespace WMS_client
{
    /// <summary>����������� ��� �����</summary>
    public class RegistrationProcess : BusinessProcess
    {
        #region Public methods
        /// <summary>����������� ��� �����</summary>
        public RegistrationProcess(WMSClient MainProcess)
            : base(MainProcess, 1)
        {
            BusinessProcessType = ProcessType.Registration;
        }

        public override void DrawControls()
        {
            MainProcess.CreateLabel("������������ ���", 19, 165, 211, MobileFontSize.Large);
            MainProcess.ToDoCommand = "����������� � �������";

            //todo: ��������
            MainProcess.CreateButton("Enter", 10, 275, 220, 35, "enter", () => OnBarcode("9786175660690"));
        }

        public override void OnBarcode(string Barcode)
        {
            if (Barcode.IsValidBarcode())
            {
                ////if (Barcode.IndexOf("SB_EM.") < 0 || Barcode.Length == 6 || !Number.IsNumber(Barcode.Substring(6))) 
                ////{
                ////    ShowMessage("���������� ������������� �����-��� ����������");
                ////    return;
                ////}
                //PerformQuery("Registration", Int32.Parse(Barcode.Substring(6)));
                //if (Parameters == null || Parameters[0] == null) return;

                //if (!((bool)(Parameters[0])))
                //{
                //    ShowMessage("��������� �� ������ � �������!");
                //    return;
                //}

                ////����������� �������!
                ////string name = Parameters[1] as string;
                MainProcess.User = Int32.Parse(Barcode.Substring(6));
                MainProcess.ClearControls();
                //������� ���� ������ ��������
                MainProcess.Process = new SelectingLampProcess(MainProcess);
            }
        }

        public override void OnHotKey(KeyAction TypeOfAction)
        {
            switch (TypeOfAction)
            {
                case KeyAction.Proceed:
                    break;
            }
        }
        #endregion
    }
}
