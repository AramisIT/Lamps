using WMS_client.db;
using System.Data.SqlServerCe;
using WMS_client.Enums;
using System.Data;
using System.Collections.Generic;
using System.Text;

namespace WMS_client.Processes.Lamps
    {
    /// <summary>Прийомка з ...</summary>
    public class AcceptionSendingDocs : BusinessProcess
        {
        #region Properties
        /// <summary>Тип комплектуючого</summary>
        private readonly TypeOfAccessories typeOfAccessory;
        /// <summary>Строки (Штрихкод; Строка в таблиці)</summary>
        private readonly Dictionary<string, DataRow> rows;
        /// <summary>Список принятих штрихкодів</summary>
        private readonly List<string> accepted;
        /// <summary>Ім'я табличної частини</summary>
        private readonly string subTableName;
        /// <summary>Візуальна таблиця</summary>
        private MobileTable visualTable;
        /// <summary>Таблиця з даними</summary>
        private DataTable sourceTable;
        #endregion

        /// <summary>Прийомка з ...</summary>
        /// <param name="MainProcess"></param>
        /// <param name="type">Тип комплектуючого</param>
        public AcceptionSendingDocs(WMSClient MainProcess, TypeOfAccessories type)
            : base(MainProcess, 1)
            {
            rows = new Dictionary<string, DataRow>();
            accepted = new List<string>();

            typeOfAccessory = type;
            IsLoad = true;
            DrawControls();
            }

        /// <summary>Прийомка з ...</summary>
        /// <param name="MainProcess"></param>
        /// <param name="topic">Заголовок</param>
        /// <param name="type">Тип комплектуючого</param>
        /// <param name="subTableName">Ім'я табличної частини</param>
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

                string command = string.Format(@"SELECT DISTINCT c.Document Id
FROM {0} c 
WHERE c.TypeOfAccessory=@Type AND c.{1}=1", subTableName, dbObject.IS_SYNCED);


                using (SqlCeCommand query = dbWorker.NewQuery(command))
                    {
                    query.AddParameter("Type", typeOfAccessory);

                    // Отримати дані для заповнення в таблиці
                    using (SqlCeDataReader reader = query.ExecuteReader())
                        {
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
                }
            }

        public override void OnBarcode(string Barcode)
            {
            //Якщо такий штрихкод наявний у таблиці
            if (Barcode.IsAccessoryBarcode() && rows.ContainsKey(Barcode))
                {
                //Прийняти
                accepted.Add(Barcode);
                //Видалити з візуальної таблиці
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
        /// <summary>Завершення процесу</summary>
        private void ok_Click()
            {
            Accept();
            MainProcess.ClearControls();
            MainProcess.Process = new SelectingLampProcess(MainProcess);
            }
        #endregion

        #region Query

        /// <summary>Збереження інформації</summary>
        private void Accept()
            {
            StringBuilder command = new StringBuilder();
            command.AppendFormat("UPDATE {0} SET {1}=0 WHERE 1=0", subTableName, dbObject.IS_SYNCED);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            int index = 0;

            foreach (string a in accepted)
                {
                command.AppendFormat(" OR Document=@{0}{1}", SynchronizerWithGreenhouse.PARAMETER, index);
                parameters.Add(string.Concat(SynchronizerWithGreenhouse.PARAMETER, index), a);
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