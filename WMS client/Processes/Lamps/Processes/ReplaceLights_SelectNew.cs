using System;
using System.Collections.Generic;
using WMS_client.Base.Visual.Constructor;
using WMS_client.Enums;
using WMS_client.db;
using System.Data.SqlServerCe;

namespace WMS_client
{
    /// <summary>�����/������������ ���������� �� ������</summary>
    public class ReplaceLights_SelectNew : BusinessProcess
    {
        /// <summary>�������� ������ ����������</summary>
        private readonly string NewLampBarCode;
        /// <summary>�������� ���������(��� �������������) ����������</summary>
        private readonly string ExistLampBarCode;

        /// <summary>�����/������������ ���������� �� ������</summary>
        /// <param name="MainProcess"></param>
        /// <param name="newLampBarCode">�������� ������ ����������</param>
        /// <param name="existLampBarCode">�������� ���������(��� �������������) ����������</param>
        public ReplaceLights_SelectNew(WMSClient MainProcess, string newLampBarCode, string existLampBarCode)
            : base(MainProcess, 1)
        {
            BusinessProcessType = ProcessType.Registration;
            FormNumber = 1;
            NewLampBarCode = newLampBarCode;
            ExistLampBarCode = existLampBarCode;
            IsLoad = true;
            DrawControls();
        }

        #region Override methods
        public override sealed void DrawControls()
        {
            if (IsLoad)
            {
                object[] parameters = GetNewIlluminatorInfo();
                ListOfLabelsConstructor list = new ListOfLabelsConstructor(MainProcess, "����� ����������",
                                                                         parameters);
                list.ListOfLabels = new List<LabelForConstructor>
                                        {
                                            new LabelForConstructor(string.Empty, false),
                                            new LabelForConstructor(string.Empty, false),
                                            new LabelForConstructor("������", ControlsStyle.LabelH2),
                                            new LabelForConstructor("������: {0}"),
                                            new LabelForConstructor("�����: {0}"),
                                            new LabelForConstructor("������� �� {0}"),
                                            new LabelForConstructor("�����������", ControlsStyle.LabelH2),
                                            new LabelForConstructor("�����: {0}"),
                                            new LabelForConstructor("������� �� {0}",1),
                                        };

                MainProcess.CreateButton("��", 20, 275, 200, 35, "ok", Ok);
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
                    {
                        MainProcess.ClearControls();
                        MainProcess.Process = new SelectingLampProcess(MainProcess);
                        break;
                    }
            }
        }
        #endregion

        #region ButtonClick
        /// <summary>�������</summary>
        private void Ok()
        {
            finishingReplaceLamps();
            MainProcess.ClearControls();
            MainProcess.Process = new SelectingLampProcess(MainProcess);
        }
        #endregion

        #region Query
        /// <summary>���������� ��� ����� ���������</summary>
        private object[] GetNewIlluminatorInfo()
        {
            SqlCeCommand query = dbWorker.NewQuery(@"
SELECT CaseModel,CaseParty,CaseWarrantly
FROM(
    SELECT 
	    0 Type
        , t.Description CaseModel
	    , p.Description CaseParty
	    , c.DateOfWarrantyEnd CaseWarrantly
    FROM Cases c
    LEFT JOIN Models t ON t.Id=c.Model
    LEFT JOIN Party p ON p.Id=c.Party
    WHERE c.BarCode=@BarCode

    UNION 

    SELECT 
	    1 Type
	    , '' CaseModel
	    , p.Description CaseParty
	    , u.DateOfWarrantyEnd CaseWarrantly
    FROM Cases c
    LEFT JOIN ElectronicUnits u ON u.Id=c.ElectronicUnit
    LEFT JOIN Models t ON t.Id=u.Model
    LEFT JOIN Party p ON p.Id=u.Party
    WHERE c.BarCode=@BarCode)t
ORDER BY Type");
            query.AddParameter("BarCode", NewLampBarCode);

            return query.SelectArray(new Dictionary<string, Enum> {{BaseFormatName.DateTime, DateTimeFormat.OnlyDate}});
        }

        /// <summary>���������� �����</summary>
        private void finishingReplaceLamps()
        {
            SqlCeCommand query = dbWorker.NewQuery(@"SELECT Map, Register, Position, Status, SyncRef FROM Cases WHERE RTRIM(BarCode)=RTRIM(@Old)");
            query.AddParameter("Old", ExistLampBarCode);
            object[] result = query.SelectArray();

            if (result != null)
            {
                int map = Convert.ToInt32(result[0]);
                int register = Convert.ToInt32(result[1]);
                int position = Convert.ToInt32(result[2]);

                Cases.ChangeLighterState(NewLampBarCode, TypesOfLampsStatus.IsWorking, false, map, register, position);
                Cases.ChangeLighterState(ExistLampBarCode, TypesOfLampsStatus.Storage, true);

                //�������� ������ � "�����������"
                string newLampRef = BarcodeWorker.GetRefByBarcode(typeof(Cases), NewLampBarCode);
                Movement.RegisterLighter(ExistLampBarCode, result[4].ToString(), OperationsWithLighters.Removing,
                                         map, register, position);
                Movement.RegisterLighter(NewLampBarCode, newLampRef, OperationsWithLighters.Installing,
                                         map, register, position);
            }
        }
        #endregion
    }
}

