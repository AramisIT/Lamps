using System;
using System.Collections.Generic;
using System.Text;
using System.Data;


namespace WMS_client
{
    public class EmptyProcess : BusinessProcess
    {
        private CallTimer timer;

        public EmptyProcess(WMSClient MainProcess)
            : base(MainProcess, 1)
        {
            BusinessProcessType = ProcessType.Waiting;
        }

        private void CheckTaskAvailable()
        {
            lock (this)
            {
                if (MainProcess.NeedToUpdate)
                {
                    MainProcess.MainForm.PerformMainThreadEvent();
                    return;
                }
                PerformQuery("CheckTaskAvailable");

                if (ResultParameters == null || ResultParameters.Length == 1) return;

                //timer.Stop();
                timer.Enable = false;
                MainProcess.MainForm.PerformMainThreadEvent();
            }
        }

        public void GetTask()
        {
            ProcessType BPType;
            try
            {
                BPType = (ProcessType)ResultParameters[0];
            }
            catch
            {
                // ��� ��� ���� ����� �� ������ ������, �������� �� ��������� ������
                timer.Enable = true;
                return;
            }

            timer.Stop();
            MainProcess.ClearControls();

            if (MainProcess.NeedToUpdate)
            {
                MainProcess.Updating();
                return;
            }



            switch (BPType)
            {
                case ProcessType.Incoming:
                    {
                        //MainProcess.Process = new IncomingProcess(MainProcess, Parameters);
                        break;
                    }
                
                case ProcessType.Selecting:
                    {
                        //MainProcess.Process = new SelectingProcess(MainProcess, Parameters);
                        break;
                    }
                
            }

        }

        public override void Start()
        {
            base.Start();
            MainProcess.MainForm.PerformInMainThread = GetTask;
            //ShowMessage("6");
            //MainProcess.MainForm.Focus();
            ////ShowMessage("7");
            ShowMessage("�������� ��������� �������!");
            timer = new CallTimer(CheckTaskAvailable, 1000);
        }

        public override void DrawControls()
        {
        }

        public override void OnBarcode(string Barcode)
        {
            lock (this)
            {
                if (!timer.Enable)
                {
                    // ������, ��� ��� �������� ������ �������
                    return;
                }

                PerformQuery("Activation", 10, Barcode);
                if (ResultParameters == null) return;
                try
                {
                    if ((bool)ResultParameters[0])
                    {
                        MainProcess.ToDoCommand = "���������";
                    }
                }
                catch
                {
                    /* ����� ����� ������������� ����� CheckTaskAvailable */
                }
            }
        }

        //public override void SetHotKeys()
        //{
        //    SetFormHotKeys(KeyAction.Exit);
        //}

        public override void OnHotKey(KeyAction TypeOfAction)
        {
            switch (TypeOfAction)
            {
                case KeyAction.Esc:
                    {
                        timer.Stop();
                        MainProcess.SendToServer("UserGetOut");
                        MainProcess.Welcome();
                        if (MainProcess.NeedToUpdate)
                        {
                            MainProcess.TryToUpdate();
                        }
                        break;
                    }
            }
        }
    }
}
