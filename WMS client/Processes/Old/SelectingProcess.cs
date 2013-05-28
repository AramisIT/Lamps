using System.Data;

namespace WMS_client
{
    public class SelectingProcess : BusinessProcess
    {
        #region Private fields

        private MobileTable table;
      
        #endregion

        #region Constructor

        public SelectingProcess(WMSClient MainProcess)
            : base(MainProcess, 1)
        {

            BusinessProcessType = ProcessType.Selecting;
            FormNumber = 1;
        }

        #endregion


        #region Override methods

        public override void DrawControls()
        {
            MainProcess.ToDoCommand = "Выберите операцию";            
            
            #region Создание меню операций
            
            var dataTable = new DataTable();
            dataTable.Columns.AddRange(new[] {
                new DataColumn("OperationName", typeof(string)),
                new DataColumn("type", typeof(short))
            });

            table = MainProcess.CreateTable("operations", 259, onRowSelected);
            table.DT = dataTable;
            table.AddColumn("Операция", "OperationName", 214);
            table.AddRow("Прием расх. материалов", 1);
            table.AddRow("Регистрация качества", 2);
            table.AddRow("Регистрация качества несорт.", 3);
           
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
                        MainProcess.Process = new RegistrationProcess(MainProcess);
                        break;
                    }

                case KeyAction.F5:
                    {
                        PerformQuery("ПолучитьСписокПланаПриходаТары");
                        if (ResultParameters == null || ResultParameters[0] == null)
                        {
                            ShowMessage("Подойдите в зону беспроводного покрытия");
                            return;
                        }

                        var dataTable = ResultParameters[0] as DataTable;
                        if (dataTable!=null&&dataTable.Rows.Count < 1)
                        {
                            ShowMessage("В полученном документе нет строк для приема!");
                            return;
                        }
                        
                        MainProcess.ClearControls();
                        MainProcess.Process = new IncomingProcess(MainProcess, dataTable);
                        break;
                    }
            }
        }

        #endregion

        private void onRowSelected(object sender, OnRowSelectedEventArgs e)
        {
            short type = (short)(e.SelectedRow["type"]);

            switch (type)
            {
                case 1:
                    {
                        OnHotKey(KeyAction.Proceed);
                        return;
                    }
                case 2:
                    {                        
                        BusinessProcess process;

                        try
                        {
                            process = new QualityRegistrationProcess(MainProcess);
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
                            process = new RawProductionQualityRegistrationProcess(MainProcess);
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

    }
}

