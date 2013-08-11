using System;
using System.Collections.Generic;
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

            }

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
