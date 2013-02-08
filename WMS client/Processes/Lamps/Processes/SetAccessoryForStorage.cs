using System.Drawing;
using WMS_client.Enums;
using WMS_client.db;

namespace WMS_client.Processes.Lamps
{
    /// <summary>Поставити світильник на зберігання</summary>
    public class SetAccessoryForStorage : BusinessProcess
    {
        /// <summary>Штрихкод светильника</summary>
        private readonly string accessoryBarcode;
        /// <summary>Штрихкод светильника</summary>
        private readonly TypeOfAccessories typeOfAccessory;

        /// <summary>Поставити світильник на зберігання</summary>
        public SetAccessoryForStorage(WMSClient MainProcess, string barcode, TypeOfAccessories type)
            : base(MainProcess, 1)
        {
            accessoryBarcode = barcode;
            typeOfAccessory = type;

            IsLoad = true;
            DrawControls();
        }

        #region Override methods
        public override sealed void DrawControls()
        {
            if (IsLoad)
            {
                string accessory = string.Empty;

                switch (typeOfAccessory)
                {
                    case TypeOfAccessories.Lamp:
                        accessory = "Лампа";
                        break;
                    case TypeOfAccessories.Case:
                        accessory = "Світильник";
                        break;
                    case TypeOfAccessories.ElectronicUnit:
                        accessory = "Ел.блок";
                        break;
                }

                MainProcess.CreateLabel(accessory+" буде поставленно на зберігання!", 5, 105, 230, 65,
                                        MobileFontSize.Multiline,
                                        MobileFontPosition.Center, MobileFontColors.Default, FontStyle.Bold);
                MainProcess.CreateLabel("Зберегти дані?", 5, 190, 230,
                                        MobileFontSize.Large, MobileFontPosition.Center, MobileFontColors.Info);

                MainProcess.CreateButton("Ок", 10, 275, 105, 35, "ok", Save_click);
                MainProcess.CreateButton("Відміна", 125, 275, 105, 35, "cancel", Cancel_click);
            }
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

        #region Buttons
        private void Save_click()
        {
            save();
        }

        private void Cancel_click()
        {
            OnHotKey(KeyAction.Esc);
        }
        #endregion

        private void save()
        {
            Accessory.SetNewState(typeOfAccessory, accessoryBarcode, TypesOfLampsStatus.Storage);
            OnHotKey(KeyAction.Esc);
        }
    }
}