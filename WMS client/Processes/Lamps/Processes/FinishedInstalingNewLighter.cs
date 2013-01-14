using WMS_client.Base.Visual.Constructor;
using System.Collections.Generic;
using WMS_client.Enums;
using WMS_client.db;
using System.Data.SqlServerCe;

namespace WMS_client
{
    /// <summary>���������� ��������� �����������</summary>
    public class FinishedInstalingNewLighter : BusinessProcess
    {
        /// <summary>�������� �����������</summary>
        private readonly string LightBarcode;
        /// <summary>�� ����� �� ������� ���������������� ����������</summary>
        private readonly object MapId;

        /// <summary>���������� ��������� �����������</summary>
        /// <param name="MainProcess"></param>
        /// <param name="parameters">����: �����, ������, �������</param>
        /// <param name="mapId">�� ����� �� ������� ���������������� ����������</param>
        /// <param name="lampBarCode">�������� �����������</param>
        public FinishedInstalingNewLighter(WMSClient MainProcess, object[] parameters, object mapId, string lampBarCode)
            : base(MainProcess, 1)
        {
            Parameters = parameters;
            FormNumber = 1;
            BusinessProcessType = ProcessType.Registration;
            MapId = mapId;

            LightBarcode = lampBarCode;

            IsLoad = true;
            DrawControls();
        }

        #region Override methods
        public override sealed void DrawControls()
        {
            if (IsLoad)
            {
                ListOfLabelsConstructor list = new ListOfLabelsConstructor(MainProcess, "������������ �²��������", Parameters);
                list.ListOfLabels = new List<LabelForConstructor>
                                        {
                                            new LabelForConstructor(string.Empty, ControlsStyle.LabelH2),
                                            new LabelForConstructor(string.Empty, ControlsStyle.LabelH2),
                                            new LabelForConstructor(string.Empty, ControlsStyle.LabelH2),
                                            new LabelForConstructor("̲��� ������������:", ControlsStyle.LabelH2),
                                            new LabelForConstructor("�����: {0}"),
                                            new LabelForConstructor("������: {0}"),
                                            new LabelForConstructor("������� �{0}")
                                        };

                MainProcess.CreateButton("O�", 10, 275, 105, 35, "ok", Ok_click);
                MainProcess.CreateButton("³����", 125, 275, 105, 35, "cancel", Cancel_click);
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

        #region ButtonClick

        /// <summary>����������</summary>
        private void Ok_click()
        {
            FinishedInstaling();
            MainProcess.ClearControls();
            MainProcess.Process = new SelectingLampProcess(MainProcess);
        }

        /// <summary>�����</summary>
        private void Cancel_click()
        {
            MainProcess.ClearControls();
            MainProcess.Process = new SelectingLampProcess(MainProcess);
        }
        #endregion

        #region Query
        /// <summary>���������� ���������� �����������</summary>
        public void FinishedInstaling()
        {
            Cases.ChangeLighterStatus(LightBarcode, TypesOfLampsStatus.IsWorking, false);

            SqlCeCommand query = dbWorker.NewQuery(
                    "UPDATE Cases SET Map=@Map,Register=@Register,Position=@Position WHERE RTRIM(Barcode)=RTRIM(@Barcode)");
            query.AddParameter("Map", MapId);
            query.AddParameter("Register", Parameters[1]);
            query.AddParameter("Position", Parameters[2]);
            query.AddParameter("Barcode", LightBarcode);
            query.ExecuteNonQuery();

            //�������� ������ � "�����������"
            Movement movement = new Movement(LightBarcode, OperationsWithLighters.Installing);
            movement.Save();
        }
        #endregion
    }
}

