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
        private IAccessory currentAccessory;

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
        /// <summary>�������������</summary>
        private static Accessory accessory;
        /// <summary>�������� (���������� ��� ���������) ��� �������������� (��� ��� � �������� ������)</summary>
        private static Type mainType;
        /// <summary>������� (���������� ��� ���������) ���������</summary>
        private static string mainTopic;
        /// <summary>�� �������������� � �������� �������</summary>
        private static long linkId = -1;
        /// <summary>������� ��� �������������� (��� ��� �� ������� ������� � ���������)</summary>
        private readonly Type currentType;
        /// <summary>������� ���������</summary>
        private string currentTopic;
        /// <summary>��������������� �����-���</summary>
        private string barcode;
        /// <summary>�� �������� ������ ���</summary>
        private bool isMainDataEntered { get { return accessory.Model != 0; } }
        /// <summary></summary>
        private readonly bool emptyBarcodeEnabled;
        /// <summary></summary>
        private readonly bool existMode;

        private MobileButton caseButton;
        private MobileButton lampButton;
        private MobileButton unitButton;
        private TypeOfAccessories currentAccessotyType;
        private TypeOfAccessories requaredAccessoryType;

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
        public AccessoryRegistration(WMSClient MainProcess, TypeOfAccessories requaredAccessoryType, Type type)
            : base(MainProcess, 1)
            {
            mainType = type;

            accessory = null;


            this.requaredAccessoryType = requaredAccessoryType;
            updateCurrentTopic();
            mainTopic = currentTopic;

            currentType = type;
            linkId = -1;
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
        /// <param name="type">������� ��� ��������������</param>
        /// <param name="prevType">���������� ��� ��������������</param>
        /// <param name="topic">������� ���������</param>
        /// <param name="id">�� �������������� � �������� �������</param>
        /// <param name="emptyBarcode">���� ������� ��������� ��������</param>
        public AccessoryRegistration(WMSClient MainProcess, TypeOfAccessories requaredAccessoryType, Type type, Type prevType, string topic, long id, bool emptyBarcode)
            : base(MainProcess, 1)
            {
            if (prevType == null)
                {
                mainType = type;
                mainTopic = topic;
                accessory = null;
                }

            this.requaredAccessoryType = requaredAccessoryType;

            currentType = type;
            currentTopic = topic;
            linkId = id;
            MainProcess.ToDoCommand = currentTopic;
            emptyBarcodeEnabled = emptyBarcode;

            IsLoad = true;
            existMode = false;
            DrawControls();
            }

        /// <summary>"��������� ��������������"</summary>
        /// <param name="MainProcess"></param>
        /// <param name="mainType">�������� (���������� ��� ���������) ��� �������������� (��� ��� � �������� ������)</param>
        /// <param name="mainTopic">������� (���������� ��� ���������) ���������</param>
        /// <param name="currentType">������� ��� �������������� (��� ��� �� ������� ������� � ���������)</param>
        /// <param name="currentTopic">������� ���������</param>
        /// <param name="accessory">�������������</param>
        /// <param name="barcode">��������������� �����-���</param>
        public AccessoryRegistration(WMSClient MainProcess, IAccessory accessory, TypeOfAccessories requaredAccessoryType, Type mainType, Type currentType, Accessory oldTypeaccessory, string barcode)
            : base(MainProcess, 1)
            {

            this.currentAccessory = accessory;

            StopNetworkConnection();
            this.requaredAccessoryType = requaredAccessoryType;
            updateCurrentTopic();

            AccessoryRegistration.accessory = oldTypeaccessory;
            AccessoryRegistration.mainType = mainType;
            this.currentType = currentType;

            this.barcode = barcode;

            IsLoad = true;
            existMode = true;

            OnBarcode(barcode);
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

                if (currentType != typeof(Cases))
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
                if (groupRegistration)
                    {
                    groupRegistrationOnBarcode(barcode);
                    return;
                    }
                MainProcess.ClearControls();
                this.barcode = barcode;


                if (currentAccessory == null || !currentAccessory.HasBarcode(barcode))
                    {
                    currentAccessory = Configuration.Current.Repository.FindAccessory(barcode.GetIntegerBarcode());
                    }

                bool accesoryIsExist = currentAccessory != null;

                //���� � ������� ��� ���������� ��������
                if (!existMode && accesoryIsExist)
                    {
                    currentAccessotyType = currentAccessory.GeAccessoryType();

                    bool isTypeLikeCurrent = currentAccessotyType == requaredAccessoryType;

                    //���� ���� �� ��������� - "�����"
                    if (!isTypeLikeCurrent)
                        {
                        ShowMessage("�������� ��� ������������ � ������ ���� ��������������!");
                        OnHotKey(KeyAction.Esc);
                        return;
                        }
                    }

                showData(accesoryIsExist, barcode);
                }
            //�� ���� ������ �������
            else
                {
                ShowMessage("������� ������ ���������!");
                OnHotKey(KeyAction.Esc);
                }
            }

        private void showData(bool accesoryIsExist, string Barcode)
            {
            initAccessory(Barcode.GetIntegerBarcode());

            readAccessory(accesoryIsExist, Barcode);

            if (accessory.Id == 0 && accessory.TypeOfWarrantly == TypesOfLampsWarrantly.None)
                {
                accessory.TypeOfWarrantly = TypesOfLampsWarrantly.Without;
                }

            //���������� ��������������� ��������
            dbObject.SetValue(accessory, dbObject.BARCODE_NAME, Barcode);

            //������ ������ ���������
            Dictionary<string, KeyValuePair<Type, object>> listOfDetail;
           
            MainProcess.ToDoCommand = currentTopic;

            //���������� ��������� ���� ��� ��������������
            drawEditableProperties();

            drawActionButtons();

            if (currentType == typeof(Cases))
                {
                groupRegistrationButton = MainProcess.CreateButton("������� ���������", 5, 275, 230, 35, string.Empty, startGroupRegistration);
                }
            //MainProcess.CreateButton("��������� �� ���������", 5, 275, 230, 35, string.Empty, fillFromPrev);
            }

        private void initAccessory(int intBarcode)
            {
            if (currentAccessory == null)
                {
                currentAccessory = AccessoryHelper.CreateNewAccessory(intBarcode, requaredAccessoryType);
                }
            }

        public override void OnHotKey(KeyAction TypeOfAction)
            {
            switch (TypeOfAction)
                {
                case KeyAction.Esc:
                    MainProcess.ClearControls();
                    MainProcess.Process = new EditSelector(MainProcess);

                    clearStaticFields();
                    break;
                }
            }

        private static void clearStaticFields()
            {
            accessory = null;
            linkId = -1;
            mainType = null;
            mainTopic = null;
            }
        #endregion

        #region Draw
        /// <summary>���������� ��������� ���� ��� ��������������</summary>
        /// <param name="listOfLabels">������ ��������� ����� ��� ��������������</param>
        private void drawEditableProperties()
            {
            int top = 42;
            int index = 0;
            const int delta = 21;

            top += delta;
            MainProcess.CreateButton(string.Format("������: {0}", currentAccessory.GetModelDescription()), 5, top, 230, 20, "modelButton", propertyButton_Click,
                new PropertyButtonInfo() { PropertyName = "Model", PropertyDescription = "������" });

            top += delta;
            MainProcess.CreateButton(string.Format("������: {0}", currentAccessory.GetStatusDescription()), 5, top, 230, 20, "modelButton", propertyButton_Click,
                new PropertyButtonInfo() { PropertyName = "Status", PropertyDescription = "������" });

            top += delta;
            MainProcess.CreateButton(string.Format("�����: {0}", currentAccessory.GetPartyDescription()), 5, top, 230, 20, "modelButton", propertyButton_Click,
                new PropertyButtonInfo() { PropertyName = "Party", PropertyDescription = "�����" });

            top += delta;
            MainProcess.CreateButton(string.Format("����������: {0}", currentAccessory.GetPartyContractor()), 5, top, 230,
                20, string.Empty, null, null, false);

            top += delta;
            MainProcess.CreateButton(string.Format("���� ����: {0}", currentAccessory.GetPartyDate()), 5, top, 230,
                20, string.Empty, null, null, false);

            top += delta;
            MainProcess.CreateButton(string.Format("��� ������: {0}", currentAccessory.GetWarrantyType()), 5, top, 230, 20, string.Empty, propertyButton_Click,
               new PropertyButtonInfo() { PropertyName = "TypeOfWarrantly", PropertyDescription = "��� ������" });

            top += delta;
            MainProcess.CreateButton(string.Format("���������� ������: {0}", currentAccessory.GetWarrantyExpiryDate()), 5, top, 230, 20, string.Empty, propertyButton_Click,
               new PropertyButtonInfo() { PropertyName = "DateOfWarrantyEnd", PropertyDescription = "���������� ������" });
            }

        private void propertyButton_Click(object sender)
            {
            var info = (PropertyButtonInfo)((sender as Button).Tag);

            MainProcess.ClearControls();
            MainProcess.Process = new ValueEditor(
                MainProcess,
                currentAccessory,
                mainType,
                mainTopic,
                currentType,
                currentTopic,
                accessory,
                requaredAccessoryType,
                info.PropertyDescription,
                barcode,
                info.PropertyName);
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

        /// <summary>�������</summary>
        /// <param name="sender">��������� ��� ���������� �������������</param>
        private void button_click(object sender)
            {
            Button button = ((Button)sender);
            Type type = button.Tag as Type;

            if (isMainDataEntered)
                {
                if (warrantlyDataIsValid())
                    {
                    if (type != null)
                        {
                        MainProcess.ClearControls();
                        bool isNewObject = accessory.IsNew;

                        //���� ��������� ��� ��������� � �������� ����� - "���������� ������������ ������"
                        if (mainType == type && linkId != -1)
                            {
                            accessory.SetValue(mainType.Name.Substring(0, mainType.Name.Length - 1), linkId);
                            //��������� ��� ���� ��� �� �������� accessory.Id
                            accessory.Write();

                            dbObject mainObj = (Accessory)Activator.CreateInstance(mainType);
                            mainObj = (Accessory)mainObj.Read(mainType, linkId, dbObject.IDENTIFIER_NAME);
                            mainObj.SetValue(currentType.Name.Substring(0, currentType.Name.Length - 1), accessory.Id);
                            mainObj.Write();
                            }

                        //������
                        accessory.Write();

                        //���� �������� ����� - ������ ��� ������� "�����������"
                        if (isNewObject)
                            {
                            //�������� ������ � "�����������"
                            Movement.RegisterLighter(accessory.BarCode, accessory.SyncRef,
                                                     OperationsWithLighters.Registration);
                            }

                        //����������� 
                        string propertyName = type.Name.Substring(0, type.Name.Length - 1);
                        object newAccessory = accessory.GetPropery(propertyName);
                        long newAccessoryId = newAccessory == null ? 0 : Convert.ToInt64(newAccessory);

                        //������� �� ��������� �������������
                        if ((newAccessoryId != 0 || (newAccessoryId == 0 && linkId != -1 && mainType == type))
                            && button.Text != okBtnText && button.Text != nextBtnText)
                            {
                            Accessory newObj = (Accessory)Activator.CreateInstance(type);
                            newObj.Read(type, newAccessoryId, dbObject.IDENTIFIER_NAME);
                            //MainProcess.Process = new AccessoryRegistration(MainProcess, mainType, mainTopic, type,
                            //                                      button.Text, newObj, newObj.BarCode);
                            accessory = newObj;
                            }
                        //������� �� ����� ��������� ��� �������������� 
                        else
                            {
                            //���� ��������� ��� ��������� � �������� �����
                            //if (mainType == type)
                            //    {
                            //    if (mainType == null || linkId == -1)
                            //        {
                            //        accessory = (Accessory)accessory.Copy();
                            //        accessory.ClearPosition();
                            //        MainProcess.Process = new AccessoryRegistration(MainProcess, mainType, mainType);
                            //        }

                            //    MainProcess.Process = new AccessoryRegistration(MainProcess, mainType, mainType);
                            //    }
                            //�� ��������� - "�������� �� �������������� � �������� ���������"
                            //else
                            //{
                            //MainProcess.Process = new AccessoryRegistration(MainProcess, type, mainType, button.Text,
                            //                                      accessory.Id, type == typeof(ElectronicUnits));
                            //}


                            //���� �� ���� ����������� ����������� ����� ��� ���������� ��������������, �� �������� ��� ����
                            if (!accessory.IsNew)
                                {
                                accessory = null;
                                }
                            }
                        }
                    }
                }
            else
                {
                showWriteErrorMessage();
                }
            }

        private static void showWriteErrorMessage()
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
            //��� ��������� ������, ���� �� ���� ���� ����� �����������!
            if ((accessory.TypeOfWarrantly == TypesOfLampsWarrantly.Without ||
                 accessory.TypeOfWarrantly == TypesOfLampsWarrantly.None) &&
                accessory.DateOfWarrantyEnd > DateTime.Now)
                {
                const string message = "��� ��������� ������, ���� �� ���� ���� ����� �����������!\r\n\r\n�������� ����?";

                if (MessageBox.Show(message, "�� ���� �������� ���", MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                    {
                    accessory.DateOfWarrantyEnd = DateTime.MinValue;
                    return true;
                    }

                return false;
                }

            //��� �������� ������, ���� �� ���� ���� ����� �����������!
            if (accessory.TypeOfWarrantly != TypesOfLampsWarrantly.Without &&
                accessory.TypeOfWarrantly != TypesOfLampsWarrantly.None &&
                accessory.DateOfWarrantyEnd < DateTime.Now)
                {
                string warrantly = EnumWorker.GetDescription(typeof(TypesOfLampsWarrantly),
                                                             (int)accessory.TypeOfWarrantly);
                string message = string.Format(
                    "��� �������� ������ '{0}', ���� �� ���� ���� ����� �����������!\r\n\r\n�������� ��� ������?",
                    warrantly);

                if (MessageBox.Show(message, "�� ���� ��������� ����", MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                    {
                    accessory.TypeOfWarrantly = TypesOfLampsWarrantly.Without;
                    return true;
                    }

                return false;
                }

            return true;
            }

        #endregion

        /// <summary>��������� ������ ��������������</summary>
        /// <param name="accesoryIsExist">������������� ����������</param>
        /// <param name="barcodeValue">��������</param>
        private void readAccessory(bool accesoryIsExist, string barcodeValue)
            {
            //���� ������������� ��� �� ��������������� - "�������"/"��������� ������������"
            if (accessory == null || (accesoryIsExist && !accessory.IsModified))
                {
                accessory = (Accessory)Activator.CreateInstance(currentType);

                if (string.IsNullOrEmpty(barcodeValue) && !accesoryIsExist)
                    {
                    return;
                    }

                if (string.IsNullOrEmpty(barcodeValue) && emptyBarcodeEnabled)
                    {
                    long id = ElectronicUnits.GetIdByEmptyBarcode(linkId);

                    if (id != 0)
                        {
                        accessory = (Accessory)accessory.Read(currentType, id, dbObject.IDENTIFIER_NAME);
                        }
                    }
                else
                    {
                    accessory = (Accessory)accessory.Read(currentType, barcodeValue, dbObject.BARCODE_NAME);
                    }

                accessory.Status = TypesOfLampsStatus.Storage;
                }
            }

        private void startGroupRegistration()
            {
            currentCase = accessory as Cases;

            if (currentCase.Lamp == 0 || currentCase.ElectronicUnit == 0)
                {
                ShowMessage("����� ��������� ����� � ��. ����!");
                return;
                }

            if (!(accessory is Cases))
                {
                return;
                }

            currentLamp = new Lamps();
            currentLamp.Read(currentCase.Lamp);

            currentUnit = new ElectronicUnits();
            currentUnit.Read(currentCase.ElectronicUnit);

            if (!string.IsNullOrEmpty(currentLamp.BarCode) || !string.IsNullOrEmpty(currentUnit.BarCode))
                {
                ShowMessage("��� ������� ��������� ����� �� ���� ����� ���� ��� �����-����");
                return;
                }

            if (isMainDataEntered && warrantlyDataIsValid())
                {
                accessory.Write();
                }
            else
                {
                showWriteErrorMessage();
                return;
                }

            groupRegistration = true;

            currentCase = new Cases();
            currentCase.Read(accessory.Id);

            groupRegistrationButton.Hide();
            groupSizeLabel = MainProcess.CreateLabel("", 5, 283, 230,
                                        MobileFontSize.Normal, MobileFontPosition.Left, MobileFontColors.Info, FontStyle.Bold);
            groupSize = 0;
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

    internal class PropertyButtonInfo
        {
        public string PropertyName { get; set; }
        public string PropertyDescription { get; set; }
        }
    }