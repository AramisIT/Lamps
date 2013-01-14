using System;
using System.Collections.Generic;
using WMS_client.Base.Visual.Constructor;
using System.Data.SqlServerCe;
using WMS_client.Enums;
using WMS_client.Processes.Lamps;
using WMS_client.db;

namespace WMS_client
{
    /// <summary>������ ����</summary>
    public class ChooseUnit : BusinessProcess
    {
        /// <summary>�������� �����</summary>
        private readonly string UnitBarcode;

        /// <summary>������ ����</summary>
        /// <param name="MainProcess"></param>
        /// <param name="unitBarcode">�������� �����</param>
        public ChooseUnit(WMSClient MainProcess, string unitBarcode)
            : base(MainProcess, 1)
        {
            FormNumber = 1;
            BusinessProcessType = ProcessType.Registration;
            UnitBarcode = unitBarcode;

            IsLoad = true;
            DrawControls();
        }

        #region Override methods
        public override sealed void DrawControls()
        {
            if (IsLoad)
            {
                ListOfLabelsConstructor list = new ListOfLabelsConstructor(MainProcess, "��. ����", getData());
                list.ListOfLabels = new List<LabelForConstructor>
                                        {
                                            new LabelForConstructor(string.Empty, false),
                                            new LabelForConstructor(string.Empty, false),
                                            new LabelForConstructor(string.Empty, false),
                                            new LabelForConstructor(string.Empty, false),
                                            new LabelForConstructor("������: {0}"),
                                            new LabelForConstructor("�����: {0}"),
                                            new LabelForConstructor("������� �� {0}")
                                        };

                MainProcess.CreateButton("������", 15, 275, 100, 35, "repair", Repair_Click);
                MainProcess.CreateButton("��������", 125, 275, 100, 35, "writeoff", Writeoff_Click);
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
        /// <summary>������</summary>
        private void Repair_Click()
        {
            MainProcess.ClearControls();
            MainProcess.Process = new RepairUnit(MainProcess, UnitBarcode);
        }

        /// <summary>��������</summary>
        private void Writeoff_Click()
        {
        }
        #endregion

        #region Query
        /// <summary>���� � �����</summary>
        private object[] getData()
        {
            SqlCeCommand query = dbWorker.NewQuery(@"SELECT
    t.Description Model
    , p.Description Party
    , e.DateOfWarrantyEnd Warrantly
FROM ElectronicUnits e
LEFT JOIN Models t ON t.Id=e.Model
LEFT JOIN Party p ON p.Id=e.Party
WHERE RTRIM(e.BarCode)=RTRIM(@BarCode)");
            query.AddParameter("BarCode", UnitBarcode);

            return query.SelectArray(new Dictionary<string, Enum> {{BaseFormatName.DateTime, DateTimeFormat.OnlyDate}});
        }
        #endregion
    }
}