using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Windows.Forms;

namespace WMS_client
{
    public class IncomingProcess : BusinessProcess
    {
        #region Private fields

        private ulong unqueCode = 0;
        private ulong unitCode = 0;

        private enum Steps
        {
            DocumentSelecting,
            Incoming
        }

        #region Элементы управления при приемке

        private Label nomenclatureLabel;
        private Label planLabel;
        private Label realLabel;
        private Label uniqueCodeLabel;

        private ulong uniqueCodeText
        {
            set
            {
                if (value == 0)
                    uniqueCodeLabel.Text = "Отмена - Esc";
                else
                    uniqueCodeLabel.Text = string.Format("Уникальный код: {0}", value);
            }
        }

        private Label actedLabel;
        private TextBox actedTextBox;

        #endregion

        private DataRow currentRowValue;
        private string nomenclatureValue;
        private DataRow currentRow
        {
            set
            {
                if (nomenclatureValue != value["Номенклатура"] as string)
                {
                    currentRowValue = value;
                    nomenclatureValue = value["Номенклатура"] as string;
                    nomenclatureLabel.Text = nomenclatureValue.Trim();

                    int plan = Convert.ToInt32(value["План"]);
                    int baseCount = Convert.ToInt32(value["КоличествоЕдиницПоУмолчанию"]);
                    planLabel.Text = string.Format("{0} палл. / {1} {2}", plan, (plan * baseCount), currentRowValue["ЕИПоУмолчанию"] as string);
                }
            }
            get
            {
                return currentRowValue;
            }
        }
        private Steps processStep;
        private MobileTable TableControl;
        private DataTable dataTable;
        private DataGrid docsVisual;
        private string TimeStart = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");

        #endregion

        #region Constructor

        public IncomingProcess(WMSClient MainProcess, DataTable dt)
        : base(MainProcess, 1)   
        {
            BusinessProcessType = ProcessType.Incoming;
            processStep = Steps.DocumentSelecting;
            dataTable = dt;
            ShowDocsList();
        } 

        #endregion

        #region methods

        private void ShowWaresVisual()
        {
            docsVisual.Show();

            docsVisual.Focus();
        }

        private void OnDataGridViewKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                OnHotKey(KeyAction.Proceed);
            }
        }

        private void ShowDocsList()
        {
            TableControl.DT = dataTable;
            TableControl.AddColumn("Документ", 84);
            TableControl.AddColumn("Контрагент", 130);

            ShowdocsVisual();
        }

        private void SetCurrentColumn()
        {
            if (dataTable.Rows.Count > 0 && docsVisual.TableStyles["Mobile"].GridColumnStyles.Count > 1)
            {
                docsVisual.CurrentCell = new DataGridCell(docsVisual.CurrentCell.RowNumber, 1);
            }
        }

        private void ShowdocsVisual()
        {           
            SetCurrentColumn();
            TableControl.Focus();
        }

        private void showReal()
        {
            var dr = DataTableEx.FindRowInTable(dataTable, unitCode, "КодЕИ");
            if (dr != null)
            {
                showReal(dr);
            }
        }

        private void showReal(DataRow dr)
        {
            int real = Convert.ToInt32(dr["Факт"]);
            int countInOnePallet = Convert.ToInt32(dr["КоличествоЕдиницПоУмолчанию"]);
            int realValue = real * countInOnePallet;
            var smallUnitCode = dr["КодЕИПоУмолчанию"].ToString();

            for (int i = dataTable.Rows.IndexOf(dr) + 1; i < dataTable.Rows.Count && dataTable.Rows[i]["КодЕИ"].ToString() == smallUnitCode; i++)
            {
                real++;
                realValue += Convert.ToInt32(dataTable.Rows[i]["Факт"]);
            }

            realLabel.Text = string.Format("{0} палл. / {1} {2}", real, realValue, dr["ЕИПоУмолчанию"] as string);
        }

        #endregion

        #region Override methods

        public override void DrawControls()
        {
            switch (processStep)
            {
                case Steps.DocumentSelecting:
                    {
                        #region Выбор документа

                        //PerformQuery("ПолучитьСписокПланаПриходаТары");
                        //if (Parameters == null || Parameters[0] == null)
                        //{
                        //    ShowMessage("Подойдите в зону беспроводного покрытия");
                        //    MainProcess.Process = new SelectingProcess(MainProcess);
                        //    return;
                        //}

                        //dataTable = Parameters[0] as DataTable;
                        //if (dataTable.Rows.Count == 0)
                        //{
                        //    ShowMessage("Нет запланированных документов для приема расходных материалов!");
                        //    MainProcess.Process = new SelectingProcess(MainProcess);
                        //    return;
                        //}

                        TableControl = MainProcess.CreateTable("WareList", 259);
                        TableControl.OnKeyPressedEvent = OnDataGridViewKeyPress;
                        docsVisual = TableControl.GetControl() as DataGrid;
                        MainProcess.ToDoCommand = "Выберите документ";

                     

                        break;
                        #endregion
                    }
                case Steps.Incoming:
                    {
                        #region Пересчет расходных материалов

                        MainProcess.ClearControls();

                        nomenclatureLabel = MainProcess.CreateLabel("<Ожидается код>", 19, 59, 215, ControlsStyle.LabelNormal).GetControl() as Label;
                        MainProcess.CreateLabel("Запланировано:", 19, 93, 201, ControlsStyle.LabelNormal);
                        planLabel = MainProcess.CreateLabel("0 / 0", 19, 120, 201, ControlsStyle.LabelRedRightAllign).GetControl() as Label;
                        MainProcess.CreateLabel("Принято:", 19, 150, 201, ControlsStyle.LabelNormal);
                        realLabel = MainProcess.CreateLabel("0 / 0", 19, 177, 201, ControlsStyle.LabelRedRightAllign).GetControl() as Label;
                        actedLabel = MainProcess.CreateLabel("Количество брака:", 19, 228, 201, ControlsStyle.LabelNormal).GetControl() as Label;
                        actedTextBox = MainProcess.CreateTextBox(136, 262, 81, "", ControlsStyle.LabelRedRightAllign, OnEnterToActTextbox, false).GetControl() as TextBox;
                        uniqueCodeLabel = MainProcess.CreateLabel("Отмена - Esc", 19, 290, 201, ControlsStyle.LabelNormal).GetControl() as Label;
                        actedLabel.Visible = false;
                        actedTextBox.Visible = false;

                        MainProcess.ToDoCommand = "ПРИЕМ РАСХ. МАТЕРИАЛОВ";
                        break;

                        #endregion
                    }
            }
        }

        public override void OnBarcode(string Barcode)
        {
            if (processStep == Steps.DocumentSelecting) return;
            bool isUniqueCode = false;
            int asterixIndex = 0;

            #region Получение кода единицы измерения, уник. кода

            if (Barcode.Length > 8 && Barcode.Substring(0, 3) == "SB_" && Barcode.IndexOf('.') > 0)
            {
                bool isWrongFormat = false;

                if (Barcode.Substring(3, 5) != "UNIT.")
                {
                    isWrongFormat = true;
                }
                else
                {
                    string barBody = Barcode.Substring(8, Barcode.Length - 8);
                    asterixIndex = barBody.IndexOf('*');

                    if (asterixIndex == -1)
                    {
                        if (!Number.IsNumber(barBody))
                        {
                            isWrongFormat = true;
                        }
                        else
                        {
                            unitCode = Convert.ToUInt64(barBody);
                        }
                    }
                    else
                    {
                        isUniqueCode = true;
                        string unitCodeStr = barBody.Substring(0, asterixIndex);
                        string uniqueCodeStr = barBody.Substring(asterixIndex + 1, barBody.Length - asterixIndex - 1);
                        if (!Number.IsNumber(unitCodeStr) || !Number.IsNumber(uniqueCodeStr))
                        {
                            isWrongFormat = true;
                        }
                        else
                        {
                            unitCode = Convert.ToUInt64(unitCodeStr);
                            unqueCode = Convert.ToUInt64(uniqueCodeStr);                           
                        }
                    }
                }
                if (isWrongFormat)
                {
                    ShowMessage("Неверный тип штрих-кода, ожидается единица измерения расх. мат.");
                    return;
                }
            }
            else
            {
                PerformQuery("НайтиКодЕдИзм", Barcode);
                if (ResultParameters == null || ResultParameters[0] == null) return;
                unitCode = Convert.ToUInt64(ResultParameters[0] as string);
                if (unitCode == 0)
                {
                    ShowMessage("Штрих-код не найден среди единиц измерения расходных материалов.");
                    return;
                }

            }

            #endregion

            var dr = DataTableEx.FindRowInTable(dataTable, unitCode, "КодЕИ");
            if (dr == null)
            {
                ShowMessage("Данного товара нет в плане прихода!");
                return;
            }

            if (isUniqueCode)
            {
                OnUniqueCode();
            }
            else
            {
                dr["Факт"] = ((int)(dr["Факт"])) + 1;
            }
            
            currentRow = dr;
            showReal(dr);

        }

        public override void OnHotKey(KeyAction TypeOfAction)
        {
            switch (TypeOfAction)
            {
                case KeyAction.BarCodeByHands:
                    {
                        BarCodeByHands();
                        break;
                    }

                case KeyAction.Esc:
                    {
                        #region Прерывание процесса

                        if (ShowQuery("Вы хотите прервать процесс?"))
                        {
                            MainProcess.ClearControls();
                            MainProcess.Process = new SelectingProcess(MainProcess);
                        }

                        break;

                        #endregion
                    }
                case KeyAction.Proceed:
                    {
                        if (processStep == Steps.DocumentSelecting)
                        {

                            var dr = dataTable.Rows[docsVisual.CurrentRowIndex];
                            DocumentNumber = System.Convert.ToInt64(dr["Номер"]).ToString();

                            ShortQuery("ПолучитьПланПриходаТары", true);
                            if (ResultParameters == null || ResultParameters[0] == null)
                            {
                                //ShowMessage("Подойдите в зону беспроводного покрытия и повторите попытку!");                                
                                return;
                            }

                            var newTable = ResultParameters[0] as DataTable;
                            if (newTable.Rows.Count == 0)
                            {
                                MainProcess.ClearControls();
                                ShowMessage("Выбранный документ не содержит строк!");
                                MainProcess.Process = new SelectingProcess(MainProcess);
                                return;
                            }

                            newTable.Columns.AddRange(new DataColumn[] {
                                new DataColumn("Факт", typeof(Int32)),
                                new DataColumn("УникальныйНомер", typeof(ulong))
                            });

                            foreach (DataRow drTemp in newTable.Rows)
                            {
                                drTemp["Факт"] = 0;
                                drTemp["УникальныйНомер"] = 0;
                            }

                            processStep = Steps.Incoming;
                            DrawControls();
                            dataTable = newTable;

                        }
                        else if (ShowQuery("Завершить операцию?"))
                        {
                            var result = dataTable.Copy();
                            result.Columns.Remove("Номенклатура");                            
                            result.Columns.Remove("КодЕИПоУмолчанию");
                            result.Columns.Remove("КоличествоЕдиницПоУмолчанию");
                            result.Columns.Remove("ЕИ");
                            result.Columns.Remove("ЕИПоУмолчанию");

                            ShortQuery("ЗавершитьПриемРасхМатериалов", result);
                            if (ResultParameters == null) return;

                            MainProcess.ClearControls();
                            MainProcess.Process = new SelectingProcess(MainProcess);
                                
                        }
                        break;
                    }
            }
        }

        #endregion

        #region Business process methods

        private void Proceed()
        {


        }

        #endregion

        private void OnEnterToActTextbox(object obj, EventArgs e)
        {
            if (!actedLabel.Visible) return;

            bool cancel = false;

            if (!Number.IsNumber(actedTextBox.Text))
            {
                cancel = true;
            }

            int count = Convert.ToInt32(actedTextBox.Text);
            var dr = DataTableEx.FindRowInTable(dataTable, unitCode, "КодЕИ");

            if (((double)(dr["КоличествоЕдиницПоУмолчанию"])) <= count)
            {
                ShowMessage(string.Format("Введенное значение - {0} превышает допустимое - {1}", count, ((double)(dr["КоличествоЕдиницПоУмолчанию"])) - 1));
                return; 
            }

            cancel = count == 0;

            actedLabel.Hide();
            actedTextBox.Hide();
            uniqueCodeText = 0; 
            if (cancel) return;

            var newR = dataTable.NewRow();

            newR["КодЕИ"] = dr["КодЕИПоУмолчанию"];
            newR["КодЕИПоУмолчанию"] = 0;

            newR["План"] = dr["КоличествоЕдиницПоУмолчанию"];
            newR["Факт"] = System.Convert.ToInt32(newR["План"]) - count;
            newR["УникальныйНомер"] = unqueCode;
            newR["КоличествоЕдиницПоУмолчанию"] = 1;

            dataTable.Rows.InsertAt(newR, dataTable.Rows.IndexOf(dr) + 1);

            showReal(dr);
        }

        private void OnUniqueCode()
        {
            var existDr = DataTableEx.FindRowInTable(dataTable, unqueCode, "УникальныйНомер");
            if (existDr != null)
            {
                if (ShowQuery(string.Format("Удалить паллету из прихода (уникальный код = {0})?", unqueCode)))
                {
                    var dr = DataTableEx.FindRowInTable(dataTable, unitCode, "КодЕИ");
                    dataTable.Rows.Remove(existDr);
                    showReal(dr);
                }
                return;
            }
            
            uniqueCodeText = unqueCode;
            actedLabel.Show();
            actedTextBox.Text = "1";
            actedTextBox.Show();

            actedTextBox.SelectionStart = 0;
            actedTextBox.SelectionLength = 1;
            actedTextBox.Focus();
        }
    }
}

