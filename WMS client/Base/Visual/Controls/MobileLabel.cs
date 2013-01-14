using System.Windows.Forms;
using System.Drawing;

namespace WMS_client
{
    public class MobileLabel : MobileControl
    {
        #region Private fields
        private readonly Label Control = new Label();
        private readonly int ExactHeight;

        public string Text
        {
            get { return Control.Text; }
            set { Control.Text = value; }
        }
        public Label Label { get { return Control; } }
        #endregion

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

        #region Public methods
        public MobileLabel(MainForm Form, string text, int left, int top, int width, int exactHeight, MobileFontSize size, MobileFontPosition position, MobileFontColors color, FontStyle fontStyle, string ControlName)
        {
            ExactHeight = exactHeight;

            Control.Left = left;
            Control.Top = top;
            Control.Text = text;

            if (width > 0)
            {
                Control.Width = width;
            }

            Control.Name = ControlName;
            SetControlStyle(size, position, color, fontStyle);
            Form.Controls.Add(Control);
        }
        
        public MobileLabel(MainForm Form, string text, int left, int top, int width, int exactHeight, ControlsStyle style, string ControlName)
        {
            ExactHeight = exactHeight;

            Control.Left = left;
            Control.Top = top;
            Control.Text = text;
            
            if (width > 0)
            {
                Control.Width = width;
            }

            Control.Name = ControlName;
            SetControlsStyle(style);
            Form.Controls.Add(Control);
        }

        public void SetControlStyle(MobileFontSize size, MobileFontPosition position, MobileFontColors color, FontStyle fontStyle)
        {
            SetFontSize(size, fontStyle);
            SetFontPosition(position);
            SetFontColor(color);
        }

        public void SetFontSize(MobileFontSize size, FontStyle fontStyle)
        {
            int fontSize;

            switch (size)
            {
                case MobileFontSize.Normal:
                    Control.Height = ExactHeight == 0 ? 20 : ExactHeight;
                    fontSize = 12;
                    break;
                case MobileFontSize.Little:
                    Control.Height = ExactHeight == 0 ? 15 : ExactHeight;
                    fontSize = 8;
                    break;
                case MobileFontSize.Large:
                    Control.Height = ExactHeight == 0 ? 30 : ExactHeight;
                    fontSize = 16;
                    break;
                case MobileFontSize.Multiline:
                    Control.Height = ExactHeight == 0 ? 75 : ExactHeight;
                    fontSize = 12;
                    break;
                default:
                    fontSize = 12;
                    break;
            }
            Control.Font = new Font("Arial", fontSize, fontStyle);
        }

        public void SetFontPosition(MobileFontPosition position)
        {
            switch (position)
            {
                case MobileFontPosition.Left:
                    Control.TextAlign = ContentAlignment.TopLeft;
                    break;
                case MobileFontPosition.Center:
                    Control.TextAlign = ContentAlignment.TopCenter;
                    break;
                case MobileFontPosition.Right:
                    Control.TextAlign = ContentAlignment.TopRight;
                    break;
            }
        }

        public void SetFontColor(MobileFontColors color)
        {
            switch (color)
            {
                case MobileFontColors.Default:
                    Control.ForeColor = Color.Black;
                    break;
                case MobileFontColors.Info:
                    Control.ForeColor = Color.DarkGreen;
                    break;
                case MobileFontColors.Empty:
                    Control.ForeColor = Color.White;
                    break;
                case MobileFontColors.Disable:
                    Control.ForeColor = Color.LightSlateGray;
                    break;
            }
        }

        public void SetControlsStyle(ControlsStyle style)
        {
            switch (style)
            {
                case ControlsStyle.LabelNormal:
                    {
                        Control.Height = ExactHeight == 0 ? 25 : ExactHeight;
                        Control.Font = new Font("Arial", 12, FontStyle.Bold);
                        Control.ForeColor = Color.FromArgb(0, 0, 192);
                        break;
                    }
                case ControlsStyle.LabelRedRightAllign:
                    {
                        Control.Height = ExactHeight == 0 ? 25 : ExactHeight;
                        Control.Font = new Font("Arial", 12, FontStyle.Bold);
                        Control.TextAlign = ContentAlignment.TopRight;
                        Control.ForeColor = Color.FromArgb(192, 0, 0);
                        break;
                    }
                case ControlsStyle.LabelLarge:
                    {
                        Control.Height = ExactHeight == 0 ? 28 : ExactHeight;
                        Control.Font = new Font("Arial", 16, FontStyle.Regular);
                        Control.ForeColor = Color.FromArgb(0, 0, 192);
                        break;
                    }
                case ControlsStyle.LabelSmall:
                    {
                        //Control.Height = 15;
                        Control.Font = new Font("Arial", 8, FontStyle.Regular);
                        Control.ForeColor = Color.FromArgb(0, 0, 192);
                        break;
                    }
                case ControlsStyle.LabelH2:
                    {
                        Control.TextAlign = ContentAlignment.TopCenter;
                        Control.Font = new Font("Arial", 10, FontStyle.Bold);
                        Control.ForeColor = Color.FromArgb(0, 0, 192);
                        break;
                    }
                case ControlsStyle.LabelH2Red:
                    {
                        Control.TextAlign = ContentAlignment.TopCenter;
                        Control.Font = new Font("Arial", 10, FontStyle.Bold);
                        Control.ForeColor = Color.FromArgb(212, 0, 56);
                        break;
                    }
                case ControlsStyle.LabelMultiline:
                    {
                        Control.TextAlign = ContentAlignment.TopCenter;
                        Control.Font = new Font("Arial", 16, FontStyle.Regular);
                        Control.ForeColor = Color.FromArgb(192, 0, 0);
                        Control.Height = ExactHeight == 0 ? 75 : ExactHeight;
                        break;
                    }
                case ControlsStyle.LabelMultilineSmall:
                    {
                        Control.TextAlign = ContentAlignment.TopCenter;
                        Control.Font = new Font("Arial", 12, FontStyle.Regular);
                        Control.ForeColor = Color.FromArgb(192, 0, 0);
                        Control.Height = ExactHeight == 0 ? 75 : ExactHeight;
                        break;
                    }

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