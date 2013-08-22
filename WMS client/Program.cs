using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using WMS_client.Repositories;
using WMS_client.Utils;
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
            if (BatteryChargeStatus.Low)
                {
                MessageBox.Show("Акумулятор розряджений. Необхідно зарядити термінал!");
                return;
                }

            if (isExistedSameProcess())
                {
                return;
                }

            Configuration.Current.Repository = new SqlCeRepository();
            Configuration.Current.InitLastBackUpTime();

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

public class MyDictTest
    {
    public MyDictTest()
        {
        TestDict();
        }

    Dictionary<int, info> dict = new Dictionary<int, info>();

    void TestDict()
        {
        for (int i = 0; i < 10000000; i++)
            {
            dict.Add(i, new info());
            }

        Trace.WriteLine(dict.Count);
        }
    }

public class info
    {
    public int Map;
    public int Position;
    public int Register;
    public int Unit;
    public int Lamp;
    }