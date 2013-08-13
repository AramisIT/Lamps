using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using WMS_client.Base.Visual.Constructor;
using WMS_client.Enums;
using WMS_client.db;
using WMS_client.Models;
using WMS_client.Processes;

namespace WMS_client
    {
    /// <summary>"Строитель редактирования"</summary>
    public class AccessoriesGroupRegistration : BusinessProcess
        {
        private Case _case;
        private Lamp lamp;
        private Unit unit;
        private List<int> barcodes = new List<int>();
        private bool registrationStarted;
        private MobileLabel barcodesQuantityLabel;

        public AccessoriesGroupRegistration(WMSClient MainProcess)
            : base(MainProcess, 1)
            {
            StopNetworkConnection();
            }

        public override sealed void DrawControls()
            {
            MainProcess.ClearControls();

            MainProcess.CreateLabel("Відскануйте", 0, 130, 240,
                MobileFontSize.Normal, MobileFontPosition.Center, MobileFontColors.Info, FontStyle.Bold);

            MainProcess.CreateLabel("ШТРИХ-КОД!", 0, 150, 240,
                MobileFontSize.Normal, MobileFontPosition.Center, MobileFontColors.Info, FontStyle.Bold);
            }

        private void exit()
            {
            OnHotKey(KeyAction.Esc);
            }

        /// <summary>Комплектующее отсканировано, нужно начать редактирование</summary>
        /// <param name="Barcode">ШтрихКод</param>
        public override sealed void OnBarcode(string barcode)
            {
            if (barcode.IsAccessoryBarcode())
                {
                if (registrationStarted)
                    {
                    int caseId = barcode.GetIntegerBarcode();
                    var existsCase = Configuration.Current.Repository.ReadCase(caseId);

                    if (existsCase == null && !barcodes.Contains(caseId))
                        {
                        barcodes.Add(caseId);
                        updateUserInfo();
                        }
                    else
                        {
                        MessageBox.Show(string.Format("Такий штрих-код вже використовується!"));
                        }
                    return;
                    }

                _case = Configuration.Current.Repository.ReadCase(barcode.GetIntegerBarcode());
                if (_case == null)
                    {
                    MessageBox.Show("Корпус не знайдено!");
                    return;
                    }

                lamp = Configuration.Current.Repository.ReadLamp(_case.Lamp);
                unit = Configuration.Current.Repository.ReadUnit(_case.Unit);

                if (lamp == null || unit == null)
                    {
                    MessageBox.Show("Для корпуса мають бути вказані лампа та електронний блок!");
                    return;
                    }

                registrationStarted = true;
                MainProcess.ClearControls();

                MainProcess.CreateLabel("Відскановано:", 0, 130, 240,
                MobileFontSize.Normal, MobileFontPosition.Center, MobileFontColors.Info, FontStyle.Bold);

                barcodesQuantityLabel = MainProcess.CreateLabel("", 0, 150, 240,
                MobileFontSize.Normal, MobileFontPosition.Center, MobileFontColors.Info, FontStyle.Bold);

                MainProcess.CreateButton("Завершити реєстрацію", 10, 270, 220, 35, string.Empty, complateOperation);

                updateUserInfo();
                }
            else
                {
                ShowMessage("Невірний формат штрихкоду!");
                exit();
                }
            }

        private void complateOperation()
            {
            if (Configuration.Current.Repository.SaveGroupOfSets(_case, lamp, unit, barcodes))
                {
                barcodes.Clear();
                exit();
                }
            else
                {
                MessageBox.Show("Не вдалося зберегти дані");
                }
            }

        private void updateUserInfo()
            {
            barcodesQuantityLabel.Text = string.Format("{0} шт.", barcodes.Count);
            }

        public override void OnHotKey(KeyAction TypeOfAction)
            {
            switch (TypeOfAction)
                {
                case KeyAction.Esc:
                    if (barcodes.Count == 0 || ShowQuery("Бажаєти відмінити операцію?"))
                        {
                        MainProcess.ClearControls();
                        MainProcess.Process = new EditSelector(MainProcess);
                        }
                    break;
                }
            }


        }




    }