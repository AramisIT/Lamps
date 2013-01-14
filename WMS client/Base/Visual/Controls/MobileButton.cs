using System.Windows.Forms;

namespace WMS_client
{
    public delegate void MobileSenderClick(object sender);
    public delegate void MobileButtonClick();

    public class MobileButton : MobileControl
    {
        #region Private fields
        private readonly MobileButtonClick MobileButtonClick;
        private readonly MobileSenderClick MobileSenderClick;
        private readonly Button Control = new Button();
        public string Text
        {
            get { return Control.Text; }
            set { Control.Text = value; }}
        public bool Enabled
        {
            get { return Control.Enabled; }
            set { Control.Enabled = value; }
        }
        public object Tag
        {
            get { return Control.Tag; }
            set { Control.Tag = value; }
        }
        #endregion

        public MobileButton(MainForm Form, string text, int left, int top, int width, int height, string ControlName, MobileButtonClick mobileButtonClick, MobileSenderClick mobileSenderClick, object tag, bool enabled)
        {
            Control.Left = left;
            Control.Top = top;
            Control.Text = text;
            Enabled = enabled;
            Tag = tag;
            MobileButtonClick = mobileButtonClick;
            MobileSenderClick = mobileSenderClick;
            Control.Click+=Control_Click;

            if (width > 0)
            {
                Control.Width = width;
            }

            if (height > 0)
            {
                Control.Height = height;
            }

            Control.Name = ControlName;
            Form.Controls.Add(Control);
        }

        void  Control_Click(object sender, System.EventArgs e)
        {
            if (MobileButtonClick!=null)
            {
                MobileButtonClick();
            }

            if (MobileSenderClick!=null)
            {
                MobileSenderClick(sender);
            }
        }

        #region Override methods
        public void Focus()
        {
            Control.Focus();
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

    }
}

