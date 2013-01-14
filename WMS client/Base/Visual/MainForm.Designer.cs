namespace WMS_client
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.Command = new System.Windows.Forms.Label();
            this.CellName = new System.Windows.Forms.Label();
            this.BarcodeLabel = new System.Windows.Forms.Label();
            this.BarcodeTextBox = new System.Windows.Forms.TextBox();
            this.LogoOffLine = new System.Windows.Forms.PictureBox();
            this.LogoOnLine = new System.Windows.Forms.PictureBox();
            this.SuspendLayout();
            // 
            // Command
            // 
            resources.ApplyResources(this.Command, "Command");
            this.Command.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.Command.Name = "Command";
            // 
            // CellName
            // 
            resources.ApplyResources(this.CellName, "CellName");
            this.CellName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.CellName.Name = "CellName";
            // 
            // BarcodeLabel
            // 
            resources.ApplyResources(this.BarcodeLabel, "BarcodeLabel");
            this.BarcodeLabel.Name = "BarcodeLabel";
            // 
            // BarcodeTextBox
            // 
            this.BarcodeTextBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            resources.ApplyResources(this.BarcodeTextBox, "BarcodeTextBox");
            this.BarcodeTextBox.Name = "BarcodeTextBox";
            this.BarcodeTextBox.TabStop = false;
            this.BarcodeTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.BarcodeTextBox_KeyPress);
            this.BarcodeTextBox.LostFocus += new System.EventHandler(this.BarcodeTextBox_LostFocus);
            // 
            // LogoOffLine
            // 
            this.LogoOffLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(236)))), ((int)(((byte)(242)))));
            resources.ApplyResources(this.LogoOffLine, "LogoOffLine");
            this.LogoOffLine.Name = "LogoOffLine";
            this.LogoOffLine.Click += new System.EventHandler(this.pictureBox1_DoubleClick);
            // 
            // LogoOnLine
            // 
            this.LogoOnLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(236)))), ((int)(((byte)(242)))));
            resources.ApplyResources(this.LogoOnLine, "LogoOnLine");
            this.LogoOnLine.Name = "LogoOnLine";
            this.LogoOnLine.Click += new System.EventHandler(this.pictureBox1_DoubleClick);
            this.LogoOnLine.Paint += new System.Windows.Forms.PaintEventHandler(this.LogoOnLine_Paint);
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(236)))), ((int)(((byte)(242)))));
            resources.ApplyResources(this, "$this");
            this.ControlBox = false;
            this.Controls.Add(this.LogoOnLine);
            this.Controls.Add(this.LogoOffLine);
            this.Controls.Add(this.BarcodeLabel);
            this.Controls.Add(this.BarcodeTextBox);
            this.Controls.Add(this.CellName);
            this.Controls.Add(this.Command);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Closed += new System.EventHandler(this.Form1_Closed);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Label Command;
        private System.Windows.Forms.Label CellName;
        private System.Windows.Forms.Label BarcodeLabel;
        private System.Windows.Forms.PictureBox LogoOffLine;
        private System.Windows.Forms.PictureBox LogoOnLine;
        private System.Windows.Forms.TextBox BarcodeTextBox;
    }
}

