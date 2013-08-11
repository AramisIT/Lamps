using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WMS_client.Base.Visual.Constructor;
using WMS_client.Enums;
using WMS_client.db;
using WMS_client.Models;

namespace WMS_client
    {
    /// <summary>"��������� ��������������"</summary>
    public class AccessoryRegistration : BusinessProcess
        {

        private int groupSizeValue;
        private int groupSize
            {
            get
                {
                return groupSizeValue;
                }
            set
                {
                groupSizeValue = value;
                groupSizeLabel.Text = string.Format("������������: {0} ��.", groupSizeValue);
                }
            }

        private MobileButton groupRegistrationButton;
        private bool groupRegistration;
        private MobileLabel groupSizeLabel;

        private Lamps currentLamp;
        private ElectronicUnits currentUnit;
        private Cases currentCase;

        #region Fields
        /// <summary>������� ���������</summary>
        private string currentTopic;

        /// <summary></summary>
        private readonly bool emptyBarcodeEnabled;
        /// <summary></summary>
        private readonly bool existMode;

        private MobileButton caseButton;
        private MobileButton lampButton;
        private MobileButton unitButton;
        private TypeOfAccessories currentAccessotyType;
        private TypeOfAccessories requaredAccessoryType;
        private AccessoriesSet accessoriesSet;
        private MobileButton modelButton;
        private MobileButton statusButton;
        private MobileButton partyButton;
        private MobileButton contractorButton;
        private MobileButton partyDateButton;
        private MobileButton warrantyTypeButton;
        private MobileButton warrantyExpiryDateButton;

        /// <summary>����� ������ ��� ���������� ������ (���������� ����)</summary>
        private const string okBtnText = "��";
        /// <summary>����� ������ ��� �������� ���</summary>
        private const string nextBtnText = "���";
        #endregion

        /// <summary>"��������� ��������������"</summary>
        /// <param name="MainProcess"></param>
        /// <param name="type">������� ��� ��������������</param>
        /// <param name="prevType">���������� ��� ��������������</param>
        /// <param name="topic">������� ���������</param>
        public AccessoryRegistration(WMSClient MainProcess, TypeOfAccessories requaredAccessoryType)
            : base(MainProcess, 1)
            {
            accessoriesSet = new AccessoriesSet();


            this.requaredAccessoryType = requaredAccessoryType;
            updateCurrentTopic();
            MainProcess.ToDoCommand = currentTopic;

            IsLoad = true;
            existMode = false;
            DrawControls();
            }

        private void updateCurrentTopic()
            {
            switch (requaredAccessoryType)
                {
                case TypeOfAccessories.Case:
                    currentTopic = "������";
                    break;

                case TypeOfAccessories.Lamp:
                    currentTopic = "�����";
                    break;

                case TypeOfAccessories.ElectronicUnit:
                    currentTopic = "����������� ����";
                    break;
                }
            }

        /// <summary>"��������� ��������������"</summary>
        /// <param name="MainProcess"></param>
        /// <param name="mainType">�������� (���������� ��� ���������) ��� �������������� (��� ��� � �������� ������)</param>
        /// <param name="mainTopic">������� (���������� ��� ���������) ���������</param>
        /// <param name="currentType">������� ��� �������������� (��� ��� �� ������� ������� � ���������)</param>
        /// <param name="currentTopic">������� ���������</param>
        /// <param name="accessory">�������������</param>
        /// <param name="barcode">��������������� �����-���</param>
        public AccessoryRegistration(WMSClient MainProcess, AccessoriesSet accessoriesSet)
            : base(MainProcess, 1)
            {
            this.accessoriesSet = accessoriesSet;
            requaredAccessoryType = accessoriesSet.CurrentAccessory.GetAccessoryType();
            updateCurrentTopic();

            IsLoad = true;
            existMode = true;
            showData();
            }

        #region Override methods
        public override sealed void DrawControls()
            {
            if (IsLoad)
                {
                MainProcess.ClearControls();
                MainProcess.CreateLabel("³���������", 0, 130, 240,
                                        MobileFontSize.Normal, MobileFontPosition.Center, MobileFontColors.Info, FontStyle.Bold);
                MainProcess.CreateLabel("�����-���!", 0, 150, 240,
                                        MobileFontSize.Normal, MobileFontPosition.Center, MobileFontColors.Info, FontStyle.Bold);

                if (requaredAccessoryType != TypeOfAccessories.Case)
                    {
                    MainProcess.CreateButton("��� ���������", 10, 270, 220, 35, string.Empty, () => OnBarcode(string.Empty));
                    }
                }
            }

        /// <summary>������������� �������������, ����� ������ ��������������</summary>
        /// <param name="Barcode">��������</param>
        public override sealed void OnBarcode(string barcode)
            {
            //���� ��� �����-��� ��������������
            if (barcode.IsValidBarcode())
                {
                MainProcess.ClearControls();

                if (accessoriesSet.CurrentAccessory == null || !accessoriesSet.CurrentAccessory.HasBarcode(barcode))
                    {
                    accessoriesSet.CurrentAccessory = Configuration.Current.Repository.FindAccessory(barcode.GetIntegerBarcode());
                    }

                bool accesoryIsExist = accessoriesSet.CurrentAccessory != null;

                //���� � ������� ��� ���������� ��������
                if (!existMode && accesoryIsExist)
                    {
                    currentAccessotyType = accessoriesSet.CurrentAccessory.GetAccessoryType();

                    bool isTypeLikeCurrent = currentAccessotyType == requaredAccessoryType;

                    //���� ���� �� ��������� - "�����"
                    if (!isTypeLikeCurrent)
                        {
                        ShowMessage("�������� ��� ������������ � ������ ���� ��������������!");
                        OnHotKey(KeyAction.Esc);
                        return;
                        }
                    }

                initAccessory(barcode.GetIntegerBarcode());
                showData();
                }
            //�� ���� ������ �������
            else
                {
                ShowMessage("������� ������ ���������!");
                OnHotKey(KeyAction.Esc);
                }
            }

        private void showData()
            {
            MainProcess.ToDoCommand = currentTopic;

            drawPropertiesButtons();

            drawActionButtons();
            }

        private void initAccessory(int intBarcode)
            {
            if (accessoriesSet.CurrentAccessory == null)
                {
                accessoriesSet.CurrentAccessory = AccessoryHelper.CreateNewAccessory(intBarcode, requaredAccessoryType);

                switch (requaredAccessoryType)
                    {
                    case TypeOfAccessories.Case:
                        accessoriesSet.Case = (Case)accessoriesSet.CurrentAccessory;
                        break;

                    case TypeOfAccessories.ElectronicUnit:
                        accessoriesSet.Unit = (Unit)accessoriesSet.CurrentAccessory;
                        break;

                    case TypeOfAccessories.Lamp:
                        accessoriesSet.Lamp = (Lamp)accessoriesSet.CurrentAccessory;
                        break;
                    }
                }
            }

        public override void OnHotKey(KeyAction TypeOfAction)
            {
            switch (TypeOfAction)
                {
                case KeyAction.Esc:
                    MainProcess.ClearControls();
                    MainProcess.Process = new EditSelector(MainProcess);


                    break;
                }
            }


        #endregion

        #region Draw
        /// <summary>���������� ��������� ���� ��� ��������������</summary>
        /// <param name="listOfLabels">������ ��������� ����� ��� ��������������</param>
        private void drawPropertiesButtons()
            {
            int top = 42;
            int index = 0;
            const int delta = 21;

            top += delta;
            modelButton = MainProcess.CreateButton(string.Empty, 5, top, 230, 20, "modelButton", propertyButton_Click,
                 new PropertyButtonInfo() { PropertyName = "Model", PropertyDescription = "������", PropertyType = typeof(Model) });

            top += delta;
            statusButton = MainProcess.CreateButton(string.Empty, 5, top, 230, 20, "modelButton", propertyButton_Click,
                new PropertyButtonInfo() { PropertyName = "Status", PropertyDescription = "������", PropertyType = typeof(TypesOfLampsStatus) });

            top += delta;
            partyButton = MainProcess.CreateButton(string.Empty, 5, top, 230, 20, "modelButton", propertyButton_Click,
                new PropertyButtonInfo() { PropertyName = "Party", PropertyDescription = "�����", PropertyType = typeof(PartyModel) });

            top += delta;
            contractorButton = MainProcess.CreateButton(string.Empty, 5, top, 230,
                20, string.Empty, null, null, false);

            top += delta;
            partyDateButton = MainProcess.CreateButton(string.Empty, 5, top, 230,
                20, string.Empty, null, null, false);

            top += delta;
            warrantyTypeButton = MainProcess.CreateButton(string.Empty, 5, top, 230, 20, string.Empty, propertyButton_Click,
               new PropertyButtonInfo() { PropertyName = "RepairWarranty", PropertyDescription = "��� ������", PropertyType = typeof(TypesOfLampsWarrantly) });

            top += delta;
            warrantyExpiryDateButton = MainProcess.CreateButton(string.Empty, 5, top, 230, 20, string.Empty, propertyButton_Click,
                new PropertyButtonInfo() { PropertyName = "WarrantyExpiryDate", PropertyDescription = "���������� ������", PropertyType = typeof(DateTime) });

            updatePropertiesButtonsText();
            }

        private void updatePropertiesButtonsText()
            {
            modelButton.Text = string.Format("������: {0}", accessoriesSet.CurrentAccessory.GetModelDescription());
            statusButton.Text = string.Format("������: {0}", accessoriesSet.CurrentAccessory.GetStatusDescription());
            partyButton.Text = string.Format("�����: {0}", accessoriesSet.CurrentAccessory.GetPartyDescription());
            contractorButton.Text = string.Format("����������: {0}", accessoriesSet.CurrentAccessory.GetPartyContractor());
            partyDateButton.Text = string.Format("���� ����: {0}", accessoriesSet.CurrentAccessory.GetPartyDate());
            warrantyTypeButton.Text = string.Format("��� ������: {0}", accessoriesSet.CurrentAccessory.GetWarrantyType());
            warrantyExpiryDateButton.Text = string.Format("���������� ������: {0}", accessoriesSet.CurrentAccessory.GetWarrantyExpiryDate());
            }

        private void propertyButton_Click(object sender)
            {
            var info = (PropertyButtonInfo)((sender as Button).Tag);

            MainProcess.ClearControls();
            MainProcess.Process = new ValueEditor(MainProcess, info, accessoriesSet);
            }

        /// <summary>
        /// Case, lamp, unit and OK buttons
        /// </summary>
        private void drawActionButtons()
            {
            const int top = 235;
            const int height = 35;

            caseButton = MainProcess.CreateButton("������", 5, top, 60, height, string.Empty, caseButton_click);
            unitButton = MainProcess.CreateButton("����", 70, top, 60, height, string.Empty, unitButton_click);
            lampButton = MainProcess.CreateButton("�����", 135, top, 60, height, string.Empty, lampButton_click);
            MainProcess.CreateButton("��", 200, top, 35, height, string.Empty, okButton_click);

            updateButtonsEnabling();
            }

        private void updateButtonsEnabling()
            {
            }

        private void caseButton_click()
            {
            }

        private void lampButton_click()
            {
            }

        private void unitButton_click()
            {
            }

        private void okButton_click()
            {
            }

        private void showWriteErrorMessage()
            {
            //��� ���� ��� �� �������� "����" ���������
            MessageBox.Show(
                "������������ ��� ����� ��������� �� ����!\r\n³���������� ���!",
                "����������..",
                MessageBoxButtons.OK,
                MessageBoxIcon.Asterisk,
                MessageBoxDefaultButton.Button1);
            }

        /// <summary>�� ��� ��� ��� �������?</summary>
        private bool warrantlyDataIsValid()
            {
            ////��� ��������� ������, ���� �� ���� ���� ����� �����������!
            //if ((accessory.TypeOfWarrantly == TypesOfLampsWarrantly.Without ||
            //     accessory.TypeOfWarrantly == TypesOfLampsWarrantly.None) &&
            //    accessory.DateOfWarrantyEnd > DateTime.Now)
            //    {
            //    const string message = "��� ��������� ������, ���� �� ���� ���� ����� �����������!\r\n\r\n�������� ����?";

            //    if (MessageBox.Show(message, "�� ���� �������� ���", MessageBoxButtons.YesNo,
            //                        MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            //        {
            //        accessory.DateOfWarrantyEnd = DateTime.MinValue;
            //        return true;
            //        }

            //    return false;
            //    }

            ////��� �������� ������, ���� �� ���� ���� ����� �����������!
            //if (accessory.TypeOfWarrantly != TypesOfLampsWarrantly.Without &&
            //    accessory.TypeOfWarrantly != TypesOfLampsWarrantly.None &&
            //    accessory.DateOfWarrantyEnd < DateTime.Now)
            //    {
            //    string warrantly = EnumWorker.GetDescription(typeof(TypesOfLampsWarrantly),
            //                                                 (int)accessory.TypeOfWarrantly);
            //    string message = string.Format(
            //        "��� �������� ������ '{0}', ���� �� ���� ���� ����� �����������!\r\n\r\n�������� ��� ������?",
            //        warrantly);

            //    if (MessageBox.Show(message, "�� ���� ��������� ����", MessageBoxButtons.YesNo,
            //                        MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            //        {
            //        accessory.TypeOfWarrantly = TypesOfLampsWarrantly.Without;
            //        return true;
            //        }

            //    return false;
            //    }

            return true;
            }

        #endregion


        private void startGroupRegistration()
            {
            //currentCase = accessory as Cases;

            //if (currentCase.Lamp == 0 || currentCase.ElectronicUnit == 0)
            //    {
            //    ShowMessage("����� ��������� ����� � ��. ����!");
            //    return;
            //    }

            //if (!(accessory is Cases))
            //    {
            //    return;
            //    }

            //currentLamp = new Lamps();
            //currentLamp.Read(currentCase.Lamp);

            //currentUnit = new ElectronicUnits();
            //currentUnit.Read(currentCase.ElectronicUnit);

            //if (!string.IsNullOrEmpty(currentLamp.BarCode) || !string.IsNullOrEmpty(currentUnit.BarCode))
            //    {
            //    ShowMessage("��� ������� ��������� ����� �� ���� ����� ���� ��� �����-����");
            //    return;
            //    }

            //if (isMainDataEntered && warrantlyDataIsValid())
            //    {
            //    accessory.Write();
            //    }
            //else
            //    {
            //    showWriteErrorMessage();
            //    return;
            //    }

            //groupRegistration = true;

            //currentCase = new Cases();
            //currentCase.Read(accessory.Id);

            //groupRegistrationButton.Hide();
            //groupSizeLabel = MainProcess.CreateLabel("", 5, 283, 230,
            //                            MobileFontSize.Normal, MobileFontPosition.Left, MobileFontColors.Info, FontStyle.Bold);
            //groupSize = 0;
            }

        private void groupRegistrationOnBarcode(string barcode)
            {
            bool barcodeExists = BarcodeWorker.IsBarcodeExist(barcode);

            if (barcodeExists)
                {
                ShowMessage("����� �����-��� ��� ���������������");
                return;
                }

            Lamps newLamp = currentLamp.CopyWithoutLinks() as Lamps;
            newLamp.Write();

            ElectronicUnits newElectronicUnit = currentUnit.CopyWithoutLinks() as ElectronicUnits;
            newElectronicUnit.Write();

            Cases newCase = currentCase.CopyWithoutLinks() as Cases;
            newCase.BarCode = barcode;
            newCase.Lamp = newLamp.Id;
            newCase.ElectronicUnit = newElectronicUnit.Id;
            newCase.Write();

            Movement.RegisterLighter(newCase.BarCode, newCase.SyncRef, OperationsWithLighters.Registration);

            groupSize++;
            }
        }

    public class PropertyButtonInfo
        {
        public string PropertyName { get; set; }
        public string PropertyDescription { get; set; }
        public Type PropertyType { get; set; }
        }

    public class AccessoriesSet
        {
        public Case Case { get; set; }
        public Lamp Lamp { get; set; }
        public Unit Unit { get; set; }

        public IAccessory CurrentAccessory { get; set; }
        }
    }