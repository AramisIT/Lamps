using System;
using System.Collections.Generic;
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
    public class AccessoryRegistration : BusinessProcess
        {
        #region Fields

        /// <summary></summary>
        private readonly bool emptyBarcodeEnabled;
        /// <summary></summary>
        private readonly bool existMode;

        private AccessoriesSet accessoriesSet;
        private TypeOfAccessories requaredAccessoryType;

        private MobileButton caseButton;
        private MobileButton lampButton;
        private MobileButton unitButton;

        private MobileButton modelButton;
        private MobileButton statusButton;
        private MobileButton partyButton;
        private MobileButton contractorButton;
        private MobileButton partyDateButton;
        private MobileButton warrantyTypeButton;
        private MobileButton warrantyExpiryDateButton;
        private bool newAccesory;
        private bool registrationAdditionalAccessory;
        private bool waitingForBarcode;

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
            StopNetworkConnection();

            accessoriesSet = new AccessoriesSet();


            this.requaredAccessoryType = requaredAccessoryType;
            updateCurrentTopic();

            IsLoad = true;
            existMode = false;
            DrawControls();
            }

        private void updateCurrentTopic()
            {
            switch (requaredAccessoryType)
                {
                case TypeOfAccessories.Case:
                    MainProcess.ToDoCommand = "������";
                    break;

                case TypeOfAccessories.Lamp:
                    MainProcess.ToDoCommand = "�����";
                    break;

                case TypeOfAccessories.ElectronicUnit:
                    MainProcess.ToDoCommand = "����������� ����";
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
                prepareForBarcode(requaredAccessoryType);
                }
            }

        private void prepareForBarcode(TypeOfAccessories typeOfAccessories)
            {
            requaredAccessoryType = typeOfAccessories;
            MainProcess.ClearControls();
            MainProcess.CreateLabel("³���������", 0, 130, 240,
                MobileFontSize.Normal, MobileFontPosition.Center, MobileFontColors.Info, FontStyle.Bold);
            MainProcess.CreateLabel("�����-���!", 0, 150, 240,
                MobileFontSize.Normal, MobileFontPosition.Center, MobileFontColors.Info, FontStyle.Bold);

            if (requaredAccessoryType != TypeOfAccessories.Case)
                {
                MainProcess.CreateButton("��� �����-����", 10, 270, 220, 35, string.Empty, () => OnBarcode(string.Empty));
                }

            waitingForBarcode = true;
            }

        /// <summary>������������� �������������, ����� ������ ��������������</summary>
        /// <param name="Barcode">��������</param>
        public override sealed void OnBarcode(string barcode)
            {
            if (!waitingForBarcode)
                {
                return;
                }

            if (barcode.IsAccessoryBarcode())
                {
                MainProcess.ClearControls();

                registrationAdditionalAccessory = accessoriesSet.CurrentAccessory != null;
                accessoriesSet.CurrentAccessory = Configuration.Current.Repository.FindAccessory(barcode.GetIntegerBarcode());

                newAccesory = accessoriesSet.CurrentAccessory == null;

                //���� � ������� ��� ���������� ��������
                if (!existMode && !newAccesory)
                    {
                    if (requaredAccessoryType != accessoriesSet.CurrentAccessory.GetAccessoryType())
                        {
                        ShowMessage("�������� ��� ������������ � ������ ���� ��������������!");
                        exit();
                        return;
                        }
                    }
                waitingForBarcode = false;
                initAccessoriesSet(barcode.GetIntegerBarcode());
                showData();
                }
            else
                {
                ShowMessage("������� ������ ���������!");
                exit();
                }
            }

        private void showData()
            {
            drawPropertiesButtons();

            drawActionButtons();

            updateCurrentTopic();
            }

        private void initAccessoriesSet(int intBarcode)
            {
            if (accessoriesSet.CurrentAccessory == null)
                {
                accessoriesSet.CurrentAccessory = AccessoryHelper.CreateNewAccessory(intBarcode, requaredAccessoryType);
                }

            switch (requaredAccessoryType)
                {
                case TypeOfAccessories.Case:
                    accessoriesSet.Case = (Case)accessoriesSet.CurrentAccessory;
                    accessoriesSet.Lamp = accessoriesSet.Lamp ?? Configuration.Current.Repository.ReadLamp(accessoriesSet.Case.Lamp);
                    accessoriesSet.Unit = accessoriesSet.Unit ?? Configuration.Current.Repository.ReadUnit(accessoriesSet.Case.Unit);
                    break;

                case TypeOfAccessories.ElectronicUnit:
                    accessoriesSet.Unit = (Unit)accessoriesSet.CurrentAccessory;
                    accessoriesSet.Unit.Barcode = intBarcode;

                    if (!newAccesory)
                        {
                        accessoriesSet.Case = accessoriesSet.Case ?? Configuration.Current.Repository.FintCaseByUnit(accessoriesSet.Unit.Id);
                        if (accessoriesSet.Case != null)
                            {
                            accessoriesSet.Lamp = accessoriesSet.Lamp ?? Configuration.Current.Repository.ReadLamp(accessoriesSet.Case.Lamp);
                            }
                        }
                    break;

                case TypeOfAccessories.Lamp:
                    accessoriesSet.Lamp = (Lamp)accessoriesSet.CurrentAccessory;
                    accessoriesSet.Lamp.Barcode = intBarcode;

                    if (!newAccesory)
                        {
                        accessoriesSet.Case = accessoriesSet.Case ?? Configuration.Current.Repository.FintCaseByLamp(accessoriesSet.Lamp.Id);
                        if (accessoriesSet.Case != null)
                            {
                            accessoriesSet.Unit = accessoriesSet.Unit ?? Configuration.Current.Repository.ReadUnit(accessoriesSet.Case.Unit);
                            }
                        }
                    break;
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
            bool isCase = requaredAccessoryType == TypeOfAccessories.Case;

            caseButton.Enabled = accessoriesSet.Case != null && !isCase;

            lampButton.Enabled = (accessoriesSet.Lamp != null || isCase || caseButton.Enabled) && requaredAccessoryType != TypeOfAccessories.Lamp;
            unitButton.Enabled = (accessoriesSet.Unit != null || isCase || caseButton.Enabled) && requaredAccessoryType != TypeOfAccessories.ElectronicUnit;
            }

        private void caseButton_click()
            {
            setNewCurrentAccessory(accessoriesSet.Case);
            }

        private void unitButton_click()
            {
            if (accessoriesSet.Unit == null)
                {
                prepareForBarcode(TypeOfAccessories.ElectronicUnit);
                }
            else
                {
                setNewCurrentAccessory(accessoriesSet.Unit);
                }
            }

        private void lampButton_click()
            {
            if (accessoriesSet.Lamp == null)
                {
                prepareForBarcode(TypeOfAccessories.Lamp);
                }
            else
                {
                setNewCurrentAccessory(accessoriesSet.Lamp);
                }
            }

        private void setNewCurrentAccessory(IAccessory accessory)
            {
            requaredAccessoryType = accessory.GetAccessoryType();
            updateCurrentTopic();
            accessoriesSet.CurrentAccessory = accessory;
            updatePropertiesButtonsText();
            updateButtonsEnabling();
            }

        private void okButton_click()
            {
            if (
                !Configuration.Current.Repository.SaveAccessoriesSet(accessoriesSet.Case, accessoriesSet.Lamp,
                    accessoriesSet.Unit))
                {
                MessageBox.Show("�� ������� �������� ���������");
                return;
                }

            exit();
            }

        private void exit()
            {
            OnHotKey(KeyAction.Esc);
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

        }




    }