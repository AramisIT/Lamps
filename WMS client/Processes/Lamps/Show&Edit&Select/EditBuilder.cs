using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WMS_client.Base.Visual.Constructor;
using WMS_client.Enums;
using WMS_client.db;

namespace WMS_client
{
    /// <summary>"��������� ��������������"</summary>
    public class EditBuilder : BusinessProcess
    {
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
        private readonly string currentTopic;
        /// <summary>��������������� �����-���</summary>
        private string barcode;
        private readonly bool emptyBarcodeEnabled;
        private readonly bool existMode;
        private const string okBtnText = "��";
        private const string nextBtnText = "���";
        #endregion

        /// <summary>"��������� ��������������"</summary>
        /// <param name="MainProcess"></param>
        /// <param name="type">������� ��� ��������������</param>
        /// <param name="prevType">���������� ��� ��������������</param>
        /// <param name="topic">������� ���������</param>
        public EditBuilder(WMSClient MainProcess, Type type, Type prevType, string topic)
            : base(MainProcess, 1)
        {
            //���� ����������� ���� ���, �� ��� "������" - ����� ������� ��������� ���������� ��������� ������
            if (prevType == null)
            {
                mainType = type;
                mainTopic = topic;
                accessory = null;
            }

            currentType = type;
            currentTopic = topic;
            linkId = -1;
            MainProcess.ToDoCommand = currentTopic;

            IsLoad = true;
            existMode = false;
            DrawControls();
        }

        /// <summary>"��������� ��������������"</summary>
        /// <param name="MainProcess"></param>
        /// <param name="type">������� ��� ��������������</param>
        /// <param name="prevType">���������� ��� ��������������</param>
        /// <param name="topic">������� ���������</param>
        /// <param name="id">�� �������������� � �������� �������</param>
        /// <param name="emptyBarcode">���� ������� ��������� ��������</param>
        public EditBuilder(WMSClient MainProcess, Type type, Type prevType, string topic, long id, bool emptyBarcode)
            : base(MainProcess, 1)
        {
            if (prevType == null)
            {
                mainType = type;
                mainTopic = topic;
                accessory = null;
            }

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
        public EditBuilder(WMSClient MainProcess, Type mainType, string mainTopic, Type currentType, string currentTopic, Accessory accessory, string barcode)
            : base(MainProcess, 1)
        {
            EditBuilder.accessory = accessory;
            EditBuilder.mainType = mainType;
            EditBuilder.mainTopic = mainTopic;
            this.currentType = currentType;
            this.currentTopic = currentTopic;
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

                MainProcess.CreateButton("��� ���������", 10, 270, 220, 35, string.Empty, () => OnBarcode(string.Empty));
            }
        }

        /// <summary>������������� �������������, ����� ������ ��������������</summary>
        /// <param name="Barcode">��������</param>
        public override sealed void OnBarcode(string Barcode)
        {
            if (Barcode.IsValidBarcode())
            {
                MainProcess.ClearControls();
                barcode = Barcode;
                bool accesoryIsExist = !string.IsNullOrEmpty(Barcode) && BarcodeWorker.IsBarcodeExist(Barcode);

                //���� � ������� ��� ���������� ��������
                if (!existMode && accesoryIsExist)
                {
                    //��� �������������� ���������
                    TypeOfAccessories typesOfAccessories = BarcodeWorker.GetTypeOfAccessoriesByBarcode(Barcode);
                    //�������� �� ��� �������������� (������������) = ���� ������� ����� ��������������� (���������������)
                    bool isTypeLikeCurrent = typesOfAccessories.ToString() + 's' != currentType.Name;

                    //���� ���� �� ��������� - "�����"
                    if (isTypeLikeCurrent)
                    {
                        ShowMessage("�������� ��� ������������� � ������ ���� ��������������!");
                        OnHotKey(KeyAction.Esc);
                        return;
                    }
                }

                showData(accesoryIsExist, Barcode);
            }
            else
            {
                ShowMessage("������� ������ ���������!");
                OnHotKey(KeyAction.Esc);
            }
        }

        private void showData(bool accesoryIsExist, string Barcode)
        {
            readAccessory(accesoryIsExist, Barcode);

            //���������� ��������������� ��������
            dbObject.SetValue(accessory, dbObject.BARCODE_NAME, Barcode);

            //������ ������ ���������
            Dictionary<string, KeyValuePair<Type, object>> listOfDetail;
            //������ ��������� ����� ��� ��������������
            List<LabelForConstructor> listOfLabels = CatalogObject.GetDetailVisualPresenter(
                currentType, out listOfDetail, accessory);
            MainProcess.ToDoCommand = currentTopic;
            //��������� ������ ���������
            listOfDetail.Add(
                mainType == currentType ? nextBtnText : okBtnText,
                new KeyValuePair<Type, object>(mainType, null));

            //���������� ��������� ���� ��� ��������������
            drawEditableProperties(listOfLabels);
            //���������� ������� ���������
            drawButtons(listOfDetail);
            //������ "��������� �� ���������"
            MainProcess.CreateButton("��������� �� ���������", 5, 275, 230, 35, string.Empty, fillFromPrev);
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
        private void drawEditableProperties(IEnumerable<LabelForConstructor> listOfLabels)
        {
            int top = 42;
            int index = 0;
            const int delta = 21;

            foreach (LabelForConstructor label in listOfLabels)
            {
                top += delta;
                string text;

                if (label.AddParameterData)
                {
                    index += label.Skip;
                    string parameter = Parameters != null && Parameters.Length > index
                                           ? Parameters[index].ToString()
                                           : string.Empty;

                    text = string.Format(label.Text.TrimEnd(), parameter);
                    index++;
                }
                else
                {
                    text = label.Text.TrimEnd();
                }

                //����� �� ������������� ������ ����
                if (label.AllowEditValue)
                {
                    MainProcess.CreateButton(text, 5, top, 230, 20, label.Name, button_Click, label.Name);
                }
                else
                {
                    MainProcess.CreateButton(text, 5, top, 230, 20, string.Empty, null, null, false);
                }
            }
        }

        /// <summary>������� �� ����� �������������� ����</summary>
        /// <param name="sender">��������� ���� ��� ��������������</param>
        private void button_Click(object sender)
        {
            Button button = sender as Button;

            if (button != null)
            {
                MainProcess.ClearControls();
                MainProcess.Process = new ValueEditor(
                    MainProcess,
                    mainType,
                    mainTopic,
                    currentType,
                    currentTopic,
                    accessory,
                    button.Text.Substring(0, button.Text.IndexOf(':')),
                    barcode,
                    button.Name);
            }
        }

        /// <summary>���������� ������� ���������</summary>
        /// <param name="listOfDetail">������ ������ ���������</param>
        private void drawButtons(Dictionary<string, KeyValuePair<Type, object>> listOfDetail)
        {
            if (listOfDetail.Count != 0)
            {
                const int top = 235;
                const int height = 35;
                int left = 15 / listOfDetail.Count;
                int width = (240 - left * (listOfDetail.Count + 1)) / listOfDetail.Count;
                int delta = left;

                foreach (KeyValuePair<string, KeyValuePair<Type, object>> detail in listOfDetail)
                {
                    //bool enable = detail.Value.Value== null;// || detail.Value.Key != typeof(Lamps);
                    MainProcess.CreateButton(detail.Key, delta, top, width, height, string.Empty, button_click,
                                             detail.Value.Key, true);
                    delta += left + width;
                }
            }
        }

        /// <summary>�������</summary>
        /// <param name="sender">��������� ��� ���������� �������������</param>
        private void button_click(object sender)
        {
            Button button = ((Button) sender);
            Type type = button.Tag as Type;

            if (warrantlyDataIsValid())
            {
                if (type != null)
                {
                    MainProcess.ClearControls();
                    bool isNewObject = accessory.IsNew;

                    //���� ��������� ��� ��������� � �������� ����� - "���������� ������������ ������"
                    if (mainType == type && linkId != -1)
                    {
                        //if (string.IsNullOrEmpty(accessory.BarCode)){accessory.SetIsNew();}
                        accessory.SetValue(mainType.Name.Substring(0, mainType.Name.Length - 1), linkId);
                        //��������� ��� ���� ��� �� �������� accessory.Id
                        accessory.Save();

                        dbObject mainObj = (Accessory) Activator.CreateInstance(mainType);
                        mainObj = (Accessory) mainObj.Read(mainType, linkId, dbObject.IDENTIFIER_NAME);
                        mainObj.SetValue(currentType.Name.Substring(0, currentType.Name.Length - 1), accessory.Id);
                        mainObj.Save();
                    }

                    //������
                    accessory.Save();

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
                        Accessory newObj = (Accessory) Activator.CreateInstance(type);
                        newObj.Read(type, newAccessoryId, dbObject.IDENTIFIER_NAME);
                        MainProcess.Process = new EditBuilder(MainProcess, mainType, mainTopic, type,
                                                              button.Text, newObj, newObj.BarCode);
                        accessory = newObj;
                    }
                        //������� �� ����� ��������� ��� �������������� 
                    else
                    {
                        //���� ��������� ��� ��������� � �������� �����
                        if (mainType == type)
                        {
                            if (mainType == null || linkId == -1)
                            {
                                accessory = (Accessory) accessory.Copy();
                                MainProcess.Process = new EditBuilder(MainProcess, mainType, mainType, mainTopic);
                            }

                            MainProcess.Process = new EditBuilder(MainProcess, mainType, mainType, mainTopic);
                        }
                            //�� ��������� - "�������� �� �������������� � �������� ���������"
                        else
                        {
                            MainProcess.Process = new EditBuilder(MainProcess, type, mainType, button.Text,
                                                                  accessory.Id, type == typeof (ElectronicUnits));
                        }


                        //���� �� ���� ����������� ����������� ����� ��� ���������� ��������������, �� �������� ��� ����
                        if (!accessory.IsNew)
                        {
                            accessory = null;
                        }
                    }
                }
            }
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
                string warrantly = EnumWorker.GetDescription(typeof (TypesOfLampsWarrantly),
                                                             (int) accessory.TypeOfWarrantly);
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

        /// <summary>��������� �� ���������</summary>
        private void fillFromPrev()
        {
            //��������� ��������, � �� �� ��������
            string currBarcode = accessory.BarCode;
            Accessory lastObj;

            //�������� ������ ���������� ��������������
            if (Accessory.GetLastAccesory(currentType, out lastObj))
            {
                accessory = lastObj.CopyWithoutLinks();
                MainProcess.ClearControls();
                accessory.Status = TypesOfLampsStatus.Storage;
                showData(false, currBarcode);
            }
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

                    if(id!=0)
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
    }
}