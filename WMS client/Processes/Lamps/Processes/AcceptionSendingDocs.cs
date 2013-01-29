using WMS_client.db;
using System.Data.SqlServerCe;
using WMS_client.Enums;
using System.Data;
using System.Collections.Generic;
using System.Text;

namespace WMS_client.Processes.Lamps
{
    public class AcceptionSendingDocs : BusinessProcess
    {
        private readonly TypeOfAccessories typeOfAccessory;
        private readonly Dictionary<string, DataRow> rows;
        private readonly List<string> accepted;
        private readonly string subTableName;
        private MobileTable visualTable;
        private DataTable sourceTable;

        public AcceptionSendingDocs(WMSClient MainProcess, TypeOfAccessories type)
            : base(MainProcess, 1)
        {
            rows = new Dictionary<string, DataRow>();
            accepted = new List<string>();

            typeOfAccessory = type;
            IsLoad = true;
            DrawControls();
        }

        public AcceptionSendingDocs(WMSClient MainProcess, string topic, TypeOfAccessories type, string subTableName)
            : base(MainProcess, 1)
        {
            rows = new Dictionary<string, DataRow>();
            accepted = new List<string>();
            this.subTableName = subTableName;

            MainProcess.ToDoCommand = topic;
            typeOfAccessory = type;
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
                                                     new DataColumn("Description", typeof (string))
                                                 });
                visualTable = MainProcess.CreateTable("Accessories", 205);
                visualTable.DT = sourceTable;
                visualTable.AddColumn("Комплектующее", "Description", 214);
                SqlCeDataReader reader = GetData();

                while (reader.Read())
                {
                    string id = reader["Id"].ToString().TrimEnd();
                    DataRow row = visualTable.AddRow(id);

                    rows.Add(id, row);
                }

                visualTable.Focus();

                MainProcess.CreateButton("Ок", 15, 275, 210, 35, "ok", ok_Click);
            }
        }



        public override void OnBarcode(string Barcode)
        {
            if(rows.ContainsKey(Barcode))
            {
                accepted.Add(Barcode);
                sourceTable.Rows.Remove(rows[Barcode]);
                rows.Remove(Barcode);
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

        #region ButtonClick
        private void ok_Click()
        {
            Accept();
            MainProcess.ClearControls();
            MainProcess.Process = new SelectingLampProcess(MainProcess);
        }
        #endregion

        #region Query
        private SqlCeDataReader GetData()
        {
            string command = string.Format(@"SELECT DISTINCT c.Document Id
FROM {0} c 
WHERE c.TypeOfAccessory=@Type AND c.IsSynced=1", subTableName);
            SqlCeCommand query = dbWorker.NewQuery(command);
            query.AddParameter("Type", typeOfAccessory);
            return query.ExecuteReader();
        }

        private void Accept()
        {
            StringBuilder command = new StringBuilder();
            command.AppendFormat("UPDATE {0} SET IsSynced=0 WHERE 1=0", subTableName);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            int index = 0;

            foreach (string a in accepted)
            {
                command.AppendFormat(" OR Document=@{0}{1}", dbSynchronizer.PARAMETER, index);
                parameters.Add(string.Concat(dbSynchronizer.PARAMETER, index), a);
                index++;
            }

            SqlCeCommand query = dbWorker.NewQuery(command.ToString());
            query.AddParameters(parameters);
            query.ExecuteNonQuery();
        }
        #endregion
    }
}