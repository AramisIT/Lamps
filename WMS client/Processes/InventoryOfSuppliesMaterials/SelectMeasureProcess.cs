using System;
using System.Data;

namespace WMS_client
{
    public class SelectMeasureProcess : BusinessProcess
    {
        #region Private fields

        private MobileTable table;
        private int typeOfInventory;
        private long cellId;
        private long nomenclatureId;
        private DataTable measuresDT;
        #endregion

        #region Constructor

        public SelectMeasureProcess(WMSClient MainProcess, int typeOfInventory, long cellId, long nomenclatureId)
            : base(MainProcess, 1)
        {
            this.typeOfInventory = typeOfInventory;
            BusinessProcessType = ProcessType.Selecting;
            FormNumber = 1;
            this.cellId = cellId;
            this.nomenclatureId = nomenclatureId;
            measuresDT = ReadMeasureFromDB();
            if (measuresDT != null)
            {
                foreach (DataRow row in measuresDT.Rows)
                {
                    table.AddRow(row["Descr"], row["Id"]);
                }
            }
        }
        #endregion


        #region Override methods

        public override void DrawControls()
        {            
                MainProcess.ClearControls();
                MainProcess.ToDoCommand = "Выберите Ед.Изм.";

                DataTable readedCells = ReadMeasureFromDB();

                #region Создание меню операций

                var dataTable = new DataTable();
                dataTable.Columns.AddRange(new DataColumn[] {
                new DataColumn("Descr", typeof(string)),
                new DataColumn("id", typeof(long))
            });

                table = MainProcess.CreateTable("Measures", 259, onRowSelected);
                table.DT = dataTable;
                table.AddColumn("Ед.Изм.", "Descr", 214);

                table.Focus();

                #endregion
        }

        public override void OnBarcode(string Barcode) { }

        public override void OnHotKey(KeyAction TypeOfAction)
        {
            switch (TypeOfAction)
            {
                case KeyAction.Esc:
                    {
                        MainProcess.ClearControls();
                        MainProcess.Process = new SelectNomenclatureProcess(MainProcess, typeOfInventory, cellId);
                        break;
                    }
            }
        }

        #endregion

        private void onRowSelected(object sender, OnRowSelectedEventArgs e)
        {
            long id = (long)(e.SelectedRow["id"]);

            BusinessProcess process;
            try
            {
                process = new InventoryOfSuppliesMaterialsProcess(MainProcess, cellId, nomenclatureId, id);
            }
            catch (ConnectionIsNotExistsException exp)
            {
                ShowMessage(exp.Message);
                return;
            }

            MainProcess.Process = process;
            return;


        }

        private DataTable ReadMeasureFromDB()
        {
            this.PerformQuery("ПолучитьПереченьЕдИзмНоменклатуры", nomenclatureId);
            if (this.Parameters == null)
            {
                return null;
            }

            return this.Parameters[0] as DataTable;
        }
    }
}
