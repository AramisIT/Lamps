using System;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using WMS_client.Models;
using WMS_client.WinProcessesManagement;

namespace WMS_client.Repositories
    {
    class AccessoryInserter<T> : SqlCeResultSet where T : IAccessory
        {
        private readonly ResultSetOptions RESULT_SET_OPTIONS = ResultSetOptions.Scrollable | ResultSetOptions.Sensitive | ResultSetOptions.Updatable;
        private Func<SqlCeConnection> getSqlConnection;
        private List<T> accessotyList;
        private string tableName;

        public AccessoryInserter(string tableName, List<T> accessotyList, Func<SqlCeConnection> getSqlConnection)
            {
            this.tableName = tableName;
            this.accessotyList = accessotyList;
            this.getSqlConnection = getSqlConnection;
            }

        internal bool InsertAccessories(Action<SqlCeUpdatableRecord, IAccessory> fillValues)
            {
            if (accessotyList.Count == 0)
                {
                return true;
                }

            bool addBarcode = accessotyList[0] is IBarcodeAccessory;
            bool addRepairWarranty = accessotyList[0] is IFixableAccessory;
            bool callFillValues = fillValues != null;

            try
                {
                using (var conn = getSqlConnection())
                    {
                    using (var sqlCeSelectCommand = conn.CreateCommand())
                        {
                        sqlCeSelectCommand.CommandText = tableName;
                        sqlCeSelectCommand.CommandType = System.Data.CommandType.TableDirect;
                        sqlCeSelectCommand.ExecuteResultSet(RESULT_SET_OPTIONS, this);

                        foreach (var accessory in accessotyList)
                            {
                            var newRow = CreateRecord();

                            newRow["Id"] = accessory.Id;
                            newRow["Model"] = accessory.Model;
                            newRow["Party"] = accessory.Party;
                            newRow["Status"] = accessory.Status;

                            object warrantyExpiryDate = null;
                            if (accessory.WarrantyExpiryDate != DateTime.MinValue)
                                {
                                warrantyExpiryDate = accessory.WarrantyExpiryDate;
                                }

                            newRow["WarrantyExpiryDate"] = warrantyExpiryDate;

                            if (addBarcode)
                                {
                                newRow["Barcode"] = ((IBarcodeAccessory)accessory).Barcode;
                                }

                            if (addRepairWarranty)
                                {
                                newRow["RepairWarranty"] = ((IFixableAccessory)accessory).RepairWarranty;
                                }

                            if (callFillValues)
                                {
                                fillValues(newRow, accessory);
                                }

                            Insert(newRow);
                            }
                        }
                    }
                }
            catch (Exception exp)
                {
                Debug.WriteLine(string.Format("Ошибка вставки в базу - {0}", exp.Message));
                return false;
                }
            finally
                {
                Close();
                }
            return true;
            }
        }
    }
