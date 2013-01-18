using System;
using System.Drawing;
using WMS_client.db;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlServerCe;
using System.Windows.Forms;

namespace WMS_client
{
    /// <summary>�������� ��������</summary>
    public class ValueEditor : BusinessProcess
    {
        /// <summary>�������������</summary>
        private static Accessory accessory;
        /// <summary>�������� (���������� ��� ���������) ��� �������������� (��� ��� � �������� ������)</summary>
        private static Type mainType;
        /// <summary>������� (���������� ��� ���������) ���������</summary>
        private static string mainTopic;
        /// <summary>������� ��� �������������� (��� ��� �� ������� ������� � ���������)</summary>
        private readonly Type currentType;
        /// <summary>������� ���������</summary>
        private readonly string currentTopic;
        /// <summary>��������������� �����-���</summary>
        private readonly string barcode;
        /// <summary>������������� ��������</summary>
        private readonly string propertyName;


        /// <summary>��������� Id</summary>
        private int selectedId;
        /// <summary>������ ���������-"��������� �������"</summary>
        private readonly List<MobileControl> controls = new List<MobileControl>();
        private Type valueType;
        private MobileButton okButton; 

        /// <summary>�������� ��������</summary>
        /// <param name="MainProcess"></param>
        /// <param name="mainType">�������� (���������� ��� ���������) ��� �������������� (��� ��� � �������� ������)</param>
        /// <param name="mainTopic">������� (���������� ��� ���������) ���������</param>
        /// <param name="currentType">������� ��� �������������� (��� ��� �� ������� ������� � ���������)</param>
        /// <param name="currentTopic">������� ���������</param>
        /// <param name="accessory">�������������</param>
        /// <param name="topic">���������</param>
        /// <param name="barcode">��������������� �����-���</param>
        /// <param name="propertyName">������������� ��������</param>
        public ValueEditor(WMSClient MainProcess, Type mainType, string mainTopic, Type currentType, string currentTopic, Accessory accessory, string topic, string barcode, string propertyName)
            : base(MainProcess, 1)
        {
            MainProcess.ToDoCommand = topic;

            ValueEditor.accessory = accessory;
            ValueEditor.mainType = mainType;
            ValueEditor.mainTopic = mainTopic;
            this.currentType = currentType;
            this.currentTopic = currentTopic;
            this.barcode = barcode;
            this.propertyName = propertyName;

            IsLoad = true;
            DrawControls();
        }

        #region Override methods
        public override sealed void DrawControls()
        {
            if (IsLoad)
            {
                okButton = MainProcess.CreateButton("Ok", 10, 275, 220, 35, string.Empty, button_Click);

                valueType = accessory.GetProperyType(propertyName);

                if (valueType == typeof (string))
                {
                    createStringControls();
                }
                else if (valueType == typeof (int))
                {
                    createIntControls();
                }
                else if (valueType == typeof (DateTime))
                {
                    createDateTimeControls();
                }
                else if (valueType.IsEnum)
                {
                    createEnumControls(valueType);
                }
                else if (valueType == typeof (long))
                {
                    createLongControls();
                }
            }
        }

        public override void OnBarcode(string Barcode)
        {
        }

        public override void OnHotKey(KeyAction TypeOfAction)
        {
        }
        #endregion

        #region �������� ������ ��������� "��������� ��������"
        private void createStringControls()
        {
            MobileTextBox textBox = (MobileTextBox)MainProcess.CreateTextBox(10, 150, 220, string.Empty, ControlsStyle.LabelLarge);
            textBox.Focus();
            controls.Add(textBox);
        }

        private void createIntControls()
        {
            MobileTextBox textBox = (MobileTextBox)MainProcess.CreateTextBox(10, 150, 220, string.Empty, ControlsStyle.LabelLarge, false);
            textBox.Focus();
            controls.Add(textBox);
        }

        #region DateTime
        private void createDateTimeControls()
        {
            MobileTextBox day = (MobileTextBox)MainProcess.CreateTextBox(50, 150, 25, string.Empty, ControlsStyle.LabelLarge, false);
            MobileControl dot1 = MainProcess.CreateLabel(".", 80, 150, 5, MobileFontSize.Large, FontStyle.Bold);
            MobileControl month = MainProcess.CreateTextBox(90, 150, 25, string.Empty, ControlsStyle.LabelLarge, false);
            MobileControl dot2 = MainProcess.CreateLabel(".", 120, 150, 5, MobileFontSize.Large, FontStyle.Bold);
            MobileControl year = MainProcess.CreateTextBox(130, 150, 50, string.Empty, ControlsStyle.LabelLarge, false);

            day.Focus();

            //��������� ����� ���: MM-dd-yyyy, � ����������: dd-MM-yyyy
            controls.Add(month);
            controls.Add(dot1);
            controls.Add(day);
            controls.Add(dot2);
            controls.Add(year);

            TextBox dayBox = (TextBox)day.GetControl();
            TextBox monthBox = (TextBox)month.GetControl();
            TextBox yearBox = (TextBox)year.GetControl();

            //dayBox.MaxLength = 2;
            dayBox.TextChanged += ValueEditor_TextChanged;
            //monthBox.MaxLength = 2;
            monthBox.TextChanged += ValueEditor_TextChanged2;
            //yearBox.MaxLength = 2;
            yearBox.TextChanged += ValueEditor_TextChanged3;            
        }

        void ValueEditor_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox != null && textBox.Text.Length >= 2 && controls.Count == 5)
            {
                int days = Convert.ToInt32(textBox.Text);

                if (days < 32)
                {
                    textBox.Text = string.Format("{0:00}", days);
                    MobileTextBox newControl = (MobileTextBox) controls[0];
                    newControl.Focus();
                }
                else
                {
                    textBox.Text = textBox.Text.Substring(0, days >= 100 ? 2 : 1);
                    textBox.Select(1,1);
                }
            }
        }

        void ValueEditor_TextChanged2(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox != null && textBox.Text.Length >= 2 && controls.Count == 5)
            {
                int months = Convert.ToInt32(textBox.Text);

                if (months < 13)
                {
                    textBox.Text = string.Format("{0:00}", months);
                    MobileTextBox newControl = (MobileTextBox) controls[4];
                    newControl.Focus();
                }
                else
                {
                    textBox.Text = textBox.Text.Substring(0, months >=100 ? 2 : 1);
                    textBox.Select(1, 1);
                }
            }
        }

        void ValueEditor_TextChanged3(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox != null && textBox.Text.Length >= 4)
            {
                int years = Convert.ToInt32(textBox.Text);

                if (years < 10000)
                {
                    textBox.Text = string.Format("{0:0000}", years);
                    okButton.Focus();
                }
                else
                {
                    textBox.Text = textBox.Text.Substring(0, years >= 10000 ? 4 : 3);
                    textBox.Select(4, 1);
                }
            }
        } 
        #endregion

        private void createEnumControls(Type vType)
        {
            DataTable sourceTable = new DataTable();
            sourceTable.Columns.AddRange(new[]
                                                     {
                                                         new DataColumn("Number", typeof (int)),
                                                         new DataColumn("Description", typeof (string))
                                                     });

            MobileTable visualTable = MainProcess.CreateTable("Enum", 200, 65);
            visualTable.OnChangeSelectedRow += visualTable_OnChangeSelectedRow;
            visualTable.DT = sourceTable;
            visualTable.AddColumn("�", "Number", 34);
            visualTable.AddColumn("�����", "Description", 180);
            
            Dictionary<int, string> list = EnumWorker.GetList(vType);

            foreach (KeyValuePair<int, string> element in list)
            {
                visualTable.AddRow(element.Key, element.Value); 
            }

            visualTable.Focus();
            controls.Add(visualTable);
        }

        private void createLongControls()
        {
            dbFieldAtt attribute = Attribute.GetCustomAttribute(
                           accessory.GetType().GetProperty(propertyName),
                           typeof(dbFieldAtt)) as dbFieldAtt;

            if (attribute != null && attribute.dbObjectType != null)
            {
                DataTable sourceTable = new DataTable();
                sourceTable.Columns.AddRange(new[]
                                                     {
                                                         new DataColumn("Number", typeof (int)),
                                                         new DataColumn("Description", typeof (string))
                                                     });

                MobileTable visualTable = MainProcess.CreateTable("Table", 200, 65);
                visualTable.OnChangeSelectedRow += visualTable_OnChangeSelectedRow;
                visualTable.DT = sourceTable;
                visualTable.AddColumn("�", "Number", 34);
                visualTable.AddColumn("�����", "Description", 180);

                string command = string.Format("SELECT Id,Description FROM {0} WHERE MarkForDeleting=0",
                                               attribute.dbObjectType.Name);
                SqlCeCommand query = dbWorker.NewQuery(command);
                DataTable table = query.SelectToTable();

                foreach (DataRow row in table.Rows)
                {
                    visualTable.AddRow(row["Id"], row["Description"]);
                }

                visualTable.Focus();
                controls.Add(visualTable);
            }
            else
            {
                MainProcess.CreateLabel("����������/�������� ����!", 5, 150, 230,
                                        MobileFontSize.Normal, MobileFontPosition.Center, MobileFontColors.Warning,
                                        FontStyle.Bold);
                controls.Add(MainProcess.CreateTable("Table", 200, 65));
            }
        }

        void visualTable_OnChangeSelectedRow(object param1, OnChangeSelectedRowEventArgs param2)
        {
            selectedId = Convert.ToInt32(param2.SelectedRow[0]);
        } 
        #endregion

        private void button_Click()
        {
            string value = concatValue();
            bool isValid;
            accessory.SetValue(propertyName, value, out isValid);

            if (isValid)
            {
                MainProcess.ClearControls();
                MainProcess.Process = new EditBuilder(MainProcess, mainType, mainTopic, currentType, currentTopic,accessory, barcode);
            }
            else
            {
                ShowMessage("������� ������ �����!");
            }
        }

        /// <summary>����������� ��������� "��������� ��������" � ������ ��������</summary>
        /// <returns>����� �������� ��������</returns>
        private string concatValue()
        {
            StringBuilder valueBuilder = new StringBuilder();

            foreach (MobileControl control in controls)
            {
                MobileTextBox tb = control as MobileTextBox;
                if(tb!=null)
                {
                    valueBuilder.Append(tb.Text);
                    continue;
                }

                MobileLabel l = control as MobileLabel;
                if(l!=null)
                {
                    valueBuilder.Append(l.Text);
                    continue;
                }

                MobileTable t = control as MobileTable;
                if (t != null)
                {
                    valueBuilder.Append(selectedId);
                }
            }

            return valueBuilder.ToString();
        }
    }
}