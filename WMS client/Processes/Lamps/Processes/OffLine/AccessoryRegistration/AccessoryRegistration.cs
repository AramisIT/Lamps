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
        private MobileLabel contractorLabel;
        private MobileLabel partyDateLabel;
        private MobileLabel warrantyTypeLabel;
        private MobileLabel warrantyExpiryDateLabel;
        private bool newAccesory;
        private bool registrationAdditionalAccessory;
        private bool waitingForBarcode;
        private MobileLabel positionLabel;
        private Unit newUnit;

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
            if (applicationIsClosing)
                {
                return;
                }
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
            if (applicationIsClosing)
                {
                return;
                }
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
            if (newUnit != null)
                {
                var foundAccessory = Configuration.Current.Repository.FindAccessory(barcode.GetIntegerBarcode());
                if (foundAccessory != null)
                    {
                    "��������������� �����-��� ��� ������������! �������� �����.".Warning();
                    return;
                    }
                replaceUnit(newUnit, barcode.GetIntegerBarcode());
                newUnit = null;
                return;
                }

            if (!waitingForBarcode)
                {
                tryReplaceOrRemoveAccessory(barcode);
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

        private void tryReplaceOrRemoveAccessory(string barcode)
            {
            if (requaredAccessoryType == TypeOfAccessories.Case)
                {
                return;
                }

            var foundAccessory = Configuration.Current.Repository.FindAccessory(barcode.GetIntegerBarcode());
            var accessoryType = AccessoryHelper.GetAccessoryType(foundAccessory);

            if (requaredAccessoryType != accessoryType && foundAccessory != null)
                {
                return;
                }

            if (foundAccessory != null)
                {
                switch (accessoryType)
                    {
                    case TypeOfAccessories.ElectronicUnit:
                        if (accessoriesSet.Unit == null)
                            {
                            return;
                            }

                        if (!accessoriesSet.Unit.Id.Equals(foundAccessory.Id))
                            {
                            if (ShowQuery("�������� ����?"))
                                {
                                if (accessoriesSet.Unit.Barcode > 0)
                                    {
                                    replaceUnit(foundAccessory as Unit, accessoriesSet.Unit.Barcode);
                                    }
                                else
                                    {
                                    newUnit = foundAccessory as Unit;
                                    "�������� �����-��� �� ������ ����. ������������ ���".ShowMessage();
                                    }
                                }
                            }
                        else
                            {
                            tryRemoveUnit(barcode);
                            }
                        break;

                    case TypeOfAccessories.Lamp:
                        if (accessoriesSet.Lamp == null)
                            {
                            return;
                            }

                        if (!accessoriesSet.Lamp.Id.Equals(foundAccessory.Id))
                            {
                            if (ShowQuery("�������� �����?"))
                                {
                                replaceLamp(foundAccessory as Lamp);
                                }
                            }
                        break;
                    }
                return;
                }

            if (!tryRemoveUnit(barcode))
                {
                tryRemoveLamp(barcode);
                }
            }

        private void tryRemoveLamp(string barcode)
            {
            if (!ShowQuery("������� �����?"))
                {
                return;
                }
            accessoriesSet.Lamp.Barcode = barcode.GetIntegerBarcode();
            if (!Configuration.Current.Repository.WriteLamp(accessoriesSet.Lamp))
                {
                ShowMessage("�� ������� ��������� �����-��� ������ �����");
                return;
                }

            accessoriesSet.Lamp = null;
            saveAccessoriesSet();
            setNewCurrentAccessory(accessoriesSet.Case);
            }

        private bool tryRemoveUnit(string barcode)
            {
            if (!ShowQuery("������� ����������� ����?"))
                {
                return false;
                }

            if (accessoriesSet.Unit.Barcode != barcode.GetIntegerBarcode())
                {
                accessoriesSet.Unit.Barcode = barcode.GetIntegerBarcode();
                if (!Configuration.Current.Repository.WriteUnit(accessoriesSet.Unit))
                    {
                    ShowMessage("�� ������� ��������� �����-��� ������� �����");
                    return false;
                    }
                }

            accessoriesSet.Unit = null;
            saveAccessoriesSet();
            setNewCurrentAccessory(accessoriesSet.Case);
            return true;
            }

        private void replaceLamp(Lamp lamp)
            {
            accessoriesSet.Lamp.Barcode = lamp.Barcode;

            if (!Configuration.Current.Repository.WriteLamp(accessoriesSet.Lamp))
                {
                ShowMessage("�� ������� ��������� �����-��� ������ �����");
                return;
                }

            lamp.Barcode = 0;
            accessoriesSet.Lamp = lamp;
            saveAccessoriesSet();
            setNewCurrentAccessory(accessoriesSet.Lamp);
            }

        private void replaceUnit(Unit newUnit, int barcodeOfOldUnit)
            {
            var newBarcodeGlued = accessoriesSet.Unit.Barcode != barcodeOfOldUnit;
            if (newBarcodeGlued)
                {
                accessoriesSet.Unit.Barcode = barcodeOfOldUnit;

                if (!Configuration.Current.Repository.WriteUnit(accessoriesSet.Unit))
                    {
                    ShowMessage("�� ������� ��������� �����-��� ������� �����");
                    return;
                    }
                }

            accessoriesSet.Unit = newUnit;
            saveAccessoriesSet();
            setNewCurrentAccessory(accessoriesSet.Unit);
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
            contractorLabel = MainProcess.CreateLabel(string.Empty, 5, top, 230,
               MobileFontSize.Normal, MobileFontPosition.Left, MobileFontColors.Info, FontStyle.Bold);

            top += delta;
            partyDateLabel = MainProcess.CreateLabel(string.Empty, 5, top, 230,
                MobileFontSize.Normal, MobileFontPosition.Left, MobileFontColors.Info, FontStyle.Bold);

            top += delta;
            warrantyTypeLabel = MainProcess.CreateLabel(string.Empty, 5, top, 230,
                MobileFontSize.Normal, MobileFontPosition.Left, MobileFontColors.Info, FontStyle.Bold);

            top += delta;
            warrantyExpiryDateLabel = MainProcess.CreateLabel(string.Empty, 5, top, 230,
                MobileFontSize.Normal, MobileFontPosition.Left, MobileFontColors.Info, FontStyle.Bold);

            top += delta;

            positionLabel = MainProcess.CreateLabel("", 5, top + 5, 230, ControlsStyle.LabelSmall);
            updatePropertiesButtonsText();
            }

        private void updatePropertiesButtonsText()
            {
            modelButton.Text = string.Format("������: {0}", accessoriesSet.CurrentAccessory.GetModelDescription());
            statusButton.Text = string.Format("������: {0}", accessoriesSet.CurrentAccessory.GetStatusDescription());
            partyButton.Text = string.Format("�����: {0}", accessoriesSet.CurrentAccessory.GetPartyDescription());
            contractorLabel.Text = string.Format("����������: {0}", accessoriesSet.CurrentAccessory.GetPartyContractor());
            partyDateLabel.Text = string.Format("���� ����: {0}", accessoriesSet.CurrentAccessory.GetPartyDate());
            warrantyTypeLabel.Text = string.Format("��� ������: {0}", accessoriesSet.CurrentAccessory.GetWarrantyType());
            warrantyExpiryDateLabel.Text = string.Format("���. ���-�: {0}", accessoriesSet.CurrentAccessory.GetWarrantyExpiryDate());

            if (accessoriesSet.Case != null)
                {
                string position = string.Empty;
                if (accessoriesSet.CurrentAccessory is Case)
                    {
                    if (accessoriesSet.Case.Map > 0)
                        {
                        position = string.Format("����� {0}; ������: {1}; �������: {2}",
                            accessoriesSet.Case.GetMapDescription(),
                            accessoriesSet.Case.Register, accessoriesSet.Case.Position);
                        }
                    else
                        {
                        position = "�� ������������ �� ����";
                        }
                    }
                positionLabel.Text = position;
                }
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
            const int top = 280;
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
            if (!checkModels())
                {
                MessageBox.Show("��������� ��������� �����");
                return;
                }

            if (
                !saveAccessoriesSet())
                {
                MessageBox.Show("�� ������� �������� ���������");
                return;
                }

            exit();
            }

        private bool saveAccessoriesSet()
            {
            Case _Case = accessoriesSet.Case;
            Lamp lamp = accessoriesSet.Lamp;
            Unit unit = accessoriesSet.Unit;
            bool ok = true;

            var repository = Configuration.Current.Repository;

            if (unit != null)
                {
                if (unit.Id <= 0)
                    {
                    unit.Id = repository.GetNextUnitId();
                    ok = ok && repository.UpdateUnits(new List<Unit>() { unit }, true);
                    }
                else
                    {
                    ok = ok && repository.UpdateUnits(new List<Unit>() { unit }, false);
                    }
                }
            if (!ok)
                {
                return false;
                }


            if (lamp != null)
                {
                if (lamp.Id <= 0)
                    {
                    lamp.Id = repository.GetNextLampId();
                    ok = ok && repository.UpdateLamps(new List<Lamp>() { lamp }, true);
                    }
                else
                    {
                    ok = ok && repository.UpdateLamps(new List<Lamp>() { lamp }, false);
                    }
                }
            if (!ok)
                {
                return false;
                }


            if (_Case != null)
                {
                _Case.Lamp = lamp == null ? 0 : lamp.Id;
                _Case.Unit = unit == null ? 0 : unit.Id;
                ok = ok && repository.UpdateCases(new List<Case>() { _Case }, false);
                }

            return ok;
            }


        private bool checkModels()
            {
            if (accessoriesSet.Case != null && accessoriesSet.Case.Model <= 0)
                {
                return false;
                }

            if (accessoriesSet.Lamp != null && accessoriesSet.Lamp.Model <= 0)
                {
                return false;
                }

            if (accessoriesSet.Unit != null && accessoriesSet.Unit.Model <= 0)
                {
                return false;
                }

            return true;
            }

        private void exit()
            {
            OnHotKey(KeyAction.Esc);
            }

        /// <summary>�� ���� ���� ��� �������?</summary>
        private bool warrantlyDataIsValid()
            {
            ////��� ��������� ������, ���� �� ���� ���� ����� ������������!
            //if ((accessory.TypeOfWarrantly == TypesOfLampsWarrantly.Without ||
            //     accessory.TypeOfWarrantly == TypesOfLampsWarrantly.None) &&
            //    accessory.DateOfWarrantyEnd > DateTime.Now)
            //    {
            //    const string message = "��� ��������� ������, ���� �� ���� ���� ����� ������������!\r\n\r\n�������� ����?";

            //    if (MessageBox.Show(message, "�� ���� ��������� ����", MessageBoxButtons.YesNo,
            //                        MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            //        {
            //        accessory.DateOfWarrantyEnd = DateTime.MinValue;
            //        return true;
            //        }

            //    return false;
            //    }

            ////��� �������� ������, ���� �� ���� ���� ����� ������������!
            //if (accessory.TypeOfWarrantly != TypesOfLampsWarrantly.Without &&
            //    accessory.TypeOfWarrantly != TypesOfLampsWarrantly.None &&
            //    accessory.DateOfWarrantyEnd < DateTime.Now)
            //    {
            //    string warrantly = EnumWorker.GetDescription(typeof(TypesOfLampsWarrantly),
            //                                                 (int)accessory.TypeOfWarrantly);
            //    string message = string.Format(
            //        "��� �������� ������ '{0}', ���� �� ���� ���� ����� ������������!\r\n\r\n�������� ��� ������?",
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