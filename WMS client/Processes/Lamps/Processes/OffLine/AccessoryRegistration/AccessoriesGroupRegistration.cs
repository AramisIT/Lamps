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
    /// <summary>"��������� ��������������"</summary>
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

            MainProcess.CreateLabel("³���������", 0, 130, 240,
                MobileFontSize.Normal, MobileFontPosition.Center, MobileFontColors.Info, FontStyle.Bold);

            MainProcess.CreateLabel("�����-���!", 0, 150, 240,
                MobileFontSize.Normal, MobileFontPosition.Center, MobileFontColors.Info, FontStyle.Bold);
            }

        private void exit()
            {
            OnHotKey(KeyAction.Esc);
            }

        /// <summary>������������� �������������, ����� ������ ��������������</summary>
        /// <param name="Barcode">��������</param>
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
                        MessageBox.Show(string.Format("����� �����-��� ��� ���������������!"));
                        }
                    return;
                    }

                _case = Configuration.Current.Repository.ReadCase(barcode.GetIntegerBarcode());
                if (_case == null)
                    {
                    MessageBox.Show("������ �� ��������!");
                    return;
                    }

                lamp = Configuration.Current.Repository.ReadLamp(_case.Lamp);
                unit = Configuration.Current.Repository.ReadUnit(_case.Unit);

                if (lamp == null || unit == null)
                    {
                    MessageBox.Show("��� ������� ����� ���� ������ ����� �� ����������� ����!");
                    return;
                    }

                registrationStarted = true;
                MainProcess.ClearControls();

                MainProcess.CreateLabel("³����������:", 0, 130, 240,
                MobileFontSize.Normal, MobileFontPosition.Center, MobileFontColors.Info, FontStyle.Bold);

                barcodesQuantityLabel = MainProcess.CreateLabel("", 0, 150, 240,
                MobileFontSize.Normal, MobileFontPosition.Center, MobileFontColors.Info, FontStyle.Bold);

                MainProcess.CreateButton("��������� ���������", 10, 270, 220, 35, string.Empty, complateOperation);

                updateUserInfo();
                }
            else
                {
                ShowMessage("������� ������ ���������!");
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
                MessageBox.Show("�� ������� �������� ���");
                }
            }

        private void updateUserInfo()
            {
            barcodesQuantityLabel.Text = string.Format("{0} ��.", barcodes.Count);
            }

        public override void OnHotKey(KeyAction TypeOfAction)
            {
            switch (TypeOfAction)
                {
                case KeyAction.Esc:
                    if (barcodes.Count == 0 || ShowQuery("������ ������� ��������?"))
                        {
                        MainProcess.ClearControls();
                        MainProcess.Process = new EditSelector(MainProcess);
                        }
                    break;
                }
            }


        }




    }