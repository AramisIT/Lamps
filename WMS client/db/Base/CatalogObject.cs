using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Reflection;
using WMS_client.Base.Visual.Constructor;
using WMS_client.Enums;
using System.Data.SqlTypes;

namespace WMS_client.db
{
    /// <summary>Объект справочника</summary>
    public abstract class CatalogObject : dbObject
    {
        /// <summary>Имя колонки "Пометка на удаление"</summary>
        public const string MARK_FOR_DELETING = "markfordeleting";
        /// <summary>Имя колонки "Опис"</summary>
        public const string DESCRIPTION = "Description";

        /// <summary>Помічений на видалення</summary>
        [dbFieldAtt(Description = "Помічений на видалення", NotShowInForm = true)]
        public bool MarkForDeleting { get; set; }
        /// <summary>Опис</summary>
        [dbFieldAtt(Description = "Опис", NotShowInForm = true)]
        public string Description { get; set; }

        /// <summary>Объект справочника</summary>
        protected CatalogObject()
        {
            Description = string.Empty;
        }

        /// <summary>Получить описание по ИД</summary>
        /// <param name="type">Тип обьекта</param>
        /// <param name="id">ИД</param>
        /// <returns>Описание</returns>
        public static string ReadDescription(Type type, object id)
        {
            if (Convert.ToInt64(id) == 0)
            {
                return string.Empty;
            }

            string command = string.Format("SELECT Description FROM {0} WHERE {1}=@Id", type.Name, IDENTIFIER_NAME);
            SqlCeCommand query = dbWorker.NewQuery(command);
            query.AddParameter("Id", id);
            object descriptionObj = query.ExecuteScalar();

            return descriptionObj == null ? string.Empty : descriptionObj.ToString();
        }

        #region Presenter
        /// <summary>Получить визуальное представление (с информацией о элементах на которые возможны переходы)</summary>
        /// <param name="id">ID комплектующего</param>
        /// <param name="typeOfAccessories">Тип комплектующего</param>
        /// <param name="topic">Заголовок</param>
        /// <param name="listOfDetail">Словарь єлементов с детальной информацией</param>
        /// <returns>Список ...</returns>
        public static List<LabelForConstructor> GetVisualPresenter(long id, TypeOfAccessories typeOfAccessories, out string topic, out Dictionary<string, KeyValuePair<Type, object>> listOfDetail)
        {
            Accessory accessory = null;

            switch (typeOfAccessories)
            {
                case TypeOfAccessories.Lamp:
                    accessory = new Lamps();
                    accessory.Read<Lamps>(id);
                    break;
                case TypeOfAccessories.Case:
                    accessory = new Cases();
                    accessory.Read<Cases>(id);
                    break;
                case TypeOfAccessories.ElectronicUnit:
                    accessory = new ElectronicUnits();
                    accessory.Read<ElectronicUnits>(id);
                    break;
            }

            return GetVisualPresenter(typeOfAccessories, accessory, out topic, out listOfDetail);
        }

        /// <summary>Получить визуальное представление (с информацией о элементах на которые возможны переходы)</summary>
        /// <param name="barcode">Штрихкод</param>
        /// <param name="topic">Заголовок</param>
        /// <param name="listOfDetail">Словарь єлементов с детальной информацией</param>
        /// <returns>Список ...</returns>
        public static List<LabelForConstructor> GetVisualPresenter(string barcode, out string topic, out Dictionary<string, KeyValuePair<Type, object>> listOfDetail)
        {
            TypeOfAccessories typeOfAccessories = BarcodeWorker.GetTypeOfAccessoriesByBarcode(barcode);
            Accessory accessory = null;

            switch (typeOfAccessories)
            {
                case TypeOfAccessories.Lamp:
                    accessory = new Lamps();
                    accessory.Read<Lamps>(barcode);
                    break;
                case TypeOfAccessories.Case:
                    accessory = new Cases();
                    accessory.Read<Cases>(barcode);
                    break;
                case TypeOfAccessories.ElectronicUnit:
                    accessory = new ElectronicUnits();
                    accessory.Read<ElectronicUnits>(barcode);
                    break;
            }

            return GetVisualPresenter(typeOfAccessories, accessory, out topic, out listOfDetail);
        }

        /// <summary>Получить визуальное представление (с информацией о элементах на которые возможны переходы)</summary>
        /// <param name="typeOfAccessories">Тип комплектующего</param>
        /// <param name="accessory">Объект</param>
        /// <param name="topic">Заголовок</param>
        /// <param name="listOfDetail">Словарь єлементов с детальной информацией</param>
        /// <returns>Список ...</returns>
        public static List<LabelForConstructor> GetVisualPresenter(TypeOfAccessories typeOfAccessories, Accessory accessory, out string topic, out Dictionary<string, KeyValuePair<Type, object>> listOfDetail)
        {
            topic = Cases.GetDescriptionOfAccessory(typeOfAccessories);
            listOfDetail = new Dictionary<string, KeyValuePair<Type, object>>();
            List<LabelForConstructor> list = new List<LabelForConstructor>();

            if (accessory != null)
            {
                Type type = accessory.GetType();
                PropertyInfo[] fields = type.GetProperties();

                foreach (PropertyInfo field in fields)
                {
                    Attribute[] attributes = Attribute.GetCustomAttributes(field);

                    foreach (Attribute a in attributes)
                    {
                        dbFieldAtt attribute = a as dbFieldAtt;

                        if (attribute != null)
                        {
                            if (attribute.NeedDetailInfo)
                            {
                                object value = field.GetValue(accessory, null);
                                listOfDetail.Add(attribute.Description, new KeyValuePair<Type, object>(attribute.dbObjectType, value));
                            }
                            else if (!attribute.NotShowInForm || attribute.ShowEmbadedInfo)
                            {
                                object value = field.GetValue(accessory, null);

                                if (attribute.dbObjectType == null)
                                {
                                    if (field.PropertyType == typeof(DateTime))
                                    {
                                        DateTime dateValue = (DateTime) value;

                                        value = dateValue != SqlDateTime.MinValue.Value
                                                    ? String.Format("{0:dd.MM.yyyy}", dateValue)
                                                    : string.Empty;
                                    }
                                    else if (field.PropertyType.IsEnum)
                                    {
                                        value = EnumWorker.GetDescription(field.PropertyType, Convert.ToInt32(value));
                                    }
                                    else if (field.PropertyType == typeof(bool))
                                    {
                                        value = (bool)value ? "+" : "-";
                                    }
                                }
                                else
                                {
                                    if (attribute.ShowEmbadedInfo)
                                    {
                                        dbObject detailObject = (dbObject)Activator.CreateInstance(attribute.dbObjectType);
                                        detailObject = (dbObject)detailObject.Read(attribute.dbObjectType, value, IDENTIFIER_NAME);
                                        Dictionary<string, KeyValuePair<Type, object>> subListOfDetail;
                                        List<LabelForConstructor> subList = GetSingleVisualPresenter(
                                            attribute.dbObjectType, out subListOfDetail, detailObject, false);

                                        list.AddRange(subList);
                                    }

                                    if(!attribute.NotShowInForm)
                                    {
                                        value = ReadDescription(attribute.dbObjectType, value);
                                    }
                                }

                                string data = String.Format("{0}: {1}", attribute.Description, value);

                                list.Add(new LabelForConstructor(data, ControlsStyle.LabelSmall, false));
                                break;
                            }
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>Получить визуальное представление только по элементу(без вложенной инф.)</summary>
        /// <param name="type">Тип объекта визуализирования</param>
        /// <param name="listOfDetail">Словарь єлементов с детальной информацией</param>
        /// <param name="accessory">Объект визуализирования</param>
        /// <returns>Список ...</returns>
        public static List<LabelForConstructor> GetSingleVisualPresenter(Type type, out Dictionary<string, KeyValuePair<Type, object>> listOfDetail, dbObject accessory)
        {
            return GetSingleVisualPresenter(type, out listOfDetail, accessory, false);
        }

        /// <summary>Получить визуальное представление только по элементу(без вложенной инф.)</summary>
        /// <param name="type">Тип объекта визуализирования</param>
        /// <param name="listOfDetail">Словарь єлементов с детальной информацией</param>
        /// <param name="accessory">Объект визуализирования</param>
        /// <param name="isDetailMode">Режим детального списка?</param>
        /// <returns>Список ...</returns>
        public static List<LabelForConstructor> GetSingleVisualPresenter(Type type, out Dictionary<string, KeyValuePair<Type, object>> listOfDetail, dbObject accessory, bool isDetailMode)
        {
            listOfDetail = new Dictionary<string, KeyValuePair<Type, object>>();
            List<LabelForConstructor> list = new List<LabelForConstructor>();

            PropertyInfo[] fields = type.GetProperties();

            foreach (PropertyInfo field in fields)
            {
                Attribute[] attributes = Attribute.GetCustomAttributes(field);

                foreach (Attribute a in attributes)
                {
                    dbFieldAtt attribute = a as dbFieldAtt;

                    if (attribute != null)
                    {
                        if (attribute.NeedDetailInfo)
                        {
                            object value = field.GetValue(accessory, null);
                            listOfDetail.Add(attribute.Description,
                                             new KeyValuePair<Type, object>(attribute.dbObjectType, value));
                        }
                        else if ((isDetailMode && attribute.ShowInEditForm) ||
                            (!isDetailMode && !attribute.NotShowInForm))
                        {
                            object value = field.GetValue(accessory, null);

                            if (attribute.dbObjectType == null)
                            {
                                if (field.PropertyType == typeof(DateTime))
                                {
                                    DateTime dateTimeValue = (DateTime)value;
                                    value = dateTimeValue == SqlDateTime.MinValue.Value
                                                ? string.Empty
                                                : String.Format("{0:dd.MM.yyyy}", dateTimeValue);
                                }
                                else if (field.PropertyType.IsEnum)
                                {
                                    value = EnumWorker.GetDescription(field.PropertyType, Convert.ToInt32(value));
                                }
                                else if (field.PropertyType == typeof(bool))
                                {
                                    value = (bool)value ? "+" : "-";
                                }
                                else if (value.Equals(0) || value.Equals(0L) || value.Equals(0D))
                                {
                                    value = string.Empty;
                                }
                            }
                            else
                            {
                                value = ReadDescription(attribute.dbObjectType, value);
                            }

                            string data = String.Format("{0}: {1}", attribute.Description, value);

                            list.Add(new LabelForConstructor(data, ControlsStyle.LabelSmall, false));
                            break;
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>Получить детальное(инф. по вложенным элементам, уровень вложености = 1) представление по элементу</summary>
        /// <param name="type">Тип объекта визуализирования</param>
        /// <param name="listOfDetail">Словарь єлементов с детальной информацией</param>
        /// <param name="accessory">Объект визуализирования</param>
        /// <returns>Список ...</returns>
        public static List<LabelForConstructor> GetDetailVisualPresenter(Type type, out Dictionary<string, KeyValuePair<Type, object>> listOfDetail, dbObject accessory)
        {
            listOfDetail = new Dictionary<string, KeyValuePair<Type, object>>();
            List<LabelForConstructor> list = new List<LabelForConstructor>();

            PropertyInfo[] fields = type.GetProperties();

            foreach (PropertyInfo field in fields)
            {
                Attribute[] attributes = Attribute.GetCustomAttributes(field);

                foreach (Attribute a in attributes)
                {
                    dbFieldAtt attribute = a as dbFieldAtt;

                    if (attribute != null)
                    {
                        if (attribute.NeedDetailInfo)
                        {
                            object value = field.GetValue(accessory, null);
                            listOfDetail.Add(attribute.Description,
                                             new KeyValuePair<Type, object>(attribute.dbObjectType, value));
                        }
                        else if (attribute.ShowInEditForm)
                        {
                            object value = field.GetValue(accessory, null);

                            if (attribute.dbObjectType == null)
                            {
                                if (field.PropertyType == typeof(DateTime))
                                {
                                    DateTime dateTimeValue = (DateTime)value;
                                    value = dateTimeValue == SqlDateTime.MinValue.Value
                                                ? string.Empty
                                                : string.Format("{0:dd.MM.yyyy}", dateTimeValue);
                                }
                                else if (field.PropertyType.IsEnum)
                                {
                                    value = EnumWorker.GetDescription(field.PropertyType, Convert.ToInt32(value));
                                }
                                else if (field.PropertyType == typeof(bool))
                                {
                                    value = (bool)value ? "+" : "-";
                                }
                                else if (value.Equals(0) || value.Equals(0L) || value.Equals(0D))
                                {
                                    value = string.Empty;
                                }
                            }
                            else
                            {
                                dbObject detailObject = (dbObject)Activator.CreateInstance(attribute.dbObjectType);
                                detailObject = (dbObject)detailObject.Read(attribute.dbObjectType, value, IDENTIFIER_NAME);
                                Dictionary<string, KeyValuePair<Type, object>> subListOfDetail;
                                List<LabelForConstructor> subList = GetSingleVisualPresenter(
                                    attribute.dbObjectType, out subListOfDetail, detailObject, true);
                                list.AddRange(subList);
                                value = ReadDescription(attribute.dbObjectType, value);
                            }

                            string data = String.Format("{0}: {1}", attribute.Description, value);

                            list.Add(new LabelForConstructor(field.Name, true, data, ControlsStyle.LabelSmall, false));
                            break;
                        }
                    }
                }
            }

            return list;
        } 

        public static string GetDescription(string tableName, object id)
        {
            if(id==null || Convert.ToInt64(id)==0)
            {
                return string.Empty;
            }

            string query = string.Format("SELECT {0} FROM {1} WHERE {2}=@{2}", DESCRIPTION, tableName, IDENTIFIER_NAME);
            SqlCeCommand command = dbWorker.NewQuery(query);
            command.AddParameter(IDENTIFIER_NAME, id);
            object desObj = command.ExecuteScalar();

            return desObj == null ? string.Empty : desObj.ToString();
        }

        public static string GetSyncRef(string tableName, object id)
        {
            if (id == null || Convert.ToInt64(id) == 0)
            {
                return string.Empty;
            }

            string query = string.Format("SELECT {0} FROM {1} WHERE {2}=@{2}", SYNCREF_NAME, tableName, IDENTIFIER_NAME);
            SqlCeCommand command = dbWorker.NewQuery(query);
            command.AddParameter(IDENTIFIER_NAME, id);
            object desObj = command.ExecuteScalar();

            return desObj == null ? string.Empty : desObj.ToString();
        }
        #endregion
    }
}