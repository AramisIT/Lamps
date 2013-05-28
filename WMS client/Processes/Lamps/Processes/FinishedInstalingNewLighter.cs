using WMS_client.Base.Visual.Constructor;
using System.Collections.Generic;
using WMS_client.Enums;
using WMS_client.db;
using System.Data.SqlServerCe;
using System;

namespace WMS_client
    {
    /// <summary>Завершение установки светильника</summary>
    public class FinishedInstalingNewLighter : BusinessProcess
        {
        /// <summary>Штрихкод светильника</summary>
        private readonly string LightBarcode;
        /// <summary>ИД карты на которую устанавливаеться светильник</summary>
        private readonly object MapId;

        /// <summary>Завершение установки светильника</summary>
        /// <param name="MainProcess"></param>
        /// <param name="parameters">Инфо: Карта, Регістр, Позиція</param>
        /// <param name="mapId">ИД карты на которую устанавливаеться светильник</param>
        /// <param name="lampBarCode">Штрихкод светильника</param>
        public FinishedInstalingNewLighter(WMSClient MainProcess, object[] parameters, object mapId, string lampBarCode)
            : base(MainProcess, 1)
            {
            ResultParameters = parameters;
            FormNumber = 1;
            BusinessProcessType = ProcessType.Registration;
            MapId = mapId;

            LightBarcode = lampBarCode;

            IsLoad = true;
            DrawControls();
            }

        #region Override methods
        public override sealed void DrawControls()
            {
            if (IsLoad)
                {
                ListOfLabelsConstructor list = new ListOfLabelsConstructor(MainProcess, "ВСТАНОВЛЕННЯ СВІТИЛЬНИКУ", ResultParameters);
                list.ListOfLabels = new List<LabelForConstructor>
                                        {
                                            new LabelForConstructor(string.Empty, ControlsStyle.LabelH2),
                                            new LabelForConstructor(string.Empty, ControlsStyle.LabelH2),
                                            new LabelForConstructor(string.Empty, ControlsStyle.LabelH2),
                                            new LabelForConstructor("МІСЦЕ ВСТАНОВЛЕННЯ:", ControlsStyle.LabelH2),
                                            new LabelForConstructor("Карта: {0}"),
                                            new LabelForConstructor("Регістр: {0}"),
                                            new LabelForConstructor("Позиція №{0}")
                                        };

                MainProcess.CreateButton("Oк", 10, 275, 105, 35, "ok", Ok_click);
                MainProcess.CreateButton("Відміна", 125, 275, 105, 35, "cancel", Cancel_click);
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

        #region ButtonClick

        /// <summary>Сохранение</summary>
        private void Ok_click()
            {
            FinishedInstaling();
            MainProcess.ClearControls();
            MainProcess.Process = new SelectingLampProcess(MainProcess);
            }

        /// <summary>Откат</summary>
        private void Cancel_click()
            {
            MainProcess.ClearControls();
            MainProcess.Process = new SelectingLampProcess(MainProcess);
            }
        #endregion

        #region Query
        /// <summary>Сохранение размещения светильника</summary>
        public void FinishedInstaling()
            {
            Cases.ChangeLighterState(LightBarcode, TypesOfLampsStatus.IsWorking, false);

            SqlCeCommand query = dbWorker.NewQuery(
                    "UPDATE Cases SET Map=@Map,Register=@Register,Position=@Position,DateOfActuality=@DateOfActuality WHERE RTRIM(Barcode)=RTRIM(@Barcode)");
            query.AddParameter("Map", MapId);
            query.AddParameter("Register", ResultParameters[1]);
            query.AddParameter("Position", ResultParameters[2]);
            query.AddParameter("Barcode", LightBarcode);
            query.AddParameter("DateOfActuality", DateTime.Now);
            query.ExecuteNonQuery();

            query = dbWorker.NewQuery("SELECT SyncRef FROM Cases WHERE RTRIM(Barcode)=RTRIM(@Barcode)");
            query.AddParameter("Barcode", LightBarcode);
            object syncRefObj = query.ExecuteScalar();
            string syncRef = syncRefObj == null ? string.Empty : syncRefObj.ToString();

            //Внесение записи в "Перемещение"
            Movement.RegisterLighter(LightBarcode, syncRef, OperationsWithLighters.Installing,
                                     (int)MapId, Convert.ToInt32(ResultParameters[1]), Convert.ToInt32(ResultParameters[2]));
            }
        #endregion
        }
    }

