using WMS_client.db;
namespace WMS_client
    {
    /// <summary>Вибір типу комплектуючого для регістрації (редагування)</summary>
    public class EditSelector : BusinessProcess
        {
        /// <summary>Вибір типу комплектуючого для регістрації (редагування)</summary>
        public EditSelector(WMSClient MainProcess)
            : base(MainProcess, 1)
            {
            }

        #region Override methods
        public override sealed void DrawControls()
            {
            MainProcess.ToDoCommand = "Оберіть комлектуюче";
            MainProcess.CreateButton("Електронний блок", 20, 75, 200, 45, "unit", unit_Click);
            MainProcess.CreateButton("Лампа", 20, 150, 200, 45, "lamp", lamp_Click);
            MainProcess.CreateButton("Корпус", 20, 225, 200, 45, "case", case_Click);
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

        #region Перехід на регестрацію (редагування) конкретного типу комплектуючого
        /// <summary>Ел.блок</summary>
        private void unit_Click()
            {
            MainProcess.ClearControls();
            MainProcess.Process = new AccessoryRegistration(MainProcess, typeof(ElectronicUnits), null, "Електронний блок");
            }

        /// <summary>Лампа</summary>
        private void lamp_Click()
            {
            MainProcess.ClearControls();
            MainProcess.Process = new AccessoryRegistration(MainProcess, typeof(Lamps), null, "Лампа");
            }

        /// <summary>Корпус</summary>
        private void case_Click()
            {
            MainProcess.ClearControls();
            MainProcess.Process = new AccessoryRegistration(MainProcess, typeof(Cases), null, "Корпус");
            }
        #endregion
        }
    }