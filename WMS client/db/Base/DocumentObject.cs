using System;
using System.Data.SqlTypes;

namespace WMS_client.db
{
    /// <summary>������ ���������</summary>
    public abstract class DocumentObject : CatalogObject
    {
        /// <summary>����������</summary>
        [dbAttributes(Description = "����������", NotShowInForm = true)]
        public bool Posted { get; set; }
        /// <summary>�����</summary>
        [dbAttributes(Description = "�����", NotShowInForm = true)]
        public int Number { get; set; }
        /// <summary>³�����������</summary>
        [dbAttributes(Description = "³�����������", NotShowInForm = true)]
        public long Responsible { get; set; }
        /// <summary>���� ���������</summary>
        [dbAttributes(Description = "���� ���������", NotShowInForm = true)]
        public DateTime Date { get; set; }

        /// <summary>������ ���������</summary>
        protected DocumentObject()
        {
            Date = SqlDateTime.MinValue.Value;
        }
    }
}