using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WMS_client.Repositories;
using WMS_client.Utils;

namespace WMS_client
    {
    public class Configuration
        {

        private Configuration()
            {
            readPDTid();
            checkIsReleaseMode();
            }

        public void InitLastBackUpTime()
            {
            try
                {
                var backUpCreator = new BackUpCreator();
                lastBackUpTime = backUpCreator.GetLastBackUpTime();
                }
            catch (Exception exp)
                {
                MessageBox.Show(string.Format("Ошибка получения даты последнего бекапа: {0}", exp.Message));
                }
            }

        private void checkIsReleaseMode()
            {
            ReleaseMode = false;
#if !DEBUG
            ReleaseMode = true;
#endif
            }

        private void readPDTid()
            {
            string settingsFileName = PathToApplication + @"\pdt_id.txt";

            string serverIdTxt = null;
            using (StreamReader idFile = File.OpenText(settingsFileName))
                {
                serverIdTxt = idFile.ReadLine();
                }

            try
                {
                TerminalId = Convert.ToInt32(serverIdTxt.Trim());
                }
            catch (Exception exp)
                {
                Debug.WriteLine(string.Format("Ошибка считывания Id терминала"));
                }
            }

        public int TerminalId { get; private set; }

        private static Configuration current;
        public static Configuration Current
            {
            get
                {
                return current ?? (current = new Configuration());
                }
            }

        public bool TimeToBackUp
            {
            get
                {

                return (lastBackUpTime.Equals(DateTime.MinValue)
                        || (((TimeSpan)(DateTime.Now - lastBackUpTime)).TotalMinutes > 60));
                }
            }

        public void FixBackUpTime()
            {
            lastBackUpTime = DateTime.Now;
            }

        private DateTime lastBackUpTime;

        public IRepository Repository { get; set; }

        public String PathToApplication
            {
            get
                {
                string path =
                    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)
                        .Replace("file:\\", string.Empty);

                return path;
                }
            }

        public bool ReleaseMode { get; private set; }
        }
    }
