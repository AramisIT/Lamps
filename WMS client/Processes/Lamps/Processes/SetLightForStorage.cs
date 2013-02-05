using System.Drawing;
using WMS_client.Enums;
using WMS_client.db;

namespace WMS_client.Processes.Lamps
{
    /// <summary>��������� ��������� �� ���������</summary>
    public class SetLightForStorage : BusinessProcess
    {
        /// <summary>�������� �����������</summary>
        private readonly string LightBarcode;

        /// <summary>��������� ��������� �� ���������</summary>
        public SetLightForStorage(WMSClient MainProcess, string lightBarcode)
            : base(MainProcess, 1)
        {
            LightBarcode = lightBarcode;
        }

        #region Override methods
        public override sealed void DrawControls()
        {
            MainProcess.CreateLabel("��������� ���� ����������� �� ���������!", 5, 105, 230, 65, MobileFontSize.Multiline, 
                MobileFontPosition.Center, MobileFontColors.Default, FontStyle.Bold);
            MainProcess.CreateLabel("�������� ���?", 5, 190, 230,
                                    MobileFontSize.Large, MobileFontPosition.Center, MobileFontColors.Info);

            MainProcess.CreateButton("��", 10, 275, 105, 35, "ok", Save_click);
            MainProcess.CreateButton("³����", 125, 275, 105, 35, "cancel", Cancel_click);
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
            Cases.ChangeLighterStatus(LightBarcode, TypesOfLampsStatus.Storage, true);
            OnHotKey(KeyAction.Esc);
        }
    }
}