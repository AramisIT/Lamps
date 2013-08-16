using System.Windows.Forms;
using System;

namespace WMS_client
    {
    public enum KeyAction
        {
        LeftKey = 37,
        RightKey = 39,
        UpKey = 38,
        DownKey = 40,
        No = 0x6A,


        Esc = 27,
        Recount = 114,          // F3
        Refresh = 114,          // F3
        Complate = 115,         // F4
        Problem = 113,          // F2

        Proceed = 116,      // F5
        Incoming = 116,     // F5
        F1 = 112,            // F1
        F5 = 116,           // F5
        RepeatPrinting = 117,   // F6
        BarCodeByHands = 122,   // F11
        Act = 119,   // F8
        F8 = 119,   // F8
        Shipping = 120,   // F9
        F9 = 120,
        F10 = 121,
        Exit = 121,              // F10
        F11 = 122,
        F12 = 123
        }

    public enum ProcessType
        {
        Incoming = 1,
        Selecting = 6,
        QualityRegistration = 7,
        RawProductionQualityRegistrationProcess = 8,
        Waiting = 100,
        Registration = 10,
        MainUserMenu = 101,
        FormDesign = 102
        }

    public abstract class BusinessProcess
        {
        #region Public fields
        protected bool IsLoad;
        public object[] ResultParameters;
        public WMSClient MainProcess;
        public ProcessType BusinessProcessType;
        public string DocumentNumber;
        public string CellBarcode;
        public string CellName;
        public int FormNumber = 0;
        public int NextFormNumber = 1;
        public bool IsExistParameters { get { return ResultParameters != null && ResultParameters.Length > 0 && ResultParameters[0] != null; } }
        public bool IsAnswerIsTrue { get { return IsExistParameters && Convert.ToBoolean(ResultParameters[0]); } }
        #endregion
        #region Public methods

        #region Constructor
        protected BusinessProcess() { }

        protected BusinessProcess(WMSClient MainProcess, int FormNumber)
            : this(MainProcess, "", "", FormNumber) { }

        protected BusinessProcess(WMSClient MainProcess, string CellName, string CellBarcode)
            : this(MainProcess, CellName, CellBarcode, 0) { }

        protected BusinessProcess(WMSClient MainProcess, string CellName, string CellBarcode, int FormNumber)
            {
            ShowProgress(1, 1);
            this.FormNumber = CellName == "" ? FormNumber : 0;
            if (FormNumber == 0)
                {
                this.CellName = CellName;
                this.CellBarcode = CellBarcode;
                }

            this.MainProcess = MainProcess;
            Start();
            }
        #endregion

        protected void StopNetworkConnection()
            {
            bool release = true;
#if DEBUG
            release = false;
#endif
            if (release)
                {
                bool startStatus = MainProcess.ConnectionAgent.WifiEnabled;
                if (startStatus)
                    {
                    MainProcess.ConnectionAgent.StopConnection();
                    }
                }
            }

        protected void StartNetworkConnection()
            {
            bool startStatus = MainProcess.ConnectionAgent.WifiEnabled;
            if (!startStatus)
                {
                MainProcess.StartConnectionAgent();
                System.Threading.Thread.Sleep(1500);
                }
            }

        public void PerformQuery(string QueryName, params object[] parameters)
            {
            ResultParameters = null;
            if (!MainProcess.OnLine && MainProcess.MainForm.IsMainThread)
                {
                ShowMessage("Нет подключения к серверу");
                return;
                }

            ResultParameters = MainProcess.PerformQuery(QueryName, parameters);
            }

        protected void ClearControls()
            {
            MainProcess.ClearControls();
            }

        //public void ShortQuery(string QueryName, params object[] parameters)
        //    {
        //    object[] NewParameters = new object[parameters.Length + 3];
        //    NewParameters[0] = "#<PDT>#";
        //    NewParameters[1] = (int)BusinessProcessType;
        //    NewParameters[2] = DocumentNumber;
        //    for (int i = 0; i < parameters.Length; i++)
        //        {
        //        NewParameters[i + 3] = parameters[i];
        //        }
        //    PerformQuery(QueryName, NewParameters);
        //    }

        public void BarCodeByHands()
            {
            MainProcess.MainForm.BarCodeByHands();
            }

        public virtual void Start()
            {
            SetEventHendlers();

            if (FormNumber == 0)
                {
                LocalizationStep();
                }
            else
                {
                DrawControls();
                }

            }

        public void ShowMessage(string msg)
            {
            MessageBox.Show(msg.ToUpper(), "aramis wms");
            }

        public bool ShowQuery(string msg)
            {
            return MessageBox.Show(msg.ToUpper(), "aramis wms", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes;
            }

        protected bool OnLine
            {
            get
                {
                return MainProcess.ConnectionAgent.OnLine;
                }
            }

        protected bool SuccessQueryResult
            {
            get
                {
                return ResultParameters != null
                       && ResultParameters.GetType() == typeof(object[])
                       && ResultParameters.Length > 0
                       && ResultParameters[0] is bool
                       && (bool)ResultParameters[0];
                }
            }
        #endregion

        #region Private methods
        private void SetEventHendlers()
            {
            if (FormNumber == 0)
                {
                MainProcess.OnBarcode = OnCellBarcode;
                MainProcess.MainForm.SetOnHotKeyPressed(OnCellHotKey);
                //MainProcess.HotKeyAgent.OnHotKeyPressed = OnCellHotKey;
                }
            else
                {
                MainProcess.OnBarcode = OnBarcode;
                MainProcess.MainForm.SetOnHotKeyPressed(OnHotKey);
                //MainProcess.HotKeyAgent.OnHotKeyPressed = OnHotKey;
                }
            }

        private void LocalizationStep()
            {
            MainProcess.ClearControls();
            MainProcess.MainForm.ShowCellName(CellName);
            //MainProcess.MainForm.HotKeyAgent.SetHotKey(KeyAction.Proceed);
            MainProcess.ToDoCommand = "Подойти к ячейке";
            }

        private void OnCellBarcode(string Barcode)
            {
            if (CellBarcode == Barcode)
                {
                OnCellHotKey(KeyAction.Proceed);
                }
            else
                {
                ShowMessage("Отсканируйте требуемый объект");
                }
            }

        private void OnCellHotKey(KeyAction Key)
            {
            if (Key == KeyAction.Proceed)
                {
                FormNumber = NextFormNumber;
                MainProcess.ClearControls();
                MainProcess.OnBarcode = OnBarcode;
                MainProcess.MainForm.SetOnHotKeyPressed(OnHotKey);

                Start();
                }
            }
        #endregion

        #region Abstract methods
        public abstract void DrawControls();
        public abstract void OnBarcode(string barcode);
        public abstract void OnHotKey(KeyAction key);
        #endregion

        /// <summary>
        /// show progress bar status
        /// </summary>
        /// <param name="value">from 0 to 100 value</param>
        protected void ShowProgress(int currentValue, int total)
            {
            if (MainProcess == null)
                {
                return;
                }

            if (total == currentValue || total == 0)
                {
                MainProcess.MainForm.ShowProgress(100);
                }
            else
                {
                var percent = (int)(100 * currentValue / total);
                MainProcess.MainForm.ShowProgress(percent);
                }
            }
        }
    }