using WMS_client.db;
using WMS_client.Enums;

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
            MainProcess.CreateButton("Електронний блок", 10, 80, 220, 40, "unit", unit_Click);
            MainProcess.CreateButton("Лампа", 10, 140, 220, 40, "lamp", lamp_Click);
            MainProcess.CreateButton("Корпус", 10, 200, 220, 40, "case", case_Click);
            MainProcess.CreateButton("Групова реєстрація комплектів", 10, 260, 220, 40, "case", groupRegistration_Click);
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
                    MainProcess.Process = new StartProcess(MainProcess);
                    break;
                }
            }
        #endregion

        #region Перехід на регестрацію (редагування) конкретного типу комплектуючого
        /// <summary>Ел.блок</summary>
        private void unit_Click()
            {
            MainProcess.ClearControls();
            MainProcess.Process = new AccessoryRegistration(MainProcess, TypeOfAccessories.ElectronicUnit);
            }

        private void groupRegistration_Click()
            {
            MainProcess.ClearControls();
            MainProcess.Process = new AccessoriesGroupRegistration(MainProcess);
            }

        /// <summary>Лампа</summary>
        private void lamp_Click()
            {
            MainProcess.ClearControls();
            MainProcess.Process = new AccessoryRegistration(MainProcess, TypeOfAccessories.Lamp);
            }

        /// <summary>Корпус</summary>
        private void case_Click()
            {
            MainProcess.ClearControls();
            MainProcess.Process = new AccessoryRegistration(MainProcess, TypeOfAccessories.Case);
            }
        #endregion
        }
    }