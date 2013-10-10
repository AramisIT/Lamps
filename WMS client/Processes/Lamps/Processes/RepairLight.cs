using System.Collections.Generic;
using System;
using WMS_client.Base.Visual.Constructor;
using WMS_client.Enums;
using System.Data.SqlServerCe;
using WMS_client.db;
using System.Windows.Forms;

namespace WMS_client.Processes.Lamps
    {
    /// <summary>Ремонт світильників</summary>
    public class RepairLight : BusinessProcess
        {
        #region Properties
        /// <summary>Шаги</summary>
        private enum Stages
            {
            Begin,
            UnderWarrantly,
            OutOfWarrantlyUnit,
            OutOfWarrantlyLamp,
            FromUnderWarrantly,
            FromOutOfWarrantly,
            ScanLampBarcode,
            ScanUnitBarcode,
            ExtractionElectricUnit,
            ExtractionLamp,
            FinishExtraction,
            Exit,
            Save
            }

        private const string REPAIR_TOPIC = "Ремонт";
        private const string REPLACE_TOPIC = "На обмін";

        /// <summary>Штрихкод светильника</summary>
        private readonly string LightBarcode;
        /// <summary>Штрихкод блока</summary>
        private string unitBarcode;
        /// <summary>Штрихкод лампы</summary>
        private string lampBarcode;
        /// <summary>Новый статус корпуса</summary>
        private TypesOfLampsStatus newCaseStatus;
        /// <summary>Новый статус эл.блока</summary>
        private TypesOfLampsStatus newUnitStatus;
        /// <summary>Новый статус лампы</summary>
        private TypesOfLampsStatus newLampStatus;
        /// <summary>Поточний крок</summary>
        private Stages stage;
        /// <summary>Нужно сохранять штрихкод блока</summary>
        private bool needSaveUnitBarcode;
        /// <summary>Нужно сохранять штрихкод лампы</summary>
        private bool needSaveLampBarcode;
        #endregion

        /// <summary>Ремонт світильників</summary>
        public RepairLight(WMSClient MainProcess, string lightBarcode)
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
                MainProcess.ClearControls();

                switch (stage)
                    {
                    case Stages.Begin:
                        unitBarcode = string.Empty;
                        stage = Cases.UnderWarranty(LightBarcode) ? Stages.UnderWarrantly : Stages.OutOfWarrantlyUnit;
                        DrawControls();
                        break;
                    case Stages.UnderWarrantly:
                        warrantlyWin();
                        break;
                    case Stages.OutOfWarrantlyUnit:
                        if (Cases.IsHaveUnit(LightBarcode))
                            {
                            messageWin(REPAIR_TOPIC,
                                       "Вилучити Електроблок?",
                                       ButtonsSet.YesNo,
                                       Stages.ScanUnitBarcode,
                                       Stages.OutOfWarrantlyLamp);
                            }
                        else
                            {
                            stage = Stages.ExtractionElectricUnit;
                            DrawControls();
                            }
                        break;
                    case Stages.OutOfWarrantlyLamp:
                        if (Cases.IsHaveUnit(LightBarcode))
                            {
                            messageWin(REPAIR_TOPIC,
                                       "Вилучити Лампу?",
                                       ButtonsSet.YesNo,
                                       Stages.ScanLampBarcode,
                                       Stages.FromOutOfWarrantly);
                            }
                        else
                            {
                            stage = Stages.ExtractionLamp;
                            DrawControls();
                            }
                        break;
                    case Stages.FromUnderWarrantly:
                        newCaseStatus = TypesOfLampsStatus.ForExchange;
                        newUnitStatus = TypesOfLampsStatus.ForExchange;
                        newLampStatus = TypesOfLampsStatus.ForExchange;

                        messageWin(REPLACE_TOPIC,
                                   "Світильник буде поставлено на обмін",
                                   ButtonsSet.OkCancel,
                                   Stages.Save,
                                   Stages.Begin);
                        break;
                    case Stages.FromOutOfWarrantly:
                        newCaseStatus = TypesOfLampsStatus.ToRepair;
                        newUnitStatus = TypesOfLampsStatus.ToRepair;
                        newLampStatus = TypesOfLampsStatus.ToRepair;

                        messageWin(REPAIR_TOPIC,
                                   "Світильник буде поставлено на ремонт",
                                   ButtonsSet.OkCancel,
                                   Stages.Save,
                                   Stages.Begin);
                        break;
                    case Stages.ScanLampBarcode:
                        if (IsLampHaveBarcode())
                            {
                            stage = Stages.ExtractionLamp;
                            DrawControls();
                            }
                        else
                            {
                            needSaveUnitBarcode = true;
                            messageWin(REPAIR_TOPIC,
                                       "Відскануйте штрихкод лампи",
                                       ButtonsSet.None,
                                       Stages.ExtractionLamp,
                                       Stages.ExtractionLamp);
                            }
                        break;
                    case Stages.ScanUnitBarcode:
                        if (IsUnitHaveBarcode())
                            {
                            stage = Stages.ExtractionElectricUnit;
                            DrawControls();
                            }
                        else
                            {
                            needSaveLampBarcode = true;
                            messageWin(REPAIR_TOPIC,
                                       "Відскануйте штрихкод електроблоку",
                                       ButtonsSet.None,
                                       Stages.ExtractionElectricUnit,
                                       Stages.ExtractionElectricUnit);
                            }
                        break;
                    case Stages.ExtractionElectricUnit:
                        stage = Stages.OutOfWarrantlyLamp;
                        DrawControls();
                        break;
                    case Stages.ExtractionLamp:
                        stage = Stages.FinishExtraction;
                        DrawControls();
                        break;
                    case Stages.FinishExtraction:
                        newCaseStatus = TypesOfLampsStatus.ToRepair;
                        newUnitStatus = TypesOfLampsStatus.Storage;
                        newLampStatus = TypesOfLampsStatus.Storage;

                        messageWin(REPAIR_TOPIC,
                                   "Світильник буде поставлено на ремонт",
                                   ButtonsSet.OkCancel,
                                   Stages.Save,
                                   Stages.Begin);
                        break;
                    case Stages.Exit:
                        OnHotKey(KeyAction.Esc);
                        break;
                    case Stages.Save:
                        save();
                        OnHotKey(KeyAction.Esc);
                        break;
                    }
                }
            }

        /// <summary>Скан штрихкода для Эл.блока/Лампы</summary>
        /// <param name="Barcode">Штрихкод</param>
        public override void OnBarcode(string Barcode)
            {
            if (Barcode.IsAccessoryBarcode())
                {
                //Скан ел.блоків?
                if (stage == Stages.ScanUnitBarcode)
                    {
                    TypeOfAccessories accessory = BarcodeWorker.GetTypeOfAccessoriesByBarcode(Barcode);

                    //Чи використовується цей штрихкод?
                    if (accessory == TypeOfAccessories.None)
                        {
                        unitBarcode = Barcode;
                        stage = Stages.ExtractionElectricUnit;
                        DrawControls();
                        }
                    else
                        {
                        MessageBox.Show("Штрихкод уже используеться!");
                        }
                    }
                //Скан ламп?
                else if (stage == Stages.ScanLampBarcode)
                    {
                    TypeOfAccessories accessory = BarcodeWorker.GetTypeOfAccessoriesByBarcode(Barcode);

                    //Чи використовується цей штрихкод?
                    if (accessory == TypeOfAccessories.None)
                        {
                        lampBarcode = Barcode;
                        stage = Stages.ExtractionLamp;
                        DrawControls();
                        }
                    else
                        {
                        MessageBox.Show("Штрихкод уже используеться!");
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
                    MainProcess.Process = new StartProcess(MainProcess);
                    break;
                }
            }
        #endregion

        #region WindMode
        /// <summary>Окно с шага "UnderWarrantly"</summary>
        private void warrantlyWin()
            {
            object[] lightData = Cases.GetLightInfo(LightBarcode);
            ListOfLabelsConstructor list = new ListOfLabelsConstructor(MainProcess, REPAIR_TOPIC, lightData);
            list.ListOfLabels = new List<LabelForConstructor>
                                    {
                                        new LabelForConstructor("УВАГА! Світильник", ControlsStyle.LabelH2Red),
                                        new LabelForConstructor("знаходиться на гарантії!", ControlsStyle.LabelH2Red),
                                        new LabelForConstructor(string.Empty, false),
                                        new LabelForConstructor("Модель: {0}"),
                                        new LabelForConstructor("Партія: {0}"),
                                        new LabelForConstructor("Гарантія до {0}"),
                                        new LabelForConstructor("Контрагент {0}"),
                                        new LabelForConstructor(string.Empty, false),
                                        new LabelForConstructor("Помітити на обмін?", ControlsStyle.LabelH2Red)
                                    };

            MainProcess.CreateButton("Так", 15, 275, 100, 35, "firstButton", button_Click, Stages.FromUnderWarrantly, true);
            MainProcess.CreateButton("Ні", 125, 275, 100, 35, "secondButton", button_Click, Stages.OutOfWarrantlyUnit, true);
            }

        /// <summary>Окно отображения информации</summary>
        /// <param name="topic">Заголовок</param>
        /// <param name="message">Сообщение</param>
        /// <param name="set">Набор кнопок навигации</param>
        /// <param name="firstButton">Делегат вызова при нажатии первой кнопки</param>
        /// <param name="secondButton">Делегат вызова при нажатии второй кнопки</param>
        private void messageWin(string topic, string message, ButtonsSet set, Stages firstButton, Stages secondButton)
            {
            MainProcess.ToDoCommand = topic;
            MainProcess.CreateLabel(message, 0, 150, 240, MobileFontSize.Multiline, MobileFontPosition.Center);

            switch (set)
                {
                case ButtonsSet.YesNo:
                    MainProcess.CreateButton("Так", 15, 275, 100, 35, "firstButton", button_Click, firstButton, true);
                    MainProcess.CreateButton("Ні", 125, 275, 100, 35, "secondButton", button_Click, secondButton, true);
                    break;
                case ButtonsSet.OkCancel:
                    MainProcess.CreateButton("Ок", 15, 275, 100, 35, "firstButton", button_Click, firstButton, true);
                    MainProcess.CreateButton("Відміна", 125, 275, 100, 35, "secondButton", button_Click, secondButton, true);
                    break;
                }
            }
        #endregion

        #region Button
        /// <summary>Переход на выбранный шаг</summary>
        private void button_Click(object sender)
            {
            stage = (Stages)((Button)sender).Tag;
            DrawControls();
            }
        #endregion

        #region Query
        #region Is ... have a barcode
        /// <summary>Есть ли у эл.блока штрихкод</summary>
        private bool IsUnitHaveBarcode()
            {
            SqlCeCommand query = dbWorker.NewQuery(@"SELECT e.Barcode
FROM Cases c 
JOIN ElectronicUnits e ON e.Id=c.ElectronicUnit
WHERE RTRIM(c.Barcode)=@Barcode");
            query.AddParameter("Barcode", LightBarcode);
            object barcode = query.ExecuteScalar();
            unitBarcode = barcode == null ? string.Empty : barcode.ToString().TrimEnd();

            return !string.IsNullOrEmpty(unitBarcode);
            }

        /// <summary>Есть ли у лампы штрихкод</summary>
        private bool IsLampHaveBarcode()
            {
            using (SqlCeCommand query = dbWorker.NewQuery(@"SELECT l.Barcode
FROM Cases c 
JOIN Lamps l ON l.Id=c.ElectronicUnit
WHERE RTRIM(c.Barcode)=@Barcode"))
                {
                query.AddParameter("Barcode", LightBarcode);
                object barcode = query.ExecuteScalar();
                unitBarcode = barcode == null ? string.Empty : barcode.ToString().TrimEnd();

                return !string.IsNullOrEmpty(unitBarcode);
                }
            }

        #endregion

        #region Save
        /// <summary>Сохранение данных</summary>
        private void save()
            {
            saveUnitData();
            saveLampData();
            saveCaseData();
            }

        /// <summary>Сохранение данных по эл.блоку</summary>
        private void saveUnitData()
            {
            string command = string.Format(
                "UPDATE ElectronicUnits SET Status=@Status,{0}=0,DateOfActuality=@Date,[Case]=0{1} WHERE {2}=@{2}",
                dbObject.IS_SYNCED, needSaveUnitBarcode ? ",Barcode=@Barcode" : string.Empty, dbObject.IDENTIFIER_NAME);
            using (SqlCeCommand query = dbWorker.NewQuery(command))
                {
                query.AddParameter("Status", newUnitStatus);
                query.AddParameter(dbObject.BARCODE_NAME, unitBarcode);
                query.AddParameter(dbObject.IDENTIFIER_NAME, Cases.GetUnitInCase(LightBarcode));
                query.AddParameter("Date", DateTime.Now);

                query.ExecuteNonQuery();
                }
            }

        /// <summary>Сохранение данных по лампе</summary>
        private void saveLampData()
            {
            if (needSaveLampBarcode)
                {
                string command = string.Format(
                    "UPDATE Lamps SET Status=@Status,{0}=0,DateOfActuality=@Date,[Case]=0{1} WHERE {2}=@{2}",
                    dbObject.IS_SYNCED, needSaveUnitBarcode ? ",Barcode=@Barcode" : string.Empty,
                    dbObject.IDENTIFIER_NAME);
                using (SqlCeCommand query = dbWorker.NewQuery(command))
                    {
                    query.AddParameter("Status", newLampStatus);
                    query.AddParameter(dbObject.BARCODE_NAME, lampBarcode);
                    query.AddParameter(dbObject.IDENTIFIER_NAME, Cases.GetLampInCase(LightBarcode));
                    query.AddParameter("Date", DateTime.Now);

                    query.ExecuteNonQuery();
                    }
                }
            }

        /// <summary>Сохранение данных по корпусу</summary>
        private void saveCaseData()
            {
            string command = string.Format(
                "UPDATE Cases SET Status=@Status,{0}=0,DateOfActuality=@Date,Lamp=0,ElectronicUnit=0 WHERE RTRIM({1})=RTRIM(@{1})",
                dbObject.IS_SYNCED, dbObject.BARCODE_NAME);
            using (SqlCeCommand query = dbWorker.NewQuery(command))
                {
                query.AddParameter("Status", (int)newCaseStatus);
                query.AddParameter(dbObject.BARCODE_NAME, LightBarcode);
                query.AddParameter("Date", DateTime.Now);

                query.ExecuteNonQuery();
                }
            }

        #endregion
        #endregion
        }
    }