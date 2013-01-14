using WMS_client.Enums;
using System;
using System.Data.SqlTypes;

namespace WMS_client.db
{
    /// <summary>Партия</summary>
    public class Party : CatalogObject, IBarcodeOwner
    {
        #region Properties
        [dbAttributes(Description = "Штрихкод", NotShowInForm = true)]
        public string BarCode { get; set; }
        /// <summary>Вид комплектуючих</summary>
        [dbAttributes(Description = "Вид комплектуючих", NotShowInForm = true)]
        public TypeOfAccessories TypeOfAccessories { get; set; }
        /// <summary>Дата патії</summary>
        [dbAttributes(Description = "Дата патії", ShowInEditForm = true, NotShowInForm = true)]
        public DateTime DateParty { get; set; }
        /// <summary>Контрагент</summary>
        [dbAttributes(Description = "Контрагент", ShowInEditForm = true, dbObjectType = typeof(Contractors))]
        public long Contractor { get; set; }
        /// <summary>Дата актуальності</summary>
        [dbAttributes(Description = "Дата актуальності", NotShowInForm = true)]
        public DateTime DateOfActSet { get; set; }
        /// <summary>Гарантія (років)</summary>
        [dbAttributes(Description = "Гарантія (років)", NotShowInForm = true)]
        public int WarrantlyYears { get; set; }
        /// <summary>Гарантія (годин)</summary>
        [dbAttributes(Description = "Гарантія (годин)", NotShowInForm = true)]
        public int WarrantlyHours { get; set; }
        [dbAttributes(Description = "IsSynced", NotShowInForm = true)]
        public bool IsSynced { get; set; } 
        #endregion

        /// <summary>Партия</summary>
        public Party()
        {
            DateParty = SqlDateTime.MinValue.Value;
            DateOfActSet = SqlDateTime.MinValue.Value;
        }

        /// <summary>Очистить все документы</summary>
        public static void ClearOldDocuments()
        {
            dbArchitector.ClearAllDataFromTable("Party");
        }

        #region Implemention of dbObject
        public override object Save()
        {
            return base.Save<Party>();
        }

        public override object Sync()
        {
            return base.Sync<Party>();
        } 
        #endregion
    }
}