using WMS_client.db;
namespace WMS_client
{
    /// <summary>Выбор типа комплектующего для регистрации (редактирования)</summary>
    public class EditSelector : BusinessProcess
    {
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

        #region Переход на регестрацию(редактирование) конкретного типа комплектующего
        private void unit_Click()
        {
            MainProcess.ClearControls();
            MainProcess.Process = new EditBuilder(MainProcess, typeof(ElectronicUnits), null, "Електронний блок");
        }

        private void lamp_Click()
        {
            MainProcess.ClearControls();
            MainProcess.Process = new EditBuilder(MainProcess, typeof(Lamps), null, "Лампа");
        }

        private void case_Click()
        {
            MainProcess.ClearControls();
            MainProcess.Process = new EditBuilder(MainProcess, typeof(Cases), null, "Корпус");
        }
        #endregion
    }
}