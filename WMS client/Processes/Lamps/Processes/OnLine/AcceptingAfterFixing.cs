using System;
using System.Data.SqlServerCe;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using WMS_client.Enums;
using WMS_client.db;

namespace WMS_client.Processes.Lamps
    {
    public class AcceptingAfterFixing : BusinessProcess
        {
        public const string START_ACCEPTING_AFTER_FIXING_BARCODE = "SB_ACPT_F_FIX.";

        private MobileTextBox dayTextBox;
        private MobileTextBox monthTextBox;
        private MobileTextBox yearTextBox;

        private MobileTextBox warrantyYearsQuantityTextBox;

        private MobileLabel acceptedLabel;
        private MobileLabel lastBarcodeLabel;
        private long documentId;
        private long contractorId;

        public AcceptingAfterFixing(WMSClient wmsClient)
            : base(wmsClient, 1)
            {

            }

        public override void DrawControls()
            {
            MainProcess.ToDoCommand = "Приймання з ремонту";

            switch (FormNumber)
                {
                case 1:
                    DrawForm1Controls();
                    break;

                case 2:
                    DrawForm2Controls();
                    break;
                }

            }

        private void DrawForm2Controls()
            {
            MainProcess.CreateLabel("Прийнято шт.:", 8, 70, 136, ControlsStyle.LabelNormal);

            acceptedLabel = (MobileLabel)MainProcess.CreateLabel("0", 144, 70, 60, ControlsStyle.LabelNormal);

            lastBarcodeLabel = (MobileLabel)MainProcess.CreateLabel("", 20, 150, 150, ControlsStyle.LabelLarge);
            }

        private void DrawForm1Controls()
            {
            MainProcess.CreateLabel("Дата накладной:", 8, 70, 160, ControlsStyle.LabelNormal);
            MainProcess.CreateLabel(".", 57, 95, 8, ControlsStyle.LabelNormal);
            MainProcess.CreateLabel(".", 118, 95, 8, ControlsStyle.LabelNormal);

            dayTextBox = (MobileTextBox)MainProcess.CreateTextBox(9, 95, 41, string.Empty, ControlsStyle.LabelLarge, false);
            monthTextBox = (MobileTextBox)MainProcess.CreateTextBox(71, 95, 41, string.Empty, ControlsStyle.LabelLarge, false);
            monthTextBox.Text = DateTime.Now.Month.ToString();
            yearTextBox = (MobileTextBox)MainProcess.CreateTextBox(132, 95, 71, string.Empty, ControlsStyle.LabelLarge, false);
            yearTextBox.Text = DateTime.Now.Year.ToString();

            MainProcess.CreateLabel("Количество лет гарантии:", 8, 175, 224, ControlsStyle.LabelNormal);
            warrantyYearsQuantityTextBox = (MobileTextBox)MainProcess.CreateTextBox(9, 207, 47, string.Empty, ControlsStyle.LabelLarge, false);
            warrantyYearsQuantityTextBox.Text = "0";

            MainProcess.CreateButton("Створити документ", 9, 250, 225, 40, string.Empty, startProcess);
            }

        private void startProcess()
            {
            if (createDocumentOnServer())
                {
                ClearControls();
                FormNumber = 2;
                DrawControls();
                }
            }

        private bool createDocumentOnServer()
            {
            if (!createDocumentRemotely())
                {
                return false;
                }

            documentId = Convert.ToInt64(ResultParameters[1]);

            return true;
            }

        private bool createDocumentRemotely()
            {
            DateTime invoiceDate;
            try
                {
                invoiceDate = new DateTime(Convert.ToInt32(yearTextBox.Text), Convert.ToInt32(monthTextBox.Text),
                                           Convert.ToInt32(dayTextBox.Text));
                }
            catch (Exception)
                {
                ShowMessage("Невірно введена дата накладної!");
                return false;
                }

            if (string.IsNullOrEmpty(warrantyYearsQuantityTextBox.Text))
                {
                warrantyYearsQuantityTextBox.Text = "0";
                }

            double yearsWarranty;
            try
                {
                yearsWarranty = Convert.ToDouble(warrantyYearsQuantityTextBox.Text);
                }
            catch (Exception)
                {
                ShowMessage(string.Format("Невірно введено кількість років гарантії!. Допустимі значеняя: 0, 1, {0} і т.д.",
                                          ((double)(1.5)).ToString()));
                return false;
                }

            if (!OnLine)
                {
                return false;
                }

            PerformQuery("CreateAcceptanceAfterFixing", invoiceDate.ToString("o"), yearsWarranty, contractorId);
            return SuccessQueryResult;
            }

        private const string CONTRACTOR_PREFIX = "SB_CONTR.";
        private bool tryGetContractorBarcode(string barcode)
            {
            if (string.IsNullOrEmpty(barcode) || barcode.Length <= CONTRACTOR_PREFIX.Length) return false;

            if (barcode.Substring(0, CONTRACTOR_PREFIX.Length).Equals(CONTRACTOR_PREFIX))
                {
                try
                    {
                    contractorId = Convert.ToInt64(barcode.Substring(CONTRACTOR_PREFIX.Length));
                    return contractorId > 0;
                    }
                catch
                    {
                    }
                }
            return false;
            }

        public override void OnBarcode(string barcode)
            {
            if (barcode.Equals(START_ACCEPTING_AFTER_FIXING_BARCODE))
                {
                leaveProcess();
                }
            else if (FormNumber == 1)
                {
                if (tryGetContractorBarcode(barcode))
                    {
                    PerformQuery("GetContractorName", contractorId);

                    if (SuccessQueryResult)
                        {
                        ShowMessage(ResultParameters[1] as string);
                        }
                    }
                }
            else if (FormNumber == 2)
                {
                if (barcode.IsValidBarcode())
                    {
                    if (tryAddDocumentRow(barcode))
                        {
                        incAcceptedUnits();
                        }
                    lastBarcodeLabel.Text = barcode;
                    }
                }
            }

        private bool tryAddDocumentRow(string barcode)
            {
            long code;
            try
                {
                code = Convert.ToInt64(barcode.Substring(1));
                }
            catch
                {
                return false;
                }
            if (acceptedRows.ContainsKey(code))
                {
                return false;
                }
           
            if (correctAccessoryStatus(barcode) && addToDocumentRemotely(barcode))
                {
                Accessory.SetState(TypeOfAccessories.ElectronicUnit, TypesOfLampsStatus.Storage, barcode);
                acceptedRows.Add(code, true);
                return true;
                }
            else
                {
                return false;
                }
            }

        private bool addToDocumentRemotely(string barcode)
            {
            if (!OnLine) return false;
            PerformQuery("AddAcceptedElectronicUnitFromFixing", documentId, barcode);
            return SuccessQueryResult;
            }

        private bool correctAccessoryStatus(string barcode)
            {
            TypesOfLampsStatus state = Accessory.GetState(TypeOfAccessories.ElectronicUnit, barcode);
            if (state == TypesOfLampsStatus.ToRepair)
                {
                return true;
                }
            else
                {
                ShowMessage(@"Комплектуюча має бути у статусі ""На ремонті""!");
                return false;
                }
            }

        private SortedList<long, bool> acceptedRows = new SortedList<long, bool>();

        private void incAcceptedUnits()
            {
            acceptedLabel.Text = (Convert.ToInt32(acceptedLabel.Text) + 1).ToString();
            }

        private void leaveProcess()
            {
            if (complateProcess())
                {
                ClearControls();
                MainProcess.Process = new SelectingLampProcess(MainProcess);
                }
            }

        public override void OnHotKey(KeyAction key)
            {
            switch (key)
                {
                case KeyAction.Esc:
                    leaveProcess();
                    break;
                }
            }

        private bool complateProcess()
            {
            switch (FormNumber)
                {
                case 1:
                    return true;
                default:
                    return complateAccepting();
                }
            }

        private bool complateAccepting()
            {
            if (!ShowQuery("Завершити приймання?"))
                {
                return false;
                }

            return true;
            }
        }
    }
