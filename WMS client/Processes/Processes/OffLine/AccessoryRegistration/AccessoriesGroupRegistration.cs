using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.WindowsMobile.Status;
using WMS_client.Base.Visual.Constructor;
using WMS_client.Enums;
using WMS_client.db;
using WMS_client.Models;
using WMS_client.Processes;
using WMS_client.Repositories;
using WMS_client.Utils;

namespace WMS_client
    {
    /// <summary>"��������� ��������������"</summary>
    public class AccessoriesGroupRegistration : BusinessProcess
        {
        private Case _Case;
        private Lamp lamp;
        private Unit unit;
        private List<int> barcodes = new List<int>();
        private bool registrationStarted;
        private MobileLabel barcodesQuantityLabel;

        public AccessoriesGroupRegistration()
            : base( 1)
            {
            if (applicationIsClosing)
                {
                return;
                }
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

                _Case = Configuration.Current.Repository.ReadCase(barcode.GetIntegerBarcode());
                if (_Case == null)
                    {
                    MessageBox.Show("������ �� ��������!");
                    return;
                    }

                lamp = Configuration.Current.Repository.ReadLamp(_Case.Lamp);
                unit = Configuration.Current.Repository.ReadUnit(_Case.Unit);

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
                "������� ������ ���������!".ShowMessage();
                exit();
                }
            }

        private void complateOperation()
            {
            if (SaveGroupOfSets())
                {
                barcodes.Clear();
                exit();
                }
            else
                {
                MessageBox.Show("�� ������� �������� ���");
                }
            }

        public bool SaveGroupOfSets()
            {
            if (BatteryChargeStatus.Low)
                {
                MessageBox.Show("���������� �����������. ������� �������� ������� �� ������� �� �������� ���!");
                return false;
                }

            IRepository repository = Configuration.Current.Repository;

            List<Unit> units = new List<Unit>();
            List<Lamp> lamps = new List<Lamp>();
            List<Case> cases = new List<Case>();

            foreach (int barcode in barcodes)
                {
                var newLamp = lamp.Copy<Lamp>();
                newLamp.Id = repository.GetNextLampId();
                lamps.Add(newLamp);

                var newUnit = unit.Copy<Unit>();
                newUnit.Id = repository.GetNextUnitId();
                units.Add(newUnit);

                var newCase = _Case.Copy<Case>();
                newCase.Id = barcode;
                newCase.Lamp = newLamp.Id;
                newCase.Unit = newUnit.Id;
                cases.Add(newCase);
                }

            return repository.UpdateUnits(units, true) && repository.UpdateLamps(lamps, true) && repository.UpdateCases(cases, true);
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
                    if (barcodes.Count == 0 || "������ ������� ��������?".Ask())
                        {
                        MainProcess.ClearControls();
                        MainProcess.Process = new EditSelector();
                        }
                    break;
                }
            }


        }




    }