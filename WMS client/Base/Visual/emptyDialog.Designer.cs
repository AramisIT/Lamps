namespace WMS_client.Base.Visual
{
    partial class emptyDialog
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
            this.closeFormTimer = new System.Windows.Forms.Timer();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // closeFormTimer
            // 
            this.closeFormTimer.Interval = 1000;
            this.closeFormTimer.Tick += new System.EventHandler(this.closeFormTimer_Tick);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Font = new System.Drawing.Font("Tahoma", 28F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.Navy;
            this.label1.Location = new System.Drawing.Point(0, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(237, 56);
            this.label1.Text = "ATOS WMS";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // emptyDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(238, 127);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "emptyDialog";
            this.Load += new System.EventHandler(this.emptyDialog_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer closeFormTimer;
        private System.Windows.Forms.Label label1;
    }
}