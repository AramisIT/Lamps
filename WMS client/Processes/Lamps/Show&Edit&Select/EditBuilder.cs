using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WMS_client.Base.Visual.Constructor;
using WMS_client.Enums;
using WMS_client.db;

namespace WMS_client
{
    /// <summary>"Строитель редактирования"</summary>
    public class EditBuilder : BusinessProcess
    {
        #region Fields
        /// <summary>Комплектующее</summary>
        private static Accessory accessory;
        /// <summary>Основной (неизменный при переходах) тип комплектующего (тот тип с которого начали)</summary>
        private static Type mainType;
        /// <summary>Основой (неизменный при переходах) заголовок</summary>
        private static string mainTopic;
        /// <summary>ИД комплектующего с которого перешли</summary>
        private static long linkId = -1;
        /// <summary>Текущий тип комплектующего (тот тип на который перешли с основного)</summary>
        private readonly Type currentType;
        /// <summary>Текущий заголовок</summary>
        private readonly string currentTopic;
        /// <summary>Отсканированный щтрих-код</summary>
        private string barcode;
        private readonly bool emptyBarcodeEnabled;
        private readonly bool existMode;
        private const string okBtnText = "Ок";
        private const string nextBtnText = "Далі";
        #endregion

        /// <summary>"Строитель редактирования"</summary>
        /// <param name="MainProcess"></param>
        /// <param name="type">Текущий тип комплектующего</param>
        /// <param name="prevType">Предыдущий тип комплектующего</param>
        /// <param name="topic">Текущий заголовок</param>
        public EditBuilder(WMSClient MainProcess, Type type, Type prevType, string topic)
            : base(MainProcess, 1)
        {
            //Если предыдущего типа нет, то это "начало" - нужно указать сохранить неизменные начальные данные
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

        /// <summary>"Строитель редактирования"</summary>
        /// <param name="MainProcess"></param>
        /// <param name="type">Текущий тип комплектующего</param>
        /// <param name="prevType">Предыдущий тип комплектующего</param>
        /// <param name="topic">Текущий заголовок</param>
        /// <param name="id">ИД комплектующего с которого перешли</param>
        /// <param name="emptyBarcode">Ввод пустого штрихкода разрешен</param>
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

        /// <summary>"Строитель редактирования"</summary>
        /// <param name="MainProcess"></param>
        /// <param name="mainType">Основной (неизменный при переходах) тип комплектующего (тот тип с которого начали)</param>
        /// <param name="mainTopic">Основой (неизменный при переходах) заголовок</param>
        /// <param name="currentType">Текущий тип комплектующего (тот тип на который перешли с основного)</param>
        /// <param name="currentTopic">Текущий заголовок</param>
        /// <param name="accessory">Комплектующее</param>
        /// <param name="barcode">Отсканированный щтрих-код</param>
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
                MainProcess.CreateLabel("Відскануйте", 0, 130, 240,
                                        MobileFontSize.Normal, MobileFontPosition.Center, MobileFontColors.Info, FontStyle.Bold);
                MainProcess.CreateLabel("ШТРИХ-КОД!", 0, 150, 240,
                                        MobileFontSize.Normal, MobileFontPosition.Center, MobileFontColors.Info, FontStyle.Bold);

                MainProcess.CreateButton("Без штрихкода", 10, 270, 220, 35, string.Empty, () => OnBarcode(string.Empty));
            }
        }

        /// <summary>Комплектующее отсканировано, нужно начать редактирование</summary>
        /// <param name="Barcode">ШтрихКод</param>
        public override sealed void OnBarcode(string Barcode)
        {
            if (Barcode.IsValidBarcode())
            {
                MainProcess.ClearControls();
                barcode = Barcode;
                bool accesoryIsExist = !string.IsNullOrEmpty(Barcode) && BarcodeWorker.IsBarcodeExist(Barcode);

                //Если в системе уже существует штрихкод
                if (!existMode && accesoryIsExist)
                {
                    //Тип комплектующего штрихкода
                    TypeOfAccessories typesOfAccessories = BarcodeWorker.GetTypeOfAccessoriesByBarcode(Barcode);
                    //Является ли тип комплектующего (существующий) = типу который нужно отредактировать (отсканированный)
                    bool isTypeLikeCurrent = typesOfAccessories.ToString() + 's' != currentType.Name;

                    //Если типы не совпадают - "Выход"
                    if (isTypeLikeCurrent)
                    {
                        ShowMessage("Штрихкод уже используеться в другом типе комплектующего!");
                        OnHotKey(KeyAction.Esc);
                        return;
                    }
                }

                showData(accesoryIsExist, Barcode);
            }
            else
            {
                ShowMessage("Невірний формат штрихкоду!");
                OnHotKey(KeyAction.Esc);
            }
        }

        private void showData(bool accesoryIsExist, string Barcode)
        {
            readAccessory(accesoryIsExist, Barcode);

            //Установить отсканированный штрихкод
            dbObject.SetValue(accessory, dbObject.BARCODE_NAME, Barcode);

            //Список кнопок переходов
            Dictionary<string, KeyValuePair<Type, object>> listOfDetail;
            //Список доступных полей для редактирования
            List<LabelForConstructor> listOfLabels = CatalogObject.GetDetailVisualPresenter(
                currentType, out listOfDetail, accessory);
            MainProcess.ToDoCommand = currentTopic;
            //Дополение списка переходов
            listOfDetail.Add(
                mainType == currentType ? nextBtnText : okBtnText,
                new KeyValuePair<Type, object>(mainType, null));

            //Отобразить доступные поля для редактирования
            drawEditableProperties(listOfLabels);
            //Отобразить кнопоки переходов
            drawButtons(listOfDetail);
            //Кнопка "Заповнити як попередній"
            MainProcess.CreateButton("Заповнити як попередній", 5, 275, 230, 35, string.Empty, fillFromPrev);
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
        /// <summary>Отобразить доступные поля для редактирования</summary>
        /// <param name="listOfLabels">Список доступных полей для редактирования</param>
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

                //Можно ли редактировать данное поле
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

        /// <summary>Переход на экран редактирования поля</summary>
        /// <param name="sender">Выбранное поле для редактирования</param>
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

        /// <summary>Отобразить кнопоки переходов</summary>
        /// <param name="listOfDetail">Список кнопок переходов</param>
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

        /// <summary>Переход</summary>
        /// <param name="sender">Выбранный тип следующего коплектующего</param>
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

                    //Если выбранный тип совпадает с основным типом - "Сохранение перекрестных ссылок"
                    if (mainType == type && linkId != -1)
                    {
                        //if (string.IsNullOrEmpty(accessory.BarCode)){accessory.SetIsNew();}
                        accessory.SetValue(mainType.Name.Substring(0, mainType.Name.Length - 1), linkId);
                        //Сохраняем для того что бы получить accessory.Id
                        accessory.Save();

                        dbObject mainObj = (Accessory) Activator.CreateInstance(mainType);
                        mainObj = (Accessory) mainObj.Read(mainType, linkId, dbObject.IDENTIFIER_NAME);
                        mainObj.SetValue(currentType.Name.Substring(0, currentType.Name.Length - 1), accessory.Id);
                        mainObj.Save();
                    }

                    //Запись
                    accessory.Save();

                    //Если документ новый - значит был процесс "Регистрация"
                    if (isNewObject)
                    {
                        //Внесение записи в "Перемещение"
                        Movement.RegisterLighter(accessory.BarCode, accessory.SyncRef,
                                                 OperationsWithLighters.Registration);
                    }

                    //Отображение 
                    string propertyName = type.Name.Substring(0, type.Name.Length - 1);
                    object newAccessory = accessory.GetPropery(propertyName);
                    long newAccessoryId = newAccessory == null ? 0 : Convert.ToInt64(newAccessory);

                    //Переход на связанное комплектующее
                    if ((newAccessoryId != 0 || (newAccessoryId == 0 && linkId != -1 && mainType == type))
                        && button.Text != okBtnText && button.Text != nextBtnText)
                    {
                        Accessory newObj = (Accessory) Activator.CreateInstance(type);
                        newObj.Read(type, newAccessoryId, dbObject.IDENTIFIER_NAME);
                        MainProcess.Process = new EditBuilder(MainProcess, mainType, mainTopic, type,
                                                              button.Text, newObj, newObj.BarCode);
                        accessory = newObj;
                    }
                        //Переход на НОВЫЙ выбранный тип комплектующего 
                    else
                    {
                        //Если выбранный тип совпадает с основным типом
                        if (mainType == type)
                        {
                            if (mainType == null || linkId == -1)
                            {
                                accessory = (Accessory) accessory.Copy();
                                MainProcess.Process = new EditBuilder(MainProcess, mainType, mainType, mainTopic);
                            }

                            MainProcess.Process = new EditBuilder(MainProcess, mainType, mainType, mainTopic);
                        }
                            //Не совпадает - "Передача ИД комплектующего с которого переходим"
                        else
                        {
                            MainProcess.Process = new EditBuilder(MainProcess, type, mainType, button.Text,
                                                                  accessory.Id, type == typeof (ElectronicUnits));
                        }


                        //Если не было произведено копирование полей для следующего комплектующего, то очистить все поля
                        if (!accessory.IsNew)
                        {
                            accessory = null;
                        }
                    }
                }
            }
        }

        /// <summary>Чи вірні дані про гарантію?</summary>
        private bool warrantlyDataIsValid()
        {
            //При відсутності гарантії, дата не може бути більше сьогоднішньої!
            if ((accessory.TypeOfWarrantly == TypesOfLampsWarrantly.Without ||
                 accessory.TypeOfWarrantly == TypesOfLampsWarrantly.None) &&
                accessory.DateOfWarrantyEnd > DateTime.Now)
            {
                const string message = "При відсутності гарантії, дата не може бути більше сьогоднішньої!\r\n\r\nЗбросити дату?";

                if (MessageBox.Show(message, "Не вірно заповнені дані", MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                {
                    accessory.DateOfWarrantyEnd = DateTime.MinValue;
                    return true;
                }

                return false;
            }

            //При наявності гарантії, дата не може бути менше сьогоднішньої!
            if (accessory.TypeOfWarrantly != TypesOfLampsWarrantly.Without &&
                accessory.TypeOfWarrantly != TypesOfLampsWarrantly.None &&
                accessory.DateOfWarrantyEnd < DateTime.Now)
            {
                string warrantly = EnumWorker.GetDescription(typeof (TypesOfLampsWarrantly),
                                                             (int) accessory.TypeOfWarrantly);
                string message = string.Format(
                    "При наявності гарантії '{0}', дата не може бути менше сьогоднішньої!\r\n\r\nЗбросити тип гарантії?",
                    warrantly);

                if (MessageBox.Show(message, "Не вірно заповнена дата", MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                {
                    accessory.TypeOfWarrantly = TypesOfLampsWarrantly.Without;
                    return true;
                }

                return false;
            }

            return true;
        }

        /// <summary>Заповнити як попередній</summary>
        private void fillFromPrev()
        {
            //Сохраняем штрихкод, а то он затрется
            string currBarcode = accessory.BarCode;
            Accessory lastObj;

            //Получаем объект последнего комплектующего
            if (Accessory.GetLastAccesory(currentType, out lastObj))
            {
                accessory = lastObj.CopyWithoutLinks();
                MainProcess.ClearControls();
                accessory.Status = TypesOfLampsStatus.Storage;
                showData(false, currBarcode);
            }
        }
        #endregion

        /// <summary>Прочитать данные комплектующего</summary>
        /// <param name="accesoryIsExist">Комплектующее существует</param>
        /// <param name="barcodeValue">Штрихкод</param>
        private void readAccessory(bool accesoryIsExist, string barcodeValue)
        {
            //Если комплектующее еще не редактировалось - "Создать"/"Прочитать существующее"
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