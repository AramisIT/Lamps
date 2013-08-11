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
        private string selectedModelRef;
        private object documentId;
        private readonly AcceptanceOfNewComponents acceptanceDoc;
        private readonly TypeOfAccessories typeOfAccessory;
        private int TypeOfAccessory { get { return (int)typeOfAccessory; } }
        //private readonly Dictionary<string, long> newElements = new Dictionary<string, long>();
        private readonly Dictionary<string, string> newComponents = new Dictionary<string, string>();
        private MobileLabel labelOfCount;
        //private int count;

        string caseModelRef;
        string lampModelRef;
        string unitModelRef;
        #endregion

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

                    documentId = idObj;
                    acceptanceDoc.Read<AcceptanceOfNewComponents>(documentId);

                    string caseModel = CatalogObject.GetDescription(typeof(WMS_client.db.Models).Name, acceptanceDoc.CaseModel);
                    string lampModel = CatalogObject.GetDescription(typeof(WMS_client.db.Models).Name, acceptanceDoc.LampModel);
                    string unitModel = CatalogObject.GetDescription(typeof(WMS_client.db.Models).Name, acceptanceDoc.UnitModel);

                    caseModelRef = CatalogObject.GetSyncRef(typeof(WMS_client.db.Models).Name, acceptanceDoc.CaseModel);
                    lampModelRef = CatalogObject.GetSyncRef(typeof(WMS_client.db.Models).Name, acceptanceDoc.LampModel);
                    unitModelRef = CatalogObject.GetSyncRef(typeof(WMS_client.db.Models).Name, acceptanceDoc.UnitModel);

                    if (acceptanceDoc.CaseModel == 0)
                        {
                        selectedModelRef = acceptanceDoc.LampModel == 0
                                              ? unitModelRef
                                              : lampModelRef;
                        }
                    else
                        {
                        selectedModelRef = caseModelRef;
                        }
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
                    MainProcess.CreateButton("Завершити приймання", 15, 275, 210, 35, "ok", ok_Click);
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
                if (newComponents.ContainsKey(Barcode) || BarcodeWorker.IsBarcodeExist(Barcode))
                    {
                    ShowMessage("Данный штрих-код вже існує у системі!");
                    }
                else
                    {
                    newComponents.Add(Barcode, selectedModelRef);

                    int currCount = Convert.ToInt32(labelOfCount.Text);
                    labelOfCount.Text = (++currCount).ToString();
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

        #region Button+Create
        /// <summary>Закрытие приемки</summary>
        private void ok_Click()
            {
            if (newComponents != null)
                {
                AcceptanceOfNewComponentsDetails.SaveArray(Convert.ToInt64(documentId), typeOfAccessory, newComponents);

                //Збереження змін для документу прийомки
                acceptanceDoc.Posted = true;
                acceptanceDoc.Write();
                }

            OnHotKey(KeyAction.Esc);
            }

        #region 4Deleting
        ///// <summary>Создание комплектующего</summary>
        ///// <param name="barcode">Штрихкод</param>
        ///// <param name="modelId">Маркировка</param>
        //private void createAccessory(string barcode, long modelId)
        //{
        //    createAccessory(typeOfAccessory, barcode, modelId);
        //}

        ///// <summary>Создание комплектующего</summary>
        ///// <param name="type">Тип комплектующего</param>
        ///// <param name="barcode">Штрихкод</param>
        ///// <param name="modelId">Маркировка</param>
        //private void createAccessory(TypeOfAccessories type, string barcode, long modelId)
        //{
        //    createAccessory(type, barcode, modelId, 0);
        //}

        ///// <summary>Создание комплектующего</summary>
        ///// <param name="type">Тип комплектующего</param>
        ///// <param name="barcode">Штрихкод</param>
        ///// <param name="modelId">Маркировка</param>
        ///// <param name="caseId">Id корпуса</param>
        ///// <returns>Id створеного комплектующего</returns>
        //private long createAccessory(TypeOfAccessories type, string barcode, long modelId, long caseId)
        //{
        //    //Якщо вже було оброблено N елементів, то очистити
        //    if (count == 30)
        //    {
        //        //Збір мусору
        //        GC.Collect();
        //        GC.WaitForPendingFinalizers();
        //        count = 0;
        //    }
        //    else
        //    {
        //        count++;
        //    }

        //    Accessory accessory = null;

        //    switch (type)
        //    {
        //        case TypeOfAccessories.Lamp:
        //            accessory = new db.Lamps();
        //            ((db.Lamps)accessory).Case = caseId;
        //            break;
        //        case TypeOfAccessories.Case:
        //            accessory = new Cases();
        //            break;
        //        case TypeOfAccessories.ElectronicUnit:
        //            accessory = new ElectronicUnits();
        //            ((ElectronicUnits) accessory).Case = caseId;
        //            break;
        //    }

        //    if (accessory != null)
        //    {
        //        bool mainAccessory = type == TypeOfAccessories.Case;

        //        accessory.BarCode = caseId == 0 ? barcode : string.Empty;
        //        accessory.Model = mainAccessory ? acceptanceDoc.CaseModel : modelId;
        //        accessory.DateOfWarrantyEnd = acceptanceDoc.InvoiceDate.AddYears(acceptanceDoc.WarrantlyYears);
        //        accessory.Party = acceptanceDoc.InvoiceNumber;
        //        accessory.TypeOfWarrantly = acceptanceDoc.TypesOfWarrantly;
        //        accessory.Status = acceptanceDoc.State;

        //        accessory.Save();

        //        if (mainAccessory)
        //        {
        //            Cases newCase = accessory as Cases;

        //            if (newCase != null)
        //            {
        //                long id = createAccessory(TypeOfAccessories.ElectronicUnit, string.Empty, acceptanceDoc.UnitModel, accessory.Id);
        //                newCase.ElectronicUnit = id;

        //                id = createAccessory(TypeOfAccessories.Lamp, string.Empty, acceptanceDoc.LampModel, accessory.Id);
        //                newCase.Lamp = id;

        //                accessory.Save();
        //            }
        //        }

        //        //Внесення запису в "Перемыщення"
        //        Movement.RegisterLighter(accessory.BarCode, accessory.SyncRef, OperationsWithLighters.Acceptance);
        //    }

        //    //SqlCeCommand query = dbWorker.NewQuery("SELECT SyncRef FROM Cases WHERE RTRIM(Barcode)=RTRIM(@Barcode)");
        //    //query.AddParameter("Barcode", barcode);
        //    //object syncRefObj = query.ExecuteScalar();
        //    //string syncRef = syncRefObj == null ? string.Empty : syncRefObj.ToString();

        //    return accessory == null ? 0 : accessory.Id;
        //} 
        #endregion
        #endregion

        #region Query

        /// <summary>Получить ID свежей приемки</summary>
        /// <returns></returns>
        private object getAcceptanceDoc()
            {
            const string sql =
                @"SELECT Id FROM AcceptanceOfNewComponents a WHERE a.Posted=0 AND a.MarkForDeleting=0 AND TypeOfAccessories=@Accessory ORDER BY a.Date DESC";
          
            using (SqlCeCommand query = dbWorker.NewQuery(sql))
                {
                query.AddParameter("Accessory", TypeOfAccessory);
                return query.ExecuteScalar();
                }
            }

        #endregion
        }
    }