using System;
using System.Data;

namespace WMS_client
{
    public class SelectNomenclatureProcess : BusinessProcess
    {
        #region Private fields

        private MobileTable table;
        private int typeOfInventory;
        private long cellId;
        private DataTable nomenclaturesDT;
        #endregion

        #region Constructor

        public SelectNomenclatureProcess(WMSClient MainProcess, int typeOfInventory, long cellId)
            : base(MainProcess, 1)
        {            
            this.typeOfInventory = typeOfInventory;
            BusinessProcessType = ProcessType.Selecting;
            FormNumber = 1;
            this.cellId = cellId;
            nomenclaturesDT = ReadNomenclatureFromDB();
            if (nomenclaturesDT != null)
            {
                foreach (DataRow row in nomenclaturesDT.Rows)
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
                MainProcess.ToDoCommand = "Выберите номенклатуру";


                #region Создание меню операций

                var dataTable = new DataTable();
                dataTable.Columns.AddRange(new DataColumn[] {
                new DataColumn("Descr", typeof(string)),
                new DataColumn("id", typeof(long))
            });

                table = MainProcess.CreateTable("Nomenclatures", 259, onRowSelected);
                table.DT = dataTable;
                table.AddColumn("Номенклатура", "Descr", 214);

                table.Focus();

                #endregion
        }

        public override void OnBarcode(string Barcode) { }

        public override void OnHotKey(KeyAction TypeOfAction)
        {
            switch (TypeOfAction)
            {
                case KeyAction.Esc:

                    MainProcess.ClearControls();
                    MainProcess.Process = new SelectCellProcess(MainProcess, typeOfInventory);
                    break;
            }
        }

        #endregion

        private void onRowSelected(object sender, OnRowSelectedEventArgs e)
        {
            long id = (long)(e.SelectedRow["id"]);

            switch (typeOfInventory)
            {
                case 2:
                    {
                        BusinessProcess process;

                        try
                        {
                            process = new InventoryOfSuppliesMaterialsProcess(MainProcess, cellId, id, 0);
                        }
                        catch (ConnectionIsNotExistsException exp)
                        {
                            ShowMessage(exp.Message);
                            return;
                        }

                        MainProcess.Process = process;
                        return;
                    }
                case 3:
                    {
                        BusinessProcess process;

                        try
                        {
                            process = new SelectMeasureProcess(MainProcess, typeOfInventory, cellId, id);
                        }
                        catch (ConnectionIsNotExistsException exp)
                        {
                            ShowMessage(exp.Message);
                            return;
                        }

                        MainProcess.Process = process;
                        return;
                    }
            }

        }

        private DataTable ReadNomenclatureFromDB()
        {
            this.PerformQuery("ПолучитьПереченьНоменклатурыВЯчейке", cellId);
            if (this.Parameters == null)
            {
                ShowMessage("В выбранной ячейке нет остатков номенклатуры");
                MainProcess.ClearControls();
                MainProcess.Process = new SelectCellProcess(MainProcess, typeOfInventory);
                return null;
            }

            return this.Parameters[0] as DataTable;
        }
    }
}
