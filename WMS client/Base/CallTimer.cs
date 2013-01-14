using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WMS_client
{
    public delegate void OnEventDelegate();    

    class CallTimer
    {
        public bool Enable = true;

        private OnEventDelegate OnEvent;
        private int Delay;
        private Thread WaitingThead;        
        private long LastTime = 0;
        private bool RunOneTime;

        public CallTimer(OnEventDelegate onEvent, int DelaySec)
            : this(onEvent,DelaySec,false) { }

        public CallTimer(OnEventDelegate onEvent, int DelaySec, bool RunOneTimeOnly)
        {
            LastTime = DateTime.Now.Ticks;
            this.Delay = DelaySec;
            this.OnEvent = onEvent;
            this.RunOneTime = RunOneTimeOnly;

            WaitingThead = new Thread(new ThreadStart(OnTimer));
            WaitingThead.Name = "Timer";
            WaitingThead.IsBackground = true;
            WaitingThead.Start();
        }

        public void Stop()
        {
            WaitingThead.Abort();
        }

        private void OnTimer()
        {
            while (true)
            {
                long MSecDiff = (DateTime.Now.Ticks - LastTime) / 10000;
                if (MSecDiff <= Delay)
                {
                    Thread.Sleep(100);
                    continue;
                }
                
                OnEvent();
                if (RunOneTime) {
                    Enable = false;
                    Stop();
                }
                if (!Enable)
                {
                    Thread.Sleep(100);
                    return;
                }
                LastTime = DateTime.Now.Ticks;
            }
        }

    }
}
