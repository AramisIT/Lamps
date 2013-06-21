using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using StorekeeperManagementServer;
using System.IO;

namespace WMS_client
{
    #region Common NameSpace types
    public enum MobileFontSize
    {
        Normal,
        Little,
        Large,
        Multiline
    }

    public enum MobileFontColors
    {
        Default,
        Info,
        Warning,
        Empty,
        Disable
    }

    public enum MobileFontPosition
    {
        Left,Center,Right
    }

    public enum ControlsStyle
    {
        LabelNormal,
        LabelRedRightAllign,
        LabelLarge,
        LabelSmall,
        LabelH2,
        LabelH2Red,
        LabelMultiline,
        TextBoxNormal,
        LabelMultilineSmall
    }

    public delegate void OnScanDelegate(string Barcode);
    public delegate void OnHotKeyPressedDelegate(KeyAction Key);
    public delegate void OnEventHandlingDelegate(object obj, EventArgs e);
    public delegate void OnKeyPressDelegate(object sender, KeyPressEventArgs e);
    public delegate void Void2paramDelegate<T1, T2>(T1 param1, T2 param2);
    #endregion

    public class WMSClient
    {
        #region Public fields

        public OnScanDelegate OnBarcode;
        public bool NeedToUpdate = false;
        public bool OnLine
        {
            get { return ConnectionAgent.OnLine; }
        }

        public int User { get; set; }

        public BusinessProcess Process;
        public MainForm MainForm;
        public string ToDoCommand
        {
            get { return MainForm.Command.Text; }
            set
            {
                if (value!=null)
                {MainForm.Command.Text = value.ToUpper();}
                RefreshPicture();
            }
        }
        public int LastLabelKey = -1;
        public string LastDoc = null;
        public ServerAgent ConnectionAgent;

        #endregion

        #region Private fields

        private readonly List<MobileControl> ControlsArray = new List<MobileControl>();

        private Thread AgentThread;

        #endregion // Private fields

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

        #region Public Methods

        #region Service methods

        public WMSClient(MainForm Form)
        {
            User = 0;
            MainForm = Form;
        }

        public void Start()
        {

            #region Чтение IP-адреса сервера

            string SettingsFileName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + @"\serverip.txt";
            SettingsFileName = SettingsFileName.Replace("file:\\", string.Empty);
            
            if (!File.Exists(SettingsFileName))
            {
                MessageBox.Show(string.Format("Не найден файл настроек [{0}]\r\n\r\nПриложение будет закрыто!", SettingsFileName));
                Application.Exit();
                return;
            }

            StreamReader SettingsFile = File.OpenText(SettingsFileName);

            string ServerIP;

            while ((ServerIP = SettingsFile.ReadLine()) != null)
            {
                if (ServerIP.Trim() != string.Empty)
                {
                    break;
                }
            }
            SettingsFile.Close();

            #endregion
            //ConnectionAgent = new ServerAgent("192.168.100.254", 8609, this, MainForm.DrawConnectionStatus, MainForm.ShowPingResult);

            try
            {
                ConnectionAgent = new ServerAgent(ServerIP, 8609, this, MainForm.DrawConnectionStatus, MainForm.ShowPingResult);
            }
            catch (Exception exp)
            {
                MessageBox.Show(string.Format("Ошибка создания агента сервера. Server IP = {0}\r\nОписание ошибки:\r\n{1}\r\nПриложение будет закрыто!", ServerIP, exp.Message));
                Application.Exit();
            }
            MainForm.ServerIP = ServerIP;

            StartConnectionAgent();

            Welcome();
        }

        public void StartConnectionAgent()
            {            
            AgentThread = new Thread(ConnectionAgent.Start);
            AgentThread.Name = "Server Agent";
            AgentThread.IsBackground = true;
            AgentThread.Start();
            }        

        public void Updating()
        {
            string PathToFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            PathToFile += "\\WMS Client Loader.exe";

            if (!File.Exists(PathToFile))
            {
                MessageBox.Show("Ошибка обновления:\r\nНе найден файл \"WMS Client Loader.exe\"!\r\nТекущая версия ПО - " + MainForm.VersionNumber.ToString(), "ARAMIS WMS");
                NeedToUpdate = false;
                return;
            }
            MessageBox.Show("Сейчас будет произведено обновление программного обеспечения!", "LOGISTON WMS");
            //SendToServer("BreakConnection");
            Thread.Sleep(500);

            System.Diagnostics.Process.Start(PathToFile, string.Empty);
            MainForm.Close();
        }

        public MobileControl GetControl(string ControlName)
        {
            return ControlsArray.FirstOrDefault(control => control.Name == ControlName);
        }

        public void RefreshPicture()
        {
            MainForm.Refresh();
        }

        #region Create
        #region Create Button
        public MobileButton CreateButton(string text, int left, int top, int width, int height, string controlName, MobileButtonClick mobileButtonClick)
        {
            return CreateButton(text, left, top, width, height, controlName, mobileButtonClick, null, null, true);
        }

        public MobileButton CreateButton(string text, int left, int top, int width, int height, string controlName, MobileSenderClick mobileSenderClick, object tag)
        {
            return CreateButton(text, left, top, width, height, controlName, null, mobileSenderClick, tag, true);
        }

        public MobileButton CreateButton(string text, int left, int top, int width, int height, string controlName, MobileSenderClick mobileSenderClick, object tag, bool enabled)
        {
            return CreateButton(text, left, top, width, height, controlName, null, mobileSenderClick, tag, enabled);
        }

        public MobileButton CreateButton(string text, int left, int top, int width, int height, string controlName, MobileButtonClick mobileButtonClick, MobileSenderClick mobileSenderClick, object tag, bool enabled)
        {
            MobileButton NewControl = new MobileButton(MainForm, text, left, top, width, height, controlName, mobileButtonClick, mobileSenderClick, tag, enabled);
            ControlsArray.Add(NewControl);
            return NewControl;
        }
        #endregion

        #region CreateLabel OLD
        public MobileLabel CreateLabel(string text, int left, int top, int width, int height, ControlsStyle style)
        {
            return CreateLabel(text, left, top, width, height, style, string.Empty);
        }

        public MobileLabel CreateLabel(string text, int left, int top, int width, ControlsStyle style)
        {
            return CreateLabel(text, left, top, width, 0, style, string.Empty);
        }
        public MobileLabel CreateLabel(string text, int left, int top, int width, ControlsStyle style, string controlName)
        {
            return CreateLabel(text, left, top, width, 0, style, controlName);
        }

        public MobileLabel CreateLabel(string text, int left, int top, int width, int height, ControlsStyle style, string controlName)
        {
            MobileLabel NewControl = new MobileLabel(MainForm, text, left, top, width, height, style, controlName);
            ControlsArray.Add(NewControl);
            return NewControl;
        }
        #endregion

        #region CreateLabel NEW
        public MobileLabel CreateLabel(string text, int left, int top, int width, int height)
        {
            return CreateLabel(text, left, top, width, height, MobileFontSize.Normal, MobileFontPosition.Left, MobileFontColors.Default, FontStyle.Regular, string.Empty);
        }

        public MobileLabel CreateLabel(string text, int left, int top, int width, MobileFontColors color)
        {
            return CreateLabel(text, left, top, width, 0, MobileFontSize.Normal, MobileFontPosition.Left, color, FontStyle.Regular, string.Empty);
        }

        public MobileLabel CreateLabel(string text, int left, int top, int width, MobileFontSize size)
        {
            return CreateLabel(text, left, top, width, 0, size, MobileFontPosition.Left, MobileFontColors.Default, FontStyle.Regular, string.Empty);
        }

        public MobileLabel CreateLabel(string text, int left, int top, int width, MobileFontSize size, FontStyle fontStyle)
        {
            return CreateLabel(text, left, top, width, 0, size, MobileFontPosition.Left, MobileFontColors.Default, fontStyle, string.Empty);
        }

        public MobileLabel CreateLabel(string text, int left, int top, int width, MobileFontSize size, MobileFontColors color)
        {
            return CreateLabel(text, left, top, width, 0, MobileFontSize.Normal, MobileFontPosition.Left, color, FontStyle.Regular, string.Empty);
        }

        public MobileLabel CreateLabel(string text, int left, int top, int width, MobileFontSize size, MobileFontPosition position)
        {
            return CreateLabel(text, left, top, width, 0, size, position, MobileFontColors.Default, FontStyle.Regular, string.Empty);
        }

        public MobileLabel CreateLabel(string text, int left, int top, int width, MobileFontSize size, MobileFontPosition position, FontStyle fontStyle)
        {
            return CreateLabel(text, left, top, width, 0, size, position, MobileFontColors.Default, fontStyle, string.Empty);
        }

        public MobileLabel CreateLabel(string text, int left, int top, int width, MobileFontSize size, MobileFontPosition position, MobileFontColors color)
        {
            return CreateLabel(text, left, top, width, 0, size, position, color, FontStyle.Regular, string.Empty);
        }

        public MobileLabel CreateLabel(string text, int left, int top, int width, MobileFontSize size, MobileFontPosition position, MobileFontColors color, FontStyle fontStyle)
        {
            return CreateLabel(text, left, top, width, 0, size, position, color, fontStyle, string.Empty);
        }

        public MobileLabel CreateLabel(string text, int left, int top, int width, int height, MobileFontSize size, MobileFontPosition position, MobileFontColors color, FontStyle fontStyle)
        {
            return CreateLabel(text, left, top, width, height, size, position, color, fontStyle, string.Empty);
        }

        public MobileLabel CreateLabel(string text, int left, int top, int width, MobileFontSize size, MobileFontPosition position, MobileFontColors color, FontStyle fontStyle, string controlName)
        {
            return CreateLabel(text, left, top, width, 0, size, position, color, fontStyle, controlName);
        }

        public MobileLabel CreateLabel(string text, int left, int top, int width, int height, MobileFontSize size, MobileFontPosition position, MobileFontColors color, FontStyle fontStyle, string controlName)
        {
            MobileLabel NewControl = new MobileLabel(MainForm, text, left, top, width, height, size, position, color, fontStyle, controlName);
            ControlsArray.Add(NewControl);
            return NewControl;
        }
        #endregion

        #region CreateTextBox
        public MobileControl CreateTextBox(int left, int top, int width, string controlName, ControlsStyle style, bool isPasswordField)
        {
            return CreateTextBox(left, top, width, controlName, style, null, isPasswordField, true);
        }

        public MobileControl CreateTextBox(int left, int top, int width, string controlName, ControlsStyle style, OnEventHandlingDelegate ProcTarget, bool isTextField)
        {
            return CreateTextBox(left, top, width, controlName, style, ProcTarget, false, isTextField);
        }

        public MobileControl CreateTextBox(int left, int top, int width, string controlName, ControlsStyle style)
        {
            return CreateTextBox(left, top, width, controlName, style, null, false, true);
        }

        public MobileControl CreateTextBox(int left, int top, int width, string controlName, ControlsStyle style, OnEventHandlingDelegate ProcTarget, bool isPasswordField, bool isTextField)
        {
            MobileControl NewControl = new MobileTextBox(MainForm, left, top, width, controlName, style, ProcTarget, isPasswordField, isTextField);
            ControlsArray.Add(NewControl);
            return NewControl;
        }
        #endregion

        #region CreateTable
        public MobileTable CreateTable(string controlName, int height)
        {
            return CreateTable(controlName, height, null);
        }

        public MobileTable CreateTable(string controlName, int height, int top)
        {
            return CreateTable(controlName, height, top, delegate(object param1, OnChangeSelectedRowEventArgs param2) {  });
        }

        public MobileTable CreateTable(string controlName, int height, Void2paramDelegate<object, OnRowSelectedEventArgs> onRowSelected)
        {
            return CreateTable(controlName, height, 60, onRowSelected);
        }

        public MobileTable CreateTable(string controlName, int height, int top, Void2paramDelegate<object, OnChangeSelectedRowEventArgs> onChangeSelectedRow)
        {
            MobileTable NewControl = new MobileTable(MainForm, controlName, height, top, onChangeSelectedRow);
            ControlsArray.Add(NewControl);
            return NewControl;
        }

        public MobileTable CreateTable(string controlName, int height, int top, Void2paramDelegate<object, OnRowSelectedEventArgs> onRowSelected)
        {
            MobileTable NewControl = new MobileTable(MainForm, controlName, height, top, onRowSelected);
            ControlsArray.Add(NewControl);
            return NewControl;
        }
        #endregion 
        #endregion

        public void onChange(object obj, EventArgs e)
        {
            MessageBox.Show(((TextBox) obj).Text);
        }

        public void OnExit()
        {
            MainForm.HotKeyAgent.UnRegisterKeys();
            AgentThread.Abort();
            ConnectionAgent.CloseAll();
        }

        public object[] PerformQuery(string QueryName, params object[] Parameters)
        {

            PackageViaWireless Package = new PackageViaWireless(0, true);
            Package.DefineQueryAndParams(QueryName, PackageConvertation.GetStrPatametersFromArray(Parameters));
            Package.ClientCode = User;
            ConnectionAgent.WaitingPackageID = Package.PackageID;
            ConnectionAgent.Executed = false;
            MainForm.ShowQueryWait();
            if (ConnectionAgent.SendPackage(Package.GetPackage()))
            {

                while (!ConnectionAgent.RequestReady & ConnectionAgent.OnLine)
                {
                    Thread.Sleep(300);
                }

                MainForm.ShowQueryComplated();

                if (ConnectionAgent.Package == null || !ConnectionAgent.Executed)
                {

                    Process.ShowMessage("Пропала связь с сервером!");
                    return null;
                }
                if (ConnectionAgent.Package.Parameters == "#ERROR:1C_AGENT_DISABLE#")
                {
                    ConnectionAgent.Package.Parameters = string.Empty;
                    ConnectionAgent.RequestReady = false;
                    Process.ShowMessage("1С-агент неактивен. Обратитесь к оператору!");
                    return null;
                }
                object[] result = PackageConvertation.GetPatametersFromStr(ConnectionAgent.Package.Parameters);

                ConnectionAgent.RequestReady = false;

                if (result.Length == 0)
                {
                    return null;
                }
                if (result.GetType() == typeof(object[]) && result.Length == 1 && result[0] == null)
                    return null;
                return result;
            }
            return null;

        }

        public void SendToServer(string QueryName, params object[] Parameters)
        {
            PackageViaWireless Package = new PackageViaWireless(0, true);
            Package.DefineQueryAndParams(QueryName, PackageConvertation.GetStrPatametersFromArray(Parameters));
            Package.ClientCode = User;
            ConnectionAgent.WaitingPackageID = Package.PackageID;
            ConnectionAgent.SendPackage(Package.GetPackage());
        }

        public void TryToUpdate()
        {
            NeedToUpdate = true;

            if (Process.GetType() == typeof(RegistrationProcess))
            {
                MainForm.PerformInMainThread = Updating;
                MainForm.PerformMainThreadEvent();
            }
        }

        public void ClearControls()
        {

            foreach (MobileControl control in ControlsArray)
            {
                MainForm.Controls.Remove(control.GetControl() as Control);
            }

            ControlsArray.Clear();
            MainForm.HideCellName();
            // MainForm.HotKeyAgent.UnRegisterKeys();
            ToDoCommand = string.Empty;
        }

        public void SelectNextControl(ref Control CurrentControl)
        {
            if (ControlsArray.Count == 0) return;
            bool SearchComplated = false;
            foreach (MobileControl Item in ControlsArray)
            {
                if (SearchComplated)
                {
                    CurrentControl = Item.GetControl() as Control;
                    return;
                }

                if (Item.GetControl() == CurrentControl)
                {
                    SearchComplated = true;
                }
            }
            CurrentControl = ControlsArray[0].GetControl() as Control;
        }

        public void RemoveControl(Control CurrentControl)
        {
            foreach (MobileControl Item in ControlsArray)
            {
                if (Item.GetControl() == CurrentControl)
                {
                    Item.Hide();
                    ControlsArray.Remove(Item);
                    return;

                }
            }
        }

        #endregion

        #region Busines process starting

        public void WaitingProcess()
        {
            //Process.ShowMessage("2");
            ClearControls();
            //Process.ShowMessage("3");
            Process = new EmptyProcess(this);
        }

        public void Welcome()
        {

            ClearControls();
            User = 0;
            bool IsDebugMode = false;
#if DEBUG
            IsDebugMode = true;
#endif
            if (IsDebugMode)
            {
                Process = new FormDesignProcess(this);
            }
            else
            {
                User = 12;
                //Process = new SelectingProcess(this);
                Process = new RegistrationProcess(this);                
            }

        }

        public void Exit()
        {
            MainForm.IsClosing = true;
            Thread.Sleep(500);
            ClearControls();
            AgentThread.Abort();
            Application.Exit();
        }

        #endregion

        #endregion
    }
}
