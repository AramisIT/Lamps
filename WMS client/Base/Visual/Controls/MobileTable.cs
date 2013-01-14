using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace WMS_client
{

    public class OnRowSelectedEventArgs : EventArgs
    {
        private readonly DataRow selectedRow;
        private readonly bool isRowSelected;

        public bool IsRowSelected
        {
            get { return isRowSelected; }
        }

        public DataRow SelectedRow
        {
            get { return selectedRow; }
        }

        public OnRowSelectedEventArgs(DataRow selectedRow)
        {
            if (selectedRow == null)
            {

                this.selectedRow = null;
                isRowSelected = false;
            }
            else
            {
                this.selectedRow = selectedRow;
                isRowSelected = true;
            }
        }

        public OnRowSelectedEventArgs()
        {
            selectedRow = null;
            isRowSelected = false;
        }

    }

    public class OnChangeSelectedRowEventArgs : EventArgs
    {
        public DataRow SelectedRow { get; set; }

        public OnChangeSelectedRowEventArgs(DataRow selectedRow)
        {
            SelectedRow = selectedRow;
        }
    }

    public class MobileTable : MobileControl
    {
        #region Private fields

        private readonly DataGrid Control = new DataGrid();
        private DataTable dt;

        #endregion

        #region Properties

        public DataGrid DataGrid
        {
            get { return Control; }
        }

        public OnKeyPressDelegate OnKeyPressedEvent
        {
            set { Control.KeyPress += new KeyPressEventHandler(value); }
        }

        public int RowsCount
        {
            get
            {
                if (dt == null) return 0;
                return dt.Rows.Count;
            }
        }

        public DataTable DT
        {
            get { return dt; }
            set
            {
                dt = value;

                if (dt != null && dt.TableName != "Mobile")
                {
                    dt.TableName = "Mobile";
                }

                Control.DataSource = dt;

                // Убираем автоматически сформировавшиеся столбики
                Control.TableStyles["Mobile"].GridColumnStyles.Clear();
                Control.RowHeadersVisible = false;
            }
        }

        #endregion

        public event Void2paramDelegate<object, OnRowSelectedEventArgs> OnRowSelected;
        public event Void2paramDelegate<object, OnChangeSelectedRowEventArgs> OnChangeSelectedRow;

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

        #region Public methods

        public MobileTable(MainForm Form, string controlName, int height, int top)
        {
            Control = new DataGrid
                          {
                              ForeColor = System.Drawing.Color.Navy,
                              Left = 3,
                              Top = top,
                              Width = 234,
                              Height = height,
                              Name = controlName,
                              Visible = false,
                              Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Regular)
                          };
            Control.TableStyles.Add(new DataGridTableStyle());
            Control.TableStyles[0].MappingName = "Mobile";
            Control.PreferredRowHeight = 25;
            Form.Controls.Add(Control);

            Control.KeyPress += onKeyPressed;
            lastSelectedRowIndex = -1;
            DataGrid.CurrentCellChanged += DataGrid_CurrentCellChanged;
        }

        public MobileTable(MainForm Form, string controlName, int height, int top, Void2paramDelegate<object, OnChangeSelectedRowEventArgs> onChangeSelectedRow)
            : this(Form, controlName, height, top)
        {
            DataGrid.CurrentCellChanged += DataGrid_CurrentCellChanged;
        }

        public MobileTable(MainForm Form, string controlName, int height, int top, Void2paramDelegate<object, OnRowSelectedEventArgs> onRowSelected):this(Form, controlName, height, top)
        {
            if (onRowSelected != null)
            {
                OnRowSelected += onRowSelected;
            }
        }

        private int lastSelectedRowIndex;

        void DataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = (DataGrid) sender;

            if (lastSelectedRowIndex != grid.CurrentCell.RowNumber)
            {
                lastSelectedRowIndex = DataGrid.CurrentCell.RowNumber;

                if(OnChangeSelectedRow!=null)
                {
                    DataRow selectedRow = dt.Rows[Control.CurrentRowIndex];
                    OnChangeSelectedRow(grid, new OnChangeSelectedRowEventArgs(selectedRow));
                }
            }
        }

        public static DataRow FindRow(DataTable Table, string Name, object Value)
        {
            return Table.Rows.Cast<DataRow>().FirstOrDefault(Row => Convert.ToString(Row[Name]) == Convert.ToString(Value));
        }

        public void Focus()
        {
            if (!Control.Visible)
            {
                Control.Show();
            }
            Control.Focus();
        }

        public DataRow AddRow(params object[] parameters)
        {
            if (dt == null || parameters.Length > dt.Columns.Count) return null;

            var dr = dt.NewRow();

            for (int i = 0; i < parameters.Length; i++)
            {
                dr[i] = parameters[i];
            }

            dt.Rows.Add(dr);

            if(dt.Rows.Count==1)
            {
                DataGrid.Select(0);
                DataGrid_CurrentCellChanged(DataGrid, new EventArgs());
            }

            return dr;
        }

        public void HideRowHeaders()
        {
            Control.RowHeadersVisible = false;
        }

        public void ClearRows()
        {
            if (dt != null)
            {
                dt.Rows.Clear();
            }
        }

        #endregion

        #region Override methods

        public void AddColumn(string ColumnName, int Width)
        {
            AddColumn(ColumnName, ColumnName, Width);
        }

        public void AddColumn(string Caption, string ColumnName, int Width)
        {
            DataGridTextBoxColumn ColumnStyle = new DataGridTextBoxColumn();
            ColumnStyle.HeaderText = Caption;
            ColumnStyle.MappingName = ColumnName;
            ColumnStyle.Width = Width;
            Control.TableStyles["Mobile"].GridColumnStyles.Add(ColumnStyle);
        }

        public override string GetName()
        {
            return Control.Name;
        }

        public override object GetControl()
        {
            return Control;
        }
        public override void Hide()
        {
            Control.Visible = false;
        }
        public override void Show()
        {
            Control.Visible = true;
        }

        #endregion

        private void onKeyPressed(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case '\r':
                    {
                        #region Обработка выбора строки нажатием <Enter>

                        if (Control.CurrentRowIndex == -1)
                        {
                            // строка не выбрана, ничего не вызываем                            
                        }
                        else
                        {
                            DataRow currentDr = dt.Rows[Control.CurrentRowIndex];
                            if (OnRowSelected != null)
                            {
                                OnRowSelected(sender, new OnRowSelectedEventArgs(currentDr));
                            }
                        }
                        return;

                        #endregion
                    }
            }
        }
    }
}
