using System;
using System.Windows.Forms;
using System.Drawing;

namespace WMS_client
{
    public class MobileTextBox : MobileControl
    {
        #region Public fields

        public string Text
        {
            get { return Control.Text; }
            set { Control.Text = value; }
        }

        #endregion

        #region Private fields

        private readonly TextBox Control = new TextBox();
        public OnEventHandlingDelegate OnChanged;

        #endregion

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

        #region Public methods

        public MobileTextBox(MainForm Form, int left, int top, int width, string controlName, ControlsStyle style, OnEventHandlingDelegate ProcTarget, bool isPasswordField, bool isTextField)
        {
            Control.ForeColor = Color.DarkGreen;
            Control.Left = left;
            Control.Top = top;
            Control.BorderStyle = BorderStyle.None;
            Control.Width = width;
            Control.Name = controlName;
            Control.Height = 25;
            Control.Font = new Font("Arial", 12, FontStyle.Bold);
            if (isPasswordField)
            {
                Control.PasswordChar = '*';
            }
            if (ProcTarget != null)
            {
                OnChanged = ProcTarget;
                Control.Validated += new EventHandler(ProcTarget);
            }
            if (isTextField)
            {
                Control.KeyPress += (OnKeyPressedCheckingText);
            }
            else
            {
                Control.KeyPress += (OnKeyPressedCheckingNumber);
            }

            Form.Controls.Add(Control);
        }

        public void Focus()
        {
            Control.Focus();
        }
        #endregion

        #region Private methods

        private void OnKeyPressedCheckingNumber(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' && OnChanged != null)
            {
                OnChanged(sender, null);
            }

            if ((e.KeyChar != '\b') && ((!Char.IsDigit(e.KeyChar) && e.KeyChar != '.')  || !IsNumber(((TextBox) sender).Text+e.KeyChar+"0")))
            {
                e.Handled = true;
            }
        }

        public static bool IsNumber(string str)
        {
            try
            {
                Convert.ToDouble(str);
                return true;
            }
            catch
            {
                return false;
            }            
        }
        
        private void OnKeyPressedCheckingText(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'x' | e.KeyChar == 'X')
            {
                e.Handled = true;
            }
        }

        #endregion

        #region Override methods

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
    }


}
