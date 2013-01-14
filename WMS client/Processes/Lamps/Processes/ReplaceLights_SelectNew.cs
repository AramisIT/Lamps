using System;
using System.Collections.Generic;
using WMS_client.Base.Visual.Constructor;
using WMS_client.Enums;
using WMS_client.db;
using System.Data.SqlServerCe;

namespace WMS_client
{
    public class ReplaceLights_SelectNew : BusinessProcess
    {
        private readonly string NewLampBarCode;
        private readonly string ExistLampBarCode;

        public ReplaceLights_SelectNew(WMSClient MainProcess, string newLampBarCode, string existLampBarCode)
            : base(MainProcess, 1)
        {
            BusinessProcessType = ProcessType.Registration;
            FormNumber = 1;
            NewLampBarCode = newLampBarCode;
            ExistLampBarCode = existLampBarCode;
            IsLoad = true;
            DrawControls();
        }

        #region Override methods
        public override sealed void DrawControls()
        {
            if (IsLoad)
            {
                object[] parameters = GetNewIlluminatorInfo();
                ListOfLabelsConstructor list = new ListOfLabelsConstructor(MainProcess, "Заміна світильника",
                                                                         parameters);
                list.ListOfLabels = new List<LabelForConstructor>
                                        {
                                            new LabelForConstructor(string.Empty, false),
                                            new LabelForConstructor(string.Empty, false),
                                            new LabelForConstructor("Корпус", ControlsStyle.LabelH2),
                                            new LabelForConstructor("Модель: {0}"),
                                            new LabelForConstructor("Партія: {0}"),
                                            new LabelForConstructor("Гарантія до {0}"),
                                            new LabelForConstructor("Електроблок", ControlsStyle.LabelH2),
                                            new LabelForConstructor("Партія: {0}"),
                                            new LabelForConstructor("Гарантія до {0}",1),
                                        };

                MainProcess.CreateButton("Ок", 20, 275, 200, 35, "ok", Ok);
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
                    {
                        MainProcess.ClearControls();
                        MainProcess.Process = new SelectingLampProcess(MainProcess);
                        break;
                    }
            }
        }
        #endregion

        #region ButtonClick
        private void Ok()
        {
            finishingReplaceLamps();
            MainProcess.ClearControls();
            MainProcess.Process = new SelectingLampProcess(MainProcess);
        }
        #endregion

        #region Query
        private object[] GetNewIlluminatorInfo()
        {
            SqlCeCommand query = dbWorker.NewQuery(@"SELECT 
	t.Description CaseModel
	, p.Description CaseParty
	, c.DateOfWarrantyEnd CaseWarrantly
FROM Cases c
LEFT JOIN Models t ON t.Id=c.Model
LEFT JOIN Party p ON p.Id=c.Party
WHERE c.BarCode=@BarCode

UNION 

SELECT 
	'' CaseModel
	, p.Description CaseParty
	, u.DateOfWarrantyEnd CaseWarrantly
FROM Cases c
LEFT JOIN ElectronicUnits u ON u.Id=c.ElectronicUnit
LEFT JOIN Models t ON t.Id=u.Model
LEFT JOIN Party p ON p.Id=u.Party
WHERE c.BarCode=@BarCode");
            query.AddParameter("BarCode", NewLampBarCode);

            return query.SelectArray(new Dictionary<string, Enum> {{BaseFormatName.DateTime, DateTimeFormat.OnlyDate}});
        }

        private void finishingReplaceLamps()
        {
            SqlCeCommand query = dbWorker.NewQuery(@"SELECT Map, Register, Position, Status FROM Cases WHERE RTRIM(BarCode)=RTRIM(@Old)");
            query.AddParameter("Old", ExistLampBarCode);
            object[] result = query.SelectArray();

            if (result != null)
            {
                string command = string.Format(
                    "UPDATE Cases SET Map=@Map,Register=@Register,Position=@Position,Status=@IsWorking,{0}=0,DateOfActuality=@Date WHERE RTRIM(BarCode)=@New",
                    dbObject.IS_SYNCED);
                query = dbWorker.NewQuery(command);
                query.AddParameter("Map", result[0]);
                query.AddParameter("Register", result[1]);
                query.AddParameter("Position", result[2]);
                query.AddParameter("IsWorking", TypesOfLampsStatus.IsWorking);
                query.AddParameter("New", NewLampBarCode);
                query.AddParameter("Date", DateTime.Now);
                query.ExecuteNonQuery();

                command = string.Format(
                    "UPDATE Cases SET Map=0,Register=0,Position=0,Status=@Storage,{0}=0,DateOfActuality=@Date WHERE RTRIM(BarCode)=@Old",
                    dbObject.IS_SYNCED);
                query = dbWorker.NewQuery(command);
                query.AddParameter("Storage", TypesOfLampsStatus.Storage);
                query.AddParameter("Old", ExistLampBarCode);
                query.AddParameter("Date", DateTime.Now);
                query.ExecuteNonQuery();
            }
        }
        #endregion
    }
}

