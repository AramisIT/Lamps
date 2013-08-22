using System.Collections.Generic;
using WMS_client.Base.Visual.Constructor;
using WMS_client.db;
using System.Data.SqlServerCe;

namespace WMS_client
    {
    /// <summary>���������� ����������</summary>
    public class CollateLight : BusinessProcess
        {
        #region Properties
        private enum Stages { Begin, Lamp, Unit }

        private readonly string LightBarcode;
        private const string TOPIC_OF_PROCESS = "��������";
        private Stages stage;
        private long lampId;
        private long unitId;
        private string lampBarcode;
        private string unitBarcode;
        #endregion

        /// <summary>���������� ����������</summary>
        public CollateLight(WMSClient MainProcess, string lightBarcode)
            : base(MainProcess, 1)
            {
            LightBarcode = lightBarcode;

            IsLoad = true;
            stage = Stages.Begin;
            DrawControls();
            }

        #region Override methods
        public override sealed void DrawControls()
            {
            if (IsLoad)
                {
                bool underWarrantly = Cases.UnderWarranty(LightBarcode);
                object[] lightData = Cases.GetLightInfo(LightBarcode);

                ListOfLabelsConstructor list = new ListOfLabelsConstructor(MainProcess, TOPIC_OF_PROCESS, lightData);
                list.ListOfLabels = new List<LabelForConstructor>
                                        {
                                            new LabelForConstructor(
                                                underWarrantly ? "�����! ���������" : string.Empty,
                                                ControlsStyle.LabelH2Red),
                                            new LabelForConstructor(
                                                underWarrantly ? "����������� �� ������!" : string.Empty,
                                                ControlsStyle.LabelH2Red),
                                            new LabelForConstructor(string.Empty, false),
                                            new LabelForConstructor("������: {0}"),
                                            new LabelForConstructor("�����: {0}"),
                                            new LabelForConstructor("������� �� {0}"),
                                            new LabelForConstructor("���������� {0}"),
                                            new LabelForConstructor(string.Empty, false),
                                            new LabelForConstructor("�������� ���������?", ControlsStyle.LabelH2Red)
                                        };

                MainProcess.CreateButton("���", 10, 275, 105, 35, "ok", Ok_click);
                MainProcess.CreateButton("ͳ", 125, 275, 105, 35, "cancel", Cancel_click);
                }
            }

        public override void OnBarcode(string Barcode)
            {
            if (Barcode.IsAccessoryBarcode())
                {
                if (BarcodeWorker.IsBarcodeExist(Barcode))
                    {
                    ShowMessage("�������� ��� ������������!");
                    }
                else
                    {
                    if (stage == Stages.Lamp)
                        {
                        lampBarcode = Barcode;
                        goToNextStage();
                        }
                    else if (stage == Stages.Unit)
                        {
                        unitBarcode = Barcode;
                        goToNextStage();
                        }
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
                }
            }
        #endregion

        #region ButtonClick
        private void Ok_click()
            {
            goToNextStage();
            }

        private void Save_click()
            {
            saveData();
            }

        private void Cancel_click()
            {
            MainProcess.ClearControls();
            MainProcess.Process = new SelectingLampProcess(MainProcess);
            }
        #endregion

        #region Stages
        private void goToNextStage()
            {
            if (stage == Stages.Begin && (collateLamp() || collateUnit() || finishCollate()))
                {
                return;
                }

            if (stage == Stages.Lamp && (collateUnit() || finishCollate()))
                {
                return;
                }

            finishCollate();
            }

        /// <summary>���� ���������� ��������� �����</summary>
        private bool collateLamp()
            {
            if (Cases.TryGetLampIdWithoutBarcode(LightBarcode, out lampId))
                {
                stage = Stages.Lamp;
                drawScanForm("�����");
                return true;
                }

            return false;
            }

        /// <summary>���� ���������� ��������� ��.�����</summary>
        private bool collateUnit()
            {
            if (Cases.TryGetUnitIdWithoutBarcode(LightBarcode, out unitId))
                {
                stage = Stages.Unit;
                drawScanForm("������������");
                return true;
                }

            return false;
            }

        /// <summary>���� ����������</summary>
        private bool finishCollate()
            {
            drawFinishForm();
            return true;
            }

        /// <summary>���������� �����</summary>
        private void saveData()
            {
            saveCaseData();

            if (lampId != 0)
                {
                saveLampData();
                }

            if (unitId != 0)
                {
                saveUnitData();
                }

            MainProcess.ClearControls();
            MainProcess.Process = new SelectingLampProcess(MainProcess);
            }
        #endregion

        #region Draw stage form
        private void drawScanForm(string accessoryStr)
            {
            MainProcess.ClearControls();
            MainProcess.ToDoCommand = TOPIC_OF_PROCESS;
            MainProcess.CreateLabel("³�������� ����� ���", 5, 150, 230,
                                    MobileFontSize.Large, MobileFontPosition.Center, MobileFontColors.Default);
            MainProcess.CreateLabel(accessoryStr, 5, 185, 230,
                                    MobileFontSize.Large, MobileFontPosition.Center, MobileFontColors.Info);
            }

        private void drawFinishForm()
            {
            MainProcess.ClearControls();
            MainProcess.ToDoCommand = TOPIC_OF_PROCESS;
            MainProcess.CreateLabel("��������� ��������!", 5, 150, 230,
                                    MobileFontSize.Large, MobileFontPosition.Center, MobileFontColors.Default);
            MainProcess.CreateLabel("�������� ���?", 5, 185, 230,
                                    MobileFontSize.Large, MobileFontPosition.Center, MobileFontColors.Info);
            MainProcess.CreateButton("��", 10, 275, 105, 35, "ok", Save_click);
            MainProcess.CreateButton("³����", 125, 275, 105, 35, "cancel", Cancel_click);
            }
        #endregion

        #region Query

        private void saveCaseData()
            {
            string command = string.Format(
                "UPDATE {0} SET Lamp=0,ElectronicUnit=0,{1}=0 WHERE {2}=@{2}",
                typeof(Cases).Name, dbObject.IS_SYNCED, dbObject.BARCODE_NAME);

            using (SqlCeCommand query = dbWorker.NewQuery(command))
                {
                query.AddParameter(dbObject.BARCODE_NAME, LightBarcode);
                query.ExecuteNonQuery();
                }
            }

        private void saveLampData()
            {
            string command = string.Format(
                "UPDATE {0} SET {1}[Case]=0,{2}=0 WHERE {3}=@{3}",
                typeof(Lamps).Name,
                string.IsNullOrEmpty(lampBarcode) ? string.Empty : string.Format("{0}=@{0}, ", dbObject.BARCODE_NAME),
                dbObject.IS_SYNCED,
                dbObject.IDENTIFIER_NAME);
            using (SqlCeCommand query = dbWorker.NewQuery(command))
                {
                query.AddParameter(dbObject.BARCODE_NAME, lampBarcode);
                query.AddParameter(SynchronizerWithGreenhouse.PARAMETER, 0);
                query.AddParameter(dbObject.IDENTIFIER_NAME, lampId);
                query.ExecuteNonQuery();
                }
            }

        private void saveUnitData()
            {
            string command = string.Format(
                "UPDATE {0} SET {1}[Case]=0,{2}=0 WHERE {3}=@{3}",
                typeof(ElectronicUnits).Name,
                string.IsNullOrEmpty(unitBarcode) ? string.Empty : string.Format("{0}=@{0}, ", dbObject.BARCODE_NAME),
                dbObject.IS_SYNCED,
                dbObject.IDENTIFIER_NAME);
            using (SqlCeCommand query = dbWorker.NewQuery(command))
                {
                query.AddParameter(dbObject.BARCODE_NAME, unitBarcode);
                query.AddParameter(SynchronizerWithGreenhouse.PARAMETER, 0);
                query.AddParameter(dbObject.IDENTIFIER_NAME, unitId);
                query.ExecuteNonQuery();
                }
            }

        #endregion
        }
    }