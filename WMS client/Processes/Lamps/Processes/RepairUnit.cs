using System.Collections.Generic;
using System;
using WMS_client.Base.Visual.Constructor;
using WMS_client.Enums;
using System.Data.SqlServerCe;
using WMS_client.db;

namespace WMS_client.Processes.Lamps
{
    public class RepairUnit : BusinessProcess
    {
        private readonly string UnitBarcode;
        
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

                if (underWarranty())
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
                            new LabelForConstructor("Помітити на обмін?", ControlsStyle.LabelH2Red)
                        });

                listOfLabels.ListOfLabels = list;

                MainProcess.CreateButton("Так", 15, 275, 100, 35, "firstButton", yes_Click);
                MainProcess.CreateButton("Ні", 125, 275, 100, 35, "secondButton", no_Click);
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
        private void yes_Click()
        {
            changeUnitStatus(TypesOfLampsStatus.ForExchange);
            OnHotKey(KeyAction.Esc);
        }

        private void no_Click()
        {
            changeUnitStatus(TypesOfLampsStatus.ToRepair);
            OnHotKey(KeyAction.Esc);
        }
        #endregion

        #region Query
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

        private void changeUnitStatus(TypesOfLampsStatus status)
        {
            SqlCeCommand query = dbWorker.NewQuery("UPDATE ElectronicUnits SET Status=@Status WHERE RTRIM(Barcode)=RTRIM(@Barcode)");
            query.AddParameter("Status", (int)status);
            query.AddParameter("Barcode", UnitBarcode);
            query.ExecuteNonQuery();
        }
        #endregion
    }
}