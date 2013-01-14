using System.Collections.Generic;
using System;
using WMS_client.Base.Visual.Constructor;
using WMS_client.Enums;
using System.Data.SqlServerCe;
using WMS_client.db;
using System.Windows.Forms;

namespace WMS_client.Processes.Lamps
{
    /// <summary>Ремонт ламп</summary>
    public class RepairLight : BusinessProcess
    {
        /// <summary>Кроки</summary>
        private enum Stages { Begin, UnderWarrantly, OutOfWarrantly, FromUnderWarrantly, FromOutOfWarrantly, ScanBarcode, ExtractionElectricUnit, Exit, Save }

        private const string REPAIR_TOPIC = "Ремонт";
        private const string REPLACE_TOPIC = "На обмін";
        
        /// <summary>Штрихкод светильника</summary>
        private readonly string LightBarcode;
        /// <summary>Штрихкод блока</summary>
        private string unitBarcode;
        /// <summary>Новый статус корпуса</summary>
        private TypesOfLampsStatus newCaseStatus;
        /// <summary>Новый статус корпуса</summary>
        private TypesOfLampsStatus newUnitStatus;
        /// <summary>Поточний крок</summary>
        private Stages stage;
        /// <summary>Нужно сохранять штрихкод блока</summary>
        private bool needSaveUnitBarcode;

        /// <summary>Ремонт ламп</summary>
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
                        stage = UnderWarranty() ? Stages.UnderWarrantly : Stages.OutOfWarrantly;
                        DrawControls();
                        break;
                    case Stages.UnderWarrantly: 
                        warrantlyWin();
                        break;
                    case Stages.OutOfWarrantly:
                        if (Cases.IsHaveUnit(LightBarcode))
                        {
                        messageWin(REPAIR_TOPIC,
                                   "Вилучити Електроблок?",
                                   ButtonsSet.YesNo,
                                   Stages.ScanBarcode,
                                   Stages.FromOutOfWarrantly);
                        }
                        else
                        {
                            stage = Stages.ExtractionElectricUnit;
                            DrawControls();
                        }
                        break;
                    case Stages.FromUnderWarrantly:
                            newCaseStatus = TypesOfLampsStatus.ForExchange;
                            newUnitStatus = TypesOfLampsStatus.ForExchange;

                            messageWin(REPLACE_TOPIC,
                                       "Світильник буде поставлено на обмін",
                                       ButtonsSet.OkCancel,
                                       Stages.Save,
                                       Stages.Begin);
                        break;
                    case Stages.FromOutOfWarrantly:
                        newCaseStatus = TypesOfLampsStatus.Repair;
                        newUnitStatus = TypesOfLampsStatus.Repair;

                        messageWin(REPAIR_TOPIC,
                                   "Світильник буде поставлено на ремонт",
                                   ButtonsSet.OkCancel,
                                   Stages.Save,
                                   Stages.Begin);
                        break;
                    case Stages.ScanBarcode:
                        if (IsUnitHaveBarcode())
                        {
                            stage = Stages.ExtractionElectricUnit;
                            DrawControls();
                        }
                        else
                        {
                            needSaveUnitBarcode = true;
                            messageWin(REPAIR_TOPIC,
                                       "Відскануйте штрихкод електроблоку",
                                       ButtonsSet.None,
                                       Stages.ExtractionElectricUnit,
                                       Stages.ExtractionElectricUnit);
                        }
                        break;
                    case Stages.ExtractionElectricUnit:
                        newCaseStatus = TypesOfLampsStatus.Repair;
                        newUnitStatus = TypesOfLampsStatus.Storage;
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
                        saveUnitBarcode();
                        OnHotKey(KeyAction.Esc);
                        break;
                }
            }
        }

        /// <summary>Скан эл. блока</summary>
        /// <param name="Barcode">Штрихкод</param>
        public override void OnBarcode(string Barcode)
        {
            if(stage == Stages.ScanBarcode)
            {
                TypeOfAccessories accessory = BarcodeWorker.GetTypeOfAccessoriesByBarcode(Barcode);

                if(accessory == TypeOfAccessories.None)
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

        #region WindMode
        /// <summary>Окно с шага "UnderWarrantly"</summary>
        private void warrantlyWin()
        {
            ListOfLabelsConstructor list = new ListOfLabelsConstructor(MainProcess, REPAIR_TOPIC, getLightInfo());
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
            MainProcess.CreateButton("Ні", 125, 275, 100, 35, "secondButton", button_Click, Stages.OutOfWarrantly, true);
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
        private void button_Click(object sender)
        {
            stage = (Stages) ((Button) sender).Tag;
            DrawControls();
        }
        #endregion

        #region Query
        /// <summary>Находится ли светильник на гарантии</summary>
        private bool UnderWarranty()
        {
            SqlCeCommand query = dbWorker.NewQuery(@"SELECT 
	CASE WHEN c.DateOfWarrantyEnd>=@EndOfDay THEN 1 ELSE 0 END UnderWarranty
FROM Cases c 
LEFT JOIN Models t ON t.Id=c.Model
LEFT JOIN Party p ON p.Id=c.Party
WHERE RTRIM(c.BarCode)=RTRIM(@BarCode)");
            query.AddParameter("BarCode", LightBarcode);
            query.AddParameter("EndOfDay", DateTime.Now.Date.AddDays(1));
            object result = query.ExecuteScalar();

            return result != null && Convert.ToBoolean(result);
        }

        /// <summary>Информация по светильнику</summary>
        /// <returns>Model, Party, DateOfWarrantyEnd, Contractor</returns>
        private object[] getLightInfo()
        {
            SqlCeCommand query = dbWorker.NewQuery(@"SELECT 
	m.Description Model
	, p.Description Party
	, c.DateOfWarrantyEnd
	, cc.Description Contractor
FROM Cases c
LEFT JOIN Models m ON m.Id=c.Model
LEFT JOIN Party p ON p.Id=c.Party
LEFT JOIN Contractors cc ON cc.Id=p.Contractor
WHERE RTRIM(c.Barcode)=RTRIM(@BarCode)");
            query.AddParameter("Barcode", LightBarcode);

            return query.SelectArray(new Dictionary<string, Enum> { { BaseFormatName.DateTime, DateTimeFormat.OnlyDate } });
        }

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

        /// <summary>Сохранение штрихкода для эл.блока</summary>
        private void saveUnitBarcode()
        {
            if (needSaveUnitBarcode)
            {
                string command = string.Format("UPDATE ElectronicUnits SET Barcode=@Barcode,{0}=0,DateOfActuality=@Date WHERE Id=@Id",
                                               dbObject.IS_SYNCED);
                SqlCeCommand query = dbWorker.NewQuery(command);
                query.AddParameter("Barcode", unitBarcode);
                query.AddParameter("Id", Cases.GetUnitInCase(LightBarcode));
                query.AddParameter("Date", DateTime.Now);
                query.ExecuteNonQuery();
            }

            object caseId = BarcodeWorker.GetIdByBarcode(LightBarcode);
            saveInBd("Cases", newCaseStatus, LightBarcode);
            saveInBd("ElectronicUnits", newUnitStatus, caseId);
        }

        /// <summary>Сохранение</summary>
        /// <param name="tableName">Имя таблицы</param>
        /// <param name="state">Статус</param>
        /// <param name="barcode">Штрахкод</param>
        private void saveInBd(string tableName, TypesOfLampsStatus state, string barcode)
        {
            string command = string.Format("UPDATE {0} SET Status=@Status,{1}=0,DateOfActuality=@Date WHERE RTRIM(Barcode)=RTRIM(@BarCode)", 
                tableName, dbObject.IS_SYNCED);
            SqlCeCommand query = dbWorker.NewQuery(command);
            query.AddParameter("Status", (int)state);
            query.AddParameter("Barcode", barcode);
            query.AddParameter("Date", DateTime.Now);

            query.ExecuteNonQuery();
        }

        /// <summary>Сохранение</summary>
        /// <param name="tableName">Имя таблицы</param>
        /// <param name="state">Статус</param>
        /// <param name="caseId">ID корпуса</param>
        private void saveInBd(string tableName, TypesOfLampsStatus state, object caseId)
        {
            string command = string.Format("UPDATE {0} SET Status=@Status,{1}=0,DateOfActuality=@Date WHERE [Case]=@Case",
                tableName, dbObject.IS_SYNCED);
            SqlCeCommand query = dbWorker.NewQuery(command);
            query.AddParameter("Status", (int)state);
            query.AddParameter("Case", caseId);
            query.AddParameter("Date", DateTime.Now);

            query.ExecuteNonQuery();
        }
        #endregion
    }
}