using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CEForms = Microsoft.WindowsCE.Forms;

namespace WMS_client
{  
    public class HotKeyProcessing : CEForms.MessageWindow
    {
        public OnHotKeyPressedDelegate OnHotKeyPressed;
        private readonly List<int> HotKeysList = new List<int>();
        private readonly MainForm MainForm;
        private string Barcode = string.Empty;
        private long BarcodeTimeStart;

        public const int WM_HOTKEY = 0x0312;

        public HotKeyProcessing(MainForm form)
        {
            MainForm = form;

            if (!MainForm.IsTest)
            {
                for (int key = 112; key <= 135; key++)
                {
                    // Перебор кнопок с F1 (112) по F24 (135)
                    if (Enum.IsDefined(typeof (KeyAction), key))
                    {
                        SetHotKey((KeyAction) key);
                    }
                }

                SetHotKey(KeyAction.Esc);
            }
            //SetHotKey(KeyAction.No);
            //for (uint i = 0; i < 1000000; i++)
            //    RegisterHotKey(Hwnd, (int)i, (uint)65536, i);
                //SetHotKeyInt(i);

            #region Регистрация клавиш в режиме отладки
            
            bool IsDebugMode = false;
#if DEBUG
            IsDebugMode = true;
#endif
            if (IsDebugMode)
            {
                for (int key = 37; key <= 40; key++)
                {   // Перебор кнопок управления стрелками
                    if (Enum.IsDefined(typeof(KeyAction), key))
                    {
                        SetHotKey((KeyAction)key);/////////////////////////Настройка//////
                    }
                }
            }  
 
            #endregion                 

           
        }

        protected override void WndProc(ref CEForms.Message msg)
        {
            switch (msg.Msg)
            {
                case WM_HOTKEY:
                    int keyId = msg.WParam.ToInt32();

                    if (keyId == 288 || (248 <= keyId && keyId <= 257))
                    {
                        // X - 0x58 (88)
                        // 0 - 0x30 (48)
                        // 9 - 0x39 (57)                        
                        onNumeralBarcodeSymbol(Convert.ToChar(keyId-200));
                    }
                    else
                    {
                        BarcodeTimeStart = 0;
                        OnHotKeyPressed((KeyAction)keyId);
                    }
                    return;
            }
            base.WndProc(ref msg);
        }

        private void onNumeralBarcodeSymbol(char symb)
        {
            if (symb == 'X')
            {                
                Barcode = "";                
                BarcodeTimeStart = DateTime.Now.Ticks;
                return;
            }

            if (BarcodeTimeStart != 0)
            {
                long MSecDiff = (DateTime.Now.Ticks - BarcodeTimeStart) / 10000;
                if (MSecDiff < 5000)
                {
                    if (symb == ']')
                    {
                        // Barcode data transfer complated
                        BarcodeTimeStart = 0;
                        if (MobileTextBox.IsNumber(Barcode))
                        {
                            MainForm.Client.OnBarcode(Barcode);
                        }
                    }
                    else
                    {
                        Barcode += symb.ToString();
                        if (Barcode.Length == 13)
                        {
                            onNumeralBarcodeSymbol(']');
                        }
                        BarcodeTimeStart = DateTime.Now.Ticks;
                    }
                }
                else
                {
                    BarcodeTimeStart = 0;
                }
            }
        }

        [DllImport("coredll.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("coredll.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public void SetHotKey(KeyAction key)
        {
            UnregisterHotKey(Hwnd, (int)key);
            RegisterHotKey(Hwnd, (int)key, 0, (uint)key);            
            HotKeysList.Add((int)key); 
        }

        public void SetHotKeyInt(uint key)
        {
            UnregisterHotKey(Hwnd, (int)key);
            RegisterHotKey(Hwnd, (int)key, 0, key);
            HotKeysList.Add((int)key);
        }

        public void UnRegisterKeys()
        {
            foreach (int i in HotKeysList) 
            {
                UnregisterHotKey(Hwnd, i);            
            }

            for (int i = 0x30; i <= 0x39; i++)
            {
                UnregisterHotKey(Hwnd, 200+i);
            }

            UnregisterHotKey(Hwnd, 288);
            
        }        
        
    }
}
