using System;
using System.Data;

namespace WMS_client
{
    public class SelectCellProcess : BusinessProcess
    {
        #region Private fields

        private DataTable cellsDT;
        private MobileTable table;
        private int typeOfInventory;
        #endregion

        #region Constructor

        public SelectCellProcess(WMSClient MainProcess, int typeOfInventory)
            : base(MainProcess, 1)
        {
            this.typeOfInventory = typeOfInventory;
            BusinessProcessType = ProcessType.Selecting;
            FormNumber = 1;
            cellsDT = ReadCellFromDB();
            if (cellsDT != null)
            {
                foreach (DataRow row in cellsDT.Rows)
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
            MainProcess.ToDoCommand = "Выберите ячейку";

            #region Создание меню операций

            var dataTable = new DataTable();
            dataTable.Columns.AddRange(new DataColumn[] {
                new DataColumn("Descr", typeof(string)),
                new DataColumn("Id", typeof(long))
            });

            table = MainProcess.CreateTable("cells", 259, onRowSelected);
            table.DT = dataTable;
            table.AddColumn("Ячейка", "Descr", 214);

            table.Focus();

            #endregion
            
        }
      
        public override void OnBarcode(string Barcode){}

        public override void OnHotKey(KeyAction TypeOfAction)
        {
            switch (TypeOfAction)
            {
                case KeyAction.Esc:
                    {
                        MainProcess.ClearControls();
                        MainProcess.Process = new SelectingProcess(MainProcess);
                        break;
                    }

                case KeyAction.F5:
                    {
                        PerformQuery("ПолучитьСписокПланаПриходаТары");
                        if (Parameters == null || Parameters[0] == null)
                        {
                            ShowMessage("Подойдите в зону беспроводного покрытия");
                            return;
                        }

                        var dataTable = Parameters[0] as DataTable;
                        if (dataTable.Rows.Count < 1)
                        {
                            ShowMessage("В полученном документе нет строк для приема!");
                            return;
                        }
                        else
                        {
                            MainProcess.ClearControls();
                            MainProcess.Process = new IncomingProcess(MainProcess, dataTable);
                            break;
                        }
                    }
            }
        }

        #endregion

        private void onRowSelected(object sender, OnRowSelectedEventArgs e)
        {
            long id = (long)(e.SelectedRow["id"]);

            switch (typeOfInventory)
            {
                case 1:
                    {
                        BusinessProcess process;

                        try
                        {
                            process = new InventoryOfSuppliesMaterialsProcess(MainProcess, id, 0, 0);
                        }
                        catch (ConnectionIsNotExistsException exp)
                        {
                            ShowMessage(exp.Message);
                            return;
                        }

                        MainProcess.Process = process;
                        return;
                    }
                default:
                    {                        
                        BusinessProcess process;

                        try
                        {
                            process = new SelectNomenclatureProcess(MainProcess, typeOfInventory, id);
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
        
        private DataTable ReadCellFromDB()
        {
            PerformQuery("ПолучитьПереченьЯчеекДляХраненияТары");
            if (Parameters == null ||
                Parameters[0] == null ||
                ((DataTable)Parameters[0]).Rows.Count == 0)
            {
                ShowMessage("Нет ячеек предназначеных для хранения тары");
                MainProcess.ClearControls();
                MainProcess.Process = new SelectingProcess(MainProcess);
                return null;
            }

            return this.Parameters[0] as DataTable;
        }
    }
}
