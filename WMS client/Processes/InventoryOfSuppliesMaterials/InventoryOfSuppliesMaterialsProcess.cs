using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;


namespace WMS_client
{
    public class InventoryOfSuppliesMaterialsProcess : BusinessProcess
    {
        #region Поля
        private bool newNomenclatureReaded = true;
        private bool useBaseMeasureInfo = false;
        private long totalCount = 0;
        private long totalBaseMeasureCount = 0;
        private DataRow currentRow;
        private long unqueCode = 0;
        private long unitCode = 0;

        private DataTable currentNomenclatureInfo;
        private long currentNomenclatureId;
        private string currentNomenclatureDescr;
        private long currentMeasureId;
        private string currentMeasureDescr;

        private long nomenclatureId;
        private long cellId;
        private long measureId;

        private string connString;
        private SqlCeEngine DBEngine;

        private Label nomenclature;
        private Dictionary<long, Label> measure = new Dictionary<long, Label>();
        private Dictionary<long, Label> measureCount = new Dictionary<long, Label>();
        private Label measureBase;
        private Label measureBaseCount;
        private Label measureTotal;
        private Label measureTotalCount;
        private Label measureCurrent;
        private TextBox measureCurrentCount;

        #endregion

        public InventoryOfSuppliesMaterialsProcess(WMSClient MainProcess, long cellId, long nomenclatureId, long measureId)
            : base(MainProcess, 1)
        {
            BusinessProcessType = ProcessType.Inventory;
            this.cellId = cellId;
            this.nomenclatureId = nomenclatureId;
            this.measureId = measureId;
            measureCurrent.Tag = null;
            ReadDataFromDB();
            if (nomenclatureId != 0)
            {
                ShowBaseData(nomenclatureId, measureId);
            }
        }

        private void ShowBaseData(long nomenclatureId, long measureId)
        {
            using (SqlCeConnection dBConnection = new SqlCeConnection(DBEngine.LocalConnectionString))
            {
                dBConnection.Open();

                #region Чтение из локальной БД информации о номенклатуре
                using (SqlCeCommand SQLCommand = dBConnection.CreateCommand())
                {
                    var idParam = SQLCommand.Parameters.Add("@Id", SqlDbType.BigInt);
                    SQLCommand.CommandText = "SELECT Descr FROM Nomenclature where id = @Id";
                    idParam.Value = (long)nomenclatureId;
                    var descr = SQLCommand.ExecuteScalar();
                    nomenclature.Text = descr.ToString();

                    if (measureId != 0)
                    {
                        SQLCommand.CommandText = "SELECT Descr FROM Measure where id = @Id";
                        idParam.Value = (long)measureId;
                        object measureDescr = SQLCommand.ExecuteScalar();
                        if (measureDescr != null)
                        {
                            measureBase.Text = (string)measureDescr;
                            measureCurrent.Text = (string)measureDescr;
                        }
                    }
                }
                #endregion
            }
        }

        public override void DrawControls()
        {
            MainProcess.ClearControls();
            nomenclature = MainProcess.CreateLabel("<Ожидается код>", 5, 70, 230, ControlsStyle.LabelNormal).GetControl() as Label;
            measureBase = MainProcess.CreateLabel("<Ожидается код>", 5, 95 + 25 * measure.Count, 165, ControlsStyle.LabelNormal).GetControl() as Label;
            measureBaseCount = MainProcess.CreateLabel("0", 170, 95 + 25 * measure.Count, 70, ControlsStyle.LabelNormal).GetControl() as Label;
            measureTotal = MainProcess.CreateLabel("Всего", 5, 120 + 25 * measure.Count, 165, ControlsStyle.LabelNormal).GetControl() as Label;
            measureTotalCount = MainProcess.CreateLabel("0", 170, 120 + 25 * measure.Count, 70, ControlsStyle.LabelNormal).GetControl() as Label;
            measureCurrent = MainProcess.CreateLabel("<Ожидается код>", 5, 270, 160, ControlsStyle.LabelNormal).GetControl() as Label;
            measureCurrentCount = MainProcess.CreateTextBox(170, 270, 65, "", ControlsStyle.LabelRedRightAllign, OnEnterToActTextbox, false).GetControl() as TextBox;
            MainProcess.ToDoCommand = "ИНВЕНТАРИЗАЦИЯ Р. МАТ.";
            InitDatabase();
        }

        public override void OnBarcode(string Barcode)
        {
            bool isUniqueCode = false;
            int asterixIndex = 0;

            #region Получение кода единицы измерения, уник. кода

            if (Barcode.Length > 8 && Barcode.Substring(0, 3) == "SB_" && Barcode.IndexOf('.') > 0)
            {
                bool isWrongFormat = false;

                if (Barcode.Substring(3, 5) != "UNIT.")
                {
                    isWrongFormat = true;
                }
                else
                {
                    string barBody = Barcode.Substring(8, Barcode.Length - 8);
                    asterixIndex = barBody.IndexOf('*');

                    if (asterixIndex == -1)
                    {
                        if (!Number.IsNumber(barBody))
                        {
                            isWrongFormat = true;
                        }
                        else
                        {
                            unitCode = Convert.ToInt64(barBody);
                        }
                    }
                    else
                    {
                        isUniqueCode = true;
                        string unitCodeStr = barBody.Substring(0, asterixIndex);
                        string uniqueCodeStr = barBody.Substring(asterixIndex + 1, barBody.Length - asterixIndex - 1);
                        if (!Number.IsNumber(unitCodeStr) || !Number.IsNumber(uniqueCodeStr))
                        {
                            isWrongFormat = true;
                        }
                        else
                        {
                            unitCode = Convert.ToInt64(unitCodeStr);
                            unqueCode = Convert.ToInt64(uniqueCodeStr);
                        }
                    }
                }
                if (isWrongFormat)
                {
                    ShowMessage("Неверный тип штрих-кода, ожидается единица измерения расх. мат.");
                    return;
                }
            }
            else
            {
                PerformQuery("НайтиКодЕдИзм", Barcode);
                if (Parameters == null || Parameters[0] == null)
                {
                    ShowMessage("Не верный штрих-код. Считайте штрих-код единицы измерения");
                    return;
                }
                unitCode = Convert.ToInt64(Parameters[0] as string);
                if (unitCode == 0)
                {
                    ShowMessage("Штрих-код не найден среди единиц измерения расходных материалов.");
                    return;
                }

            }

            #endregion

            if (currentMeasureId != unitCode)
            {
                if (ReadInfo())
                {
                    DataRow row = GetCurrentRow(isUniqueCode);
                    if (row == null)
                    {
                        return;
                    }
                    else
                    {
                        currentRow = row;
                    }
                    currentMeasureId = unitCode;
                }
                else
                {
                    return;
                }
            }

            if (isUniqueCode)
            {
                OnUniqueCode();
            }
            else
            {
                currentRow["FactValue"] = ((long)(currentRow["FactValue"])) + 1;
                totalCount += (long)currentRow["basecount"];
                if ((long)currentRow["basecount"] == 1)
                {
                    totalBaseMeasureCount += 1;
                }
            }

            RepaintFormControls();
        }

        private DataRow GetCurrentRow(bool isUniqueCode)
        {
            foreach (DataRow row in currentNomenclatureInfo.Rows)
            {
                if ((long)row["measureid"] == unitCode && (isUniqueCode && unqueCode == (long)row["UniqueNumber"] || (long)row["UniqueNumber"] == 0))
                {
                    return row;
                }
            }
            return null;
        }

        private bool ReadInfo()
        {
            using (SqlCeConnection dBConnection = new SqlCeConnection(DBEngine.LocalConnectionString))
            {
                dBConnection.Open();

                #region Чтение из локальной БД информации о номенклатуре
                using (SqlCeCommand SQLCommand = dBConnection.CreateCommand())
                {
                    SQLCommand.CommandText = "SELECT NomId FROM Measure where id = @Id";
                    var idParam = SQLCommand.Parameters.Add("@Id", SqlDbType.BigInt);
                    idParam.Value = unitCode;
                    var nomId = SQLCommand.ExecuteScalar();

                    object measureDescr = null;

                    if (currentMeasureId != unitCode)
                    {
                        SQLCommand.CommandText = "SELECT Descr FROM Measure where id = @Id";
                        measureDescr = SQLCommand.ExecuteScalar();
                        if (measureDescr != null)
                        {
                            currentMeasureDescr = (string)measureDescr;
                        }
                    }

                    if (nomId == null)
                    {
                        ShowMessage("Данная единица измерения не запланирована к инвентаризации");
                        return false;
                    }
                    else if (currentNomenclatureId == (long)nomId)
                    {
                        if (measureDescr != null)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (currentNomenclatureId != 0)
                    {
                        WriteDataToDB();
                    }

                    SQLCommand.CommandText = "SELECT Descr FROM Nomenclature where id = @Id";
                    idParam.Value = (long)nomId;
                    var descr = SQLCommand.ExecuteScalar();
                    currentNomenclatureDescr = descr.ToString();

                    SQLCommand.CommandText = "select t.id measureid, t.descr, t. basecount, r.UniqueNumber, r.Planvalue, r.factvalue from (SELECT m.id , m.descr, m.basecount from Measure m where nomid = @nomid) t left join Remains r on t.id = r.id order by t.basecount desc";
                    var nomIdParam = SQLCommand.Parameters.Add("@nomId", SqlDbType.BigInt);
                    nomIdParam.Value = (long)nomId;
                    var reader = SQLCommand.ExecuteReader();
                    currentNomenclatureId = (long)nomId;
                    currentNomenclatureInfo = ReaderToDT(SQLCommand.ExecuteReader());
                    newNomenclatureReaded = true;
                    return true;
                }
                #endregion
            }
        }

        private void FillMeasuresList()
        {
            long curMeasureId = 0;
            int i = 0;
            long totalMeasureCount = 0;
            long basecount = 0;
            totalCount = 0;
            totalBaseMeasureCount = 0;
            measure = new Dictionary<long, Label>();
            measureCount = new Dictionary<long, Label>();
            useBaseMeasureInfo = false;
            foreach (DataRow row in currentNomenclatureInfo.Rows)
            {
                if ((long)row["basecount"] != 1)
                {
                    if (curMeasureId != (long)row["measureid"])
                    {
                        if (i > 0)
                        {
                            Label measureCountLabel = MainProcess.CreateLabel(totalMeasureCount.ToString(), 170, 70 + 25 * i, 70, ControlsStyle.LabelNormal).GetControl() as Label;
                            measureCount.Add(curMeasureId, measureCountLabel);
                            totalCount += totalMeasureCount * basecount;
                        }
                        i++;
                        totalMeasureCount = 0;
                        basecount = (long)row["basecount"];
                        curMeasureId = (long)row["measureid"];
                        Label measureLabel = MainProcess.CreateLabel(row["descr"].ToString(), 5, 70 + 25 * i, 165, ControlsStyle.LabelNormal).GetControl() as Label;
                        measure.Add(curMeasureId, measureLabel);
                    }
                    totalMeasureCount += (long)row["factvalue"];
                }
                else
                {
                    useBaseMeasureInfo = true;
                    totalBaseMeasureCount += (long)row["factvalue"];
                    totalCount += (long)row["factvalue"];
                    measureBase.Text = row["descr"].ToString();
                }
            }

            if (i > 0)
            {
                Label measureCountLabel = MainProcess.CreateLabel(totalMeasureCount.ToString(), 170, 70 + 25 * i, 70, ControlsStyle.LabelNormal).GetControl() as Label;
                measureCount.Add(curMeasureId, measureCountLabel);
                totalCount += totalMeasureCount * basecount;
            }

        }

        private void OnUniqueCode()
        {
            currentRow["FactValue"] = currentRow["PlanValue"];
            totalBaseMeasureCount += (long)currentRow["FactValue"];
            totalCount += (long)currentRow["FactValue"];
        }

        private void RepaintFormControls()
        {
            if (newNomenclatureReaded)
            {
                MainProcess.ClearControls();
                measureBase = MainProcess.CreateLabel("", 5, 10, 165, ControlsStyle.LabelNormal).GetControl() as Label;
                FillMeasuresList();
                measureBase.Top = 95 + 25 * measure.Count;

                nomenclature = MainProcess.CreateLabel(currentNomenclatureDescr, 5, 70, 230, ControlsStyle.LabelNormal).GetControl() as Label;
                measureBaseCount = MainProcess.CreateLabel(totalBaseMeasureCount.ToString(), 170, 95 + 25 * measure.Count, 70, ControlsStyle.LabelNormal).GetControl() as Label;
                measureTotal = MainProcess.CreateLabel("Всего", 5, (useBaseMeasureInfo ? 120 : 95) + 25 * measure.Count, 165, ControlsStyle.LabelNormal).GetControl() as Label;
                measureTotalCount = MainProcess.CreateLabel(totalCount.ToString(), 170, (useBaseMeasureInfo ? 120 : 95) + 25 * measure.Count, 70, ControlsStyle.LabelNormal).GetControl() as Label;
                measureCurrent = MainProcess.CreateLabel(currentMeasureDescr, 5, 270, 160, ControlsStyle.LabelNormal).GetControl() as Label;
                measureCurrentCount = MainProcess.CreateTextBox(170, 270, 65, "", ControlsStyle.LabelRedRightAllign, OnEnterToActTextbox, false).GetControl() as TextBox;
                MainProcess.ToDoCommand = "ИНВЕНТАРИЗАЦИЯ Р. МАТ.";
                newNomenclatureReaded = false;
                measureBase.Visible = useBaseMeasureInfo;
                measureBaseCount.Visible = useBaseMeasureInfo;
            }
            else
            {
                measureTotalCount.Text = totalCount.ToString();
                if (measureCount.ContainsKey(currentMeasureId))
                {
                    measureCount[currentMeasureId].Text = currentRow["FactValue"].ToString();
                }
                else
                {
                    measureBaseCount.Text = totalBaseMeasureCount.ToString();
                }
            }
        }

        public override void OnHotKey(KeyAction TypeOfAction)
        {
            switch (TypeOfAction)
            {
                case KeyAction.Esc:

                    if (ShowQuery("Вы хотите прервать процесс?"))
                    {
                        MainProcess.ClearControls();
                        MainProcess.Process = new SelectInventoryTypeProcess(MainProcess);
                    }

                    break;
                case KeyAction.Proceed:
                    WriteDataToDB();
                    PerformQuery("ЗавершитьИнвентаризациюМатериалов", UploadToDT("select m.nomid Nomenclature, m.id Measure, r.UniqueNumber UniqueNumber, sum(r.Factvalue) FactValue, sum(r.PlanValue) PlanValue from measure m left join remains r on m.id = r.id group by m.nomid, m.id, r.UniqueNumber"), cellId);
                    if (Parameters == null) return;

                    MainProcess.ClearControls();
                    MainProcess.Process = new SelectingProcess(MainProcess);
                    break;
            }
        }

        private void OnEnterToActTextbox(object obj, EventArgs e)
        {

            bool cancel = false;

            if (!Number.IsNumber(measureCurrentCount.Text))
            {
                cancel = true;
            }

            if (cancel) return;

            int count = Convert.ToInt32(measureCurrentCount.Text);

            cancel = count == 0;

            if (cancel) return;

            currentRow["FactValue"] = (long)currentRow["FactValue"] + count;
            totalCount += (long)currentRow["basecount"] * count;
            if ((long)currentRow["basecount"] == 1)
            {
                totalBaseMeasureCount += count;
            }
            RepaintFormControls();
        }

        private void ReadDataFromDB()
        {
            this.PerformQuery("ПолучитьОстаткиРасходныхМатериалов", cellId, nomenclatureId, measureId);
            if (this.Parameters == null)
            {
                return;
            }

            DataTable nomenclatures = this.Parameters[0] as DataTable;
            DataTable measures = this.Parameters[1] as DataTable;
            DataTable remains = this.Parameters[2] as DataTable;

            using (SqlCeConnection dBConnection = new SqlCeConnection(DBEngine.LocalConnectionString))
            {
                dBConnection.Open();

                #region Заполнение справочника номенклатура
                using (SqlCeCommand SQLCommand = dBConnection.CreateCommand())
                {
                    // Удаление старых записей номенклатуры
                    SQLCommand.CommandText = "DELETE FROM Nomenclature";
                    SQLCommand.ExecuteNonQuery();

                    SQLCommand.CommandText = "INSERT INTO Nomenclature (id, descr) VALUES (@Id, @Descr)";

                    var idParam = SQLCommand.Parameters.Add("@Id", SqlDbType.BigInt);
                    var descrParam = SQLCommand.Parameters.Add("@Descr", SqlDbType.NChar, 50);

                    foreach (DataRow dr in nomenclatures.Rows)
                    {
                        idParam.Value = System.Convert.ToInt64(dr["Id"]);
                        descrParam.Value = dr["Descr"];
                        SQLCommand.ExecuteNonQuery();
                    }
                }
                #endregion

                #region Заполнение справочника единицы измерения
                using (SqlCeCommand SQLCommand = dBConnection.CreateCommand())
                {
                    // Удаление старых записей номенклатуры
                    SQLCommand.CommandText = "DELETE FROM Measure";
                    SQLCommand.ExecuteNonQuery();

                    SQLCommand.CommandText = "INSERT INTO Measure (nomId, id, descr, basecount) VALUES (@NomId, @Id, @Descr, @basecount)";

                    var idParam = SQLCommand.Parameters.Add("@Id", SqlDbType.BigInt);
                    var descrParam = SQLCommand.Parameters.Add("@Descr", SqlDbType.NChar, 50);
                    var nomIdParam = SQLCommand.Parameters.Add("@NomId", SqlDbType.BigInt);
                    var basecountParam = SQLCommand.Parameters.Add("@basecount", SqlDbType.BigInt);

                    foreach (DataRow dr in measures.Rows)
                    {
                        idParam.Value = System.Convert.ToInt64(dr["Id"]);
                        descrParam.Value = dr["Descr"];
                        nomIdParam.Value = dr["NomId"];
                        basecountParam.Value = dr["basecount"];
                        SQLCommand.ExecuteNonQuery();
                    }
                }
                #endregion

                #region Заполнение таблицы остатков
                using (SqlCeCommand SQLCommand = dBConnection.CreateCommand())
                {
                    // Удаление старых записей номенклатуры
                    SQLCommand.CommandText = "DELETE FROM Remains";
                    SQLCommand.ExecuteNonQuery();

                    SQLCommand.CommandText = "INSERT INTO Remains (id, UniqueNumber, PlanValue, Factvalue) VALUES (@Id, @UniqueNumber, @Plan, 0)";

                    var idParam = SQLCommand.Parameters.Add("@Id", SqlDbType.BigInt);
                    var descrParam = SQLCommand.Parameters.Add("@UniqueNumber", SqlDbType.NChar, 50);
                    var nomIdParam = SQLCommand.Parameters.Add("@Plan", SqlDbType.BigInt);

                    foreach (DataRow dr in remains.Rows)
                    {
                        idParam.Value = System.Convert.ToInt64(dr["Id"]);
                        descrParam.Value = dr["UniqueNumber"];
                        nomIdParam.Value = dr["Plan"];
                        SQLCommand.ExecuteNonQuery();
                    }
                }
                #endregion
            }
        }

        private void InitDatabase()
        {
            string DataBaseFileName = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + @"\aramis_wms_inventory.sdf";
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
                        // Справочник номенклатуры
                        SQLCommand.CommandText = "CREATE TABLE Nomenclature (Id bigint CONSTRAINT pkID PRIMARY KEY,Descr nchar(50) NOT NULL)";
                        SQLCommand.ExecuteNonQuery();

                        // Справочник единиц измерения
                        SQLCommand.CommandText = "CREATE TABLE Measure (NomId bigint NOT NULL, Id bigint NOT NULL, Descr nchar(50) NOT NULL, basecount bigint NOT NULL)";
                        SQLCommand.ExecuteNonQuery();

                        // Справочник остатков
                        SQLCommand.CommandText = "CREATE TABLE Remains (Id bigint not null, UniqueNumber bigint Null, PlanValue bigint NULL, FactValue bigint null)";
                        SQLCommand.ExecuteNonQuery();
                    }
                }

                #endregion
            }
            else
            {
                #region Очистка результирующиих таблиц

                using (SqlCeConnection dBConnection = new SqlCeConnection(DBEngine.LocalConnectionString))
                {
                    dBConnection.Open();

                    using (SqlCeCommand SQLCommand = dBConnection.CreateCommand())
                    {
                        // Очистка справочника номенклатуры
                        SQLCommand.CommandText = "DELETE FROM Nomenclature";
                        SQLCommand.ExecuteNonQuery();

                        // Очистка справочника единиц измерения
                        SQLCommand.CommandText = "DELETE FROM Measure";
                        SQLCommand.ExecuteNonQuery();

                        // очиста таблицы остатков
                        SQLCommand.CommandText = "DELETE FROM Remains";
                        SQLCommand.ExecuteNonQuery();
                    }
                }

                #endregion
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

        private void WriteDataToDB()
        {
            if (currentNomenclatureInfo != null)
            {
                using (SqlCeConnection dBConnection = new SqlCeConnection(DBEngine.LocalConnectionString))
                {
                    dBConnection.Open();
                    using (SqlCeCommand SQLCommand = dBConnection.CreateCommand())
                    {
                        SQLCommand.CommandText = "UPDATE remains SET FactValue = @FactValue WHERE Id = @MeasureId AND UniqueNumber = @UniqueNumber";

                        var measureIdParam = SQLCommand.Parameters.AddWithValue("@MeasureId", SqlDbType.BigInt);
                        var uniqueNumberParam = SQLCommand.Parameters.AddWithValue("@UniqueNumber", SqlDbType.BigInt);
                        var factValueParam = SQLCommand.Parameters.AddWithValue("@FactValue", SqlDbType.BigInt);

                        foreach (DataRow dr in currentNomenclatureInfo.Rows)
                        {
                            measureIdParam.Value = dr["measureid"];
                            uniqueNumberParam.Value = dr["UniqueNumber"];
                            factValueParam.Value = dr["factvalue"];
                            SQLCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
            else
            {
                if (ShowQuery("Нет введенных данных. Прервать процесс?"))
                {
                    MainProcess.ClearControls();
                    MainProcess.Process = new SelectInventoryTypeProcess(MainProcess);
                }
            }
        }
    }
}
