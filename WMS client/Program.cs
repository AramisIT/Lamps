using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using AramisPDTClient;
using WMS_client.Repositories;
using WMS_client.Utils;
using WMS_client.WinProcessesManagement;
using System.Reflection;

namespace WMS_client
    {
    static class Program
        {
        

        [MTAThread]
        static void Main()
            {
            if (new SystemInfo().IsExistedSameProcess()) return;

            var releaseMode = false;
#if !DEBUG
            releaseMode = true;
#endif
            new SystemInfo().SetReleaseMode(releaseMode);

            if (BatteryChargeStatus.Low)
                {
                MessageBox.Show("Акумулятор розряджений. Необхідно зарядити термінал!");
                return;
                }

            Configuration.Current.Repository = new SqlCeRepository();
            Configuration.Current.InitLastBackUpTime();

            BusinessProcess.OnProcessCreated += new Action<BusinessProcess>(BusinessProcess_OnProcessCreated);

            MainForm mform = new MainForm(typeof(StartProcess));
            //   mform.MinimizeBox = true;
            //   mform.WindowState = FormWindowState.Normal;
            Application.Run(mform);
            }

        static void BusinessProcess_OnProcessCreated(BusinessProcess process)
            {
            if (Configuration.Current.TimeToBackUp)
                {
                bool lowPower;
                if (Configuration.Current.Repository.IsIntactDatabase(out lowPower))
                    {
                    if (!lowPower)
                        {
                        var backUpCreator = new BackUpCreator();
                        if (backUpCreator.CreateBackUp())
                            {
                            "Создана копия базы!".ShowMessage();
                            Configuration.Current.FixBackUpTime();
                            }
                        }
                    }
                else
                    {
                    "База даних пошкоджена. Необхідно звернутись до адміністратора.".ShowMessage();
                    process.TerminateApplication();
                    }
                }
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