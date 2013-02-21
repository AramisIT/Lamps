using System;
using System.Collections.Generic;
using WMS_client.Base.Visual.Constructor;
using System.Data.SqlServerCe;
using WMS_client.Enums;
using WMS_client.Processes.Lamps;
using WMS_client.db;

namespace WMS_client
{
    /// <summary>Выбрана лампы</summary>
    public class ChooseLamp : BusinessProcess
    {
        /// <summary>Штрихкод лампы</summary>
        private readonly string LampBarcode;

        /// <summary>Выбрана лампы</summary>
        /// <param name="MainProcess"></param>
        /// <param name="lampBarcode">Штрихкод лампы</param>
        public ChooseLamp(WMSClient MainProcess, string lampBarcode)
            : base(MainProcess, 1)
        {
            FormNumber = 1;
            BusinessProcessType = ProcessType.Registration;
            LampBarcode = lampBarcode;

            IsLoad = true;
            DrawControls();
        }

        #region Override methods
        public override sealed void DrawControls()
        {
            if (IsLoad)
            {
                TypesOfLampsStatus state = Accessory.GetState(TypeOfAccessories.Lamp, LampBarcode);
                ListOfLabelsConstructor list = new ListOfLabelsConstructor(MainProcess, "Лампа", getData());
                list.ListOfLabels = new List<LabelForConstructor>
                                        {
                                            new LabelForConstructor(string.Empty, false),
                                            new LabelForConstructor(string.Empty, false),
                                            new LabelForConstructor(string.Empty, false),
                                            new LabelForConstructor(string.Empty, false),
                                            new LabelForConstructor("Модель: {0}"),
                                            new LabelForConstructor("Партія: {0}"),
                                            new LabelForConstructor("Гарантія до {0}")
                                        };


                if (state == TypesOfLampsStatus.ToRepair)
                {
                    MainProcess.CreateButton("Зберігання", 15, 275, 100, 35, "storage", Storage_Click);
                }
                else
                {
                    MainProcess.CreateButton("Ремонт", 15, 275, 100, 35, "repair", Repair_Click);
                }
                MainProcess.CreateButton("Списання", 125, 275, 100, 35, "writeoff", Writeoff_Click, null, null, state != TypesOfLampsStatus.ToCharge);
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
        private void Storage_Click()
        {
            MainProcess.ClearControls();
            MainProcess.Process = new SetAccessoryNewState(MainProcess, LampBarcode, TypeOfAccessories.Lamp, TypesOfLampsStatus.Storage);
        }

        private void Repair_Click()
        {
        }

        private void Writeoff_Click()
        {
            MainProcess.ClearControls();
            MainProcess.Process = new SetAccessoryNewState(MainProcess, LampBarcode, TypeOfAccessories.Lamp, TypesOfLampsStatus.ToCharge);
        }
        #endregion

        #region Query
        /// <summary>Инфо о лампе</summary>
        /// <returns>Масив иформации</returns>
        private object[] getData()
        {
            SqlCeCommand query = dbWorker.NewQuery(@"SELECT
    t.Description Model
    , p.Description Party
    , l.DateOfWarrantyEnd Warrantly
FROM Lamps l
LEFT JOIN Models t ON t.Id=l.Model
LEFT JOIN Party p ON p.Id=l.Party
WHERE RTRIM(l.BarCode)=RTRIM(@BarCode)");
            query.AddParameter("BarCode", LampBarcode);

            return query.SelectArray(new Dictionary<string, Enum> {{BaseFormatName.DateTime, DateTimeFormat.OnlyDate}});
        }
        #endregion
    }
}