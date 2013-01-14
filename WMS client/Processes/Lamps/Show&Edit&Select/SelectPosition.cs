using System.Data;
using WMS_client.db;
using System.Data.SqlServerCe;
using System.Collections.Generic;

namespace WMS_client
{
    /// <summary>Выбор позиции для светильника</summary>
    public class SelectPosition : BusinessProcess
    {
        /// <summary>Информация по карте</summary>
        private readonly MapInfo MapInfo;
        /// <summary>Номер регистра</summary>
        private readonly string Register;
        /// <summary>Штрихкод светильника</summary>
        private readonly string LampBarCode;

        /// <summary>Выбор позиции для светильника</summary>
        /// <param name="MainProcess"></param>
        /// <param name="mapInfo">Информация по карте</param>
        /// <param name="register">Номер регистра</param>
        /// <param name="lampBarCode">Штрихкод светильника</param>
        public SelectPosition(WMSClient MainProcess, MapInfo mapInfo, string register, string lampBarCode)
            : base(MainProcess, 1)
        {
            FormNumber = 1;
            BusinessProcessType = ProcessType.Registration;
            MainProcess.ToDoCommand = "Оберіть позицію";
            MapInfo = mapInfo;
            Register = register;
            LampBarCode = lampBarCode;

            IsLoad = true;
            DrawControls();
        }

        #region Override methods
        public override sealed void DrawControls()
        {
            if (IsLoad)
            {
                DataTable sourceTable = new DataTable();
                sourceTable.Columns.AddRange(new[] {new DataColumn("Position", typeof (string))});
                MobileTable visualTable = MainProcess.CreateTable("Positions", 259, onRowSelected);
                visualTable.DT = sourceTable;
                visualTable.AddColumn("№Позиции", "Position", 214);

                List<object> list = getFilledPosition();

                for (int i = 1; i <= 25;i++ )
                {
                    if(!list.Contains(i))
                    {
                        visualTable.AddRow(i);
                    }
                }

                visualTable.Focus();
            }
        }

        /// <summary>Позиция выбрана</summary>
        private void onRowSelected(object sender, OnRowSelectedEventArgs e)
        {
            string position = e.SelectedRow["Position"].ToString();
            MainProcess.ClearControls();
            MainProcess.Process = new InstallingNewLighter(MainProcess, LampBarCode)
                                      {
                                          MapInfo = MapInfo,
                                          Register = Register,
                                          Position = position
                                      };
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
                    MainProcess.Process = new InstallingNewLighter(MainProcess, LampBarCode) { MapInfo = MapInfo };
                    break;
            }
        }
        #endregion

        #region Query
        private List<object> getFilledPosition()
        {
            SqlCeCommand query =
                dbWorker.NewQuery("SELECT c.Position FROM Cases c WHERE c.Map=@Map AND c.Register=@Register");
            query.AddParameter("Map", MapInfo.Id);
            query.AddParameter("Register", Register);

            return query.SelectToList();
        }
        #endregion
    }
}