using System.Collections.Generic;
using System;
using System.Drawing;
using WMS_client.Enums;
using System.Data.SqlServerCe;
using WMS_client.db;

namespace WMS_client.Processes.Lamps
{
    /// <summary>Приймання нового коплектующего</summary>
    public class AcceptanceOfNewAccessory : BusinessProcess
    {
        #region Variables
        //private MobileLabel marking;
        //private DataRow selectedRow;
        //private string selectedModelName;
        private long selectedModelId;
        private object documentId;
        private readonly AcceptanceOfNewComponents acceptanceDoc;
        private readonly TypeOfAccessories typeOfAccessory;
        private int TypeOfAccessory { get { return (int)typeOfAccessory; } }
        private readonly Dictionary<string, long> newElements = new Dictionary<string, long>();
        private MobileLabel labelOfCount;
        private int count;
        #endregion

        //#region Names of Columns
        //private const string MODEL_NAME_COLUMN = "ModelName";
        //private const string MODEL_ID_COLUMN = "ModelId";
        //private const string PLAN_COLUMN = "Plan";
        //private const string FACT_COLUMN = "Fact";
        //#endregion

        /// <summary>Приемка нового коплектующего</summary>
        /// <param name="MainProcess"></param>
        /// <param name="topic">Заголовок</param>
        /// <param name="accessory">Тип комплектующего</param>
        public AcceptanceOfNewAccessory(WMSClient MainProcess, string topic, TypeOfAccessories accessory)
            : base(MainProcess, 1)
        {
            MainProcess.ToDoCommand = topic;
            FormNumber = 1;
            BusinessProcessType = ProcessType.Registration;
            typeOfAccessory = accessory;
            acceptanceDoc = new AcceptanceOfNewComponents();

            IsLoad = true;
            DrawControls();
        }

        #region Override methods
        public override sealed void DrawControls()
        {
            if (IsLoad)
            {
                object idObj = getAcceptanceDoc();

                if (idObj != null)
                {
                    //marking = MainProcess.CreateLabel("Модель не обрано", 5, 60, 230, 45,
                    //                                  MobileFontSize.Multiline, MobileFontPosition.Center,
                    //                                  MobileFontColors.Info, FontStyle.Bold);
                                       
                    //documentId = idObj;
                    //SqlCeDataReader reader = getAcceptanceInfo(idObj);

                    //DataTable sourceTable = new DataTable();
                    //sourceTable.Columns.AddRange(new[]
                    //                                 {
                    //                                     new DataColumn(MODEL_NAME_COLUMN, typeof (string)),
                    //                                     new DataColumn(MODEL_ID_COLUMN, typeof (int)),
                    //                                     new DataColumn(PLAN_COLUMN, typeof (int)),
                    //                                     new DataColumn(FACT_COLUMN, typeof (int))
                    //                                 });

                    //MobileTable visualTable = MainProcess.CreateTable("Models", 160, 105);
                    //visualTable.OnChangeSelectedRow += visualTable_OnChangeSelectedRow;
                    //visualTable.DT = sourceTable;
                    //visualTable.AddColumn("Модель", MODEL_NAME_COLUMN, 144);
                    //visualTable.AddColumn("План", PLAN_COLUMN, 35);
                    //visualTable.AddColumn("Факт", FACT_COLUMN, 35);

                    //while (reader.Read())
                    //{
                    //    object name = reader[MODEL_NAME_COLUMN];
                    //    object id = reader[MODEL_ID_COLUMN];
                    //    object count = reader[PLAN_COLUMN];

                    //    visualTable.AddRow(name, id, count, 0);
                    //}

                    //visualTable.Focus();

                    documentId = idObj;
                    acceptanceDoc.Read<AcceptanceOfNewComponents>(documentId);

                    if(acceptanceDoc.CaseModel==0)
                    {
                        selectedModelId = acceptanceDoc.LampModel == 0
                                              ? acceptanceDoc.UnitModel
                                              : acceptanceDoc.LampModel;
                    }
                    else
                    {
                        selectedModelId = acceptanceDoc.CaseModel;
                    }

                    string caseModel = CatalogObject.GetDescription(typeof (Models).Name, acceptanceDoc.CaseModel);
                    string lampModel = CatalogObject.GetDescription(typeof (Models).Name, acceptanceDoc.LampModel);
                    string unitModel = CatalogObject.GetDescription(typeof (Models).Name, acceptanceDoc.UnitModel);
                    const string emptyModel = "-";
                    MainProcess.CreateLabel(
                        string.Concat("Корпус: ", string.IsNullOrEmpty(caseModel) ? emptyModel : caseModel)
                        , 5, 95, 230, MobileFontSize.Normal, MobileFontPosition.Left, MobileFontColors.Default);
                    MainProcess.CreateLabel(
                        string.Concat("Лампа: ", string.IsNullOrEmpty(lampModel) ? emptyModel : lampModel)
                        , 5, 130, 230, MobileFontSize.Normal, MobileFontPosition.Left, MobileFontColors.Default);
                    MainProcess.CreateLabel(
                        string.Concat("Блок: ", string.IsNullOrEmpty(unitModel) ? emptyModel : unitModel)
                        , 5, 165, 230, MobileFontSize.Normal, MobileFontPosition.Left, MobileFontColors.Default);
                    labelOfCount = MainProcess.CreateLabel("0", 0, 215, 240, MobileFontSize.Large,
                                                           MobileFontPosition.Center, MobileFontColors.Info);
                    MainProcess.CreateButton("Ок", 15, 275, 210, 35, "ok", ok_Click);
                }
                else
                {
                    MainProcess.CreateLabel(
                        "Не знайдено жодного відкритого документа \"Прийомка нового комплектуючого\" для обраного типу!",
                        5, 115, 230, 150, MobileFontSize.Multiline, MobileFontPosition.Center, MobileFontColors.Warning, FontStyle.Bold);
                }
            }
        }

        /// <summary>Отсканировано комплектующее для приемки</summary>
        public override void OnBarcode(string Barcode)
        {
            if (Barcode.IsValidBarcode())
            {
                //int planCount = Convert.ToInt32(selectedRow[PLAN_COLUMN]);
                //int factValue = Convert.ToInt32(selectedRow[FACT_COLUMN]);

                //if (planCount > factValue)
                //{
                if (newElements.ContainsKey(Barcode) || BarcodeWorker.IsBarcodeExist(Barcode))
                {
                    ShowMessage("Данный штрих-код вже існує у системі!");
                }
                else
                {
                    newElements.Add(Barcode, selectedModelId);

                    int currCount = Convert.ToInt32(labelOfCount.Text);
                    labelOfCount.Text = (++currCount).ToString();
                }
                //}
                //else
                //{
                //    ShowMessage("Змініть модель!");
                //}
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

        //#region Changes
        ///// <summary>Маркировка изменена</summary>
        //void visualTable_OnChangeSelectedRow(object sender, OnChangeSelectedRowEventArgs e)
        //{
        //    selectedRow = e.SelectedRow;
        //    selectedModelName = selectedRow[MODEL_NAME_COLUMN].ToString();
        //    selectedModelId = Convert.ToInt32(selectedRow[MODEL_ID_COLUMN]);
        //    marking.Text = string.Format("Обрана модель:\r\n{0}", selectedModelName.TrimEnd());
        //}
        //#endregion

        #region Button+Create
        /// <summary>Закрытие приемки</summary>
        private void ok_Click()
        {
            if (newElements != null)
            {
                //if (newElements.Count>10)
                //{
                //    ShowMessage("Процес збереження може зайняти багато часу.\r\nОчікуйте завершення процессу!");
                //}

                acceptanceDoc.Read<AcceptanceOfNewComponents>(documentId);
                DateTime t1 = DateTime.Now;

                foreach (KeyValuePair<string, long> element in newElements)
                {
                    //Створення
                    createAccessory(element.Key, element.Value);
                }

                //Збереження змін для документу прийомки
                //acceptanceDoc.Posted = true;
                //acceptanceDoc.Save();

                TimeSpan time = DateTime.Now - t1;
                ShowMessage(time.ToString());
            }

            OnHotKey(KeyAction.Esc);
        }

        /// <summary>Создание комплектующего</summary>
        /// <param name="barcode">Штрихкод</param>
        /// <param name="modelId">Маркировка</param>
        private void createAccessory(string barcode, long modelId)
        {
            createAccessory(typeOfAccessory, barcode, modelId);
        }

        /// <summary>Создание комплектующего</summary>
        /// <param name="type">Тип комплектующего</param>
        /// <param name="barcode">Штрихкод</param>
        /// <param name="modelId">Маркировка</param>
        private void createAccessory(TypeOfAccessories type, string barcode, long modelId)
        {
            createAccessory(type, barcode, modelId, 0);
        }

        /// <summary>Создание комплектующего</summary>
        /// <param name="type">Тип комплектующего</param>
        /// <param name="barcode">Штрихкод</param>
        /// <param name="modelId">Маркировка</param>
        /// <param name="caseId">Id корпуса</param>
        /// <returns>Id созданного комплектующего</returns>
        private long createAccessory(TypeOfAccessories type, string barcode, long modelId, long caseId)
        {
            //Якщо вже було оброблено N елементів, то очистити
            if (count == 30)
            {
                //Збір мусору
                GC.Collect();
                GC.WaitForPendingFinalizers();
                count = 0;
            }
            else
            {
                count++;
            }

            Accessory accessory = null;

            switch (type)
            {
                case TypeOfAccessories.Lamp:
                    accessory = new db.Lamps();
                    ((db.Lamps)accessory).Case = caseId;
                    break;
                case TypeOfAccessories.Case:
                    accessory = new Cases();
                    break;
                case TypeOfAccessories.ElectronicUnit:
                    accessory = new ElectronicUnits();
                    ((ElectronicUnits) accessory).Case = caseId;
                    break;
            }

            if (accessory != null)
            {
                bool mainAccessory = type == TypeOfAccessories.Case;

                accessory.BarCode = caseId == 0 ? barcode : string.Empty;
                accessory.Model = mainAccessory ? acceptanceDoc.CaseModel : modelId;
                accessory.DateOfWarrantyEnd = acceptanceDoc.InvoiceDate.AddYears(acceptanceDoc.WarrantlyYears);
                accessory.Party = acceptanceDoc.InvoiceNumber;
                accessory.TypeOfWarrantly = acceptanceDoc.TypesOfWarrantly;
                accessory.Status = acceptanceDoc.State;

                accessory.Save();

                if (mainAccessory)
                {
                    Cases newCase = accessory as Cases;

                    if (newCase != null)
                    {
                        long id = createAccessory(TypeOfAccessories.ElectronicUnit, string.Empty, acceptanceDoc.UnitModel, accessory.Id);
                        newCase.ElectronicUnit = id;

                        id = createAccessory(TypeOfAccessories.Lamp, string.Empty, acceptanceDoc.LampModel, accessory.Id);
                        newCase.Lamp = id;

                        accessory.Save();
                    }
                }

                //Внесение записи в "Перемещение"
                Movement.RegisterLighter(accessory.BarCode, accessory.SyncRef, OperationsWithLighters.Acceptance);
            }

            //SqlCeCommand query = dbWorker.NewQuery("SELECT SyncRef FROM Cases WHERE RTRIM(Barcode)=RTRIM(@Barcode)");
            //query.AddParameter("Barcode", barcode);
            //object syncRefObj = query.ExecuteScalar();
            //string syncRef = syncRefObj == null ? string.Empty : syncRefObj.ToString();

            return accessory == null ? 0 : accessory.Id;
        }
        #endregion

        #region Query
        /// <summary>Получить ID свежей приемки</summary>
        /// <returns></returns>
        private object getAcceptanceDoc()
        {
            SqlCeCommand query = dbWorker.NewQuery(@"SELECT Id FROM AcceptanceOfNewComponents a WHERE a.Posted=0 AND a.MarkForDeleting=0 AND TypeOfAccessories=@Accessory ORDER BY a.Date DESC");
            query.AddParameter("Accessory", TypeOfAccessory);

            return query.ExecuteScalar();
        }

//        /// <summary>Инфорация про приемку</summary>
//        /// <param name="id">ID приемки</param>
//        private SqlCeDataReader getAcceptanceInfo(object id)
//        {
//            SqlCeCommand query = dbWorker.NewQuery(@"SELECT m.Description ModelName, m.Id ModelId, s.[Plan]
//FROM SubAcceptanceOfNewComponentsMarkingInfo s 
//LEFT JOIN Models m ON m.Id=s.Marking
//WHERE s.Id=@Id AND m.Id IS NOT NULL");
//            query.AddParameter("Id", id);

//            return query.ExecuteReader();
//        }
        #endregion
    }
}