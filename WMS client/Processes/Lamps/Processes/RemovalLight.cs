using WMS_client.Base.Visual.Constructor;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using WMS_client.Enums;
using WMS_client.db;
using System;

namespace WMS_client
{
    /// <summary>�������� �����������</summary>
    public class RemovalLight : BusinessProcess
    {
        /// <summary>�������� ����������</summary>
        private readonly string LightBarcode;
        /// <summary>�� ����� � ��� ������</summary>
        private int map;
        /// <summary>����� ������� � ��� ������</summary>
        private int position;
        /// <summary>����� ������� � ����� ������</summary>
        private int register;

        /// <summary>�������� �����������</summary>
        public RemovalLight(WMSClient MainProcess, string lightBarcode)
            : base(MainProcess, 1)
        {
            LightBarcode = lightBarcode;

            IsLoad = true;
            DrawControls();
        }

        #region Override methods
        public override sealed void DrawControls()
        {
            if (IsLoad)
            {
                object[] data = getLightPositionInfo();
                map = Convert.ToInt32(data[1]);
                register = Convert.ToInt32(data[2]);
                position = Convert.ToInt32(data[3]);

                ListOfLabelsConstructor list = new ListOfLabelsConstructor(MainProcess, "�������� �²��������", data);
                list.ListOfLabels = new List<LabelForConstructor>
                                        {
                                            new LabelForConstructor(string.Empty, ControlsStyle.LabelH2),
                                            new LabelForConstructor(string.Empty, ControlsStyle.LabelH2),
                                            new LabelForConstructor(string.Empty, ControlsStyle.LabelH2),
                                            new LabelForConstructor("������������:", ControlsStyle.LabelH2),
                                            new LabelForConstructor("�����: {0}"),
                                            new LabelForConstructor("������: {0}", 1),
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
        /// <summary>���������� ��������. ���������� ����������</summary>
        private void Ok_click()
        {
            finish();
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
        /// <summary>������� �����������</summary>
        /// <returns>�����, Register, Position</returns>
        private object[] getLightPositionInfo()
        {
            SqlCeCommand query = dbWorker.NewQuery(@"SELECT m.Description, c.Map MapId, c.Register, c.Position 
FROM Cases c
LEFT JOIN Maps m ON m.Id=c.Map
WHERE RTRIM(c.Barcode)=@Barcode");
            query.AddParameter("Barcode", LightBarcode);
            return query.SelectArray();
        }

        /// <summary>���������� (����������)</summary>
        private void finish()
        {
            Cases.ChangeLighterState(LightBarcode, TypesOfLampsStatus.Storage, true);

            SqlCeCommand query = dbWorker.NewQuery("SELECT SyncRef FROM Cases WHERE RTRIM(Barcode)=RTRIM(@Barcode)");
            query.AddParameter("Barcode", LightBarcode);
            object syncRefObj = query.ExecuteScalar();
            string syncRef = syncRefObj == null ? string.Empty : syncRefObj.ToString();

            //�������� ������ � "�����������"
            Movement.RegisterLighter(LightBarcode, syncRef, OperationsWithLighters.Removing, map, register, position);
        }
        #endregion
    }
}

