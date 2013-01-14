using WMS_client.Enums;
using WMS_client.db;

namespace WMS_client.Processes.Lamps
{
    /// <summary>"Процессы"</summary>
    public class Processes : BusinessProcess
    {
        /// <summary>Кроки</summary>
        private enum Stages { First, Acceptance, Writeoff, Repair, Exchange }
        /// <summary>Поточний крок</summary>
        private Stages Stage;

        /// <summary>"Процессы"</summary>
        public Processes(WMSClient MainProcess)
            : base(MainProcess, 1)
        {
            IsLoad = true;
            DrawControls();
        }

        #region Override methods
        public override sealed void DrawControls()
        {
            MainProcess.ClearControls();

            switch (Stage)
            {
                case Stages.First:
                    MainProcess.ToDoCommand = "Оберіть процесс";
                    MainProcess.CreateButton("Прийомка", 10, 75, 220, 35, string.Empty, acceptance_Click);
                    MainProcess.CreateButton("Списання", 10, 120, 220, 35, string.Empty, writeoff_Click);
                    MainProcess.CreateButton("Ремонт", 10, 165, 220, 35, string.Empty, repair_Click);
                    MainProcess.CreateButton("Обмін", 10, 210, 220, 35, string.Empty, exchange_Click);
                    break;
                case Stages.Acceptance:
                    drawAccessoriesBtn("Приймання", acceptance_Click);
                    break;
                case Stages.Writeoff:
                    drawAccessoriesBtn("Списання", writeoff_Click);
                    break;
                case Stages.Repair:
                    drawAccessoriesBtn("Ремонт", repair_Click);
                    break;
                    case Stages.Exchange:
                    drawAccessoriesBtn("Приймання", acceptance_Click);
                    break;
            }
        }

        /// <summary>Отображение кнопок комплектующих</summary>
        /// <param name="process">Строковая приставка процесса</param>
        /// <param name="click">Делегат действия при клике</param>
        private void drawAccessoriesBtn(string process, MobileSenderClick click)
        {
            MainProcess.ToDoCommand = "Оберіть тип комлектуючого";
            MainProcess.CreateButton("Лампа", 10, 75, 220, 35, string.Empty, click,
                                     new object[] {TypeOfAccessories.Lamp, string.Concat(process, " ламп")});
            MainProcess.CreateButton("Эл.блок", 10, 120, 220, 35, string.Empty, click,
                                     new object[]
                                         {TypeOfAccessories.ElectronicUnit, string.Concat(process, " эл.блоків")});
            MainProcess.CreateButton("Корпус", 10, 165, 220, 35, string.Empty, click,
                                     new object[] {TypeOfAccessories.Case, string.Concat(process, " корпусів")});
        }

        public override void OnBarcode(string Barcode)
        {
        }

        public override void OnHotKey(KeyAction TypeOfAction)
        {
            switch (TypeOfAction)
            {
                case KeyAction.Esc:
                    if (Stage == Stages.First)
                    {
                        MainProcess.ClearControls();
                        MainProcess.Process = new SelectingLampProcess(MainProcess);
                    }
                    else
                    {
                        Stage = Stages.First;
                        DrawControls();
                    }
                    break;
            }
        }
        #endregion

        #region Button
        /// <summary>Выбрано "Приемка"</summary>
        private void acceptance_Click()
        {
            Stage = Stages.Acceptance;
            DrawControls();
        }

        /// <summary>Выбрано "Списание"</summary>
        private void writeoff_Click()
        {
            Stage = Stages.Writeoff;
            DrawControls();
        }

        /// <summary>Выбрано "Ремонт"</summary>
        private void repair_Click()
        {
            Stage = Stages.Repair;
            DrawControls();
        }

        /// <summary>Выбрано "Обмен"</summary>
        private void exchange_Click()
        {
            Stage = Stages.Exchange;
            DrawControls();
        }

        /// <summary>Приемка выбранного типа комплектующего</summary>
        private void acceptance_Click(object sender)
        {
            object[] parameters = (object[])((System.Windows.Forms.Button)sender).Tag;
            TypeOfAccessories type = (TypeOfAccessories)parameters[0];
            string topic = parameters[1].ToString();

            MainProcess.ClearControls();
            MainProcess.Process = new AcceptanceOfNewAccessory(MainProcess, topic, type);
        }

        /// <summary>Списание выбранного типа комплектующего</summary>
        private void writeoff_Click(object sender)
        {
            object[] parameters = (object[])((System.Windows.Forms.Button)sender).Tag;
            TypeOfAccessories type = (TypeOfAccessories)parameters[0];
            string topic = parameters[1].ToString();

            MainProcess.ClearControls();
            MainProcess.Process = new AcceptionSendingDocs(MainProcess, topic, type, typeof(SubSendingToChargeChargeTable).Name);
        }

        /// <summary>Ремонт выбранного типа комплектующего</summary>
        private void repair_Click(object sender)
        {
            object[] parameters = (object[])((System.Windows.Forms.Button)sender).Tag;
            TypeOfAccessories type = (TypeOfAccessories)parameters[0];
            string topic = parameters[1].ToString();

            MainProcess.ClearControls();
            MainProcess.Process = new AcceptionSendingDocs(MainProcess, topic, type, typeof(SubSendingToRepairRepairTable).Name);
        }
        #endregion
    }
}