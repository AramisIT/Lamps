using WMS_client.db;
namespace WMS_client
    {
    /// <summary>���� ���� �������������� ��� ���������� (�����������)</summary>
    public class EditSelector : BusinessProcess
        {
        /// <summary>���� ���� �������������� ��� ���������� (�����������)</summary>
        public EditSelector(WMSClient MainProcess)
            : base(MainProcess, 1)
            {
            }

        #region Override methods
        public override sealed void DrawControls()
            {
            MainProcess.ToDoCommand = "������ �����������";
            MainProcess.CreateButton("����������� ����", 20, 75, 200, 45, "unit", unit_Click);
            MainProcess.CreateButton("�����", 20, 150, 200, 45, "lamp", lamp_Click);
            MainProcess.CreateButton("������", 20, 225, 200, 45, "case", case_Click);
            }

        public override void OnBarcode(string Barcode)
            {
            }

        public override void OnHotKey(KeyAction TypeOfAction)
            {
            switch (TypeOfAction)
                {
                case KeyAction.Esc:
                    MainProcess.ClearControls();
                    MainProcess.Process = new SelectingLampProcess(MainProcess);
                    break;
                }
            }
        #endregion

        #region ������� �� ����������� (�����������) ����������� ���� ��������������
        /// <summary>��.����</summary>
        private void unit_Click()
            {
            MainProcess.ClearControls();
            MainProcess.Process = new AccessoryRegistration(MainProcess, typeof(ElectronicUnits), null, "����������� ����");
            }

        /// <summary>�����</summary>
        private void lamp_Click()
            {
            MainProcess.ClearControls();
            MainProcess.Process = new AccessoryRegistration(MainProcess, typeof(Lamps), null, "�����");
            }

        /// <summary>������</summary>
        private void case_Click()
            {
            MainProcess.ClearControls();
            MainProcess.Process = new AccessoryRegistration(MainProcess, typeof(Cases), null, "������");
            }
        #endregion
        }
    }