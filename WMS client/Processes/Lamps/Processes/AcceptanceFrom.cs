using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Text;
using WMS_client.db;

namespace WMS_client.Processes.Lamps
    {
    /// <summary>Приймання з ...</summary>
    public class AcceptanceFrom : BusinessProcess
        {
        /// <summary>Строки (штрихкод; строка в таблиці)</summary>
        private readonly Dictionary<string, DataRow> rows;
        /// <summary>Список прийнятих штрихкодів</summary>
        private readonly List<string> accepted;
        /// <summary>Назва табличної частини документу</summary>
        private readonly string tableName;
        /// <summary>Візуальна таблиця</summary>
        private MobileTable visualTable;
        /// <summary>Таблиця з даними</summary>
        private DataTable sourceTable;

        /// <summary>Приймання з ...</summary>
        /// <param name="MainProcess"></param>
        /// <param name="topic">Заголовок (з чого?)</param>
        /// <param name="table">Назва табличної частини документу</param>
        public AcceptanceFrom(WMSClient MainProcess, string topic, string table)
            : base(MainProcess, 1)
            {
            rows = new Dictionary<string, DataRow>();
            accepted = new List<string>();

            MainProcess.ToDoCommand = topic;
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
            //Якщо штрихкод наявний в таблиці - прийняти комплектуюче
            if (Barcode.IsAccessoryBarcode())
                {
                if (rows.ContainsKey(Barcode))
                    {
                    accepted.Add(Barcode);
                    sourceTable.Rows.Remove(rows[Barcode]);
                    rows.Remove(Barcode);
                    }

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

        /// <summary>Отримати дані для заповлення таблиці</summary>
        private SqlCeDataReader GetData()
            {
            string command = string.Format(@"SELECT DISTINCT c.Document Id
FROM {0} c 
WHERE c.{1}=1", tableName, dbObject.IS_SYNCED);

            using (SqlCeCommand query = dbWorker.NewQuery(command))
                {
                return query.ExecuteReader();
                }
            }

        /// <summary>Збереження інформації по прийомці</summary>
        private void Accept()
            {
            StringBuilder command = new StringBuilder();
            command.AppendFormat("UPDATE {0} SET {1}=0 WHERE 1=0", tableName, dbObject.IS_SYNCED);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            int index = 0;

            foreach (string a in accepted)
                {
                command.AppendFormat(" OR Document=@{0}{1}", dbSynchronizer.PARAMETER, index);
                parameters.Add(string.Concat(dbSynchronizer.PARAMETER, index), a);
                index++;
                }

            using (SqlCeCommand query = dbWorker.NewQuery(command.ToString()))
                {
                query.AddParameters(parameters);
                query.ExecuteNonQuery();
                }
            }

        #endregion
        }
    }