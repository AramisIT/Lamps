using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WMS_client.Base.Visual
{
    public partial class EmptyDialog : Form
    {
        public EmptyDialog()
        {
            InitializeComponent();
        }

        private void closeFormTimer_Tick(object sender, EventArgs e)
        {
            Close();
        }

        private void emptyDialog_Load(object sender, EventArgs e)
        {
            closeFormTimer.Enabled = true;
        }
    }
}