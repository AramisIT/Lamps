using System.Collections.Generic;
using WMS_client.Enums;
using System;
using System.Windows.Forms;
using System.Data.SqlServerCe;
using WMS_client.db;

namespace WMS_client
    {
    /// <summary>����� �������� (��� ������������)</summary>
    public class SelectingLampProcess : BusinessProcess
        {
        /// <summary>����� �������� (��� ������������)</summary>
        /// <param name="MainProcess">�������� �������</param>
        public SelectingLampProcess(WMSClient MainProcess)
            : base(MainProcess, 1)
            {
            BusinessProcessType = ProcessType.Selecting;
            FormNumber = 1;
            }

        #region Override methods
        public override void DrawControls()
            {
            MainProcess.ToDoCommand = "������ ������";
            MainProcess.CreateButton("����", 20, 75, 200, 45, "info", info_Click);
            MainProcess.CreateButton("�������", 20, 150, 200, 45, "process", process_Click);
            MainProcess.CreateButton("����������", 20, 225, 200, 45, "registration", registration_Click);
            }

        public override void OnBarcode(string Barcode)
            {
            if (Barcode.IsValidBarcode())
                {
                //��� �������������� ���������� �� ���������� (���� �� ��������, �� ��� = None)
                TypeOfAccessories type = BarcodeWorker.GetTypeOfAccessoriesByBarcode(Barcode);

                //������� �� ��������� ������ �������� �� ���� ��������������
                switch (type)
                    {
                    case TypeOfAccessories.Lamp:
                        lampProcess(Barcode);
                        break;
                    case TypeOfAccessories.ElectronicUnit:
                        unitProcess(Barcode);
                        break;
                    case TypeOfAccessories.Case:
                        caseProcess(Barcode);
                        break;
                    default:
                        ShowMessage("�� ���� ������������ � ����� ����������!");
                        break;
                    }
                }
            }

        public override void OnHotKey(KeyAction TypeOfAction)
            {
            switch (TypeOfAction)
                {
                //�����
                case KeyAction.Esc:
                    MainProcess.ClearControls();
                    MainProcess.Process = new RegistrationProcess(MainProcess);
                    break;
                //��������
                case KeyAction.Complate:
                    if (MessageBox.Show(
                        "�������� ��� ������ �� ���?",
                        "�������",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                        {
                        dbArchitector.ClearAll();
                        }
                    break;
                //�������������
                case KeyAction.Proceed:
                    new dbSynchronizer(MainProcess);
                    break;
                }
            }
        #endregion

        #region Processes
        /// <summary>������� �� ������� � ������</summary>
        /// <param name="barcode">�������� ��������������</param>
        private void lampProcess(string barcode)
            {
            MainProcess.ClearControls();
            MainProcess.Process = new ChooseLamp(MainProcess, barcode);
            }

        /// <summary>������� �� ������� � ������</summary>
        /// <param name="barcode">�������� ��������������</param>
        private void unitProcess(string barcode)
            {
            MainProcess.ClearControls();
            MainProcess.Process = new ChooseUnit(MainProcess, barcode);
            }

        /// <summary>������� �� ������� � ��������</summary>
        /// <param name="Barcode">�������� ��������������</param>
        private void caseProcess(string Barcode)
            {
            bool onHectar = isCasePerHectare(Barcode);
            object[] array;

            //�������� �� ����� = �� �������
            if (onHectar)
                {
                array = LuminaireOnHectareInfo(Barcode);

                if (array.Length != 0)
                    {
                    MainProcess.ClearControls();
                    MainProcess.Process = new ChooseLighterOnHectare(MainProcess, array, Barcode);
                    }
                }
            else
                {
                array = LuminairePerHectareInfo(Barcode);

                if (array.Length != 0)
                    {
                    MainProcess.ClearControls();
                    MainProcess.Process = new ChooseLighterPerHectare(MainProcess, array, Barcode);
                    }
                }
            }
        #endregion

        #region ButtonClick
        /// <summary>������� �� �������� "����������"</summary>
        private void info_Click()
            {
            MainProcess.ClearControls();
            MainProcess.Process = new VisualPresenter(MainProcess);
            }

        /// <summary>������� ��� ������ �������</summary>
        private void process_Click()
            {
            MainProcess.ClearControls();
            MainProcess.Process = new Processes.Lamps.Processes(MainProcess);
            }

        /// <summary>����������/�����������</summary>
        private void registration_Click()
            {
            MainProcess.ClearControls();
            MainProcess.Process = new EditSelector(MainProcess);
            }
        #endregion

        #region Query
        /// <summary>��������� �� ������?</summary>
        /// <param name="barcode">�������� ����������</param>
        /// <returns>��������� �� ������?</returns>
        private bool isCasePerHectare(string barcode)
            {
            SqlCeCommand command = dbWorker.NewQuery(@"SELECT c.Status FROM Cases c WHERE RTRIM(BarCode)=@BarCode");
            command.AddParameter("@BarCode", barcode);
            object result = command.ExecuteScalar();

            if (result == null)
                {
                return false;
                }

            TypesOfLampsStatus state = (TypesOfLampsStatus)Convert.ToInt32(result);
            return state == TypesOfLampsStatus.IsWorking;
            }

        /// <summary>���������� �� ���������� �� ������</summary>
        /// <param name="barcode">�������� ����������</param>
        /// <returns>����������</returns>
        private object[] LuminaireOnHectareInfo(string barcode)
            {
            SqlCeCommand command = dbWorker.NewQuery(@"SELECT CaseModel, CaseParty, CaseWarrantly
FROM (
    SELECT 
        0 Type
	    , t.Description CaseModel
	    , p.Description CaseParty
	    , c.DateOfWarrantyEnd CaseWarrantly
    FROM Cases c
    LEFT JOIN Models t ON t.Id=c.Model
    LEFT JOIN Party p ON p.Id=c.Party
    WHERE RTRIM(c.BarCode) like @BarCode

    UNION 

    SELECT  
        1 Type
	    , t.Description CaseModel
	    , p.Description CaseParty
	    , l.DateOfWarrantyEnd CaseWarrantly
    FROM Cases c
    LEFT JOIN Lamps l ON l.Id=c.Lamp
    LEFT JOIN Models t ON t.Id=l.Model
    LEFT JOIN Party p ON p.Id=l.Party
    WHERE RTRIM(c.BarCode) like @BarCode

    UNION 

    SELECT 
        2 Type
	    , '' CaseModel
	    , p.Description CaseParty
	    , u.DateOfWarrantyEnd CaseWarrantly
    FROM Cases c
    LEFT JOIN ElectronicUnits u ON u.Id=c.ElectronicUnit
    LEFT JOIN Models t ON t.Id=u.Model
    LEFT JOIN Party p ON p.Id=u.Party
    WHERE RTRIM(c.BarCode) like @BarCode) t
ORDER BY Type");
            command.AddParameter("@BarCode", barcode);

            return command.SelectArray(new Dictionary<string, Enum> { { BaseFormatName.DateTime, DateTimeFormat.OnlyDate } });
            }

        /// <summary>���������� �� ���������� �� �� ������</summary>
        /// <param name="barcode">�������� ����������</param>
        /// <returns>����������</returns>
        private object[] LuminairePerHectareInfo(string barcode)
            {
            SqlCeCommand command = dbWorker.NewQuery(@"
SELECT CaseModel, CaseParty, CaseWarrantly
FROM (
    SELECT
        0 Type
	    , t.Description CaseModel
	    , p.Description CaseParty
	    , c.DateOfWarrantyEnd CaseWarrantly
    FROM Cases c
    LEFT JOIN Models t ON t.Id=c.Model
    LEFT JOIN Party p ON p.Id=c.Party
    WHERE RTRIM(c.BarCode) like @BarCode

    UNION 

    SELECT
        1 Type
	    , '' CaseModel
	    , p.Description CaseParty
	    , u.DateOfWarrantyEnd CaseWarrantly
    FROM Cases c
    LEFT JOIN ElectronicUnits u ON u.Id=c.ElectronicUnit
    LEFT JOIN Models t ON t.Id=u.Model
    LEFT JOIN Party p ON p.Id=u.Party
    WHERE RTRIM(c.BarCode) like @BarCode) t
ORDER BY Type");
            command.AddParameter("@BarCode", barcode);

            return command.SelectArray(new Dictionary<string, Enum> { { BaseFormatName.DateTime, DateTimeFormat.OnlyDate } });
            }
        #endregion
        }
    }

