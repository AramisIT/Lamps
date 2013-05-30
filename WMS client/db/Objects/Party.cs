using WMS_client.Enums;
using System;
using System.Data.SqlTypes;

namespace WMS_client.db
{
    /// <summary>������</summary>
    [dbElementAtt(DescriptionLength = 35)]
    public class Party : CatalogObject, IBarcodeOwner
    {
        #region Properties
        /// <summary>��������</summary>
        [dbFieldAtt(Description = "��������", NotShowInForm = true)]
        public string BarCode { get; set; }
        /// <summary>��� �������������</summary>
        [dbFieldAtt(Description = "��� �������������", NotShowInForm = true)]
        public TypeOfAccessories TypeOfAccessories { get; set; }
        /// <summary>���� ���</summary>
        [dbFieldAtt(Description = "���� ���", ShowInEditForm = true, NotShowInForm = true)]
        public DateTime DateParty { get; set; }
        /// <summary>����������</summary>
        [dbFieldAtt(Description = "����������", ShowInEditForm = true, dbObjectType = typeof(Contractors))]
        public long Contractor { get; set; }
        /// <summary>���� �����������</summary>
        [dbFieldAtt(Description = "���� �����������", NotShowInForm = true)]
        public DateTime DateOfActSet { get; set; }
        /// <summary>������� (����)</summary>
        [dbFieldAtt(Description = "������� (����)", NotShowInForm = true)]
        public int WarrantlyYears { get; set; }
        /// <summary>������� (�����)</summary>
        [dbFieldAtt(Description = "������� (�����)", NotShowInForm = true)]
        public int WarrantlyHours { get; set; }
        /// <summary>������ ������������� � ��������</summary>
        [dbFieldAtt(Description = "IsSynced", NotShowInForm = true)]
        public bool IsSynced { get; set; } 
        #endregion

        /// <summary>������</summary>
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