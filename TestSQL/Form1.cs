using System;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TestSQL
    {
    public partial class Form1 : Form
        {

        public static void FileToByteArray(string fileName)
            {
            //byte[] buff = null;
            //FileStream fs = new FileStream(fileName,
            //                               FileMode.Open,
            //                               FileAccess.Read);
            //BinaryReader br = new BinaryReader(fs);
            //long numBytes = new FileInfo(fileName).Length;
            //buff = br.ReadBytes((int)numBytes);
            //return buff;

            long totalSize = 0;
            var fi = new FileInfo(fileName);
            long fSize = fi.Length;
            Trace.WriteLine(string.Format("File size: {0}", fSize));
            string newFileName = string.Format("{0}\\Copy_{1}", Path.GetDirectoryName(fileName), Path.GetFileName(fileName));
            using (Stream destin = File.OpenWrite(newFileName))
                {

                using (Stream source = File.OpenRead(fileName))
                    {
                    byte[] buffer = new byte[512];
                    int lastValue = 0;
                    int bytesRead = source.Read(buffer, 0, buffer.Length);
                    bool stop = false;
                    while (bytesRead > 0)
                        {
                        totalSize += bytesRead;
                        // dest.Write(buffer, 0, bytesRead);
                        Trace.WriteLine(bytesRead);
                        destin.Write(buffer, 0, bytesRead);

                        try
                            {
                            lastValue = bytesRead;
                            bytesRead = source.Read(buffer, 0, buffer.Length);
                            }
                        catch (Exception exp)
                            {
                            Trace.WriteLine(exp.Message);
                            stop = true;
                            bytesRead = 0;
                            }
                        }
                    }

                destin.Close();
                }

            Trace.WriteLine(string.Format("Total size: {0}", totalSize));
            }


        public Form1()
            {
            InitializeComponent();
            }

        private void button1_Click(object sender, EventArgs e)
            {
            //OpenFileDialog fd = new OpenFileDialog();
            //fd.Filter = "*.sdf|*.sdf";

            //if (fd.ShowDialog() == DialogResult.OK)
            //    {
            //    //Application.Run(new Form1());
            //    FileToByteArray(fd.FileName);
            //    }
            //return;

            var t = new TestSqlClass(null);
            t.Test();
            Trace.WriteLine(t.TimeMiliSec);
            }


        }


    }