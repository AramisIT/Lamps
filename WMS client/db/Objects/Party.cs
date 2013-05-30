using WMS_client.Enums;
using System;
using System.Data.SqlTypes;

namespace WMS_client.db
{
    /// <summary>Партия</summary>
    [dbElementAtt(DescriptionLength = 35)]
    public class Party : CatalogObject, IBarcodeOwner
    {
        #region Properties
        /// <summary>Штрихкод</summary>
        [dbFieldAtt(Description = "Штрихкод", NotShowInForm = true)]
        public string BarCode { get; set; }
        /// <summary>Вид комплектуючих</summary>
        [dbFieldAtt(Description = "Вид комплектуючих", NotShowInForm = true)]
        public TypeOfAccessories TypeOfAccessories { get; set; }
        /// <summary>Дата патії</summary>
        [dbFieldAtt(Description = "Дата патії", ShowInEditForm = true, NotShowInForm = true)]
        public DateTime DateParty { get; set; }
        /// <summary>Контрагент</summary>
        [dbFieldAtt(Description = "Контрагент", ShowInEditForm = true, dbObjectType = typeof(Contractors))]
        public long Contractor { get; set; }
        /// <summary>Дата актуальності</summary>
        [dbFieldAtt(Description = "Дата актуальності", NotShowInForm = true)]
        public DateTime DateOfActSet { get; set; }
        /// <summary>Гарантія (років)</summary>
        [dbFieldAtt(Description = "Гарантія (років)", NotShowInForm = true)]
        public int WarrantlyYears { get; set; }
        /// <summary>Гарантія (годин)</summary>
        [dbFieldAtt(Description = "Гарантія (годин)", NotShowInForm = true)]
        public int WarrantlyHours { get; set; }
        /// <summary>Статус синхронизации с сервером</summary>
        [dbFieldAtt(Description = "IsSynced", NotShowInForm = true)]
        public bool IsSynced { get; set; } 
        #endregion

        /// <summary>Партия</summary>
        public Party()
        {
            Description = string.Empty;
            DateParty = SqlDateTime.MinValue.Value;
            DateOfActSet = SqlDateTime.MinValue.Value;
        }

        #region Implemention of dbObject
        public override object Write()
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