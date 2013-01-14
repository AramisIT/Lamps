using System;
using System.Data.SqlTypes;

namespace WMS_client.db
{
    /// <summary>Объект документа</summary>
    public abstract class DocumentObject : CatalogObject
    {
        /// <summary>Проведений</summary>
        [dbAttributes(Description = "Проведений", NotShowInForm = true)]
        public bool Posted { get; set; }
        /// <summary>Номер</summary>
        [dbAttributes(Description = "Номер", NotShowInForm = true)]
        public int Number { get; set; }
        /// <summary>Відповідальний</summary>
        [dbAttributes(Description = "Відповідальний", NotShowInForm = true)]
        public long Responsible { get; set; }
        /// <summary>Дата створення</summary>
        [dbAttributes(Description = "Дата створення", NotShowInForm = true)]
        public DateTime Date { get; set; }

        /// <summary>Объект документа</summary>
        protected DocumentObject()
        {
            Date = SqlDateTime.MinValue.Value;
        }
    }
}