using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Windows.Forms;

namespace WMS_client
    {
    public class FormDesignProcess : BusinessProcess
        {

        private bool MoveObjects = true;
        private bool MoveObjectsState
            {
            get { return MoveObjects; }
            set
                {
                MoveObjects = value;
                RefreshStatus();
                }
            }

        private System.Drawing.Color ControlColor;
        private Control CurrentControl;

        private void RefreshStatus()
            {
            MainProcess.ToDoCommand = String.Format("Design: {0}", MoveObjects ? "Move" : "Resize");
            }

        private void DrawBorderForSelectObject(object sender, PaintEventArgs e)
            {
            System.Drawing.Graphics gr = e.Graphics;
            gr.DrawRectangle(new System.Drawing.Pen(System.Drawing.Color.Red, 3), (sender as Control).ClientRectangle);
            }

        public FormDesignProcess(WMSClient MainProcess)
            : base(MainProcess, 1)
            {
            BusinessProcessType = ProcessType.FormDesign;
            }

        public override void DrawControls()
            {
            MainProcess.ClearControls();
            RefreshStatus();
            }

        public override void Start()
            {
            base.Start();

            }


        public override void OnBarcode(string Barcode)
            {
            if (CurrentControl is TextBox)
                {
                (CurrentControl as TextBox).Text = Barcode;
                }
            if (CurrentControl is Label)
                {
                (CurrentControl as Label).Text = Barcode;
                }
            }

        public override void OnHotKey(KeyAction TypeOfAction)
            {
            switch (TypeOfAction)
                {
                case KeyAction.F9:

                    if (CurrentControl != null)
                        {
                        ShowMessage(string.Format("Left = {0}\r\nTop = {1}\r\nWidth = {2}\r\nHeight = {3}", CurrentControl.Left, CurrentControl.Top, CurrentControl.Width, CurrentControl.Height));
                        }
                    break;

                case KeyAction.F8:

                    if (CurrentControl != null)
                        {
                        Control cont = CurrentControl;
                        MainProcess.SelectNextControl(ref CurrentControl);
                        CurrentControl.Paint += new System.Windows.Forms.PaintEventHandler(DrawBorderForSelectObject);
                        ControlColor = CurrentControl.BackColor;
                        CurrentControl.BackColor = System.Drawing.Color.Red;
                        CurrentControl.Refresh();

                        MainProcess.RemoveControl(cont);


                        }
                    break;

                case KeyAction.F10:

                    MainProcess.CreateLabel("label", 5, 70, 100, ControlsStyle.LabelNormal);
                    if (CurrentControl == null)
                        {
                        MainProcess.SelectNextControl(ref CurrentControl);
                        CurrentControl.Paint += new System.Windows.Forms.PaintEventHandler(DrawBorderForSelectObject);
                        ControlColor = CurrentControl.BackColor;
                        CurrentControl.BackColor = System.Drawing.Color.Red;
                        CurrentControl.Refresh();
                        }
                    break;

                case KeyAction.Complate:

                    MainProcess.CreateTextBox(5, 100, 150, "ñreateTextBox", ControlsStyle.TextBoxNormal);
                    if (CurrentControl == null)
                        {
                        MainProcess.SelectNextControl(ref CurrentControl);
                        CurrentControl.Paint += new System.Windows.Forms.PaintEventHandler(DrawBorderForSelectObject);
                        ControlColor = CurrentControl.BackColor;
                        CurrentControl.BackColor = System.Drawing.Color.Red;
                        CurrentControl.Refresh();
                        }
                    break;

                case KeyAction.F12:

                    var t = MainProcess.CreateTable("WareList", 30);
                    if (CurrentControl == null)
                        {
                        MainProcess.SelectNextControl(ref CurrentControl);
                        CurrentControl.Paint += new System.Windows.Forms.PaintEventHandler(DrawBorderForSelectObject);
                        ControlColor = CurrentControl.BackColor;
                        CurrentControl.BackColor = System.Drawing.Color.Red;
                        CurrentControl.Refresh();
                        }
                    t.Show();
                    break;

                case KeyAction.F5:

                    if (CurrentControl != null)
                        {
                        CurrentControl.BackColor = ControlColor;
                        CurrentControl.Paint -= new System.Windows.Forms.PaintEventHandler(DrawBorderForSelectObject);
                        }

                    MainProcess.SelectNextControl(ref CurrentControl);
                    CurrentControl.Paint += new System.Windows.Forms.PaintEventHandler(DrawBorderForSelectObject);
                    ControlColor = CurrentControl.BackColor;
                    CurrentControl.BackColor = System.Drawing.Color.Red;
                    CurrentControl.Refresh();
                    break;

                case KeyAction.Recount:

                    MoveObjectsState = !MoveObjectsState;
                    break;

                case KeyAction.UpKey:

                    if (CurrentControl == null) break;

                    if (MoveObjectsState)
                        {
                        CurrentControl.Top--;
                        }
                    else
                        {
                        CurrentControl.Height--;
                        }
                    break;

                case KeyAction.DownKey:

                    if (CurrentControl == null) break;
                    if (MoveObjectsState)
                        {
                        CurrentControl.Top++;
                        }
                    else
                        {
                        CurrentControl.Height++;
                        }
                    break;

                case KeyAction.LeftKey:

                    if (CurrentControl == null) break;

                    if (MoveObjectsState)
                        {
                        CurrentControl.Left--;
                        }
                    else
                        {
                        CurrentControl.Width--;
                        }
                    break;

                case KeyAction.RightKey:

                    if (CurrentControl == null) break;
                    if (MoveObjectsState)
                        {
                        CurrentControl.Left++;
                        }
                    else
                        {
                        CurrentControl.Width++;
                        }
                    break;


                }
            }
        }
    }
