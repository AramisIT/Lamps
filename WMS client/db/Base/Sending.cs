using System;
using WMS_client.Enums;

namespace WMS_client.db
{
    public abstract class Sending : CatalogObject, IBarcodeOwner
    {
        /// <summary>����</summary>
        [dbAttributes(Description = "����", NotShowInForm = true)]
        public DateTime Date { get; set; }
        /// <summary>��������</summary>
        [dbAttributes(Description = "��������", NotShowInForm = true)]
        public string BarCode { get; set; }
        [dbAttributes(Description = "IsSynced", NotShowInForm = true)]
        public bool IsSynced { get; set; }
        /// <summary>����������</summary>
        [dbAttributes(Description = "����������", dbObjectType = typeof(Contractors))]
        public int Contractor { get; set; }
        /// <summary>��� ��������������</summary>
        [dbAttributes(Description = "��� ��������������")]
        public TypeOfAccessories TypeOfAccessory { get; set; }

        protected Sending()
        {
            BarCode = string.Empty;
        }
    }
}