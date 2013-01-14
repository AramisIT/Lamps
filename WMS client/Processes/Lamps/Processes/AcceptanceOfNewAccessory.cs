using System.Collections.Generic;
using System.Data;
using System;
using System.Drawing;
using WMS_client.Enums;
using System.Data.SqlServerCe;
using WMS_client.db;

namespace WMS_client.Processes.Lamps
{
    /// <summary>Приемка нового коплектующего</summary>
    public class AcceptanceOfNewAccessory : BusinessProcess
    {
        private MobileLabel marking;
        private DataRow selectedRow;
        private string selectedMarking;
        private int selectedMarkingId;
        private object documentId;
        private AcceptanceOfNewComponents acceptanceDoc;
        private readonly TypeOfAccessories typeOfAccessory;
        private int TypeOfAccessory { get { return (int) typeOfAccessory; } }
        private readonly Dictionary<string, string> newElements = new Dictionary<string, string>();

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
                    marking = MainProcess.CreateLabel("Маркування не обрано", 5, 60, 230, 45,
                                                      MobileFontSize.Multiline, MobileFontPosition.Center,
                                                      MobileFontColors.Info, FontStyle.Bold);
                    documentId = idObj;
                    SqlCeDataReader reader = getAcceptanceInfo(idObj);

                    DataTable sourceTable = new DataTable();
                    sourceTable.Columns.AddRange(new[]
                                                     {
                                                         new DataColumn("Marking", typeof (string)),
                                                         new DataColumn("MarkingId", typeof (int)),
                                                         new DataColumn("Plan", typeof (int)),
                                                         new DataColumn("Fact", typeof (int))
                                                     });

                    MobileTable visualTable = MainProcess.CreateTable("Markings", 160, 105);
                    visualTable.OnChangeSelectedRow += visualTable_OnChangeSelectedRow;
                    visualTable.DT = sourceTable;
                    visualTable.AddColumn("Маркування", "Marking", 144);
                    visualTable.AddColumn("План", "Plan", 35);
                    visualTable.AddColumn("Факт", "Fact", 35);

                    while (reader.Read())
                    {
                        visualTable.AddRow(reader["Marking"], reader["MarkingId"], reader["Plan"], 0);
                    }

                    visualTable.Focus();

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

        /// <summary>Маркировка изменена</summary>
        void visualTable_OnChangeSelectedRow(object sender, OnChangeSelectedRowEventArgs e)
        {
            selectedRow = e.SelectedRow;
            selectedMarking = selectedRow["Marking"].ToString();
            selectedMarkingId = Convert.ToInt32(selectedRow["MarkingId"]);
            marking.Text = string.Format("Обране маркування:\r\n{0}", selectedMarking.TrimEnd());
        }

        /// <summary>Отсканировано комплектующее для приемки</summary>
        public override void OnBarcode(string Barcode)
        {
            int planCount = Convert.ToInt32(selectedRow["Plan"]);
            int factValue = Convert.ToInt32(selectedRow["Fact"]);

            if (planCount > factValue)
            {
                if (newElements.ContainsKey(Barcode) || BarcodeWorker.IsBarcodeExist(Barcode))
                {
                    ShowMessage("Данный штрих-код уже существует в системе!");
                }
                else
                {
                    //todo: Сохранять маркировку или ссылку на "Модель"?
                    newElements.Add(Barcode, selectedMarking);
                    selectedRow["Fact"] = factValue + 1;
                }
            }
            else
            {
                ShowMessage("Змініть маркування!");
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

        #region Button
        /// <summary>Закрытие приемки</summary>
        private void ok_Click()
        {
            if (newElements != null)
            {
                acceptanceDoc = new AcceptanceOfNewComponents();
                acceptanceDoc.Read<AcceptanceOfNewComponents>(documentId);

                foreach (KeyValuePair<string, string> element in newElements)
                {
                    createAccessory(element.Key, element.Value);
                }

                acceptanceDoc.Posted = true;
                acceptanceDoc.Save();
            }

            OnHotKey(KeyAction.Esc);
        }

        private void createAccessory(string barcode, string docMarking)
        {
            createAccessory(typeOfAccessory, barcode, docMarking);
        }

        private void createAccessory(TypeOfAccessories type, string barcode, string docMarking)
        {
            createAccessory(type, barcode, docMarking, 0);
        }

        private long createAccessory(TypeOfAccessories type, string barcode, string docMarking, long caseId)
        {
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
                accessory.BarCode = caseId == 0 ? barcode : string.Empty;
                accessory.Marking = docMarking;
                accessory.DateOfWarrantyEnd = acceptanceDoc.InvoiceDate.AddYears(acceptanceDoc.WarrantlyYears);
                accessory.Model = acceptanceDoc.Model;
                accessory.Party = acceptanceDoc.InvoiceNumber;
                accessory.TypeOfWarrantly = acceptanceDoc.TypesOfWarrantly;

                accessory.Save();

                if (type == TypeOfAccessories.Case)
                {
                    long id = Convert.ToInt64(BarcodeWorker.GetIdByBarcode(barcode));
                    accessory.Id = id;
                    id = createAccessory(TypeOfAccessories.ElectronicUnit, string.Empty, docMarking, id);
                    ((Cases) accessory).ElectronicUnit = id;
                    accessory.Save();
                }
            }

            return Convert.ToInt64(BarcodeWorker.GetIdByBarcode(barcode));
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

        /// <summary>Инфорация про приемку</summary>
        /// <param name="id">ID приемки</param>
        private SqlCeDataReader getAcceptanceInfo(object id)
        {
            SqlCeCommand query = dbWorker.NewQuery(@"SELECT m.Description Marking, m.Id MarkingId, s.[Plan]
FROM SubAcceptanceOfNewComponentsMarkingInfo s 
LEFT JOIN Models m ON m.Id=s.Marking
WHERE s.Id=@Id");
            query.AddParameter("Id", id);

            return query.ExecuteReader();
        }
        #endregion
    }
}