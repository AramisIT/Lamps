using System;

namespace WMS_client.db
{
    /// <summary>Аттребути</summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class dbAttributes : Attribute
    {
        /// <summary>Представлення поля</summary>
        public string Description { get; set; }
        /// <summary>Не відображати на формі</summary>
        public bool NotShowInForm { get; set; }
        /// <summary>Відображати на формі редагування</summary>
        public bool ShowInEditForm { get; set; }
        /// <summary></summary>
        public Type dbObjectType { get; set; }
        /// <summary>Відобразити детальну інформацію</summary>
        public bool NeedDetailInfo { get; set; }
        /// <summary>Відобразити вкладену інформацію</summary>
        public bool ShowEmbadedInfo { get; set; }
    }
}