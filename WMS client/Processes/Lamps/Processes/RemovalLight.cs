using WMS_client.Base.Visual.Constructor;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using WMS_client.Enums;
using WMS_client.db;
using System;

namespace WMS_client
{
    /// <summary>Демонтаж светильника</summary>
    public class RemovalLight : BusinessProcess
    {
        /// <summary>Штрихкод світильника</summary>
        private readonly string LightBarcode;
        /// <summary>ІД карти з якої знімаємо</summary>
        private int map;
        /// <summary>Номер позиції з якої знімаємо</summary>
        private int position;
        /// <summary>Номер регістру з якого знімаємо</summary>
        private int register;

        /// <summary>Демонтаж светильника</summary>
        public RemovalLight(WMSClient MainProcess, string lightBarcode)
            : base(MainProcess, 1)
        {
            LightBarcode = lightBarcode;

            IsLoad = true;
            DrawControls();
        }

        #region Override methods
        public override sealed void DrawControls()
        {
            if (IsLoad)
            {
                object[] data = getLightPositionInfo();
                map = Convert.ToInt32(data[1]);
                register = Convert.ToInt32(data[2]);
                position = Convert.ToInt32(data[3]);

                ListOfLabelsConstructor list = new ListOfLabelsConstructor(MainProcess, "ДЕМОНТАЖ СВІТИЛЬНИКУ", data);
                list.ListOfLabels = new List<LabelForConstructor>
                                        {
                                            new LabelForConstructor(string.Empty, ControlsStyle.LabelH2),
                                            new LabelForConstructor(string.Empty, ControlsStyle.LabelH2),
                                            new LabelForConstructor(string.Empty, ControlsStyle.LabelH2),
                                            new LabelForConstructor("Розташування:", ControlsStyle.LabelH2),
                                            new LabelForConstructor("Карта: {0}"),
                                            new LabelForConstructor("Регістр: {0}", 1),
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
        /// <summary>Завершення операції. Збереження інформації</summary>
        private void Ok_click()
        {
            finish();
            MainProcess.ClearControls();
            MainProcess.Process = new SelectingLampProcess(MainProcess);
        }

        /// <summary>Вихід</summary>
        private void Cancel_click()
        {
            MainProcess.ClearControls();
            MainProcess.Process = new SelectingLampProcess(MainProcess);
        }
        #endregion

        #region Query
        /// <summary>Позиция светильника</summary>
        /// <returns>Карта, Register, Position</returns>
        private object[] getLightPositionInfo()
        {
            SqlCeCommand query = dbWorker.NewQuery(@"SELECT m.Description, c.Map MapId, c.Register, c.Position 
FROM Cases c
LEFT JOIN Maps m ON m.Id=c.Map
WHERE RTRIM(c.Barcode)=@Barcode");
            query.AddParameter("Barcode", LightBarcode);
            return query.SelectArray();
        }

        /// <summary>Завершение (Сохранение)</summary>
        private void finish()
        {
            Cases.ChangeLighterState(LightBarcode, TypesOfLampsStatus.Storage, true);

            SqlCeCommand query = dbWorker.NewQuery("SELECT SyncRef FROM Cases WHERE RTRIM(Barcode)=RTRIM(@Barcode)");
            query.AddParameter("Barcode", LightBarcode);
            object syncRefObj = query.ExecuteScalar();
            string syncRef = syncRefObj == null ? string.Empty : syncRefObj.ToString();

            //Внесение записи в "Перемещение"
            Movement.RegisterLighter(LightBarcode, syncRef, OperationsWithLighters.Removing, map, register, position);
        }
        #endregion
    }
}

