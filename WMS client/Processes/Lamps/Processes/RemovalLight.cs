using WMS_client.Base.Visual.Constructor;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using WMS_client.Enums;
using WMS_client.db;

namespace WMS_client
{
    /// <summary>Демонтаж светильника</summary>
    public class RemovalLight : BusinessProcess
    {
        private readonly string LightBarcode;

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
                ListOfLabelsConstructor list = new ListOfLabelsConstructor(MainProcess, "ДЕМОНТАЖ СВІТИЛЬНИКУ", getLightPositionInfo());
                list.ListOfLabels = new List<LabelForConstructor>
                                        {
                                            new LabelForConstructor(string.Empty, ControlsStyle.LabelH2),
                                            new LabelForConstructor(string.Empty, ControlsStyle.LabelH2),
                                            new LabelForConstructor(string.Empty, ControlsStyle.LabelH2),
                                            new LabelForConstructor("Розташування:", ControlsStyle.LabelH2),
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
        private void Ok_click()
        {
            finish();
            MainProcess.ClearControls();
            MainProcess.Process = new SelectingLampProcess(MainProcess);
        }

        private void Cancel_click()
        {
            MainProcess.ClearControls();
            MainProcess.Process = new SelectingLampProcess(MainProcess);
        }
        #endregion

        #region Query
        private object[] getLightPositionInfo()
        {
            SqlCeCommand query = dbWorker.NewQuery(@"SELECT m.Description, c.Register, c.Position 
FROM Cases c
LEFT JOIN Maps m ON m.Id=c.Map
WHERE RTRIM(c.Barcode)=@Barcode");
            query.AddParameter("Barcode", LightBarcode);
            return query.SelectArray();
        }

        private void finish()
        {
            Cases.ChangeLighterStatus(LightBarcode, TypesOfLampsStatus.Storage, true);
        }
        #endregion
    }
}

