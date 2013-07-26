using WMS_client.Base.Visual.Constructor;
using System.Collections.Generic;
using System;
using WMS_client.db;
using System.Data.SqlServerCe;
using WMS_client.Enums;

namespace WMS_client.Processes.Lamps
    {
    /// <summary>Заміна комплектуючого</summary>
    public class ReplacingAccessory : BusinessProcess
        {
        private readonly object CaseBarcode;
        private readonly object NewAccessoryBarcode;
        private readonly bool replaceProcess;
        private readonly TypeOfAccessories type;

        private string accessoryDescription;
        private string accessoryTable;
        private string accessoryField;

        public ReplacingAccessory(WMSClient MainProcess, object caseBarcode, object accessoryBarcode, bool replaceProcess, TypeOfAccessories type)
            : base(MainProcess, 1)
            {
            CaseBarcode = caseBarcode;
            NewAccessoryBarcode = accessoryBarcode;
            FormNumber = 1;
            BusinessProcessType = ProcessType.Registration;
            this.replaceProcess = replaceProcess;
            this.type = type;

            IsLoad = true;
            fillStr();
            DrawControls();
            }

        private void fillStr()
            {
            accessoryTable = Cases.GetTableNameForAccessory(type);
            accessoryDescription = Cases.GetDescriptionOfAccessory(type);
            accessoryField = Cases.GetColumnOfAccessory(type);
            }

        #region Override methods
        public override sealed void DrawControls()
            {
            if (IsLoad)
                {
                bool underWarranty;
                object[] array = getInfo(out underWarranty);
                List<LabelForConstructor> listOfLabels;

                if (replaceProcess)
                    {
                    if (underWarranty)
                        {
                        listOfLabels = new List<LabelForConstructor>
                                           {
                                               new LabelForConstructor("УВАГА!", ControlsStyle.LabelH2Red),
                                               new LabelForConstructor("Вилучене комплектуюче",ControlsStyle.LabelH2Red),
                                               new LabelForConstructor("знаходиться на гарантії!", ControlsStyle.LabelH2Red),
                                           };
                        }
                    else
                        {
                        listOfLabels = new List<LabelForConstructor>
                                           {
                                               new LabelForConstructor(string.Empty, false),
                                               new LabelForConstructor("Комплектуюче", ControlsStyle.LabelH2),
                                               new LabelForConstructor("не на гарантії!", ControlsStyle.LabelH2),
                                           };
                        }
                    }
                else
                    {
                    listOfLabels = new List<LabelForConstructor>
                                           {
                                               new LabelForConstructor(string.Empty, false),
                                               new LabelForConstructor(string.Empty, false),
                                               new LabelForConstructor(string.Empty, false)
                                           };
                    }

                listOfLabels.AddRange(new List<LabelForConstructor>
                                          {
                                              new LabelForConstructor(string.Empty, false),
                                              new LabelForConstructor(accessoryDescription, ControlsStyle.LabelH2),
                                              new LabelForConstructor("Модель: {0}",1),
                                              new LabelForConstructor("Партія: {0}"),
                                              new LabelForConstructor("Гарантія до {0}"),
                                              new LabelForConstructor(string.Empty, false),
                                              new LabelForConstructor(replaceProcess && underWarranty
                                                                          ? "Помітити на ремонт?"
                                                                          : string.Empty,
                                                                      ControlsStyle.LabelH2)
                                          });

                new ListOfLabelsConstructor(MainProcess, accessoryDescription, array) { ListOfLabels = listOfLabels };

                if (replaceProcess)
                    {
                    if (underWarranty)
                        {
                        MainProcess.CreateButton("Так", 15, 275, 100, 35, "yes", yes_Click);
                        MainProcess.CreateButton("Ні", 125, 275, 100, 35, "no", no_Click);
                        }
                    else
                        {
                        MainProcess.CreateButton("Замінити", 15, 275, 210, 35, "ok", no_Click);
                        }
                    }
                else
                    {
                    MainProcess.CreateButton("Ок", 15, 275, 210, 35, "ok", ok_Click);
                    }
                }
            }

        public override void OnBarcode(string Barcode)
            {
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
        private void yes_Click()
            {
            finish(true);
            }

        private void no_Click()
            {
            finish(false);
            }

        private void ok_Click()
            {
            object caseId = BarcodeWorker.GetIdByBarcode(CaseBarcode);
            //Статус корпуса, в который запихиваем новое комплектующее
            TypesOfLampsStatus status = Accessory.GetState(TypeOfAccessories.Case, CaseBarcode.ToString());
            bool isLamp = accessoryTable == typeof(db.Lamps).Name;
            object accessoryId = BarcodeWorker.GetIdByBarcode(NewAccessoryBarcode.ToString());

            string command = string.Format(
                "UPDATE {0} SET Status=@IsWorking,[Case]=@Case,{1}=0,DateOfActuality=@Date{2} WHERE RTRIM(BarCode)=@Barcode",
                accessoryTable,
                dbObject.IS_SYNCED,
                isLamp ? ",Barcode=@NewBarCode" : string.Empty);
            using (SqlCeCommand query = dbWorker.NewQuery(command))
                {
                query.AddParameter("Barcode", NewAccessoryBarcode);
                query.AddParameter("NewBarCode", string.Empty);
                query.AddParameter("Case", caseId);
                query.AddParameter("IsWorking", status);
                query.AddParameter("Date", DateTime.Now);
                query.ExecuteNonQuery();
                }

            using (SqlCeCommand query = dbWorker.NewQuery(string.Format(
                    "UPDATE Cases SET {0}=@Id,{1}=0,DateOfActuality=@Date WHERE RTRIM(BarCode)=@Case",
                    accessoryField,
                    dbObject.IS_SYNCED)))
                {
                query.AddParameter("Case", CaseBarcode);
                query.AddParameter("Id", accessoryId);
                query.AddParameter("Date", DateTime.Now);
                query.ExecuteNonQuery();
                }
            //query = dbWorker.NewQuery(string.Format(
            //    "UPDATE {0} SET Status=@IsWorking,[Case]=@Id,{1}=0,DateOfActuality=@Date WHERE {2}=@Id",
            //    accessoryTable,
            //    dbObject.IS_SYNCED,
            //    dbObject.IDENTIFIER_NAME));
            //query.AddParameter("Id", caseId);
            //query.AddParameter(dbObject.IDENTIFIER_NAME, accessoryId);
            //query.AddParameter("Date", DateTime.Now);
            //query.ExecuteNonQuery();

            //string caseBarcode = CaseBarcode.ToString();
            //installMovement(caseBarcode);
            OnHotKey(KeyAction.Esc);
            }

        private void finish(bool isForExchange)
            {
            object[] result = null;
            //Штрихкод установленной лампы
            using (SqlCeCommand query = dbWorker.NewQuery(string.Format(@"
SELECT a.Barcode, a.SyncRef
FROM Cases c
LEFT JOIN {0} a ON a.Id=c.{1}
WHERE RTRIM(c.BarCode)=@BarCode", accessoryTable, accessoryTable.Substring(0, accessoryTable.Length - 1))))
                {
                query.AddParameter("BarCode", CaseBarcode);
                result = query.SelectArray();
                }

            if (result != null && result.Length == 2)
                {
                object oldLampBarcode = result[0];
                object accessoryRef = result[1];
                object caseId = BarcodeWorker.GetIdByBarcode(CaseBarcode);
                object newAccessoryId = BarcodeWorker.GetIdByBarcode(NewAccessoryBarcode);
                bool isLamp = accessoryTable == typeof(db.Lamps).Name;

                string partOfCommand = string.Format(
                    "UPDATE {0} SET Status=@Status{1},[Case]=@Case,{2}=0,DateOfActuality=@Date{3} WHERE ",
                    accessoryTable,
                    type != TypeOfAccessories.ElectronicUnit
                        ? ", Barcode=@NewBarcode "
                        : string.Empty,
                    dbObject.IS_SYNCED,
                    "{0}");

                //Статус комплектующего, которое меняем
                TypesOfLampsStatus status = Accessory.GetState(TypeOfAccessories.Lamp, oldLampBarcode.ToString());

                //Старое комплектующее
                using (SqlCeCommand query = dbWorker.NewQuery(string.Concat(
                    string.Format(partOfCommand, ",DrawdownDate=@DrawdownDate"),
                    " RTRIM(SyncRef)=RTRIM(@SyncRef)")))
                    {
                    query.AddParameter("Status",
                                       isForExchange ? TypesOfLampsStatus.ToRepair : TypesOfLampsStatus.Storage);
                    query.AddParameter("Case", 0);
                    query.AddParameter("LampBarcode", oldLampBarcode);
                    query.AddParameter("NewBarcode", isLamp ? NewAccessoryBarcode : oldLampBarcode);
                    query.AddParameter(dbObject.SYNCREF_NAME, accessoryRef);
                    query.AddParameter("Date", DateTime.Now);
                    query.AddParameter("DrawdownDate", DateTime.Now);
                    query.ExecuteNonQuery();
                    }

                //Новое комплектующее
                using (
                    SqlCeCommand query =
                        dbWorker.NewQuery(string.Concat(string.Format(partOfCommand, string.Empty), " Id=@Id")))
                    {
                    query.AddParameter("Status", status);
                    query.AddParameter("Case", caseId);
                    query.AddParameter("Id", newAccessoryId);
                    query.AddParameter("NewBarcode", isLamp ? string.Empty : oldLampBarcode);
                    query.AddParameter("Date", DateTime.Now);
                    query.ExecuteNonQuery();
                    }

                //Замена комплектующего в светильнике
                using (SqlCeCommand query = dbWorker.NewQuery(string.Format(
                    "UPDATE CASES SET {0}=@NewAccessoryId,{1}=0,DateOfActuality=@Date WHERE RTRIM(BarCode)=RTRIM(@CaseBarcode)",
                    accessoryField,
                    dbObject.IS_SYNCED)))
                    {
                    query.AddParameter("CaseBarcode", CaseBarcode);
                    query.AddParameter("NewAccessoryId", newAccessoryId);
                    query.AddParameter("Date", DateTime.Now);
                    query.ExecuteNonQuery();
                    }
                //Завершение
                //string caseBarcode = CaseBarcode.ToString();
                //installMovement(caseBarcode);
                //removeMovement(oldLampBarcode.ToString(), caseBarcode);
                OnHotKey(KeyAction.Esc);
                }
            }

        //private void installMovement(string caseBarCode)
        //{
        //    List<object> data = getPositionInfo(caseBarCode);
        //    string newBarcode = NewAccessoryBarcode.ToString();
        //    string syncRef = BarcodeWorker.GetRefByBarcode(accessoryTable, newBarcode);

        //    Movement.RegisterLighter(newBarcode, syncRef, OperationsWithLighters.Installing,
        //                             (int) data[0], (int) data[1], (int) data[2]);
        //}

        //private void removeMovement(string oldBarcode, string caseBarCode)
        //{
        //    List<object> data = getPositionInfo(caseBarCode);
        //    string syncRef = BarcodeWorker.GetRefByBarcode(accessoryTable, oldBarcode);

        //    Movement.RegisterLighter(oldBarcode, syncRef, OperationsWithLighters.Removing,
        //                             (int) data[0], (int) data[1], (int) data[2]);
        //}

        //private List<object> getPositionInfo(string caseBarCode)
        //{
        //    string query = string.Format("SELECT Map,Register,Position From {0} WHERE RTRIM({1})=RTRIM(@{1})",
        //                                 typeof (Cases).Name, dbObject.BARCODE_NAME);
        //    SqlCeCommand command = dbWorker.NewQuery(query);
        //    command.AddParameter(dbObject.BARCODE_NAME, caseBarCode);

        //    return command.SelectToList();
        //}
        #endregion

        #region Query

        private object[] getInfo(out bool underWarranty)
            {
            string table = type == TypeOfAccessories.Lamp ? "Lamps" : "ElectronicUnits";
            using (SqlCeCommand query = dbWorker.NewQuery(string.Format(@"SELECT 
	CASE WHEN l.DateOfWarrantyEnd>=@EndOfDay THEN 1 ELSE 0 END UnderWarranty
	, t.Description CaseModel
	, p.Description CaseParty
	, l.DateOfWarrantyEnd CaseWarrantly
FROM {0} l 
LEFT JOIN Cases c ON l.Id=c.Lamp
LEFT JOIN Models t ON t.Id=l.Model
LEFT JOIN Party p ON p.Id=l.Party
WHERE RTRIM(l.BarCode) like @BarCode", table)))
                {
                query.AddParameter("BarCode", NewAccessoryBarcode);
                query.AddParameter("EndOfDay", DateTime.Now.Date.AddDays(1));
                object[] result = query.SelectArray();
                underWarranty = result != null && Convert.ToBoolean(result[0]);

                return result;
                }
            }

        #endregion
        }
    }