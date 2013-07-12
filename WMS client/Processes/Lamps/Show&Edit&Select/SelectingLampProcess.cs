using System.Collections.Generic;
using WMS_client.Enums;
using System;
using System.Windows.Forms;
using System.Data.SqlServerCe;
using WMS_client.Processes.Lamps;
using WMS_client.db;
using WMS_client.Processes.Lamps.Sync;

namespace WMS_client
    {
    /// <summary>Выбор процесса (для светильников)</summary>
    public class SelectingLampProcess : BusinessProcess
        {
        IServerIdProvider serverIdProvider = null;
        /// <summary>Выбор процесса (для светильников)</summary>
        /// <param name="MainProcess">Основной процесс</param>
        public SelectingLampProcess(WMSClient MainProcess)//, IServerIdProvider serverIdProvider)
            : base(MainProcess, 1)
            {
            //if (serverIdProvider == null)
            //    {
            //    throw new ArgumentException("ServerIdProvider");
            //    }
            this.serverIdProvider = new ServerIdProvider(); //serverIdProvider;
            BusinessProcessType = ProcessType.Selecting;
            FormNumber = 1;
            }

        #region Override methods
        public override void DrawControls()
            {
            MainProcess.ToDoCommand = "Оберіть процес";
            MainProcess.CreateButton("Інфо", 20, 75, 200, 45, "info", info_Click);
            MainProcess.CreateButton("Процеси", 20, 150, 200, 45, "process", process_Click);
            MainProcess.CreateButton("Реєстрація", 20, 225, 200, 45, "registration", registration_Click);
            MainProcess.CreateLabel("Синхронізація - F5", 25, 280, 230, MobileFontSize.Large);
            }

        public override void OnBarcode(string barcode)
            {
            if (barcode.Equals(AcceptingAfterFixing.START_ACCEPTING_AFTER_FIXING_BARCODE))
                {
                MainProcess.ClearControls();
                MainProcess.Process = new AcceptingAfterFixing(MainProcess);
                }
            else if (barcode.IsValidPositionBarcode())
                {
                tryPlacingLight(barcode);
                }
            else if (barcode.IsValidBarcode())
                {
                //Тип комплектуючого визначений за штрихкодом (якщо ШК відсутній, то тип = None)
                TypeOfAccessories type = BarcodeWorker.GetTypeOfAccessoriesByBarcode(barcode);

                //Перехід на відповідний процес відповідно до типу комплектуючого
                switch (type)
                    {
                    case TypeOfAccessories.Lamp:
                        lampProcess(barcode);
                        break;
                    case TypeOfAccessories.ElectronicUnit:
                        unitProcess(barcode);
                        break;
                    case TypeOfAccessories.Case:
                        caseProcess(barcode);
                        break;
                    default:
                        ShowMessage("Не існує комплектуюче з таким штрихкодом!");
                        break;
                    }
                }
            }

        private void tryPlacingLight(string barcode)
            {

            long map;
            int register;
            int position;
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
                //Назад
                case KeyAction.Esc:
                    MainProcess.ClearControls();
                    MainProcess.Process = new RegistrationProcess(MainProcess);
                    break;
                //Очищення
                case KeyAction.Complate:
                    if (MessageBox.Show(
                        "Очистить все данные на ТСД?",
                        "Очистка",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                        {
                        dbArchitector.ClearAll();
                        }
                    break;
                //Синхронізація
                case KeyAction.Proceed:
                    new dbSynchronizer(MainProcess, serverIdProvider);
                    MainProcess.ClearControls();
                    MainProcess.Process = new SelectingLampProcess(MainProcess);
                    break;
                }
            }
        #endregion

        #region Processes
        /// <summary>Перехід до процесу з лампою</summary>
        /// <param name="barcode">Штрихкод комплектуючого</param>
        private void lampProcess(string barcode)
            {
            MainProcess.ClearControls();
            MainProcess.Process = new ChooseLamp(MainProcess, barcode);
            }

        /// <summary>Перехід до процесу з блоком</summary>
        /// <param name="barcode">Штрихкод комплектуючого</param>
        private void unitProcess(string barcode)
            {
            MainProcess.ClearControls();
            MainProcess.Process = new ChooseUnit(MainProcess, barcode);
            }

        /// <summary>Перехід до процесу з корпусом</summary>
        /// <param name="Barcode">Штрихкод комплектуючого</param>
        private void caseProcess(string Barcode)
            {
            bool onHectar = isCasePerHectare(Barcode);
            object[] array;

            //работает ли лампа = на гектаре
            if (onHectar)
                {
                array = LuminaireOnHectareInfo(Barcode);

                if (array.Length != 0)
                    {
                    MainProcess.ClearControls();
                    MainProcess.Process = new ChooseLighterOnHectare(MainProcess, array, Barcode);
                    }
                }
            else
                {
                array = LuminairePerHectareInfo(Barcode);

                if (array.Length != 0)
                    {
                    MainProcess.ClearControls();
                    MainProcess.Process = new ChooseLighterPerHectare(MainProcess, array, Barcode);
                    }
                }
            }
        #endregion

        #region ButtonClick
        /// <summary>Перехід до процессу "Інформація"</summary>
        private void info_Click()
            {
            MainProcess.ClearControls();
            MainProcess.Process = new Info(MainProcess);
            }

        /// <summary>Перехід для вибору процесу</summary>
        private void process_Click()
            {
            MainProcess.ClearControls();
            MainProcess.Process = new Processes.Lamps.Processes(MainProcess);
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
            SqlCeCommand command = dbWorker.NewQuery(@"SELECT c.Status FROM Cases c WHERE RTRIM(BarCode)=@BarCode");
            command.AddParameter("@BarCode", barcode);
            object result = command.ExecuteScalar();

            if (result == null)
                {
                return false;
                }

            TypesOfLampsStatus state = (TypesOfLampsStatus)Convert.ToInt32(result);
            return state == TypesOfLampsStatus.IsWorking;
            }

        /// <summary>Інформація по світильнику на гектарі</summary>
        /// <param name="barcode">Штрихкод світильника</param>
        /// <returns>Інформація</returns>
        private object[] LuminaireOnHectareInfo(string barcode)
            {
            SqlCeCommand command = dbWorker.NewQuery(@"SELECT CaseModel, CaseParty, CaseWarrantly
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
ORDER BY Type");
            command.AddParameter("@BarCode", barcode);

            return command.SelectArray(new Dictionary<string, Enum> { { BaseFormatName.DateTime, DateTimeFormat.OnlyDate } });
            }

        /// <summary>Інформація по світильнику не на гектарі</summary>
        /// <param name="barcode">Штрихкод світильника</param>
        /// <returns>Інформація</returns>
        private object[] LuminairePerHectareInfo(string barcode)
            {
            SqlCeCommand command = dbWorker.NewQuery(@"
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
ORDER BY Type");
            command.AddParameter("@BarCode", barcode);

            return command.SelectArray(new Dictionary<string, Enum> { { BaseFormatName.DateTime, DateTimeFormat.OnlyDate } });
            }
        #endregion
        }
    }

