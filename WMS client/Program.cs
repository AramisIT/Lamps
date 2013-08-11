using System;
using System.Windows.Forms;
using System.Diagnostics;
using WMS_client.Repositories;
using WMS_client.WinProcessesManagement;
using System.Reflection;

namespace WMS_client
    {
    static class Program
        {
        private const string STARTUP_PATH = @"\Program files\WMS_client\WMS client.exe";
        [MTAThread]
        static void Main()
        {
            if (isExistedSameProcess())
                {
                return;
                }

            Configuration.Current.Repository = new SqlCeRepository();

            MainForm mform = new MainForm();
         //   mform.MinimizeBox = true;
         //   mform.WindowState = FormWindowState.Normal;
            Application.Run(mform);
            }

        private static bool isExistedSameProcess()
            {
            int currentPId = Process.GetCurrentProcess().Id;
            ProcessInfo[] processes = ProcessCE.GetProcesses();
            foreach (ProcessInfo pInfo in processes)
                {
                if (pInfo.Pid.ToInt32() == currentPId)
                    {
                    continue;
                    }
                if (pInfo.FullPath.Equals(STARTUP_PATH))
                    {
                    return true;
                    }
                }
            return false;
            }
        }
    }