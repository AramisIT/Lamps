using System.Windows.Forms;
using System;
using WMS_client.db;
using System.Data.SqlServerCe;
using System.Data;

namespace WMS_client
    {
    /// <summary>Установка светильника (начало)</summary>
    public class InstallingNewLighter : BusinessProcess
        {
        /// <summary>Инфо о карте</summary>
        public MapInfo MapInfo
            {
            get { return z_MapInfo; }
            set
                {
                z_MapInfo = value;
                updateMap();
                }
            }
        private MapInfo z_MapInfo;

        /// <summary>Регистр</summary>
        public string Register
            {
            get { return z_Register; }
            set
                {
                z_Register = value;
                updateRegister();
                }
            }
        private string z_Register;

        /// <summary>Позиция</summary>
        public string Position
            {
            get { return z_Position; }
            set
                {
                z_Position = value;
                updatePosition();
                }
            }
        private string z_Position;

        private MobileButton positionBtn;
        private MobileButton registerBtn;
        private MobileLabel mapLabel;
        private MobileLabel registerLabel;
        private MobileLabel positionLabel;

        private MobileTextBox registerTextBox;
        private const string NOT_CHOOSEN = "Не обрано";
        private readonly string LampBarCode;

        /// <summary>Установка светильника (начало)</summary>
        /// <param name="MainProcess"></param>
        /// <param name="lampBarCode">Штрихкод светильника</param>
        public InstallingNewLighter(WMSClient MainProcess, string lampBarCode)
            : base(MainProcess, 1)
            {
            FormNumber = 1;
            BusinessProcessType = ProcessType.Registration;
            LampBarCode = lampBarCode;
            IsLoad = true;
            }

        #region Override methods
        public override sealed void DrawControls()
            {
            MainProcess.ToDoCommand = "Вкажіть місце";

            MainProcess.CreateButton("Карта", 15, 80, 100, 35, "map", selectMap);
            registerBtn = MainProcess.CreateButton("Регістр", 15, 125, 100, 35, "register", selectRegister, null, null, false);
            positionBtn = MainProcess.CreateButton("Позиція", 15, 170, 100, 35, "position", selectPostition, null, null, false);

            mapLabel = MainProcess.CreateLabel(NOT_CHOOSEN, 120, 90, 100, MobileFontSize.Normal, MobileFontPosition.Center);
            registerLabel = MainProcess.CreateLabel(NOT_CHOOSEN, 120, 135, 100, MobileFontSize.Normal, MobileFontPosition.Center);
            positionLabel = MainProcess.CreateLabel(NOT_CHOOSEN, 120, 180, 100, MobileFontSize.Normal, MobileFontPosition.Center);

            MainProcess.CreateButton("Заповнити як попередній", 20, 230, 200, 35, "fillLikePrev", fillLikePrev);
            MainProcess.CreateButton("Ok", 20, 275, 200, 35, "ok", Ok);
            }

        public override void OnBarcode(string Barcode)
            {
            }

        public override void OnHotKey(KeyAction TypeOfAction)
            {
            switch (TypeOfAction)
                {
                case KeyAction.Esc:
                    MainProcess.ClearControls();
                    MainProcess.Process = new SelectingLampProcess(MainProcess);
                    break;
                }
            }
        #endregion

        #region ButtonClick
        /// <summary>Выбор карты</summary>
        private void selectMap()
            {
            MainProcess.ClearControls();
            MainProcess.Process = new SelectMap(MainProcess, 0, LampBarCode)
                                      {
                                          MapInfo = MapInfo
                                      };
            }

        /// <summary>Ввод регистра</summary>
        private void selectRegister()
            {
            if (registerTextBox == null)
                {
                registerTextBox =
                    (MobileTextBox)
                    MainProcess.CreateTextBox(120, 135, 100, "registerTB", ControlsStyle.LabelNormal,
                                              onRegisterTextChanged, false);
                registerTextBox.Text = registerLabel.Text != NOT_CHOOSEN ? registerLabel.Text : string.Empty;
                registerTextBox.Focus();
                registerLabel.Hide();
                }
            else
                {
                string oldValue = registerLabel.Text;
                hideTextBox();

                if (!oldValue.Equals(registerLabel.Text))
                    {
                    clearPosition();
                    }
                }
            }

        /// <summary>Выбор позиции</summary>
        private void selectPostition()
            {
            hideTextBox();

            if (string.IsNullOrEmpty(registerLabel.Text) || registerLabel.Text.Equals(NOT_CHOOSEN))
                {
                ShowMessage("Не заполнен регистр!");
                }
            else
                {
                MainProcess.ClearControls();
                MainProcess.Process = new SelectPosition(MainProcess, MapInfo, registerLabel.Text, LampBarCode);
                }
            }

        private void fillLikePrev()
            {
            DataTable table = null;

            using (SqlCeCommand command = dbWorker.NewQuery(@"
SELECT m.Id MapId,m.Description,m.RegisterFrom,m.RegisterTo,c.Register 
FROM Cases c 
JOIN Maps m ON m.Id=c.Map
WHERE c.Status=1
ORDER BY DateOfActuality DESC"))
                {
                table = command.SelectToTable();
                }

            if (table != null && table.Rows.Count > 0)
                {
                DataRow row = table.Rows[0];
                MapInfo = new MapInfo(
                    row["MapId"], row[CatalogObject.DESCRIPTION].ToString(),
                    Convert.ToInt32(row["RegisterFrom"]), Convert.ToInt32(row["RegisterTo"]));
                Register = row["Register"].ToString();
                clearPosition();
                }
            }

        /// <summary>Далее</summary>
        public void Ok()
            {
            if (validateInputData())
                {
                //Переход на новый скрин
                object[] parameters = new object[]
                                          {
                                              mapLabel.Text,
                                              registerLabel.Text,
                                              positionLabel.Text
                                          };
                MainProcess.ClearControls();
                MainProcess.Process = new FinishedInstalingNewLighter(MainProcess, parameters, MapInfo.Id, LampBarCode);
                }
            else
                {
                ShowMessage("Не правильно заполнены некоторые поля!");
                }
            }
        #endregion

        #region Operation with label
        /// <summary></summary>
        private void hideTextBox()
            {
            if (registerTextBox != null)
                {
                registerLabel.Show();

                if (string.IsNullOrEmpty(registerTextBox.Text))
                    {
                    clearRegister();
                    }
                else
                    {
                    int registerValue;

                    try
                        {
                        registerValue = int.Parse(registerTextBox.Text);
                        }
                    catch (FormatException)
                        {
                        registerTextBox.Text = string.Empty;
                        registerValue = 0;
                        }

                    if (registerValue >= MapInfo.Range.X && registerValue <= MapInfo.Range.Y)
                        {
                        registerLabel.Text = registerTextBox.Text;
                        registerLabel.SetControlsStyle(ControlsStyle.LabelNormal);
                        }
                    else
                        {
                        ShowMessage(string.Format("Допустимый диапазон значений:\r\n{0}-{1}",
                                                  MapInfo.Range.X,
                                                  MapInfo.Range.Y));
                        }
                    }

                MainProcess.RemoveControl((Control)registerTextBox.GetControl());
                registerTextBox = null;
                }
            }

        private void onRegisterTextChanged(object obj, EventArgs e)
            {
            }

        /// <summary>Очистить регистр</summary>
        private void clearRegister()
            {
            registerLabel.Text = NOT_CHOOSEN;
            registerLabel.SetControlsStyle(ControlsStyle.LabelH2Red);
            }

        /// <summary>Очистить позицию</summary>
        private void clearPosition()
            {
            positionLabel.Text = NOT_CHOOSEN;
            positionLabel.SetControlsStyle(ControlsStyle.LabelH2Red);
            }

        /// <summary>Обновить карту</summary>
        private void updateMap()
            {
            if (MapInfo.IsSelected)
                {
                mapLabel.Text = MapInfo.Description;
                mapLabel.SetControlsStyle(ControlsStyle.LabelNormal);
                registerBtn.Enabled = true;
                positionBtn.Enabled = true;
                }
            }

        /// <summary>Обновить регистр</summary>
        private void updateRegister()
            {
            registerLabel.Text = Register;
            registerLabel.SetControlsStyle(ControlsStyle.LabelNormal);
            }

        /// <summary>Обновить позицию</summary>
        private void updatePosition()
            {
            positionLabel.Text = Position;
            positionLabel.SetControlsStyle(ControlsStyle.LabelNormal);
            }
        #endregion

        /// <summary>Проверка правильности заполнения полей</summary>
        /// <returns></returns>
        private bool validateInputData()
            {
            hideTextBox();

            return validateLabelData(mapLabel) && validateLabelData(registerLabel) && validateLabelData(positionLabel);
            }

        /// <summary>Проверка валидности метки</summary>
        /// <param name="label">Метка</param>
        private bool validateLabelData(MobileLabel label)
            {
            return !label.Text.Equals(NOT_CHOOSEN);
            }
        }
    }

