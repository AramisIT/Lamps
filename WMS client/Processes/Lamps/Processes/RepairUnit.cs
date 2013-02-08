using System.Collections.Generic;
using System;
using WMS_client.Base.Visual.Constructor;
using WMS_client.Enums;
using System.Data.SqlServerCe;
using WMS_client.db;

namespace WMS_client.Processes.Lamps
{
    /// <summary>Ремонт ел.блоків</summary>
    public class RepairUnit : BusinessProcess
    {
        /// <summary>Штрихкод блоку</summary>
        private readonly string UnitBarcode;

        /// <summary>Ремонт ел.блоків</summary>
        /// <param name="MainProcess"></param>
        /// <param name="unitBarcode">Штрихкод блоку</param>
        public RepairUnit(WMSClient MainProcess, string unitBarcode)
            : base(MainProcess, 1)
        {
            UnitBarcode = unitBarcode;

            IsLoad = true;
            DrawControls();
        }

        #region Override methods
        public override sealed void DrawControls()
        {
            if (IsLoad)
            {
                MainProcess.ClearControls();

                ListOfLabelsConstructor listOfLabels = new ListOfLabelsConstructor(MainProcess, "Ремонт", getUnitInfo());
                List<LabelForConstructor> list = new List<LabelForConstructor>();
                bool underWarrantly = underWarranty();

                if (underWarrantly)
                {
                    list.Add(new LabelForConstructor(string.Empty, ControlsStyle.LabelH2Red));
                    list.Add(new LabelForConstructor("УВАГА! Ел.блок", ControlsStyle.LabelH2Red));
                    list.Add(new LabelForConstructor("знаходиться на гарантії!", ControlsStyle.LabelH2Red));
                }
                else
                {
                    list.Add(new LabelForConstructor(string.Empty, ControlsStyle.LabelH2Red));
                    list.Add(new LabelForConstructor(string.Empty, ControlsStyle.LabelH2Red));
                    list.Add(new LabelForConstructor("Ел.блок не на гарантії", ControlsStyle.LabelH2));
                }

                list.AddRange(
                    new List<LabelForConstructor>
                        {
                            new LabelForConstructor(string.Empty, false),
                            new LabelForConstructor("Модель: {0}"),
                            new LabelForConstructor("Партія: {0}"),
                            new LabelForConstructor("Гарантія до {0}"),
                            new LabelForConstructor("Контрагент {0}"),
                            new LabelForConstructor(string.Empty, false),
                            new LabelForConstructor(underWarrantly ? "Помітити на обмін?" : string.Empty,
                                                    ControlsStyle.LabelH2Red)
                        });

                listOfLabels.ListOfLabels = list;

                if (underWarrantly)
                {
                    MainProcess.CreateButton("Так", 15, 275, 100, 35, "exchangeButton", exchange_Click);
                    MainProcess.CreateButton("Ні", 125, 275, 100, 35, "repairButton", repair_Click);
                }
                else
                {
                    MainProcess.CreateButton("Ок", 15, 275, 100, 35, "repairButton", repair_Click);
                    MainProcess.CreateButton("Відміна", 125, 275, 100, 35, "exitButton", exit_click);
                }
            }
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

        #region Buttons
        /// <summary>Завершення процесу. Помітити на обмін</summary>
        private void exchange_Click()
        {
            changeUnitStatus(TypesOfLampsStatus.ForExchange);
            OnHotKey(KeyAction.Esc);
        }

        /// <summary>Завершення процесу. Помітити на ремонт</summary>
        private void repair_Click()
        {
            changeUnitStatus(TypesOfLampsStatus.ToRepair);
            OnHotKey(KeyAction.Esc);
        }

        /// <summary>Завершення процесу. Вихід</summary>
        private void exit_click()
        {
            OnHotKey(KeyAction.Esc);
        }
        #endregion

        #region Query
        /// <summary>Чи знаходиться блок на гарантії?</summary>
        private bool underWarranty()
        {
            SqlCeCommand query = dbWorker.NewQuery(@"SELECT 
	CASE WHEN e.DateOfWarrantyEnd>=@EndOfDay THEN 1 ELSE 0 END UnderWarranty
FROM ElectronicUnits e 
LEFT JOIN Models t ON t.Id=e.Model
LEFT JOIN Party p ON p.Id=e.Party
WHERE RTRIM(e.BarCode)=RTRIM(@BarCode)");
            query.AddParameter("BarCode", UnitBarcode);
            query.AddParameter("EndOfDay", DateTime.Now.Date.AddDays(1));
            object result = query.ExecuteScalar();

            return result != null && Convert.ToBoolean(result);
        }

        /// <summary>Отримати інформації по блоку</summary>
        private object[] getUnitInfo()
        {
            SqlCeCommand query = dbWorker.NewQuery(@"SELECT 
	m.Description Model
	, p.Description Party
	, e.DateOfWarrantyEnd
	, c.Description Contractor
FROM ElectronicUnits e
LEFT JOIN Models m ON m.Id=e.Model
LEFT JOIN Party p ON p.Id=e.Party
LEFT JOIN Contractors c ON c.Id=p.Contractor
WHERE RTRIM(e.Barcode)=RTRIM(@Barcode)");
            query.AddParameter("Barcode", UnitBarcode);

            return query.SelectArray(new Dictionary<string, Enum> { { BaseFormatName.DateTime, DateTimeFormat.OnlyDate } });
        }

        /// <summary>Змінити статус ел.блоку</summary>
        /// <param name="status">Новий статус</param>
        private void changeUnitStatus(TypesOfLampsStatus status)
        {
            string command = string.Format("UPDATE ElectronicUnits SET Status=@Status,{0}=@{0} WHERE RTRIM({1})=RTRIM(@{1})",
                dbObject.IS_SYNCED, dbObject.BARCODE_NAME);
            SqlCeCommand query = dbWorker.NewQuery(command);
            query.AddParameter("Status", (int)status);
            query.AddParameter(dbObject.IS_SYNCED, false);
            query.AddParameter(dbObject.BARCODE_NAME, UnitBarcode);
            query.ExecuteNonQuery();
        }
        #endregion
    }
}