using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WMS_client.Repositories;

namespace WMS_client
    {
    public class Configuration
        {

        private Configuration()
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

        }
    }
