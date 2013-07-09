using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace WMS_client.Processes.Lamps.Sync
    {
    public interface IServerIdProvider
        {
        int ServerId { get; }
        }

    public class ServerIdProvider : IServerIdProvider
        {

        private  int serverId = 0;

        public ServerIdProvider()
            {
            this.setServerId();
            }

        private void setServerId()
            {
            string SettingsFileName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + @"\pdt_id.txt";
            SettingsFileName = SettingsFileName.Replace("file:\\", string.Empty);

            if (!File.Exists(SettingsFileName))
                {
                MessageBox.Show(string.Format("Не найден файл настроек [{0}]\r\n\r\nПриложение будет закрыто!", SettingsFileName));
                Application.Exit();
                return;
                }

            StreamReader SettingsFile = File.OpenText(SettingsFileName);

            string serverIdTxt = string.Empty ;
            while ((serverIdTxt = SettingsFile.ReadLine()) != null)
                {
                if (serverIdTxt.Trim() != string.Empty)
                    {
                    break;
                    }
                }
            SettingsFile.Close();

            serverId = Convert.ToInt32(serverIdTxt);
            }

        public int ServerId
            {
            get
                {
                return serverId;
                }
            }
        }
    }
