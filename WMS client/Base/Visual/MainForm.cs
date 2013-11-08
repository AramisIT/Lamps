using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Intermec.DataCollection;
using Microsoft.WindowsMobile.Status;
using WMS_client.db;
using WMS_client.Base.Visual;
using WMS_client.Utils;

namespace WMS_client
    {
    public partial class MainForm : Form
        {
        #region Public fields

        public bool IsTest { get; private set; }
        public OnEventDelegate PerformInMainThread;
        public HotKeyProcessing HotKeyAgent;
        public bool IsClosing = false;
        public WMSClient Client;
        private const int versionNumber = 64;

        public int VersionNumber
            {
            get { return versionNumber; }
            }

        public String ServerIP;

        public bool IsMainThread
            {
            get { return !LogoOnLine.InvokeRequired; }
            }

        #endregion

        #region Private fields

        //int? CursorX = null;
        //int? CursorY = null;
        //long TimeTicks;
        private readonly Color TextColor = Color.FromArgb(214, 223, 246);
        private readonly Color IndicatorColor = Color.FromArgb(168, 0, 21);
        //private string Barcode;
        //private long BarcodeTimeStart = 0;
        private const string symbols = "-\\|/";
        private int symbolsNum;
        private string PingResult = "";
        private readonly BarcodeReader IntermecBarcodeReader;

        private delegate void FVoid1IntDelegate(int key);

        #endregion

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

        #region Public methods

        public MainForm()
            : this(false)
            {
            }

        public MainForm(bool test)
            {
            IsTest = test;
            InitializeComponent();
            Height = 320;
            Width = 240;

            //SqlCeEngine engine = new SqlCeEngine("Data Source='SD-MMCard\\DCIM\\Test333.sdf';");

            //if (!(File.Exists(@"SD-MMCard\DCIM\Test333.sdf")))
            //    engine.CreateDatabase();

            //SqlCeConnection connection = new SqlCeConnection(engine.LocalConnectionString);
            //connection.Open();

            //SqlCeCommand command = connection.CreateCommand();
            //command.CommandText = "SELECT * FROM dar";
            //SqlCeDataReader result = command.ExecuteReader();
            //while (result.Read())
            //{
            //    MessageBox.Show(result[0].ToString());
            //}

            if (!IsTest)
                {
                try
                    {
                    IntermecBarcodeReader = new BarcodeReader();
                    IntermecBarcodeReader.BarcodeRead += OnBarcodeRead;
                    IntermecBarcodeReader.ThreadedRead(true);
                    }
                catch (Exception exc)
                    {
                    Console.Write(exc.Message);
                    }
                }

            HotKeyAgent = new HotKeyProcessing(this);

            Client = new WMSClient(this);
            Client.Start();
            if (
                File.Exists(
                    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) +
                    "\\update"))
                {
                Client.NeedToUpdate = true;
                }
            }

        public void PerformMainThreadEvent()
            {
            if (InvokeRequired)
                {
                try
                    {
                    Invoke(new OnEventDelegate(PerformMainThreadEvent), null);
                    }
                catch
                    {
                    }
                }
            else
                {
                PerformInMainThread();
                }
            }

        public void DrawConnectionStatus(bool IsOnline)
            {
            if (IsClosing) return;

            if (LogoOnLine.InvokeRequired)
                {
                try
                    {
                    Invoke(new SetConnectionStatusDelegate(DrawConnectionStatus), new object[] { IsOnline });
                    }
                catch
                    {
                    }
                }
            else
                {
                LogoOnLine.Visible = IsOnline;
                if (IsOnline)
                    {
                    //Note: Online
                    BackColor = Color.FromArgb(219, 236, 242);
                    Command.ForeColor = Color.DarkGreen;
                    }
                else
                    {
                    //Note: Offline
                    BackColor = Color.FromArgb(220, 220, 220);
                    Command.ForeColor = Color.FromArgb(58, 58, 58);
                    }
                }
            }

        public void ShowQueryWait()
            {
            if (!LogoOnLine.InvokeRequired)
                {
                LogoOnLine.Size = new Size(168, 19);
                LogoOnLine.Location = new Point(36, 8);

                LogoOffLine.Size = new Size(168, 19);
                LogoOffLine.Location = new Point(36, 8);
                Refresh();
                }
            }

        public void ShowQueryComplated()
            {
            if (!LogoOnLine.InvokeRequired)
                {
                LogoOnLine.Size = new Size(240, 34);
                LogoOnLine.Location = new Point(0, 0);

                LogoOffLine.Size = new Size(240, 34);
                LogoOffLine.Location = new Point(0, 0);
                Refresh();
                }
            }

        public void ShowPingResult(string result)
            {
            if (LogoOnLine.InvokeRequired)
                {
                try
                    {
                    Invoke(new FVoid1StringDelegate(ShowPingResult), new object[] { result });
                    }
                catch
                    {
                    }
                }
            else
                {

                PingResult = String.Format("{0}Ping reply: {1}", symbols[symbolsNum].ToString(), result);
                symbolsNum++;
                if (symbolsNum > 3) symbolsNum = 0;
                //if (!PingResult.Visible) { PingResult.Visible = true; }
                LogoOnLine.Refresh();
                }
            }

        public void ShowCellName(string NewCellName)
            {
            CellName.Text = NewCellName;
            CellName.Visible = true;
            }

        public void HideCellName()
            {
            CellName.Visible = false;
            }

        public void BarCodeOnTDC(string Barcode)
            {
            if (LogoOnLine.InvokeRequired)
                {
                try
                    {
                    Invoke(new FVoid1StringDelegate(BarCodeOnTDC), new object[] { Barcode });
                    }
                catch (Exception exc)
                    {
                    Console.Write(exc.Message);
                    }
                }
            else
                {
                Client.Process.OnBarcode(Barcode);
                }
            }

        public void PressKeyOnTDC(int KeyCode)
            {
            if (LogoOnLine.InvokeRequired)
                {
                try
                    {
                    Invoke(new FVoid1IntDelegate(PressKeyOnTDC), new object[] { KeyCode });
                    }
                catch
                    {
                    }
                }
            else
                {
                HotKeyAgent.OnHotKeyPressed((KeyAction)KeyCode);
                //Client.Process.OnHotKey((KeyAction)KeyCode);
                }
            }

        public void BarCodeByHands()
            {
            BarcodeLabel.Visible = true;
            BarcodeTextBox.Enabled = true;
            BarcodeTextBox.Visible = true;
            Command.Visible = false;
            BarcodeTextBox.Focus();
            }

        public void BarCodeByHandsCancel()
            {
            BarcodeLabel.Visible = false;
            BarcodeTextBox.Enabled = false;
            BarcodeTextBox.Visible = false;
            Command.Visible = true;
            }

        public void SetOnHotKeyPressed(OnHotKeyPressedDelegate OnHotKeyPressed)
            {
            HotKeyAgent.OnHotKeyPressed = OnHotKeyPressed;
            }

        #endregion

        #region Private methods

        private void OnBarcodeRead(object sender, BarcodeReadEventArgs e)
            {
            Client.OnBarcode(e.strDataBuffer);
            }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
            {
            if (
                MessageBox.Show(String.Format("[{0}]\r\n\r\nЗАКРЫТЬ ПРИЛОЖЕНИЕ? Заряд {1} %", ServerIP, BatteryChargeStatus.ChargeValue),
                    "Aramis WMS Ver." + VersionNumber.ToString(), MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                {
                Close();
                Client.ConnectionAgent.CloseAll();
                Application.Exit();

                }
            }

        private void Form1_Closed(object sender, EventArgs e)
            {
            //IntermecBarcodeReader.Dispose();
            //Client.OnExit();
            dbWorker.Dispose();
            }

        private void BarcodeTextBox_LostFocus(object sender, EventArgs e)
            {
            BarCodeByHandsCancel();
            string barcode = BarcodeTextBox.Text.Trim().Replace(Convert.ToString((char)160), "");
            BarcodeTextBox.Text = "";
            if (barcode != "")
                {
                Client.OnBarcode(barcode);
                }
            }

        private void BarcodeTextBox_KeyPress(object sender, KeyPressEventArgs e)
            {
            if (e.KeyChar == '\r')
                {
                BarcodeTextBox_LostFocus(sender, null);
                }
            }

        #endregion

        private void LogoOnLine_Paint(object sender, PaintEventArgs e)
            {
            if (PingResult.Length > 0)
                {
                Graphics g = e.Graphics;
                g.DrawString(PingResult[0].ToString(), new Font("Tahoma", 8, FontStyle.Bold),
                    new SolidBrush(IndicatorColor), 29, 19);
                g.DrawString(PingResult.Substring(1), new Font("Tahoma", 8, FontStyle.Bold),
                    new SolidBrush(TextColor), 45, 20);

                g.DrawString(DateTime.Now.ToString("HH:mm:ss"), new Font("Tahoma", 8, FontStyle.Bold),
                    new SolidBrush(TextColor), 176, 20);
                PingResult = "";
                }
            }

        private void Form1_Load(object sender, EventArgs e)
            {
            (new EmptyDialog()).ShowDialog();
            WindowState = FormWindowState.Maximized;
            }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
            {
            //string str = e.KeyData.ToString();
            //MessageBox.Show(str);

            //if (e.KeyCode == Keys.D1 && Client.Process is SelectingProcess)
            //{
            //    HotKeyAgent.OnHotKeyPressed(KeyAction.Proceed);
            //}
            //else 
            if (e.Shift && e.KeyCode == Keys.ShiftKey)
                {
                HotKeyAgent.OnHotKeyPressed(KeyAction.Proceed);
                }
            else if (e.KeyCode == Keys.Escape)
                {
                //if (Client.Process is RegistrationProcess)
                //{
                //    (Client.GetControl("Login") as MobileTextBox).Text = "a";
                //    (Client.GetControl("Password") as MobileTextBox).Text = "a";
                //}

                HotKeyAgent.OnHotKeyPressed(KeyAction.Esc);
                }
            }

        //private void Form1_MouseDown(object sender, MouseEventArgs e)
        //{
        //    CursorX = e.X;
        //    CursorY = e.Y;
        //    TimeTicks = DateTime.Now.Ticks;
        //}

        //private void Form1_MouseUp(object sender, MouseEventArgs e)
        //{
        //    CursorX = null;
        //    CursorY = null;

        //    if (Client.OnLine && TimeTicks == DateTime.Now.Ticks)
        //    {
        //        Client.SendToServer("DoMouseClick");
        //    }
        //}

        //private void Form1_MouseMove(object sender, MouseEventArgs e)
        //{
        //    if (CursorX == null) return;

        //    int? DeltaX = e.X - CursorX;
        //    int? DeltaY = e.Y - CursorY;

        //    CursorX = e.X;
        //    CursorY = e.Y;

        //    if ((DeltaX != 0 || DeltaY != 0) && Client.OnLine)
        //    {
        //        //Client.SendToServer("MouseMove", DeltaX.ToString(), DeltaY.ToString());
        //        Client.SendToServer("MouseMove", (-DeltaY).ToString(), (DeltaX).ToString());
        //    }
        //}

        internal void ShowProgress(int value)
            {
            if (value == 100)
                {
                progressBar.Visible = false;
                }
            else
                {
                progressBar.Visible = true;
                progressBar.Value = value;
                }
            }
        }
    }