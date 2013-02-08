using System.Collections.Generic;
using WMS_client.Base.Visual.Constructor;
using WMS_client.Enums;
using WMS_client.Processes.Lamps;
using WMS_client.db;

namespace WMS_client
{
    /// <summary>������ ���������� �� �� �������</summary>
    public class ChooseLighterPerHectare : BusinessProcess
    {
        /// <summary>�������� �����������</summary>
        private readonly string CaseBarcode;

        /// <summary>������ ���������� �� �� �������</summary>
        /// <param name="MainProcess"></param>
        /// <param name="parameters">����� ���������� � �����������</param>
        /// <param name="lampBarCode">�������� �����������</param>
        public ChooseLighterPerHectare(WMSClient MainProcess, object[] parameters, string lampBarCode)
            : base(MainProcess, 1)
        {
            Parameters = parameters;
            FormNumber = 1;
            BusinessProcessType = ProcessType.Registration;

            CaseBarcode = lampBarCode;
            
            IsLoad = true;
            DrawControls();
        }

        #region Override methods
        public override sealed void DrawControls()
        {
            if (IsLoad)
            {
                TypesOfLampsStatus state = Accessory.GetStatus(TypeOfAccessories.Case, CaseBarcode);
                ListOfLabelsConstructor list = new ListOfLabelsConstructor(MainProcess, Parameters);
                list.ListOfLabels = new List<LabelForConstructor>
                                        {
                                            new LabelForConstructor("������", ControlsStyle.LabelH2),
                                            new LabelForConstructor("������: {0}"),
                                            new LabelForConstructor("�����: {0}"),
                                            new LabelForConstructor("������� �� {0}"),
                                            new LabelForConstructor("�����������", ControlsStyle.LabelH2),
                                            new LabelForConstructor("�����: {0}",1),
                                            new LabelForConstructor("������� �� {0}"),
                                        };

                MainProcess.CreateButton("���������", 15, 225, 100, 35, "installNew", InstallNew);
                MainProcess.CreateButton("��������", 125, 225, 100, 35, "collate", Collate);

                if (state == TypesOfLampsStatus.Repair || state == TypesOfLampsStatus.ToRepair)
                {
                    MainProcess.CreateButton("���������", 15, 275, 100, 35, "storages", Storages);
                }
                else
                {
                    MainProcess.CreateButton("������", 15, 275, 100, 35, "repair", Repair);
                }

                MainProcess.CreateButton("������", 125, 275, 100, 35, "writeoff", WriteOff);
            }
        }

        public override void OnBarcode(string Barcode)
        {
            TypeOfAccessories type = BarcodeWorker.GetTypeOfAccessoriesByBarcode(Barcode);

            switch (type)
            {
                case TypeOfAccessories.Lamp:
                    MainProcess.ClearControls();
                    MainProcess.Process = new ReplacingAccessory(MainProcess, CaseBarcode, Barcode,
                                                            Cases.IsCaseHaveAccessory(CaseBarcode, type),
                                                            TypeOfAccessories.Lamp);
                    break;
                case TypeOfAccessories.Case:
                    break;
                case TypeOfAccessories.ElectronicUnit:
                    MainProcess.ClearControls();
                    MainProcess.Process = new ReplacingAccessory(MainProcess, CaseBarcode, Barcode,
                                                            Cases.IsCaseHaveAccessory(CaseBarcode, type),
                                                            TypeOfAccessories.ElectronicUnit);
                    break;
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
        /// <summary>���������</summary>
        private void InstallNew()
        {
            MainProcess.ClearControls();
            MainProcess.Process = new InstallingNewLighter(MainProcess, CaseBarcode);
        }

        /// <summary>��������</summary>
        private void Collate()
        {
            MainProcess.ClearControls();
            MainProcess.Process = new CollateLight(MainProcess, CaseBarcode);
        }

        /// <summary>������</summary>
        private void Repair()
        {
            MainProcess.ClearControls();
            MainProcess.Process = new RepairLight(MainProcess, CaseBarcode);
        }

        /// <summary>���������</summary>
        private void Storages()
        {
            MainProcess.ClearControls();
            MainProcess.Process = new SetAccessoryForStorage(MainProcess, CaseBarcode, TypeOfAccessories.Case);
        }

        /// <summary>��������</summary>
        private void WriteOff()
        {
        }
        #endregion
    }
}

