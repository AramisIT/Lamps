using System.Collections.Generic;
using WMS_client.Base.Visual.Constructor;
using WMS_client.db;
using System.Data.SqlServerCe;

namespace WMS_client
    {
    /// <summary>Розбирання світильника</summary>
    public class CollateLight : BusinessProcess
        {
        #region Properties
        private enum Stages { Begin, Lamp, Unit }

        private readonly string LightBarcode;
        private const string TOPIC_OF_PROCESS = "Розібрати";
        private Stages stage;
        private long lampId;
        private long unitId;
        private string lampBarcode;
        private string unitBarcode;
        #endregion

        /// <summary>Розбирання світильника</summary>
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
                                                underWarrantly ? "УВАГА! Світильник" : string.Empty,
                                                ControlsStyle.LabelH2Red),
                                            new LabelForConstructor(
                                                underWarrantly ? "знаходиться на гарантії!" : string.Empty,
                                                ControlsStyle.LabelH2Red),
                                            new LabelForConstructor(string.Empty, false),
                                            new LabelForConstructor("Модель: {0}"),
                                            new LabelForConstructor("Партія: {0}"),
                                            new LabelForConstructor("Гарантія до {0}"),
                                            new LabelForConstructor("Контрагент {0}"),
                                            new LabelForConstructor(string.Empty, false),
                                            new LabelForConstructor("Розібрати світильник?", ControlsStyle.LabelH2Red)
                                        };

                MainProcess.CreateButton("Так", 10, 275, 105, 35, "ok", Ok_click);
                MainProcess.CreateButton("Ні", 125, 275, 105, 35, "cancel", Cancel_click);
                }
            }

        public override void OnBarcode(string Barcode)
            {
            if (Barcode.IsAccessoryBarcode())
                {
                if (BarcodeWorker.IsBarcodeExist(Barcode))
                    {
                    ShowMessage("Штрихкод уже используется!");
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

        /// <summary>Етап сканування штрихкоду лампи</summary>
        private bool collateLamp()
            {
            if (Cases.TryGetLampIdWithoutBarcode(LightBarcode, out lampId))
                {
                stage = Stages.Lamp;
                drawScanForm("ЛАМПИ");
                return true;
                }

            return false;
            }

        /// <summary>Етап сканування штрихкоду ел.блоку</summary>
        private bool collateUnit()
            {
            if (Cases.TryGetUnitIdWithoutBarcode(LightBarcode, out unitId))
                {
                stage = Stages.Unit;
                drawScanForm("ЭЛЕКТРОБЛОКУ");
                return true;
                }

            return false;
            }

        /// <summary>Етап завершення</summary>
        private bool finishCollate()
            {
            drawFinishForm();
            return true;
            }

        /// <summary>Збереження даних</summary>
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
            MainProcess.CreateLabel("Відскануте штрих код", 5, 150, 230,
                                    MobileFontSize.Large, MobileFontPosition.Center, MobileFontColors.Default);
            MainProcess.CreateLabel(accessoryStr, 5, 185, 230,
                                    MobileFontSize.Large, MobileFontPosition.Center, MobileFontColors.Info);
            }

        private void drawFinishForm()
            {
            MainProcess.ClearControls();
            MainProcess.ToDoCommand = TOPIC_OF_PROCESS;
            MainProcess.CreateLabel("Світильник розібрано!", 5, 150, 230,
                                    MobileFontSize.Large, MobileFontPosition.Center, MobileFontColors.Default);
            MainProcess.CreateLabel("Зберегти дані?", 5, 185, 230,
                                    MobileFontSize.Large, MobileFontPosition.Center, MobileFontColors.Info);
            MainProcess.CreateButton("Ок", 10, 275, 105, 35, "ok", Save_click);
            MainProcess.CreateButton("Відміна", 125, 275, 105, 35, "cancel", Cancel_click);
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