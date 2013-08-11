using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TestSQL
    {
    internal static class Program
        {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        private static void Main()
            {
            Application.Run(new Form1());

            }


        }
    }