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
    public class ConnectionIsNotExistsException : Exception
    {
        public ConnectionIsNotExistsException(string message)
            : base("Необходима связь с сервером, подойдите в зону радиопокрытия:\r\n" + message) { }
    }

    public class QualityRegistrationProcess : BusinessProcess
    {

        // ПРИМЕЧАНИЕ. 
        // Поскольку в ранней версии вместо типа работы использовалась номенклатура,
        // то везде в этом коде вместо WorkType используется Nomenclature и следует понимать,
        // что на самом деле это все относится к типу работ

        #region Private fields

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
                    labelControl.Text = "<скан. тип работы>";
                }
                else
                {
                    labelControl.Text = value.Trim();
                }
            }
        }
        private long currentNomenclatureId = 0;

        private MobileTable table;

        #endregion

        #region Constructor

        public QualityRegistrationProcess(WMSClient MainProcess)
            : base(MainProcess, 1)
        {

            BusinessProcessType = ProcessType.QualityRegistration;
            FormNumber = 1;
        }

        #endregion

        #region Override methods

        public override void DrawControls()
        {
            InitDatabase();

            if (!isEmptyCriterions)
            {
                isEmptyCriterions = ShowQuery("Выполнить обновление типов работы и критериев оценивания?");
            }

            if (isEmptyCriterions)
            {
                if (!UpdateRules())
                {
                    throw new ConnectionIsNotExistsException("нужно обновить критерии оценивания и типы работ!");
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
                new DataColumn("Mark", typeof(int))
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

            string prefix = "SB_WTYP.";
            if (Barcode.Length > 8 && Barcode.Substring(0, 8) == prefix && Number.IsNumber(Barcode.Substring(8)))
            {
                #region Обработка штрих-кода типа работы

                long nomenclatureId = System.Convert.ToInt64(Barcode.Substring(8));
                if (currentNomenclatureId == nomenclatureId)
                {
                    ShowMessage("Вы отсканировали текущий тип работ.");
                    return;
                }

                string name = GetNomenclatureDescr(nomenclatureId);
                if (name == null)
                {
                    if (ShowQuery("Данный тип работ не найден. Необходимо обновить базу на устройстве и повторить сканирование.\r\nОбновить базу на устройстве?"))
                    {
                        UpdateRules();
                    }
                    return;
                }

                ShowRules(nomenclatureId);
                currentNomenclatureId = nomenclatureId;
                Nomenclature = name;

                #endregion
            }
            else if (Barcode.Length == 13 && Number.IsNumber(Barcode))
            {
                // Обработка штрих-кода паллеты
                if (currentNomenclatureId == 0)
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

                        if (currentNomenclatureId != 0)
                        {
                            WriteMarks();
                            currentNomenclatureId = 0;
                            table.ClearRows();
                            Nomenclature = "";
                        }

                        PerformQuery("ЗаписатьОценкиПоКачеству",
                            UploadToDT("SELECT NomenclatureId, PalletNo FROM Pallets ORDER BY NomenclatureId"),
                            UploadToDT("SELECT NomenclatureId, CriterionsId, Mark FROM Marks"));

                        if (Parameters == null || Parameters[0] == null)
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
            string DataBaseFileName = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + @"\aramis_wms.sdf";
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
                        SQLCommand.CommandText = "CREATE TABLE Criterions (Id bigint CONSTRAINT pkID PRIMARY KEY, Descr nchar(50) NOT NULL)";
                        SQLCommand.ExecuteNonQuery();

                        // Таблица соответствия, позволяет определить, какие критерии используются для каждой номенклатуры
                        SQLCommand.CommandText = "CREATE TABLE NomenclatureCriterions (Number int NOT NULL, NomenclatureId bigint NOT NULL, CriterionsId bigint NOT NULL)";
                        SQLCommand.ExecuteNonQuery();

                        // Таблица оценок
                        SQLCommand.CommandText = "CREATE TABLE Marks (NomenclatureId bigint NOT NULL, CriterionsId bigint NOT NULL, Mark int NOT NULL)";
                        SQLCommand.ExecuteNonQuery();

                        // Таблица со списком отсканированных паллет
                        SQLCommand.CommandText = "CREATE TABLE Pallets (NomenclatureId bigint NOT NULL, PalletNo nchar(11) CONSTRAINT pkPalletNo PRIMARY KEY)";
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
            this.PerformQuery("ПолучитьОбновленияПоОцениваниюКачества");
            if (Parameters == null)
                return false;

            DataTable dt;

            using (SqlCeConnection dBConnection = new SqlCeConnection(DBEngine.LocalConnectionString))
            {
                dBConnection.Open();

                #region Загрузка справочника Номенклатура

                dt = Parameters[0] as DataTable;
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

                dt = Parameters[1] as DataTable;
                using (SqlCeCommand SQLCommand = dBConnection.CreateCommand())
                {
                    // Удаление старых записей критериев оценивания
                    SQLCommand.CommandText = "DELETE FROM Criterions";
                    SQLCommand.ExecuteNonQuery();

                    SQLCommand.CommandText = "INSERT INTO Criterions (id, descr) VALUES (@Id, @Descr)";

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

                #region Загрузка правил проверки

                dt = Parameters[2] as DataTable;
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
                        nomenclatureIdParam.Value = System.Convert.ToInt64(dr["WorkTypeId"]);
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

        private bool ShowRules(long id)
        {
            if (currentNomenclatureId != 0)
            {
                WriteMarks();
            }

            bool isExistResult = false;

            table.DT.Rows.Clear();
            using (SqlCeConnection dBConnection = new SqlCeConnection(DBEngine.LocalConnectionString))
            {
                dBConnection.Open();
                using (SqlCeCommand SQLCommand = dBConnection.CreateCommand())
                {
                    SQLCommand.CommandText = "SELECT rules.CriterionsId As Id, criteria.Descr As Descr, Marks.Mark FROM NomenclatureCriterions As rules JOIN Criterions As criteria ON rules.CriterionsId = criteria.Id LEFT JOIN Marks ON rules.CriterionsId = Marks.CriterionsId and rules.NomenclatureId = Marks.NomenclatureId WHERE rules.NomenclatureId = @Id Order By rules.Number";
                    SQLCommand.Parameters.AddWithValue("@Id", id);
                    using (SqlCeDataReader result = SQLCommand.ExecuteReader())
                    {
                        while (result.Read())
                        {
                            object obj = result["Mark"];
                            if (obj == DBNull.Value) obj = 0;
                            table.AddRow((result["Descr"] as string).Trim(), result["Id"], obj);
                            isExistResult = true;
                        }
                    }
                }
            }

            return isExistResult;
        }

        private void onRowSelected(object sender, OnRowSelectedEventArgs e)
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

            dr["Mark"] = mark;
            
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
                        // Паллета уже была отканирована
                        return;
                    }

                    SQLCommand.CommandText = "INSERT INTO Pallets(NomenclatureId, PalletNo) VALUES (@NomenclatureId, @PalletNo)";
                    SQLCommand.Parameters.AddWithValue("@NomenclatureId", currentNomenclatureId);
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

                    SQLCommand.CommandText = "DELETE FROM Marks WHERE NomenclatureId = @NomenclatureId";
                    var nomenclatureIdParam = SQLCommand.Parameters.Add("@NomenclatureId", SqlDbType.BigInt);
                    nomenclatureIdParam.Value = currentNomenclatureId;
                    SQLCommand.ExecuteNonQuery(); 
                    
                    #endregion

                    SQLCommand.CommandText = "INSERT INTO Marks(NomenclatureId, CriterionsId, Mark) VALUES (@NomenclatureId, @CriterionsId, @Mark)";
                    
                    var criterionsIdParam = SQLCommand.Parameters.AddWithValue("@CriterionsId", SqlDbType.BigInt);
                    var markParam = SQLCommand.Parameters.AddWithValue("@Mark", SqlDbType.Int);
                    

                    foreach (DataRow dr in table.DT.Rows)
                    {
                        criterionsIdParam.Value = dr["CriterionId"];
                        markParam.Value = dr["Mark"];
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
            
                foreach(DataRow dr in dataReader.GetSchemaTable().Rows)
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

