using WMS_client.db;
using WMS_client.Enums;
using WMS_client.Processes.Lamps;

namespace WMS_client
    {
    /// <summary>Вибір типу комплектуючого для регістрації (редагування)</summary>
    public class EditSelector : BusinessProcess
        {
        /// <summary>Вибір типу комплектуючого для регістрації (редагування)</summary>
        public EditSelector()
            : base(1)
            {
            }

        #region Override methods
        public override sealed void DrawControls()
            {
            MainProcess.ToDoCommand = "Вибір реєстрації";

            var buttonTop = 10;
            const int topDelta = 50;

            buttonTop += topDelta;
            MainProcess.CreateButton("Непрацюючі світильники", 10, buttonTop, 220, 40, "unit", damagedLights_Click);

            buttonTop += topDelta;
            MainProcess.CreateButton("Електронний блок", 10, buttonTop, 220, 40, "unit", unit_Click);

            buttonTop += topDelta;
            MainProcess.CreateButton("Лампа", 10, buttonTop, 220, 40, "lamp", lamp_Click);

            buttonTop += topDelta;
            MainProcess.CreateButton("Корпус", 10, buttonTop, 220, 40, "case", case_Click);

            buttonTop += topDelta;
            MainProcess.CreateButton("Групова реєстрація комплектів", 10, buttonTop, 220, 40, "case", groupRegistration_Click);
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
                    MainProcess.Process = new StartProcess();
                    break;
                }
            }
        #endregion

        private void damagedLights_Click()
            {
            MainProcess.ClearControls();
            MainProcess.Process = new DamagedLightsRegistration();
            }

        private void unit_Click()
            {
            MainProcess.ClearControls();
            MainProcess.Process = new AccessoryRegistration(TypeOfAccessories.ElectronicUnit);
            }

        private void groupRegistration_Click()
            {
            MainProcess.ClearControls();
            MainProcess.Process = new AccessoriesGroupRegistration();
            }

        private void lamp_Click()
            {
            MainProcess.ClearControls();
            MainProcess.Process = new AccessoryRegistration(TypeOfAccessories.Lamp);
            }

        private void case_Click()
            {
            MainProcess.ClearControls();
            MainProcess.Process = new AccessoryRegistration(TypeOfAccessories.Case);
            }

        }
    }