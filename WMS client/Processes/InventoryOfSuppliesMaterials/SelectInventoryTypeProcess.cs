using System;
using System.Data;

namespace WMS_client
{
    public class SelectInventoryTypeProcess : BusinessProcess
    {
        #region Private fields

        private MobileTable table;

        #endregion

        #region Constructor

        public SelectInventoryTypeProcess(WMSClient MainProcess)
            : base(MainProcess, 1)
        {

            BusinessProcessType = ProcessType.Selecting;
            FormNumber = 1;
        }

        #endregion


        #region Override methods

        public override void DrawControls()
        {
            MainProcess.ClearControls();
            MainProcess.ToDoCommand = "Выберите тип проверки";

            #region Создание меню операций

            var dataTable = new DataTable();
            dataTable.Columns.AddRange(new DataColumn[] {
                new DataColumn("OperationName", typeof(string)),
                new DataColumn("type", typeof(short))
            });

            table = MainProcess.CreateTable("operations", 259, onRowSelected);
            table.DT = dataTable;
            table.AddColumn("Тип инвентаризации", "OperationName", 214);
            table.AddRow("Проверка места хранения", 1);
            table.AddRow("Проверка номенклатуры", 2);
            table.AddRow("Проверка ед.изм.", 3);

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
                        MainProcess.Process = new SelectingProcess(MainProcess);
                        break;
                    }
            }
        }

        #endregion

        private void onRowSelected(object sender, OnRowSelectedEventArgs e)
        {
            short type = (short)(e.SelectedRow["type"]);
            BusinessProcess process;

            try
            {
                process = new SelectCellProcess(MainProcess, type);
            }
            catch (ConnectionIsNotExistsException exp)
            {
                ShowMessage(exp.Message);
                return;
            }

            MainProcess.Process = process;


        }

    }
}
