using System.Collections.Generic;
using WMS_client.Base.Visual.Constructor;
using WMS_client.Enums;
using WMS_client.Processes.Lamps;
using WMS_client.db;

namespace WMS_client
{
    /// <summary>Выбран светильник на гектаре</summary>
    public class ChooseLighterOnHectare : BusinessProcess
    {
        /// <summary>Штрихкод свтильника</summary>
        private readonly string CaseBarcode;

        /// <summary>Выбран светильник на гектаре</summary>
        /// <param name="MainProcess"></param>
        /// <param name="parameters">Масив инф. о светильнике</param>
        /// <param name="caseBarcode">Штрихкод свтильника</param>
        public ChooseLighterOnHectare(WMSClient MainProcess, object[] parameters, string caseBarcode) : base(MainProcess, 1)
        {
            Parameters = parameters;
            FormNumber = 1;
            BusinessProcessType = ProcessType.Registration;
            CaseBarcode = caseBarcode;
            
            IsLoad = true;
            DrawControls();
        }

        #region Override methods
        public override sealed void DrawControls()
        {
            if (IsLoad)
            {
                ListOfLabelsConstructor list = new ListOfLabelsConstructor(MainProcess, Parameters);
                list.ListOfLabels = new List<LabelForConstructor>
                                        {
                                            new LabelForConstructor("Корпус", ControlsStyle.LabelH2),
                                            new LabelForConstructor("Модель: {0}"),
                                            new LabelForConstructor("Партія: {0}"),
                                            new LabelForConstructor("Гарантія до {0}"),
                                            new LabelForConstructor("Лампа", ControlsStyle.LabelH2),
                                            new LabelForConstructor("Модель: {0}"),
                                            new LabelForConstructor("Партія: {0}"),
                                            new LabelForConstructor("Гарантія до {0}"),
                                            new LabelForConstructor("Електроблок", ControlsStyle.LabelH2),
                                            new LabelForConstructor("Партія: {0}",1),
                                            new LabelForConstructor("Гарантія до {0}"),
                                        };

                MainProcess.CreateButton("Демонтаж", 15, 275, 100, 35, "breakDown", BreakDown);
                MainProcess.CreateButton("Перемістити", 125, 275, 100, 35, "installNew", InstallNew);
            }
        }

        public override void OnBarcode(string Barcode)
        {
            if (Barcode.IsValidBarcode())
            {
                //Тип отсканированого комплектующего
                TypeOfAccessories type = BarcodeWorker.GetTypeOfAccessoriesByBarcode(Barcode);

                switch (type)
                {
                        //Лампа - установка/замена лампы
                    case TypeOfAccessories.Lamp:
                        MainProcess.ClearControls();
                        MainProcess.Process = new ReplacingAccessory(
                            MainProcess, CaseBarcode, Barcode,
                            Cases.IsCaseHaveAccessory(CaseBarcode, TypeOfAccessories.Lamp),
                            TypeOfAccessories.Lamp);
                        break;
                        //Корпус - установка/замена корпуса
                    case TypeOfAccessories.Case:
                        MainProcess.ClearControls();
                        MainProcess.Process = new ReplaceLights_SelectNew(MainProcess, Barcode, CaseBarcode);
                        break;
                        //Ел.блок - установка/замена блока
                    case TypeOfAccessories.ElectronicUnit:
                        MainProcess.ClearControls();
                        MainProcess.Process = new ReplacingAccessory(
                            MainProcess, CaseBarcode, Barcode,
                            Cases.IsCaseHaveAccessory(CaseBarcode, TypeOfAccessories.ElectronicUnit),
                            TypeOfAccessories.ElectronicUnit);
                        break;
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
                case KeyAction.Proceed:
                    OnBarcode("9786175660690");
                    break;
            }
        }
        #endregion

        #region ButtonClick
        /// <summary>Демонтаж</summary>
        private void BreakDown()
        {
            MainProcess.ClearControls();
            MainProcess.Process = new RemovalLight(MainProcess, CaseBarcode);
        }

        /// <summary>Установка</summary>
        private void InstallNew()
        {
            MainProcess.ClearControls();
            MainProcess.Process = new InstallingNewLighter(MainProcess, CaseBarcode);
        }
        #endregion
    }
}

