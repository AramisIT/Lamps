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
            MainProcess.ClearControls();
            barcode = Barcode;
            bool accesoryIsExist = !string.IsNullOrEmpty(Barcode) && BarcodeWorker.IsBarcodeExist(Barcode);

            //���� � ������� ��� ���������� ��������
            if (accesoryIsExist)
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
                mainType == currentType ? "���" : "��",
                new KeyValuePair<Type, object>(mainType, null));

            //���������� ��������� ���� ��� ��������������
            drawEditableProperties(listOfLabels);
            //���������� ������� ���������
            drawButtons(listOfDetail);
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
                const int top = 275;
                const int height = 35;
                int left = 15 / listOfDetail.Count;
                int width = (240 - left * (listOfDetail.Count + 1)) / listOfDetail.Count;
                int delta = left;

                foreach (KeyValuePair<string, KeyValuePair<Type, object>> detail in listOfDetail)
                {
                    bool enable = detail.Value.Value== null || detail.Value.Key != typeof(Lamps);
                    MainProcess.CreateButton(detail.Key, delta, top, width, height, string.Empty, button_click,
                                             detail.Value.Key, enable);
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

            MainProcess.ClearControls();
            accessory.Save();

            //���� ��������� ��� ��������� � �������� ����� - "���������� ������������ ������"
            if (mainType == type)
            {
                if (mainType != null && linkId != -1)
                {
                    if (string.IsNullOrEmpty(accessory.BarCode))
                    {
                        accessory.SetIsNew();
                    }
                    accessory.SetValue(mainType.Name.Substring(0, mainType.Name.Length - 1), linkId);
                    accessory.Save();
                    
                    dbObject mainObj = (Accessory) Activator.CreateInstance(mainType);
                    mainObj = (Accessory) mainObj.Read(mainType, linkId, dbObject.IDENTIFIER_NAME);
                    mainObj.SetValue(currentType.Name.Substring(0, currentType.Name.Length - 1), accessory.Id);
                    mainObj.Save();

                    MainProcess.Process = new EditBuilder(MainProcess, mainType, mainType, mainTopic);
                }
                else
                {
                    accessory = (Accessory) accessory.Copy();
                    MainProcess.Process = new EditBuilder(MainProcess, mainType, mainType, mainTopic);
                }
            }
            //�� ��������� - "�������� �� �������������� � �������� ���������"
            else
            {
                MainProcess.Process = new EditBuilder(MainProcess, type, mainType, button.Text, accessory.Id,
                                                      type == typeof (ElectronicUnits));
            }

            //���� �� ���� ����������� ����������� ����� ��� ���������� ��������������, �� �������� ��� ����
            if (!accessory.IsNew)
            {
                accessory = null;
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
            }
        }
    }
}