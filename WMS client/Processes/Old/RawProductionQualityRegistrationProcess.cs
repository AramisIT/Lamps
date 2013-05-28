using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlServerCe;
using System.IO;
using System.Drawing;

namespace WMS_client
{


    public class RawProductionQualityRegistrationProcess : BusinessProcess
    {

        // ПРИМЕЧАНИЕ. 
        // Поскольку в ранней версии вместо типа работы использовалась номенклатура,
        // то везде в этом коде вместо WorkType используется Nomenclature и следует понимать,
        // что на самом деле это все относится к типу работ
        struct BarcodeValue
        {
            private long nomID;
            private long depID;
            private string barcode;

            public string Value
            {
                get { return barcode; }
                set
                {
                    int asterixIndex = value.IndexOf("*");
                    if (asterixIndex > 0)
                    {
                        string nomString = value.Substring(0, asterixIndex);
                        string depString = value.Substring(asterixIndex + 1);
                        if (Number.IsNumber(nomString) && Number.IsNumber(depString))
                        {
                            nomID = Convert.ToInt64(nomString);
                            depID = Convert.ToInt64(depString);
                            barcode = value;
                        }
                        else
                        {
                            nomID = -1;
                            depID = -1;
                            barcode = "";
                        }
                    }
                }
            }

            public long DepartmentID
            {
                get { return depID; }
                set { depID = value; }
            }

            public long NomenclatureID
            {
                get { return nomID; }
            }

            public bool IsEquals(BarcodeValue b)
            {
                return nomID == b.nomID && depID == b.depID;
            }
        }

        struct ParentCriterionData
        {
            public DataRow Row
            {
                private set;
                get;
            }

            public Dictionary<long, long> SubCriterionsId
            {
                get;
                set;
            }

            public ParentCriterionData(DataRow row)
                : this()
            {
                SubCriterionsId = new Dictionary<long, long>();
                Row = row;
            }

            public void SetRow(DataRow row)
            {
                Row = row;
            }
        }

        #region Private fields

        private Dictionary<long, ParentCriterionData> ParentCriterions;

        private SqlCeEngine DBEngine;
        string connString;
        private bool isEmptyCriterions = true;

        private Label labelControl;
        private TextBox textBoxControl;

        public string Nomenclature
        {
            set
            {
                if (value.Length == 0)
                {
                    labelControl.Text = "<скан. номенклатуру>";
                }
                else
                {
                    labelControl.Text = value.Trim();
                }
            }
        }
        BarcodeValue currentBarcodeValue = new BarcodeValue();



        private MobileTable table;

        #endregion

        #region Constructor

        public RawProductionQualityRegistrationProcess(WMSClient MainProcess)
            : base(MainProcess, 1)
        {

            BusinessProcessType = ProcessType.RawProductionQualityRegistrationProcess;
            FormNumber = 1;
        }

        #endregion

        #region Override methods

        public override void DrawControls()
        {
            InitDatabase();

            if (!isEmptyCriterions)
            {
                isEmptyCriterions = ShowQuery("Выполнить обновление номенклатуры и критериев оценивания?");
            }

            if (isEmptyCriterions)
            {
                if (!UpdateRules())
                {
                    throw new ConnectionIsNotExistsException("нужно обновить критерии оценивания и номенклатуру!");
                }
            }

            MainProcess.ClearControls();

            MainProcess.ToDoCommand = "Регистрация качества";

            labelControl = (MainProcess.CreateLabel("", 5, 59, 229, 39, ControlsStyle.LabelMultilineSmall).GetControl()) as Label;
            textBoxControl = (MainProcess.CreateTextBox(0, 0, 0, "", ControlsStyle.LabelH2, onTextBoxEndEdit, false)).GetControl() as TextBox;
            textBoxControl.LostFocus += new EventHandler(textBoxControl_LostFocus);
            textBoxControl.Hide();
            Nomenclature = "";

            #region Создание рабочей таблицы

            var dataTable = new DataTable();
            dataTable.Columns.AddRange(new DataColumn[] {
                new DataColumn("CriterionName", typeof(string)),
                new DataColumn("CriterionId", typeof(long)),
                new DataColumn("Mark", typeof(int)),
                new DataColumn("HaveSubCriterion", typeof(bool)),
                new DataColumn("ParentCriterionId", typeof(long))
            });

            #endregion

            table = MainProcess.CreateTable("operations", 217, 99, onRowSelected);
            table.DT = dataTable;
            table.AddColumn("Критерий", "CriterionName", 180);
            table.AddColumn("Балл", "Mark", 34);

            table.Focus();
        }

        public override void OnBarcode(string Barcode)
        {
            textBoxControl.Hide();

            string prefix = "SB_NAC.";
            int asterixIndex = Barcode.IndexOf("*");
            string barcodePrefix = "";
            BarcodeValue barcodeValue = new BarcodeValue();

            if (Barcode.Length > 7)
            {
                barcodePrefix = Barcode.Substring(0, 7);
                barcodeValue.Value = Barcode.Substring(7);
            }

            if (barcodePrefix == prefix && barcodeValue.Value != "")
            {
                #region Обработка штрих-кода номенклатуры

                if (currentBarcodeValue.IsEquals(barcodeValue))
                {
                    ShowMessage("Вы отсканировали текущий тип работ.");
                    return;
                }

                string name = GetNomenclatureDescr(barcodeValue.NomenclatureID);
                if (name == null)
                {
                    if (ShowQuery("Данная номенклатура не найдена. Необходимо обновить базу на устройстве и повторить сканирование.\r\nОбновить базу на устройстве?"))
                    {
                        UpdateRules();
                    }
                    return;
                }

                ShowRules(barcodeValue);
                currentBarcodeValue.Value = barcodeValue.Value;
                Nomenclature = name;

                #endregion
            }
            else if (Barcode.Length == 13 && Number.IsNumber(Barcode))
            {
                // Обработка штрих-кода паллеты
                if (currentBarcodeValue.Value == "")
                {
                    // Еще не был отсканирован тип работы
                    ShowMessage("Начните со сканирования штрих-кода типа работы");
                    return;
                }
                AddPalletNumber(Barcode.Substring(1, 11));
            }
            else
            {
                ShowMessage("Ожидается штрих-код паллеты или типа работы!");
            }
        }

        public override void OnHotKey(KeyAction TypeOfAction)
        {
            textBoxControl.Hide();

            switch (TypeOfAction)
            {
                case KeyAction.Esc:
                    {
                        if (ShowQuery("Отменить операцию (все собранные данные будут утеряны) ?"))
                        {
                            MainProcess.ClearControls();
                            MainProcess.Process = new SelectingProcess(MainProcess);
                        }
                        break;
                    }

                case KeyAction.F1:
                case KeyAction.Proceed:
                    {
                        #region Завершение процесса

                        if (!ShowQuery("Завершить процесс?"))
                        {
                            return;
                        }

                        if (currentBarcodeValue.Value != "")
                        {
                            WriteMarks();
                            currentBarcodeValue.Value = "";
                            table.ClearRows();
                            Nomenclature = "";
                        }

                        PerformQuery("ЗаписатьОценкиПоКачествуНП",
                            UploadToDT("SELECT NomenclatureId, DepartmentId, PalletNo FROM Pallets ORDER BY NomenclatureId"),
                            UploadToDT("SELECT NomenclatureId, DepartmentId, CriterionsId, ParentCriterionId, Mark FROM Marks"));

                        if (ResultParameters == null || ResultParameters[0] == null)
                        {
                            ShowMessage("Подойдите в зону беспроводного покрытия");
                            return;
                        }

                        MainProcess.ClearControls();
                        MainProcess.Process = new SelectingProcess(MainProcess);
                        break;

                        #endregion
                    }
            }
        }

        #endregion

        #region Business process methods

        private void InitDatabase()
        {
            string DataBaseFileName = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + @"\aramis_wms_NS.sdf";
            //System.IO.File.Delete(DataBaseFileName);

            connString = String.Format("Data Source='{0}';", DataBaseFileName);
            DBEngine = new SqlCeEngine(connString);

            if (!File.Exists(DataBaseFileName))
            {
                #region Создание файла базы данных

                DBEngine.CreateDatabase();

                using (SqlCeConnection dBConnection = new SqlCeConnection(DBEngine.LocalConnectionString))
                {
                    dBConnection.Open();

                    using (SqlCeCommand SQLCommand = dBConnection.CreateCommand())
                    {
                        // Справочник типов работ 

                        // ПРИМЕЧАНИЕ. 
                        // Поскольку в ранней версии вместо типа работы использовалась номенклатура,
                        // то везде в этом коде вместо WorkType используется Nomenclature
                        SQLCommand.CommandText = "CREATE TABLE Nomenclature (Id bigint CONSTRAINT pkID PRIMARY KEY,Descr nchar(50) NOT NULL)";
                        SQLCommand.ExecuteNonQuery();

                        // Справочник "Критерии оценивания"
                        SQLCommand.CommandText = "CREATE TABLE Criterions (Id bigint, Descr nchar(50), HaveSubCriterion bit, ParentCriterionId bigint, LineNumber int)";
                        SQLCommand.ExecuteNonQuery();

                        // Таблица соответствия, позволяет определить, какие критерии используются для каждой номенклатуры
                        SQLCommand.CommandText = "CREATE TABLE NomenclatureCriterions (Number int NOT NULL, NomenclatureId bigint NOT NULL, CriterionsId bigint NOT NULL)";
                        SQLCommand.ExecuteNonQuery();

                        // Таблица оценок
                        SQLCommand.CommandText = "CREATE TABLE Marks (NomenclatureId bigint NOT NULL, DepartmentId bigint NOT NULL, CriterionsId bigint NOT NULL, ParentCriterionId bigint NOT NULL, Mark int NOT NULL)";
                        SQLCommand.ExecuteNonQuery();

                        // Таблица со списком отсканированных паллет
                        SQLCommand.CommandText = "CREATE TABLE Pallets (NomenclatureId bigint NOT NULL, DepartmentId bigint NOT NULL, PalletNo nchar(50) CONSTRAINT pkPalletNo PRIMARY KEY)";
                        SQLCommand.ExecuteNonQuery();
                    }
                }

                #endregion
            }
            else
            {
                isEmptyCriterions = false;

                #region Очистка результирующиих таблиц: Marks и Pallets

                using (SqlCeConnection dBConnection = new SqlCeConnection(DBEngine.LocalConnectionString))
                {
                    dBConnection.Open();

                    using (SqlCeCommand SQLCommand = dBConnection.CreateCommand())
                    {
                        // Очистка оценок
                        SQLCommand.CommandText = "DELETE FROM Marks";
                        SQLCommand.ExecuteNonQuery();

                        // Очистка отсканированных паллет
                        SQLCommand.CommandText = "DELETE FROM Pallets";
                        SQLCommand.ExecuteNonQuery();
                    }
                }

                #endregion
            }

        }

        private bool UpdateRules()
        {
            this.PerformQuery("ПолучитьОбновленияПоОцениваниюКачестваНП");
            if (ResultParameters == null)
                return false;

            DataTable dt;

            using (SqlCeConnection dBConnection = new SqlCeConnection(DBEngine.LocalConnectionString))
            {
                dBConnection.Open();

                #region Загрузка справочника Номенклатура

                dt = ResultParameters[0] as DataTable;
                using (SqlCeCommand SQLCommand = dBConnection.CreateCommand())
                {
                    // Удаление старых записей номенклатуры
                    SQLCommand.CommandText = "DELETE FROM Nomenclature";
                    SQLCommand.ExecuteNonQuery();

                    SQLCommand.CommandText = "INSERT INTO Nomenclature (id, descr) VALUES (@Id, @Descr)";

                    var idParam = SQLCommand.Parameters.Add("@Id", SqlDbType.BigInt);
                    var descrParam = SQLCommand.Parameters.Add("@Descr", SqlDbType.NChar, 50);

                    foreach (DataRow dr in dt.Rows)
                    {
                        idParam.Value = System.Convert.ToInt64(dr["Id"]);
                        descrParam.Value = dr["Descr"];
                        SQLCommand.ExecuteNonQuery();
                    }
                }

                #endregion

                #region Загрузка справочника Критерии оценивания

                dt = ResultParameters[1] as DataTable;
                using (SqlCeCommand SQLCommand = dBConnection.CreateCommand())
                {
                    // Удаление старых записей критериев оценивания
                    SQLCommand.CommandText = "DELETE FROM Criterions";
                    SQLCommand.ExecuteNonQuery();

                    SQLCommand.CommandText = "INSERT INTO Criterions (id, descr, HaveSubCriterion, ParentCriterionId, LineNumber) VALUES (@Id, @Descr, @HaveSubCriterion, @ParentCriterionId, @LineNumber)";

                    var idParam = SQLCommand.Parameters.Add("@Id", SqlDbType.BigInt);
                    var descrParam = SQLCommand.Parameters.Add("@Descr", SqlDbType.NChar, 50);
                    var haveSubCriterionParam = SQLCommand.Parameters.Add("@HaveSubCriterion", SqlDbType.Bit);
                    var parentCriterionIdParam = SQLCommand.Parameters.Add("@ParentCriterionId", SqlDbType.BigInt);
                    var lineNumberParam = SQLCommand.Parameters.Add("@LineNumber", SqlDbType.Int);

                    foreach (DataRow dr in dt.Rows)
                    {
                        idParam.Value = System.Convert.ToInt64(dr["Id"]);
                        descrParam.Value = dr["Descr"];
                        haveSubCriterionParam.Value = System.Convert.ToBoolean(dr["HaveSubCriterion"]);
                        parentCriterionIdParam.Value = System.Convert.ToInt64(dr["ParentCriterionId"]);
                        lineNumberParam.Value = System.Convert.ToInt32(dr["LineNumber"]);
                        SQLCommand.ExecuteNonQuery();
                    }
                }

                #endregion

                #region Загрузка правил проверки

                dt = ResultParameters[2] as DataTable;
                using (SqlCeCommand SQLCommand = dBConnection.CreateCommand())
                {
                    // Удаление старых записей 
                    SQLCommand.CommandText = "DELETE FROM NomenclatureCriterions";
                    SQLCommand.ExecuteNonQuery();

                    SQLCommand.CommandText = "INSERT INTO NomenclatureCriterions (Number, NomenclatureId, CriterionsId) VALUES (@Number, @NomenclatureId, @CriterionsId)";

                    var numberParam = SQLCommand.Parameters.Add("@Number", SqlDbType.Int);
                    var nomenclatureIdParam = SQLCommand.Parameters.Add("@NomenclatureId", SqlDbType.BigInt);
                    var criterionsIdParam = SQLCommand.Parameters.Add("@CriterionsId", SqlDbType.BigInt);


                    foreach (DataRow dr in dt.Rows)
                    {
                        numberParam.Value = System.Convert.ToInt32(dr["Number"]);
                        nomenclatureIdParam.Value = System.Convert.ToInt64(dr["NomenclatureId"]);
                        criterionsIdParam.Value = System.Convert.ToInt64(dr["CriterionId"]);

                        SQLCommand.ExecuteNonQuery();
                    }
                }

                #endregion

            }

            return true;
        }

        private string GetNomenclatureDescr(long id)
        {
            using (SqlCeConnection dBConnection = new SqlCeConnection(DBEngine.LocalConnectionString))
            {
                dBConnection.Open();

                using (SqlCeCommand SQLCommand = dBConnection.CreateCommand())
                {
                    SQLCommand.CommandText = "SELECT Descr FROM Nomenclature WHERE Id = @Id";
                    SQLCommand.Parameters.AddWithValue("@Id", id);
                    object result = SQLCommand.ExecuteScalar();
                    if (result == null) return null;
                    return (result as string).Trim();
                }
            }
        }

        private bool ShowRules(BarcodeValue barcodeVal)
        {
            if (currentBarcodeValue.Value != "")
            {
                WriteMarks();
            }

            ParentCriterions = new Dictionary<long, ParentCriterionData>();

            bool isExistResult = false;

            table.DT.Rows.Clear();
            using (SqlCeConnection dBConnection = new SqlCeConnection(DBEngine.LocalConnectionString))
            {
                dBConnection.Open();
                using (SqlCeCommand SQLCommand = dBConnection.CreateCommand())
                {
                    SQLCommand.CommandText = "select \r\n" +
                        "c.Id,\r\n" +
                        "c.Descr,\r\n" +
                        "c.HaveSubCriterion,\r\n" +
                        "c.ParentCriterionId,\r\n" +
                        "c.LineNumber,\r\n" +
                        "m.Mark\r\n" +
                        "from Criterions c\r\n" +
                        "join NomenclatureCriterions r\r\n" +
                        "on c.Id = r.CriterionsId \r\n" +
                        "or c.ParentCriterionId = r.CriterionsId\r\n" +
                        "left join Marks m\r\n" +
                        "on c.Id = m.CriterionsId\r\n" +
                        "and r.NomenclatureId = m.NomenclatureId\r\n" +
                        "and c.ParentCriterionId = m.ParentCriterionId\r\n" +
                        "and m.DepartmentId = @DepartmentId\r\n" +
                        "where r.NomenclatureId = @NomenclatureId\r\n" +
                        "order by r.Number, c.LineNumber";
                    SQLCommand.Parameters.AddWithValue("@NomenclatureId", barcodeVal.NomenclatureID);
                    SQLCommand.Parameters.AddWithValue("@DepartmentId", barcodeVal.DepartmentID);
                    using (SqlCeDataReader result = SQLCommand.ExecuteReader())
                    {
                        while (result.Read())
                        {
                            object obj = result["Mark"];
                            if (obj == DBNull.Value) obj = 0;

                            long criterionsId = (long)result["Id"];
                            bool haveSubCriterion = (bool)result["HaveSubCriterion"];
                            long parentCriterionId = (long)result["ParentCriterionId"];

                            DataRow row = table.AddRow(((int)result["LineNumber"] > 0 ? (" " + result["LineNumber"].ToString() + ". ") : "") + ((string)result["Descr"]).Trim(),
                                criterionsId, obj, haveSubCriterion, parentCriterionId);
                            isExistResult = true;

                            if (haveSubCriterion)
                            {
                                if (!ParentCriterions.ContainsKey(criterionsId))
                                {
                                    ParentCriterions.Add(criterionsId, new ParentCriterionData(row));
                                }
                                else
                                {
                                    ParentCriterions[criterionsId].SetRow(row);
                                }
                            }

                            if (parentCriterionId > 0)
                            {
                                if (!ParentCriterions.ContainsKey(parentCriterionId))
                                {
                                    ParentCriterions.Add(parentCriterionId, new ParentCriterionData());
                                    ParentCriterions[parentCriterionId].SubCriterionsId.Add(criterionsId, (int)obj);
                                }
                                else
                                {
                                    ParentCriterions[parentCriterionId].SubCriterionsId.Add(criterionsId, (int)obj);
                                }
                            }
                        }
                    }

                }
            }

            foreach (ParentCriterionData pcd in ParentCriterions.Values)
            {
                long sum = 0;
                foreach (long val in pcd.SubCriterionsId.Values)
                {
                    sum = sum + val;
                }
                pcd.Row["Mark"] = 100 - sum;
            }

            return isExistResult;
        }

        private void onRowSelected(object sender, OnRowSelectedEventArgs e)
        {
            if ((bool)e.SelectedRow["HaveSubCriterion"])
            {
                table.DataGrid.CurrentRowIndex = table.DataGrid.CurrentRowIndex < (table.RowsCount - 1) ? table.DataGrid.CurrentRowIndex + 1 : table.DataGrid.CurrentRowIndex;
                onRowSelected(sender, new OnRowSelectedEventArgs(table.DT.Rows[table.DataGrid.CurrentRowIndex]));
            }
            else
            {
                DataGridCell editCell = table.DataGrid.CurrentCell;

                Rectangle cellPos = table.DataGrid.GetCellBounds(editCell.RowNumber, 1);
                textBoxControl.Left = cellPos.Left + 3;
                textBoxControl.Top = cellPos.Top + table.DataGrid.Top;
                textBoxControl.Width = cellPos.Width;
                textBoxControl.Height = cellPos.Height;

                textBoxControl.Tag = e.SelectedRow;
                textBoxControl.Text = e.SelectedRow["Mark"].ToString();
                textBoxControl.Show();
                textBoxControl.SelectionStart = 0;
                textBoxControl.SelectionLength = textBoxControl.Text.Length;
                textBoxControl.Focus();
            }
        }

        private void onTextBoxEndEdit(object obj, EventArgs e)
        {
            DataRow dr = (DataRow)(textBoxControl.Tag);
            long mark;

            try
            {
                mark = System.Convert.ToInt32(textBoxControl.Text);
            }
            catch
            {
                mark = 0;
            }
            if (mark > 100)
            {
                mark = 100;
            }
            long parentCriterionId = (long)dr["ParentCriterionId"];
            long criterionId = (long)dr["CriterionId"];
            dr["Mark"] = mark;

            if (parentCriterionId > 0)
            {
                ParentCriterionData PCD = ParentCriterions[parentCriterionId];
                PCD.SubCriterionsId[criterionId] = mark;
                long sum = 0;
                foreach (long val in PCD.SubCriterionsId.Values)
                {
                    sum = sum + val;
                }
                PCD.Row["Mark"] = 100 - sum;
            }

            if (textBoxControl.Visible)
            {
                table.DataGrid.CurrentRowIndex = table.DataGrid.CurrentRowIndex < (table.RowsCount - 1) ? table.DataGrid.CurrentRowIndex + 1 : table.DataGrid.CurrentRowIndex;
            }

            textBoxControl.Hide();
            table.Focus();
        }

        private void textBoxControl_LostFocus(object sender, EventArgs e)
        {
            textBoxControl.Hide();
        }

        private void AddPalletNumber(string palletBarcode)
        {
            while (palletBarcode.Length > 1 && palletBarcode[0] == '0')
            {
                palletBarcode = palletBarcode.Substring(1, palletBarcode.Length - 1);
            }

            using (SqlCeConnection dBConnection = new SqlCeConnection(DBEngine.LocalConnectionString))
            {
                dBConnection.Open();
                using (SqlCeCommand SQLCommand = dBConnection.CreateCommand())
                {
                    SQLCommand.CommandText = "SELECT NomenclatureId FROM Pallets WHERE PalletNo = @PalletNo";
                    SQLCommand.Parameters.AddWithValue("@PalletNo", palletBarcode);
                    if (SQLCommand.ExecuteScalar() != null)
                    {
                        // Паллета уже была отcканирована
                        return;
                    }

                    SQLCommand.CommandText = "INSERT INTO Pallets(DepartmentId, NomenclatureId, PalletNo) VALUES (@DepartmentId, @NomenclatureId, @PalletNo)";
                    SQLCommand.Parameters.AddWithValue("@DepartmentId", currentBarcodeValue.DepartmentID);
                    SQLCommand.Parameters.AddWithValue("@NomenclatureId", currentBarcodeValue.NomenclatureID);
                    SQLCommand.ExecuteNonQuery();
                }
            }

        }

        private void WriteMarks()
        {
            using (SqlCeConnection dBConnection = new SqlCeConnection(DBEngine.LocalConnectionString))
            {
                dBConnection.Open();
                using (SqlCeCommand SQLCommand = dBConnection.CreateCommand())
                {

                    #region Удаление старых записей для данной номенклатуры

                    SQLCommand.CommandText = "DELETE FROM Marks WHERE NomenclatureId = @NomenclatureId and DepartmentId = @DepartmentId";
                    var nomenclatureIdParam = SQLCommand.Parameters.Add("@NomenclatureId", SqlDbType.BigInt);
                    var departmentIdParam = SQLCommand.Parameters.Add("@DepartmentId", SqlDbType.BigInt);
                    nomenclatureIdParam.Value = currentBarcodeValue.NomenclatureID;
                    departmentIdParam.Value = currentBarcodeValue.DepartmentID;
                    SQLCommand.ExecuteNonQuery();

                    #endregion

                    SQLCommand.CommandText = "INSERT INTO Marks(DepartmentId, NomenclatureId, CriterionsId, Mark, ParentCriterionId) VALUES (@DepartmentId, @NomenclatureId, @CriterionsId, @Mark, @ParentCriterionId)";

                    var criterionsIdParam = SQLCommand.Parameters.AddWithValue("@CriterionsId", SqlDbType.BigInt);
                    var markParam = SQLCommand.Parameters.AddWithValue("@Mark", SqlDbType.Int);
                    var parentCriterionIdParam = SQLCommand.Parameters.AddWithValue("@ParentCriterionId", SqlDbType.BigInt);
                    departmentIdParam.Value = currentBarcodeValue.DepartmentID;

                    foreach (DataRow dr in table.DT.Rows)
                    {
                        criterionsIdParam.Value = dr["CriterionId"];
                        markParam.Value = dr["Mark"];
                        parentCriterionIdParam.Value = dr["ParentCriterionId"];
                        SQLCommand.ExecuteNonQuery();
                    }
                }
            }
        }

        private DataTable UploadToDT(string sqlCommandText)
        {
            using (SqlCeConnection dBConnection = new SqlCeConnection(DBEngine.LocalConnectionString))
            {
                dBConnection.Open();
                using (SqlCeCommand SQLCommand = dBConnection.CreateCommand())
                {
                    SQLCommand.CommandText = sqlCommandText;

                    return ReaderToDT(SQLCommand.ExecuteReader());
                }
            }
        }

        private DataTable ReaderToDT(SqlCeDataReader dataReader)
        {
            DataTable dt = new DataTable();

            foreach (DataRow dr in dataReader.GetSchemaTable().Rows)
            {
                dt.Columns.Add(dr["ColumnName"] as string, (Type)(dr["DataType"]));
            }

            while (dataReader.Read())
            {

                DataRow dr = dt.NewRow();

                foreach (DataColumn dc in dt.Columns)
                {
                    dr[dc] = dataReader[dc.ColumnName];
                }

                dt.Rows.Add(dr);
            }

            return dt;
        }

        #endregion

    }
}

