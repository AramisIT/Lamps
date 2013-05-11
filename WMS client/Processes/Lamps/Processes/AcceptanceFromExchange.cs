using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Linq;
using WMS_client.db;
using System;
using System.Text;

namespace WMS_client.Processes.Lamps
    {
    /// <summary>��������� � �����</summary>
    public class AcceptanceFromExchange : BusinessProcess
        {
        #region Properties, variables, consts
        /// <summary>�������� (������; ������(��������; ��������))</summary>
        private readonly Dictionary<long, List<KeyValuePair<long, string>>> accepted;
        /// <summary>����� ���������</summary>
        private readonly string docName;
        /// <summary>����� �������� �������</summary>
        private readonly string tableName;
        /// <summary>³������� �������</summary>
        private MobileTable visualTable;
        /// <summary>������� �����</summary>
        private DataTable sourceTable;
        /// <summary>������ ������</summary>
        private DataRow selectedRow;
        /// <summary>Id ������ ��������</summary>
        private long selectedAcceptanceId;
        /// <summary>Id ������ �����</summary>
        private long selectedModelId;
        /// <summary>������� "Id"</summary>
        private const string ACC_ID_COLUMN = "Id";
        /// <summary>������� "Description"</summary>
        private const string DESCRIPTION_COLUMN = "Description";
        /// <summary>������� "NomenclatureId"</summary>
        private const string MODEL_ID_COLUMN = "NomenclatureId";
        /// <summary>������� "Amount"</summary>
        private const string AMOUNT_COLUMN = "Amount";
        #endregion

        /// <summary>��������� � �����</summary>
        /// <param name="MainProcess"></param>
        /// <param name="topic">���������</param>
        /// <param name="doc">����� ���������</param>
        /// <param name="table">����� �������� �������</param>
        public AcceptanceFromExchange(WMSClient MainProcess, string topic, string doc, string table)
            : base(MainProcess, 1)
            {
            accepted = new Dictionary<long, List<KeyValuePair<long, string>>>();

            MainProcess.ToDoCommand = topic;
            docName = doc;
            tableName = table;
            IsLoad = true;
            DrawControls();
            }

        #region Override methods
        public override sealed void DrawControls()
            {
            if (IsLoad)
                {
                sourceTable = new DataTable();
                sourceTable.Columns.AddRange(new[]
                                                 {
                                                     new DataColumn(ACC_ID_COLUMN, typeof (long)),
                                                     new DataColumn(DESCRIPTION_COLUMN, typeof (string)),
                                                     new DataColumn(AMOUNT_COLUMN, typeof (int)),
                                                     new DataColumn(MODEL_ID_COLUMN, typeof (long))
                                                 });
                visualTable = MainProcess.CreateTable("Accessories", 205);
                visualTable.DT = sourceTable;
                visualTable.AddColumn("�������������", DESCRIPTION_COLUMN, 184);
                visualTable.AddColumn("���-��", AMOUNT_COLUMN, 30);
                visualTable.OnChangeSelectedRow += visualTable_OnChangeSelectedRow;
                SqlCeDataReader reader = GetData();

                while (reader.Read())
                    {
                    long id = Convert.ToInt64(reader[ACC_ID_COLUMN]);
                    string description = reader[DESCRIPTION_COLUMN].ToString().TrimEnd();
                    long nomenclatureId = Convert.ToInt64(reader[MODEL_ID_COLUMN]);

                    visualTable.AddRow(id, description, 0, nomenclatureId);
                    }

                visualTable.Focus();
                MainProcess.CreateButton("��", 15, 275, 210, 35, "ok", ok_Click);
                }
            }

        public override void OnBarcode(string Barcode)
            {
            if (Barcode.IsValidBarcode())
                {
                if (!accepted.ContainsKey(selectedModelId))
                    {
                    accepted.Add(selectedModelId, new List<KeyValuePair<long, string>>());
                    }
                else
                    {
                    //�� ����� �������� ������ � ��������� ����������
                    if (accepted[selectedModelId].Any(a => a.Value == Barcode))
                        {
                        ShowMessage("����� �������� ��� ��� ������������!");
                        return;
                        }
                    }

                //���������� ������
                accepted[selectedModelId].Add(new KeyValuePair<long, string>(selectedAcceptanceId, Barcode));

                //��������� ������� � �������
                int amount = (int)selectedRow[AMOUNT_COLUMN];
                selectedRow[AMOUNT_COLUMN] = ++amount;
                }
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

        #region Implemention of Events
        /// <summary>������������ ��������</summary>
        void visualTable_OnChangeSelectedRow(object sender, OnChangeSelectedRowEventArgs e)
            {
            selectedRow = e.SelectedRow;
            selectedModelId = Convert.ToInt64(selectedRow[MODEL_ID_COLUMN]);
            selectedAcceptanceId = Convert.ToInt64(selectedRow[ACC_ID_COLUMN]);
            }

        /// <summary>���������� �������</summary>
        private void ok_Click()
            {
            Accept();
            MainProcess.ClearControls();
            MainProcess.Process = new SelectingLampProcess(MainProcess);
            }
        #endregion

        #region Query
        /// <summary>�������� ��� ��� ���������� �������</summary>
        private SqlCeDataReader GetData()
            {
            string command = string.Format(
                @"SELECT DISTINCT c.Id {0}, m.Description {1}, m.Id {2}
FROM {3} d
JOIN {4} c ON RTRIM(CAST(c.Id AS NCHAR(10)))=RTRIM({5}) 
JOIN Models m ON m.Id=c.Nomenclature
WHERE d.{6}=1",
                ACC_ID_COLUMN, DESCRIPTION_COLUMN, MODEL_ID_COLUMN,
                docName, tableName, dbObject.BARCODE_NAME, dbObject.IS_SYNCED);

            SqlCeCommand query = dbWorker.NewQuery(command);
            return query.ExecuteReader();
            }

        /// <summary>���������� ���������� �� ���������� ��������</summary>
        private void Accept()
            {
            StringBuilder whereClause = new StringBuilder();
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            if (accepted.Count > 0)
                {
                int index = 0;
                whereClause.Append("AND (1=0");

                //Data
                foreach (KeyValuePair<long, List<KeyValuePair<long, string>>> row in accepted)
                    {
                    foreach (KeyValuePair<long, string> v in row.Value)
                        {
                        string currParameter = string.Concat(dbSynchronizer.PARAMETER, index++);

                        whereClause.AppendFormat(" OR RTRIM({0})=RTRIM(@{1})", dbObject.BARCODE_NAME, currParameter);
                        parameters.Add(currParameter, v.Key.ToString());

                        AcceptanceAccessoriesFromExchangeDetails details = new AcceptanceAccessoriesFromExchangeDetails
                                                                               {
                                                                                   Id = v.Key,
                                                                                   BarCode = v.Value,
                                                                                   Nomenclature = (int)row.Key
                                                                               };
                        details.Save(false);
                        }
                    }

                whereClause.Append(")");
                }

            //Doc
            string command = string.Format("UPDATE {0} SET {1}=0 WHERE 1=1 {2}",
                                           docName, dbObject.IS_SYNCED, whereClause);
            SqlCeCommand query = dbWorker.NewQuery(command);
            query.AddParameters(parameters);
            query.ExecuteNonQuery();

            }
        #endregion
        }
    }