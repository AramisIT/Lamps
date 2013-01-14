using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using StorekeeperManagementServer;
using System.Runtime.InteropServices;
using System.IO;

namespace WMS_client
{
    public delegate void SetConnectionStatusDelegate(bool IsOnline);
    public delegate void FVoid1StringDelegate(string str1);

    public class ServerAgent
    {
        #region Public fields
        
        public PackageViaWireless Package;
        public string WaitingPackageID = "";
        public bool RequestReady = false;
        public bool OnLine
        {
            get { return ConnectionEstablished; }
        }

        #endregion

        #region Private fields
        // #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   # 
        private TcpClient TCPClient;
        private NetworkStream TCPStream;
        private bool ConnectionEstablished = false;
        private string IPAddress;
        private int PortNumber;
        private WMSClient Client;       
        private bool PingSent = false;
        private int SendKeyCode;
        private string SendBarcode;
        private DataDrawing DataRepresent;


        public bool Executed = false;

        #endregion

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

        #region Public methods

        public ServerAgent(string IPAddress, int PortNumber, WMSClient Client, SetConnectionStatusDelegate DrawConnectionStatus, FVoid1StringDelegate ShowPingResult)
        {
            this.Client = Client;
            this.IPAddress = IPAddress;
            this.PortNumber = PortNumber;
            WriteToFile("Start", true);
            DataRepresent = new DataDrawing(DrawConnectionStatus, ShowPingResult);
        }

        public void Start()
        {
            while (true)
            {
                SetConnectionStatus(false);
                while (!Connect()) ;
                ReadPackages();
            }
        }

        public bool SendPackage(Byte[] Package)
        {
            lock (this)
            {
                #region Sending first time
                bool repeat = false;
                try
                {
                    TCPStream.Write(Package, 0, Package.Length);
                    WriteToFile(" >> Write [" + Encoding.GetEncoding(1251).GetString(Package, 0, Package.Length) + "]", false);
                    return true;
                }
                catch
                {
                    repeat = Connect();
                }
                #endregion

                #region Sending repeat if error occurred
                if (repeat)
                {
                    try
                    {
                        TCPStream.Write(Package, 0, Package.Length);
                        WriteToFile(" >> Write [" + Encoding.GetEncoding(1251).GetString(Package, 0, Package.Length) + "]", false);
                        return true;
                    }
                    catch
                    {
                        WriteToFile(" Writing error [" + Encoding.GetEncoding(1251).GetString(Package, 0, Package.Length) + "]", false);
                        //Console.WriteLine("Can't send data: " + exp.Message);
                    }
                }
                #endregion
                return false;
            }
        }

        public void CloseAll()
        {

            try { TCPStream.Close(); }
            catch { }

            try { TCPClient.Close(); }
            catch { }

        }

        #endregion

        #region Private methods
        // #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   #   


        #region Установка времени на устройстве

        [DllImport("coredll.dll")]
        private extern static void GetSystemTime(ref SYSTEMTIME lpSystemTime);

        [DllImport("coredll.dll")]
        private extern static uint SetSystemTime(ref SYSTEMTIME lpSystemTime);

        private struct SYSTEMTIME
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
        }

        private SYSTEMTIME GetTime()
        {
            // Call the native GetSystemTime method
            // with the defined structure.
            SYSTEMTIME stime = new SYSTEMTIME();
            GetSystemTime(ref stime);

            return stime;

            //// Show the current time.           
            //MessageBox.Show("Current Time: " +
            //    stime.wHour.ToString() + ":"
            //    + stime.wMinute.ToString());
        }

        private void SetTime(string StringTime)
        {
            System.Globalization.CultureInfo CultureInfo = new System.Globalization.CultureInfo("ru-ru");
            DateTime TimeOnServer = DateTime.ParseExact(StringTime, "dd.MM.yyyy HH:mm:ss", CultureInfo);
            SYSTEMTIME systime = new SYSTEMTIME();


            systime.wHour = (ushort)(TimeOnServer.Hour - 2);
            systime.wMinute = (ushort)TimeOnServer.Minute;
            systime.wSecond = (ushort)TimeOnServer.Second;
            systime.wMilliseconds = (ushort)TimeOnServer.Millisecond;

            systime.wYear = (ushort)TimeOnServer.Year;
            systime.wMonth = (ushort)TimeOnServer.Month;
            systime.wDay = (ushort)TimeOnServer.Day;

            SetSystemTime(ref systime);
            ushort NewHour = (ushort)DateTime.Now.Hour;

            if (NewHour - systime.wHour != 2) // +2 GMT: Kiev 
            {
                int hour = systime.wHour - (NewHour - systime.wHour - 2);
                ushort realHour = (ushort)(hour % 24);
                systime.wHour = realHour;

                if (hour > realHour)
                {
                    systime.wDay++;
                }
                else if (hour < realHour)
                {
                    systime.wDay--;
                }


                SetSystemTime(ref systime);
            }
        }


        #endregion

        private bool Connect()
        {
           
            try
            {
                TCPClient = new TcpClient(IPAddress, PortNumber);
            }
            catch(Exception exc)
            {
                Console.WriteLine("Can't connect: " + exc.Message);
                return false;
            }

            String ConnResult = "";
                 
            try
            {
                TCPStream = TCPClient.GetStream();
                byte[] StreamTest = new byte[10];

                IAsyncResult NetStreamReadRes = TCPStream.BeginRead(StreamTest, 0, StreamTest.Length, null, null);

                if (NetStreamReadRes.AsyncWaitHandle.WaitOne(1500, false))
                {

                    int streamLength = TCPStream.EndRead(NetStreamReadRes);
                    ConnResult = Encoding.GetEncoding(1251).GetString(StreamTest, 0, streamLength);
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("Can't create the network stream: " + exc.Message);
                return false;
            }
            if (ConnResult != "$M$_$ERVER") return false;

            SetConnectionStatus(true);
            if (Client.User != 0)
            {
                PackageViaWireless Package = new PackageViaWireless(0, true);
                Package.DefineQueryAndParams("ConnectionRecovery", "");
                Package.ClientCode = Client.User;
                SendPackage(Package.GetPackage());
            }
            // Запуск пинга сервера 

            //PingAgent = new CallTimer(PingServer, 500);
            return true;
        }

        private bool isPinging()
        {
            lock (this)
            {
                return PingSent;
            }
        }

        private void PingSend(bool value)
        {
            lock (this)
            {
                PingSent = value;
            }
        }

        private void ReadPackages()
        {
            #region Define local variables

            string StorekeeperQuery = "", StorekeeperQueryHead = "";

            //Byte[] emptyData = System.Text.Encoding.GetEncoding(1251).GetBytes("");

            //int streamLength;

            #endregion

            while (true)
            {
                #region Getting package

                if (PackageViaWireless.isCompletelyPackage(StorekeeperQueryHead))
                {
                    StorekeeperQuery = StorekeeperQueryHead;
                    StorekeeperQueryHead = "";
                }
                else
                {
                    StorekeeperQuery = ReadStream();
                }

                Package = null;
                if (StorekeeperQuery == null) return;

                StorekeeperQuery = StorekeeperQueryHead + StorekeeperQuery;
                WriteToFile(" << Read Query [" + StorekeeperQuery + "]", false);

                if (!PackageViaWireless.isCompletelyPackage(StorekeeperQuery)) continue;

                Package = new PackageViaWireless(StorekeeperQuery, out StorekeeperQueryHead);

                StorekeeperQuery = "";

                #endregion

                #region Pinging server

                if (Package.QueryName == "Ping")
                {
                    Package.QueryName = "PingReply";
                    SendPackage(Package.GetPackage());
                    continue;
                }

                if (Package.QueryName == "PingReply")
                {

                    WriteToFile("? ShowPingResult");
                    DataRepresent.ShowPingValue(PackageConvertation.GetPatametersFromStr(Package.Parameters)[0] as string);
                        //Client.MainForm.ShowPingResult(PackageConvertation.GetPatametersFromStr(Package.Parameters)[0] as string);
                    WriteToFile("OK ShowPingResult");

                    continue;
                }

                #endregion

                if (Package.QueryName == "KeyPressing")
                {
                    SendKeyCode = (int)PackageConvertation.GetPatametersFromStr(Package.Parameters)[0];
                    var CallTimer = new CallTimer(SendKey,1,true);                    
                    continue;
                }

                if (Package.QueryName == "BarcodeEvent")
                {
                    SendBarcode = PackageConvertation.GetPatametersFromStr(Package.Parameters)[0] as string;
                    var CallTimer = new CallTimer(SendKey,1,true);                    
                    continue;
                }                

                if (Package.QueryName == "TimeSynchronization")
                {
                    SetTime(PackageConvertation.GetPatametersFromStr(Package.Parameters)[0] as string);
                    continue;
                }

                if (Package.QueryName == "FileTransmit")
                {
                    object[] Params = PackageConvertation.GetPatametersFromStr(Package.Parameters);
                    StorekeeperQueryHead = AcceptFile(Params[0] as string, (int)Params[1], StorekeeperQueryHead, Convert.ToBoolean(Params[2]));
                    continue;
                }

                #region Message handling

                if (Package.QueryName == "Message")
                {
                    object[] Parameters = PackageConvertation.GetPatametersFromStr(Package.Parameters);
                    System.Windows.Forms.MessageBox.Show(Parameters[0] as string);

                    // В параметры записывается только ID, текс сообщения уже не нужен
                    Package.Parameters = ((int)Parameters[1]).ToString();
                    SendPackage(Package.GetPackage());
                    continue;
                }
                #endregion

                #region PackageHandling
                if (Package.PackageID != WaitingPackageID) continue;
                Executed = true;
                RequestReady = true;

                while (RequestReady)
                {
                    Thread.Sleep(100);
                }
                #endregion
            }
        }

        private void SendKey()
        {
            if (SendBarcode != null) {
                Client.MainForm.BarCodeOnTDC(SendBarcode);
                SendBarcode = null;
            }
            else
            {
                Client.MainForm.PressKeyOnTDC(SendKeyCode);
            }
        }

        private string AcceptFile(string FileName, int FileSize, string Head, bool update)
        {
            Byte[] recivedData = new Byte[8192];
            int BytesWrite = 0;
            int StreamLength = Head.Length;

            try
            {
                int FSize = 0;
                string PathToFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                string FolderName = PathToFile + "\\" + (update ? "update" : "received");
                if (!System.IO.Directory.Exists(FolderName))
                {
                    System.IO.Directory.CreateDirectory(FolderName);
                }
                string FullFileName = FolderName + "\\" + FileName;
                if (System.IO.File.Exists(FullFileName))
                {
                    int dotIndex = FullFileName.LastIndexOf(".");
                    FullFileName = String.Format("{0}-{1}{2}", FullFileName.Substring(0, dotIndex), DateTime.Now.ToString("dd.MM.yy_hh.mm.ss"), FullFileName.Substring(dotIndex));
                }
                FileStream WrittingStream = File.Open(FullFileName, FileMode.Create);

                if (StreamLength > 0)
                {
                    BytesWrite = Math.Min(StreamLength, FileSize);
                    WrittingStream.Write(Encoding.GetEncoding(1251).GetBytes(Head), 0, BytesWrite);
                    FSize += BytesWrite;

                    if (FSize == FileSize)
                    {
                        InformFileAccepted(update);
                        return Head.Substring(BytesWrite);
                    }
                }
                else
                {
                    StreamLength = -1;
                }

                while (StreamLength != 0 && FSize < FileSize)
                {
                    StreamLength = TCPStream.Read(recivedData, 0, recivedData.Length);
                    BytesWrite = Math.Min(StreamLength, FileSize - FSize);
                    WrittingStream.Write(recivedData, 0, BytesWrite);
                    FSize += BytesWrite;
                }

                WrittingStream.Close();
            }
            catch
            {
                return "";
            }
            InformFileAccepted(update);

            string LeftString = Encoding.GetEncoding(1251).GetString(recivedData, 0, StreamLength);
            return LeftString.Substring(BytesWrite);
        }

        private void InformFileAccepted(bool update)
        {
            // Отправка сообщение серверу, что файл получен
            PackageViaWireless Package = new PackageViaWireless(Client.User, true);
            Package.DefineQueryAndParams("FileAccepted", "");
            SendPackage(Package.GetPackage());
            System.Threading.Thread.Sleep(1000);
            if (update)
                lock (Client)
                {
                    Client.TryToUpdate();
                }
        }

        private string ReadStream()
        {

            IAsyncResult NetStreamReadRes;
            Byte[] recivedData = new Byte[512];
            int streamLength = 0;
            string ResultString = "";
            StringBuilder SB = new StringBuilder();

            do
            {
                try
                {
                    NetStreamReadRes = TCPStream.BeginRead(recivedData, 0, recivedData.Length, null, null);

                    while (!NetStreamReadRes.AsyncWaitHandle.WaitOne(100, false)) ;

                    streamLength = TCPStream.EndRead(NetStreamReadRes);
                    NetStreamReadRes = null;
                    ResultString = Encoding.GetEncoding(1251).GetString(recivedData, 0, streamLength);
                    WriteToFile(" << Read STREAM [" + ResultString + "]", false);
                    SB.Append(ResultString);
                }
                catch
                {
                    SetConnectionStatus(false);
                    return null;
                }
            } while (streamLength == 512 && ResultString.Substring(507) != "#END>");


            return SB.ToString();

        }

        private void SetConnectionStatus(bool IsOnline)
        {
            WriteToFile("?  SetConnectionStatus " + IsOnline.ToString());
            ConnectionEstablished = IsOnline;
            DataRepresent.ShowOnLineStatus(IsOnline);
            WriteToFile("OK SetConnectionStatus " + IsOnline.ToString());
        }

        private void WriteToFile(string buffer)
        {
            WriteToFile(buffer, false);
        }

        private void WriteToFile(string buffer, bool rewrite)
        {
            lock (this)
            {
                try
                {
                    string fileName = @"\Client.txt";
                    StreamWriter myFile;
                    if (rewrite)
                    {
                        myFile = File.CreateText(fileName);
                    }
                    else
                    {
                        myFile = File.AppendText(fileName);
                    }

                    myFile.WriteLine("\t" + DateTime.Now.ToString("HH:mm:ss"));
                    myFile.WriteLine(buffer);
                    myFile.WriteLine("");
                    myFile.Close();
                }
                catch (Exception e)
                {
                    string s = e.Message;
                }
            }
        }

        #endregion
    }
}
