using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using WMS_client.Repositories;

namespace WMS_client.Utils
    {
    class BackUpCreator
        {
        private string fileName;
        private const string BACKUP_DIRECTORY_NAME = "backups";
        private readonly string fullBackupDirectoryPath = string.Format(@"{0}\{1}", Configuration.Current.PathToApplication, BACKUP_DIRECTORY_NAME);
        private readonly string sourceFileName = string.Format(@"{0}\{1}", Configuration.Current.PathToApplication, SqlCeRepository.DATABASE_FILE_NAME);

        public BackUpCreator()
            {
            fileName = string.Format(@"{0}\{1}.sdf", fullBackupDirectoryPath, DateTime.Now.ToString(DATETIME_FORMAT));
            }

        public bool CreateBackUp()
            {
            if (!checkBackupDirectory())
                {
                return false;
                }

            if (!copyFile())
                {
                return false;
                }

            deleteOldFiles();

            return true;
            }

        private void deleteOldFiles()
            {
            string[] files = Directory.GetFiles(fullBackupDirectoryPath);
            foreach (var fileName in files)
                {
                if (fileName.EndsWith(DATABASE_EXTENTION))
                    {
                    deleteIfOld(fileName);
                    }
                }
            }

        private const string DATABASE_EXTENTION = ".sdf";
        private const string DATETIME_FORMAT = "yy-MM-dd hh_mm_ss";

        private DateTime getBackupDateTime(string fileName)
            {
            string dateTime = Path.GetFileNameWithoutExtension(fileName);
            DateTime dateOfFile = DateTime.Now;

            try
                {
                dateOfFile = DateTime.ParseExact(dateTime, DATETIME_FORMAT, null);
                }
            catch (Exception exp)
                {
                Trace.WriteLine(string.Format("Ошибка при получении даты бекапа: ", exp.Message));
                return DateTime.MinValue;
                }

            return dateOfFile;
            }

        private void deleteIfOld(string fileName)
            {
            var dateOfFile = getBackupDateTime(fileName);
            if (dateOfFile.Equals(DateTime.MinValue))
                {
                return;
                }

            int minutesElapsed = (int)((TimeSpan)(DateTime.Now - dateOfFile)).TotalMinutes;
            if (minutesElapsed > 60 * 5)
                {
                try
                    {
                    File.Delete(fileName);
                    }
                catch (Exception exp)
                    {
                    Trace.WriteLine(string.Format("Ошибка при удалении старого файла: ", exp.Message));
                    return;
                    }
                }
            }

        private bool copyFile()
            {
            bool errorOccured = false;
            try
                {
                using (Stream destin = File.OpenWrite(fileName))
                    {
                    using (Stream source = File.OpenRead(sourceFileName))
                        {
                        byte[] buffer = new byte[1024];
                        int bytesRead = source.Read(buffer, 0, buffer.Length);

                        while (!errorOccured && bytesRead > 0)
                            {
                            destin.Write(buffer, 0, bytesRead);

                            try
                                {
                                bytesRead = source.Read(buffer, 0, buffer.Length);
                                }
                            catch (Exception exp)
                                {
                                MessageBox.Show(string.Format("Ошибка чтения файла: {0}", exp.Message));
                                Trace.WriteLine(exp.Message);
                                errorOccured = true;
                                bytesRead = 0;
                                }
                            }
                        }

                    destin.Close();
                    }

                if (errorOccured)
                    {
                    undoOperation();
                    return false;
                    }
                }
            catch (Exception copyExp)
                {
                MessageBox.Show(string.Format("Ошибка копирования файла: {0}", copyExp.Message));
                return false;
                }

            return true;
            }

        private void undoOperation()
            {
            try
                {
                File.Delete(fileName);
                }
            catch
                {
                }
            }

        private bool checkBackupDirectory()
            {
            if (!System.IO.Directory.Exists(fullBackupDirectoryPath))
                {
                try
                    {
                    System.IO.Directory.CreateDirectory(fullBackupDirectoryPath);
                    }
                catch (Exception exp)
                    {
                    Trace.WriteLine(string.Format("Ошибка создания директории бекапа: {0}", exp.Message));
                    }
                }

            return true;
            }

        internal DateTime GetLastBackUpTime()
            {
            if (!checkBackupDirectory())
                {
                return DateTime.MinValue;
                }

            DateTime maxDateTime = DateTime.MinValue;

            string[] files = Directory.GetFiles(fullBackupDirectoryPath);
            foreach (var fileName in files)
                {
                if (fileName.EndsWith(DATABASE_EXTENTION))
                    {
                    var dateOfFile = getBackupDateTime(fileName);
                    if (dateOfFile.Equals(DateTime.MinValue))
                        {
                        continue;
                        }

                    bool fileEmpty = (new FileInfo(fileName)).Length == 0;
                    if (fileEmpty)
                        {
                        File.Delete(fileName);
                        continue;
                        }

                    if (maxDateTime < dateOfFile)
                        {
                        maxDateTime = dateOfFile;
                        }
                    }
                }

            return maxDateTime;
            }
        }
    }
