using System.Collections.Generic;
using WMS_client.Base.Visual.Constructor;
using WMS_client.Enums;
using WMS_client.Processes.Lamps;
using WMS_client.db;

namespace WMS_client
{
    /// <summary>������ ���������� �� �������</summary>
    public class ChooseLighterOnHectare : BusinessProcess
    {
        /// <summary>�������� ����������</summary>
        private readonly string CaseBarcode;

        /// <summary>������ ���������� �� �������</summary>
        /// <param name="MainProcess"></param>
        /// <param name="parameters">����� ���. � �����������</param>
        /// <param name="caseBarcode">�������� ����������</param>
        public ChooseLighterOnHectare(WMSClient MainProcess, object[] parameters, string caseBarcode) : base(MainProcess, 1)
        {
            Parameters = parameters;
            FormNumber = 1;
            BusinessProcessType = ProcessType.Registration;
            CaseBarcode = caseBarcode;
            
            IsLoad = true;
            DrawControls();
        }

        #region Override methods
        public override sealed void DrawControls()
        {
            if (IsLoad)
            {
                ListOfLabelsConstructor list = new ListOfLabelsConstructor(MainProcess, Parameters);
                list.ListOfLabels = new List<LabelForConstructor>
                                        {
                                            new LabelForConstructor("������", ControlsStyle.LabelH2),
                                            new LabelForConstructor("������: {0}"),
                                            new LabelForConstructor("�����: {0}"),
                                            new LabelForConstructor("������� �� {0}"),
                                            new LabelForConstructor("�����", ControlsStyle.LabelH2),
                                            new LabelForConstructor("������: {0}"),
                                            new LabelForConstructor("�����: {0}"),
                                            new LabelForConstructor("������� �� {0}"),
                                            new LabelForConstructor("�����������", ControlsStyle.LabelH2),
                                            new LabelForConstructor("�����: {0}",1),
                                            new LabelForConstructor("������� �� {0}"),
                                        };

                MainProcess.CreateButton("��������", 15, 275, 100, 35, "breakDown", BreakDown);
                MainProcess.CreateButton("����������", 125, 275, 100, 35, "installNew", InstallNew);
            }
        }

        public override void OnBarcode(string Barcode)
        {
            if (Barcode.IsValidBarcode())
            {
                //��� ��������������� ��������������
                TypeOfAccessories type = BarcodeWorker.GetTypeOfAccessoriesByBarcode(Barcode);

                switch (type)
                {
                        //����� - ���������/������ �����
                    case TypeOfAccessories.Lamp:
                        MainProcess.ClearControls();
                        MainProcess.Process = new ReplacingAccessory(
                            MainProcess, CaseBarcode, Barcode,
                            Cases.IsCaseHaveAccessory(CaseBarcode, TypeOfAccessories.Lamp),
                            TypeOfAccessories.Lamp);
                        break;
                        //������ - ���������/������ �������
                    case TypeOfAccessories.Case:
                        MainProcess.ClearControls();
                        MainProcess.Process = new ReplaceLights_SelectNew(MainProcess, Barcode, CaseBarcode);
                        break;
                        //��.���� - ���������/������ �����
                    case TypeOfAccessories.ElectronicUnit:
                        MainProcess.ClearControls();
                        MainProcess.Process = new ReplacingAccessory(
                            MainProcess, CaseBarcode, Barcode,
                            Cases.IsCaseHaveAccessory(CaseBarcode, TypeOfAccessories.ElectronicUnit),
                            TypeOfAccessories.ElectronicUnit);
                        break;
                }
            }
        }

        public override void OnHotKey(KeyAction TypeOfAction)
        {
            switch (TypeOfAction)
            {
                case KeyAction.Esc:
                    MainProcess.ClearControls();
                    MainProcess.Process = new SelectingLampProcess(MainProcess);
                    break;
                case KeyAction.Proceed:
                    OnBarcode("9786175660690");
                    break;
            }
        }
        #endregion

        #region ButtonClick
        /// <summary>��������</summary>
        private void BreakDown()
        {
            MainProcess.ClearControls();
            MainProcess.Process = new RemovalLight(MainProcess, CaseBarcode);
        }

        /// <summary>���������</summary>
        private void InstallNew()
        {
            MainProcess.ClearControls();
            MainProcess.Process = new InstallingNewLighter(MainProcess, CaseBarcode);
        }
        #endregion
    }
}

