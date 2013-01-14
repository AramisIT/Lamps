using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace WMS_client
{
    class DataDrawing
    {
        #region Private fields

        private string PingValueText = null;
        private bool? OnLineStatus = null;
        
        private SetConnectionStatusDelegate DrawConnectionStatus;
        private FVoid1StringDelegate ShowPingResult;
        private Thread PerformanceThread;

        #endregion

        #region Constructor
        
        public DataDrawing(SetConnectionStatusDelegate DrawConnectionStatus, FVoid1StringDelegate ShowPingResult)
        {
            this.DrawConnectionStatus = DrawConnectionStatus;
            this.ShowPingResult = ShowPingResult;
        } 
        
        #endregion

        #region Private properties

        private string PingValue
        {            
            set 
            {                
                lock (this)
                {
                    PingValueText = value;
                }                
            }

            get
            {
                lock (this)
                {
                    return PingValueText;
                } 
            }
        }

        private bool? OnLine
        {            
            set 
            {                
                lock (this)
                {
                    OnLineStatus = value;
                }                
            }

            get
            {
                lock (this)
                {
                    return OnLineStatus;
                }
            }
        }        
        
        #endregion

        #region Public methods
        
        public void ShowOnLineStatus(bool Status)
        {
            OnLine = Status;
            ThreadSetReady();
        }

        public void ShowPingValue(string PingResult)
        {
            PingValue = PingResult;
            ThreadSetReady();
        }

        #endregion

        #region Private methods

        private void ThreadSetReady()
        {
            if (PerformanceThread == null)
            {
                ThreadInitialization();
            }
            else
            { 
            }
        }

        private void ThreadInitialization()
        {
            PerformanceThread = new Thread(new ThreadStart(Start));
            PerformanceThread.IsBackground = true;
            PerformanceThread.Name = "DataDrawingThread";
            PerformanceThread.Start();
        }

        private void Start()
        {
            while (true)
            {
                if (PingValue != null)
                {
                    ShowPingResult(PingValue);
                    PingValue = null;
                }

                if (OnLine != null)
                {
                    bool OnLineValue = (bool)OnLine;
                    DrawConnectionStatus(OnLineValue);
                    SetNullToOnLine(OnLineValue);
                    //if ((bool)OnLine != OnLineValue)
                    //    System.Windows.Forms.MessageBox.Show("Пока отображался один статус, он установился другим статусом !");
                    //OnLine = null;
                }

                Thread.Sleep(100);
            }
        }

        private void SetNullToOnLine(bool OldValue)
        {
            lock (this)
            {
                if (OnLineStatus == OldValue)
                {
                    OnLineStatus = null;
                }
            }
        }

        #endregion
    }
}
