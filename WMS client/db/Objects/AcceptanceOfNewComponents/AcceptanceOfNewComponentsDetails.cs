using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Data;
using WMS_client.Enums;

namespace WMS_client.db
{
    /// <summary>Приймання нового комплектуючого.Дані для сервера</summary>
    public class AcceptanceOfNewComponentsDetails
    {
        /// <summary>Id документу прийомки</summary>
        [dbFieldAtt(Description = "Id документу прийомки", NotShowInForm = true)]
        public long DocumentId { get; set; }
        /// <summary>Тип комплектуючого</summary>
        [dbFieldAtt(Description = "Тип комплектуючого", NotShowInForm = true)]
        public TypeOfAccessories TypeOfAccessory { get; set; }
        /// <summary>Штрихкод</summary>
        [dbFieldAtt(Description = "Штрихкод", NotShowInForm = true)]
        public string BarCode { get; set; }
        /// <summary>Модель.Ссылка</summary>
        [dbFieldAtt(Description = "Модель.Ссылка", NotShowInForm = true)]
        public string ModelRef { get; set; }

        private const string SAVE_QUERY = "INSERT INTO AcceptanceOfNewComponentsDetails(DocumentId,TypeOfAccessory,BarCode,ModelRef) VALUES(@DocumentId,@TypeOfAccessory,@BarCode,@ModelRef)";

        public void Save()
        {
            SaveItem(DocumentId, TypeOfAccessory, BarCode, ModelRef);
        }

        public static void SaveItem(long documentId, TypeOfAccessories typeOfAccessory, string barcode, string modelRef)
        {
            //todo: вже можно не зберігати ModelRef 
            SqlCeCommand query = dbWorker.NewQuery(SAVE_QUERY);
            query.AddParameter("DocumentId", documentId);
            query.AddParameter("TypeOfAccessory", (int)typeOfAccessory);
            query.AddParameter(dbObject.BARCODE_NAME, barcode);
            query.AddParameter("ModelRef", modelRef);
            query.ExecuteNonQuery();
        }

        public static void SaveArray(long docId, TypeOfAccessories typeOfAccessory, Dictionary<string, string> newElements)
        {
            foreach (KeyValuePair<string, string> element in newElements)
            {
                SaveItem(docId, typeOfAccessory, element.Key, element.Value);
            }
        }

        public static DataTable GetAllData()
        {
            SqlCeCommand command = dbWorker.NewQuery("SELECT * FROM AcceptanceOfNewComponentsDetails");
            return command.SelectToTable();
        }
    }
}