using System;
using System.Drawing;
using WMS_client.Base.Visual.Constructor;
using WMS_client.Enums;
using WMS_client.db;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data.SqlServerCe;

namespace WMS_client
{
    /// <summary>Демонстратор (Инфо)</summary>
    public class VisualPresenter : BusinessProcess
    {
        /// <summary>Демонстратор (Инфо)</summary>
        public VisualPresenter(WMSClient MainProcess)
            : base(MainProcess, 1)
        {
        }

        #region Override methods
        public override sealed void DrawControls()
        {
            MainProcess.ClearControls();
            MainProcess.CreateLabel("Відскануйте", 0, 140, 240, MobileFontSize.Normal, MobileFontPosition.Center, MobileFontColors.Info, FontStyle.Bold);
            MainProcess.CreateLabel("ШТРИХ-КОД!", 0, 170, 240, MobileFontSize.Normal, MobileFontPosition.Center, MobileFontColors.Info, FontStyle.Bold);
        }

        public override void OnBarcode(string Barcode)
        {
            if (Barcode.IsValidBarcode())
            {
                showInfoByBarcode(Barcode);
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

        #region DrawButton
        /// <summary>Кнопки перехода на другие элмеенты</summary>
        /// <param name="listOfDetail">Словарь данных для кнопок [Текст кнопки; [Тип элемента; Штрих-код элемента]] </param>
        private void drawButtons(Dictionary<string, KeyValuePair<Type, object>> listOfDetail)
        {
            if (listOfDetail.Count != 0)
            {
                const int top = 275;
                const int height = 35;
                int left = 15/listOfDetail.Count;
                int width = (240 - left*(listOfDetail.Count+1))/listOfDetail.Count;
                int delta = left;

                foreach (KeyValuePair<string, KeyValuePair<Type, object>> detail in listOfDetail)
                {
                    long id = Convert.ToInt64(detail.Value.Value);

                    MainProcess.CreateButton(detail.Key, delta, top, width, height, detail.Value.Key.Name, button_click,
                                             detail.Value.Value, id!=0);
                    delta += left + width;
                }
            }
        }

        /// <summary>Переход на другое элемент</summary>
        /// <param name="sender">Кнопка</param>
        private void button_click(object sender)
        {
            Button button = ((Button) sender);
            string command = string.Format("SELECT {0},{1} FROM {2} WHERE {3}=@Id",
                                           dbObject.BARCODE_NAME, dbObject.IDENTIFIER_NAME,
                                           button.Name, dbObject.IDENTIFIER_NAME);
            SqlCeCommand query = dbWorker.NewQuery(command);
            query.AddParameter("Id", button.Tag);
            List<object> values = query.SelectToList();

            if (values != null && values.Count ==2 && values[0]!=null)
            {
                string barcode = values[0].ToString().TrimEnd();

                if (!string.IsNullOrEmpty(barcode))
                {
                    showInfoByBarcode(barcode);
                }
                else
                {
                    long id = values[1] != null ? Convert.ToInt64(values[1]) : 0;
                    TypeOfAccessories typeOfAccessories = button.Name == typeof(Lamps).Name
                                                              ? TypeOfAccessories.Lamp
                                                              : TypeOfAccessories.ElectronicUnit;
                    showInfoById(id, typeOfAccessories);
                }
            }
        }
        #endregion

        #region Show
        /// <summary>Отображение</summary>
        /// <param name="barcode">Штрихкод</param>
        private void showInfoByBarcode(string barcode)
        {
            MainProcess.ClearControls();

            ListOfLabelsConstructor list = new ListOfLabelsConstructor(MainProcess);
            string topic;
            Dictionary<string, KeyValuePair<Type, object>> listOfDetail;

            //Отображаем текстовое инфо о элементе
            list.ListOfLabels = CatalogObject.GetVisualPresenter(barcode, out topic, out listOfDetail);
            MainProcess.ToDoCommand = topic;
            //Отображаем кнопки для перехода на связанные элементы
            drawButtons(listOfDetail);
        }

        /// <summary>Отображение</summary>
        /// <param name="id">Id</param>
        /// <param name="typeOfAccessories">Тип комплектующего</param>
        private void showInfoById(long id, TypeOfAccessories typeOfAccessories)
        {
            MainProcess.ClearControls();

            ListOfLabelsConstructor list = new ListOfLabelsConstructor(MainProcess);
            string topic;
            Dictionary<string, KeyValuePair<Type, object>> listOfDetail;

            //Отображаем текстовое инфо о элементе
            list.ListOfLabels = CatalogObject.GetVisualPresenter(id, typeOfAccessories, out topic, out listOfDetail);
            MainProcess.ToDoCommand = topic;
            //Отображаем кнопки для перехода на связанные элементы
            drawButtons(listOfDetail);
        } 
        #endregion
    }
}