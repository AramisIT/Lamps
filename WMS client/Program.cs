using System;
using System.Windows.Forms;

namespace WMS_client
{
    static class Program
    {
        [MTAThread]
        static void Main()
        {
            Application.Run(new MainForm());
        }
    }
}