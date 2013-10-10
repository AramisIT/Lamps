using System.Collections.Generic;
using System.Diagnostics;
using WMS_client.Enums;
using System;
using System.Windows.Forms;
using System.Data.SqlServerCe;
using WMS_client.Models;
using WMS_client.Processes.Lamps;
using WMS_client.db;

namespace WMS_client
    {
    /// <summary>Выбор процесса (для светильников)</summary>
    public class StartProcess : BusinessProcess
        {
        private MobileButton wifiOffButton;
        private MobileLabel connectionStatusLabel;

        /// <summary>Выбор процесса (для светильников)</summary>
        /// <param name="MainProcess">Основной процесс</param>
        public StartProcess(WMSClient MainProcess)//, IServerIdProvider serverIdProvider)
            : base(MainProcess, 1)
            {
            BusinessProcessType = ProcessType.Selecting;
            FormNumber = 1;
            }

        #region Override methods
        public override void DrawControls()
            {
            MainProcess.ToDoCommand = "Оберіть процес";

            MainProcess.CreateButton("Режим сканера", 10, 75, 220, 40, string.Empty, scannerMode_Click);
            MainProcess.CreateButton("Реєстрація", 10, 125, 220, 40, "registration", registration_Click);

            connectionStatusLabel = MainProcess.CreateLabel(string.Empty, 10, 195, 230, MobileFontSize.Normal);
            wifiOffButton = MainProcess.CreateButton(string.Empty, 10, 220, 220, 40, "WifiOff", changeConnectionStatus);
            updateWifiOnOffButtonState(MainProcess.ConnectionAgent.WifiEnabled);

            MainProcess.CreateLabel("Синхронізація - F5", 25, 280, 230, MobileFontSize.Large);
            }

        private void changeConnectionStatus()
            {
            bool startStatus = MainProcess.ConnectionAgent.WifiEnabled;
            if (startStatus)
                {
                MainProcess.ConnectionAgent.StopConnection();
                }
            else
                {
                MainProcess.StartConnectionAgent();
                }
            updateWifiOnOffButtonState(!startStatus);
            }

        private void updateWifiOnOffButtonState(bool wifiEnabled)
            {
            wifiOffButton.Text = wifiEnabled ? "ВІД'ЄДНАТИ" : "ПІД'ЄДНАТИ";
            connectionStatusLabel.Text = (wifiEnabled ? "" : "не ") + "під'єднан до мережі";
            }

        public override void OnBarcode(string barcode)
            {
          if (barcode.IsValidPositionBarcode())
                {
                tryPlacingLight(barcode);
                }
            else if (barcode.IsAccessoryBarcode())
                {
                var foundAccessory = Configuration.Current.Repository.FindAccessory(barcode.GetIntegerBarcode());
                var accessoryType = AccessoryHelper.GetAccessoryType(foundAccessory);
                if (accessoryType == TypeOfAccessories.Case && "Переместить светильник в цех?".Ask())
                    {
                    var _Case = foundAccessory as Case;
                    _Case.Map = 0;
                    _Case.Position = 0;
                    _Case.Register = 0;

                    if (!Configuration.Current.Repository.WriteCase(_Case))
                        {
                        "Не удалось переместить корпус в цех!".Warning();
                        return;
                        }
                    }
                }
            }

        private void tryPlacingLight(string barcode)
            {

            int map;
            Int16 register;
            byte position;
            if (barcode.GetPositionData(out map, out register, out position))
                {
                ClearControls();
                MainProcess.Process = new PlacingOnMap(MainProcess, map, register, position);
                }
            }

        public override void OnHotKey(KeyAction TypeOfAction)
            {
            switch (TypeOfAction)
                {
                //Синхронізація
                case KeyAction.Proceed:
                    new SynchronizerWithGreenhouse(MainProcess);
                    MainProcess.ClearControls();
                    MainProcess.Process = new StartProcess(MainProcess);
                    break;
                }
            }
        #endregion
        
        #region ButtonClick
        /// <summary>Перехід до процессу "Інформація"</summary>
        private void scannerMode_Click()
            {
            MainProcess.ClearControls();
            MainProcess.Process = new ScannerMode(MainProcess);
            }

        public bool SaveAccessoriesSet(Case _Case, Lamp lamp, Unit unit)
            {
            bool ok = true;

            var repository = Configuration.Current.Repository;

            if (unit != null)
                {
                if (unit.Id <= 0)
                    {
                    unit.Id = repository.GetNextUnitId();
                    ok = ok && repository.UpdateUnits(new List<Unit>() { unit }, true);
                    }
                else
                    {
                    ok = ok && repository.UpdateUnits(new List<Unit>() { unit }, false);
                    }
                }
            if (!ok)
                {
                return false;
                }


            if (lamp != null)
                {
                if (lamp.Id <= 0)
                    {
                    lamp.Id = repository.GetNextLampId();
                    ok = ok && repository.UpdateLamps(new List<Lamp>() { lamp }, true);
                    }
                else
                    {
                    ok = ok && repository.UpdateLamps(new List<Lamp>() { lamp }, false);
                    }
                }
            if (!ok)
                {
                return false;
                }


            if (_Case != null)
                {
                _Case.Lamp = lamp == null ? 0 : lamp.Id;
                _Case.Unit = unit == null ? 0 : unit.Id;
                ok = ok && repository.UpdateCases(new List<Case>() { _Case }, false);
                }

            return ok;
            }

        private void fixLamps()
            {
            List<int> ids = Configuration.Current.Repository.GetCasesIds();

            if (ids.Count == 0)
                {
                ShowMessage("База уже исправлена!");
                return;
                }
            var cases = Configuration.Current.Repository.ReadCases(ids);

            foreach (var _case in cases)
                {
                Lamp lamp = null;
                if (_case.Lamp == 0)
                    {
                    lamp = new Lamp();
                    lamp.Model = 3;
                    lamp.Party = 11;
                    }

                Unit unit = null;
                if (_case.Unit == 0)
                    {
                    unit = new Unit();
                    unit.Model = 1;
                    unit.Party = 10;
                    }

                if (!SaveAccessoriesSet(_case, lamp, unit))
                    {
                    ShowMessage("Не удалось исправить запись");
                    return;
                    }
                }

            ShowMessage("Записи исправлены");

            //for (int caseId = 3100; caseId <= 3407; caseId++)
            //    {
            //    if (!fixCase(caseId))
            //        {
            //        Trace.WriteLine(string.Format("Can't update case. Case id - {0}", caseId));
            //        }
            //    Trace.WriteLine(caseId);
            //    }
            }

        private bool fixCase(int caseId)
            {
            const string selectCommand = @"select Lamps.Id lampId from Lamps
join cases on cases.LampSync = Lamps.SyncRef
where Cases.Id = @caseId";

            object result;

            using (SqlCeCommand query = dbWorker.NewQuery(selectCommand))
                {
                query.AddParameter("caseId", caseId);
                result = query.ExecuteScalar();
                }

            if (result == null)
                {
                return true;
                }

            int lampId = (int)result;

            const string updateCommand = "update Cases set Lamp = @lampId where Cases.Id = @caseId";
            using (SqlCeCommand query = dbWorker.NewQuery(updateCommand))
                {
                query.AddParameter("caseId", caseId);
                query.AddParameter("lampId", lampId);
                return query.ExecuteNonQuery() == 1;
                }

            }

       

        /// <summary>Регістрація/редагування</summary>
        private void registration_Click()
            {
            MainProcess.ClearControls();
            MainProcess.Process = new EditSelector(MainProcess);
            }
        #endregion

        #region Query

        /// <summary>Світильник на гектарі?</summary>
        /// <param name="barcode">Штрихкод світильника</param>
        /// <returns>Світильник на гектарі?</returns>
        private bool isCasePerHectare(string barcode)
            {
            using (
                SqlCeCommand command = dbWorker.NewQuery(@"SELECT c.Status FROM Cases c WHERE RTRIM(BarCode)=@BarCode"))
                {
                command.AddParameter("@BarCode", barcode);
                object result = command.ExecuteScalar();

                if (result == null)
                    {
                    return false;
                    }

                TypesOfLampsStatus state = (TypesOfLampsStatus)Convert.ToInt32(result);
                return state == TypesOfLampsStatus.IsWorking;
                }
            }

        /// <summary>Інформація по світильнику на гектарі</summary>
        /// <param name="barcode">Штрихкод світильника</param>
        /// <returns>Інформація</returns>
        private object[] LuminaireOnHectareInfo(string barcode)
            {
            using (SqlCeCommand command = dbWorker.NewQuery(@"SELECT CaseModel, CaseParty, CaseWarrantly
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
ORDER BY Type"))
                {
                command.AddParameter("@BarCode", barcode);

                return
                    command.SelectArray(new Dictionary<string, Enum>
                        {
                            {BaseFormatName.DateTime, DateTimeFormat.OnlyDate}
                        });
                }
            }

        /// <summary>Інформація по світильнику не на гектарі</summary>
        /// <param name="barcode">Штрихкод світильника</param>
        /// <returns>Інформація</returns>
        private object[] LuminairePerHectareInfo(string barcode)
            {
            using (SqlCeCommand command = dbWorker.NewQuery(@"
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
ORDER BY Type"))
                {
                command.AddParameter("@BarCode", barcode);

                return
                    command.SelectArray(new Dictionary<string, Enum>
                        {
                            {BaseFormatName.DateTime, DateTimeFormat.OnlyDate}
                        });
                }
            }

        #endregion
        }
    }

