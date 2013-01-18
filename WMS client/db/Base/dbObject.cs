using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlServerCe;

namespace WMS_client.db
{
    /// <summary>Объкт базыданных</summary>
    public abstract class dbObject
    {
        /// <summary>Колонка идентификатора синхронизации</summary>
        public const string SYNCREF_NAME = "SyncRef";
        /// <summary>Колонка идентификатора</summary>
        public const string IDENTIFIER_NAME = "Id";
        /// <summary>Колонка штрихкода</summary>
        public const string BARCODE_NAME = "BarCode";
        /// <summary>Колонка флага состояния синхронизации</summary>
        public const string IS_SYNCED = "issynced";
        /// <summary>Длинна идентификатора синхронизации</summary>
        private const int REF_LENGTH = 25;

        /// <summary>Сохранить</summary>
        /// <returns>Id</returns>
        public abstract object Save();
        /// <summary>Сохранить</summary>
        /// <returns>Id</returns>
        public abstract object Sync();

        /// <summary>Идентификатор для синхронизации</summary>
        [dbFieldAtt(Description = "Идентификатор для синхронизации", NotShowInForm = true)]
        public string SyncRef { get; set; }
        /// <summary>Id</summary>
        [dbFieldAtt(Description = "Id", NotShowInForm = true)]
        public long Id { get; set; }
        /// <summary>Обьект изменен</summary>
        public bool IsModified { get; protected set; }
        /// <summary>ДатаВремя последнего изменения</summary>
        [dbFieldAtt(Description = "LastModified", NotShowInForm = true)]
        public DateTime LastModified { get; set; }

        /// <summary>Объкт базыданных</summary>
        protected dbObject()
        {
            IsNew = true;
        }

        #region IsNew
        /// <summary>Элемент новый</summary>
        public bool IsNew { get; private set; }

        /// <summary>Пометить элемент как новый</summary>
        public void SetIsNew()
        {
            Id = 0;
            IsNew = true;
        }

        /// <summary>Пометить элемент как не новый</summary>
        public void SetNotNew()
        {
            IsNew = false;
        } 
        #endregion

        #region Сохранение в БД
        /// <summary>Сохранить объект</summary>
        /// <returns>Id</returns>
        public virtual object Save<T>() where T : dbObject
        {
            return SaveChanges<T>(false);
        }

        /// <summary>Синхронизировать объект</summary>
        /// <returns>Id</returns>
        public virtual object Sync<T>() where T : dbObject
        {
            return SaveChanges<T>(true);
        }

        /// <summary>Сохранить изменения в объекте</summary>
        /// <param name="sync">Синхронизация?</param>
        /// <returns>Id</returns>
        protected object SaveChanges<T>(bool sync) where T : dbObject
        {
            object idValue;
            LastModified = DateTime.Now;
            ISynced syncObj = this as ISynced;

            if (syncObj != null)
            {
                syncObj.IsSynced = sync;
            }

            if (IsNew)
            {
                if (string.IsNullOrEmpty(SyncRef))
                {
                    SyncRef = generateSyncRef();
                }

                idValue = CreateNew<T>();
            }
            else
            {
                idValue = Update<T>();
            }

            return idValue;
        }

        private string generateSyncRef()
        {
            StringBuilder refStr = new StringBuilder();
            Random rand = new Random();

            for (int i = 0; i < REF_LENGTH; i++)
            {
                refStr.Append((char) rand.Next(60, 122));
            }

            return refStr.ToString();
        }

        /// <summary>Создание нового объекта</summary>
        /// <returns>Id</returns>
        private object CreateNew<T>() where T : dbObject
        {
            Type type = typeof (T);
            PropertyInfo[] properties = type.GetProperties();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            StringBuilder columnsStr = new StringBuilder();
            StringBuilder parameterStr = new StringBuilder();
            object newId = 0;
            string idName = IDENTIFIER_NAME.ToLower();

            foreach (PropertyInfo property in properties)
            {
                Attribute attribute = Attribute.GetCustomAttribute(property, typeof (dbFieldAtt));

                if (attribute != null)
                {
                    object value = property.GetValue(this, null);

                    if (property.Name.ToLower().Equals(idName))
                    {
                        if (Convert.ToInt64(value) == 0)
                        {
                            newId = getNewId();
                            value = newId;
                        }
                        else
                        {
                            newId = value;
                        }
                    }

                    parameters.Add(property.Name, value);

                    columnsStr.Append("[");
                    columnsStr.Append(property.Name);
                    columnsStr.Append("],");

                    parameterStr.Append("@");
                    parameterStr.Append(property.Name);
                    parameterStr.Append(",");
                }
            }

            string command = string.Format("INSERT INTO {0}({1}) VALUES({2})",
                                           type.Name,
                                           columnsStr.ToString(0, columnsStr.Length - 1),
                                           parameterStr.ToString(0, parameterStr.Length - 1));

            SqlCeCommand query = dbWorker.NewQuery(command);
            query.AddParameters(parameters);
            query.ExecuteNonQuery();
            IsNew = false;

            Id = Convert.ToInt64(newId);
            return newId;
        }

        /// <summary>Обновление объекта</summary>
        /// <returns>Id</returns>
        private object Update<T>() where T : dbObject
        {
            object idValue = 0; 
            Type type = typeof (T);
            PropertyInfo[] properties = type.GetProperties();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            StringBuilder line = new StringBuilder();
            string idName = IDENTIFIER_NAME.ToLower();

            foreach (PropertyInfo property in properties)
            {
                Attribute attribute = Attribute.GetCustomAttribute(property, typeof (dbFieldAtt));

                if (attribute != null)
                {
                    object value = property.GetValue(this, null);
                    parameters.Add(property.Name, value);

                    if (property.Name.ToLower().Equals(idName))
                    {
                        idValue = value;
                    }
                    else
                    {
                        line.AppendFormat("[{0}]=@{0},", property.Name);
                    }
                }
            }

            string command = string.Format("UPDATE {0} SET {1} WHERE [{2}]=@Id",
                                           type.Name,
                                           line.ToString(0, line.Length - 1),
                                           IDENTIFIER_NAME);
            SqlCeCommand query = dbWorker.NewQuery(command);
            query.AddParameters(parameters);
            query.ExecuteNonQuery();

            return idValue;
        }

        /// <summary>Получить новый Id для объекта</summary>
        /// <returns>Новый Id</returns>
        private object getNewId()
        {
            Type type = GetType();
            string command = string.Format("SELECT [{0}]+1 Id FROM {1} ORDER BY [{0}] DESC", IDENTIFIER_NAME, type.Name);
            SqlCeCommand query = dbWorker.NewQuery(command);
            object newId = query.ExecuteScalar();

            return newId ?? 1;
        }
        #endregion

        #region Чтение из БД
        /// <summary>Прочитать данные объекта с БД</summary>
        /// <returns>Объект</returns>
        public virtual T Read<T>() where T : dbObject
        {
            return Read<T>(Id, IDENTIFIER_NAME);
        }

        /// <summary>Прочитать данные объекта с БД</summary>
        /// <param name="id">Значение Id</param>
        /// <returns>Объект</returns>
        public virtual T Read<T>(object id) where T : dbObject
        {
            return Read<T>(id, IDENTIFIER_NAME);
        }

        /// <summary>Прочитать данные объекта с БД</summary>
        /// <param name="id">Значение Id</param>
        /// <returns>Объект</returns>
        public virtual T Read<T>(long id) where T : dbObject
        {
            return Read<T>(id, IDENTIFIER_NAME);
        }

        /// <summary>Прочитать данные объекта с БД</summary>
        /// <param name="id">Значение Id</param>
        /// <returns>Объект</returns>
        public void Read(long id)
        {
            Read(GetType(), id, IDENTIFIER_NAME);
        }

        /// <summary>Прочитать данные объекта с БД</summary>
        /// <param name="id">Значение идентификатора</param>
        /// <param name="idColumn">Название колонки идентификатора</param>
        /// <returns>Объект</returns>
        public virtual T Read<T>(object id, string idColumn) where T : dbObject
        {
            return (T) Read(typeof (T), id, idColumn);
        }

        /// <summary>Прочитать данные объекта с БД</summary>
        /// <param name="type">Тип объекта</param>
        /// <param name="id">Значение идентификатора</param>
        /// <param name="idColumn">Название колонки идентификатора</param>
        /// <returns>Объект</returns>
        public virtual object Read(Type type, object id, string idColumn)
        {
            PropertyInfo[] properties = type.GetProperties();
            StringBuilder line = new StringBuilder();

            foreach (PropertyInfo field in properties)
            {
                dbFieldAtt attributes = Attribute.GetCustomAttribute(field, typeof (dbFieldAtt)) as dbFieldAtt;

                if (attributes != null)
                {
                    line.AppendFormat("[{0}],", field.Name);
                }
            }

            string command = string.Format("SELECT {0} FROM {1} WHERE {2}=@Id", line.ToString(0, line.Length - 1),
                                           type.Name, idColumn);
            SqlCeCommand query = dbWorker.NewQuery(command);
            query.AddParameter("Id", id);
            SqlCeDataReader reader = query.ExecuteReader();

            while (reader.Read())
            {
                foreach (PropertyInfo property in properties)
                {
                    dbFieldAtt attributes = Attribute.GetCustomAttribute(property, typeof (dbFieldAtt)) as dbFieldAtt;

                    if (attributes != null)
                    {
                        object value = reader[property.Name];

                        if (property.PropertyType == typeof (int))
                        {
                            value = Convert.ToInt32(value);
                        }
                        if (property.PropertyType == typeof(long))
                        {
                            value = Convert.ToInt64(value);
                        }
                        else if (property.PropertyType == typeof (double))
                        {
                            value = Convert.ToDouble(value);
                        }

                        property.SetValue(this, value, null);
                    }
                }
            }
            
            //Если Id=0, значит такой обьект не найден -> он новый
            IsNew = Id == 0;

            return this;
        }
        #endregion

        #region Copy
        /// <summary>Копировать объект</summary>
        /// <returns>Скопированный объект</returns>
        public dbObject Copy()
        {
            return Copy(this);
        }

        /// <summary>Копировать объект</summary>
        /// <param name="source">Объект-исходник</param>
        /// <returns>Скопированный объект</returns>
        public static dbObject Copy(dbObject source)
        {
            if (source != null)
            {
                Type type = source.GetType();
                dbObject copy = (dbObject) Activator.CreateInstance(type);
                PropertyInfo[] propertyInfos = type.GetProperties();

                foreach (PropertyInfo property in propertyInfos)
                {
                    dbFieldAtt attributes = Attribute.GetCustomAttribute(property, typeof (dbFieldAtt)) as dbFieldAtt;

                    if (attributes != null)
                    {
                        object value = property.GetValue(source, null);
                        property.SetValue(copy, value, null);
                    }
                }

                ISynced synced = copy as ISynced;
                if (synced != null)
                {
                    synced.IsSynced = false; 
                }

                IBarcodeOwner barcode = copy as IBarcodeOwner;
                if (barcode != null)
                {
                    barcode.BarCode = string.Empty;
                }

                copy.Id = 0;

                return copy;
            }

            return null;
        }
        #endregion

        #region SetValue
        /// <summary>Установить значение</summary>
        /// <param name="propertyName">Имя свойства</param>
        /// <param name="value">Значение</param>
        /// <returns>Значение</returns>
        public object SetValue(string propertyName, object value)
        {
            return SetValue(this, propertyName, value);
        }

        /// <summary>Установить значение</summary>
        /// <param name="propertyName">Имя свойства</param>
        /// <param name="value">Значение</param>
        /// <param name="isValid">Правильный ли формат значения</param>
        /// <returns>Значение</returns>
        public object SetValue(string propertyName, object value, out bool isValid)
        {
            return SetValue(this, propertyName, value,out isValid);
        }

        /// <summary>Установить значение</summary>
        /// <param name="obj">Объект для которого устанавливаеться значение</param>
        /// <param name="propertyName">Имя свойства</param>
        /// <param name="value">Значение</param>
        /// <returns>Значение</returns>
        public static object SetValue(dbObject obj, string propertyName, object value)
        {
            bool isValid;
            return SetValue(obj, propertyName, value, out isValid);
        }

        /// <summary>Установить значение</summary>
        /// <param name="obj">Объект для которого устанавливаеться значение</param>
        /// <param name="propertyName">Имя свойства</param>
        /// <param name="value">Значение</param>
        /// <param name="isValid">Правильный ли формат значения</param>
        /// <returns>Значение</returns>
        public static object SetValue(dbObject obj, string propertyName, object value, out bool isValid)
        {
            isValid = true;
            Type type = obj.GetType();
            PropertyInfo property = type.GetProperty(propertyName);

            if (property != null)
            {
                if (property.PropertyType == typeof (long))
                {
                    value = Convert.ToInt64(value);
                }
                else if (property.PropertyType == typeof (int))
                {
                    value = Convert.ToInt32(value);
                }
                else if (property.PropertyType == typeof (double))
                {
                    value = Convert.ToDouble(value);
                }
                else if (property.PropertyType == typeof (DateTime))
                {
                    try
                    {
                        value = Convert.ToDateTime(value);
                    }
                    catch
                    {
                        value = DateTime.MaxValue;
                        isValid = false;
                    }
                }
                else if (property.PropertyType.IsEnum)
                {
                    value = Enum.Parse(property.PropertyType, value.ToString(), false);
                }
                else if (property.PropertyType == typeof (string))
                {
                    value = value.ToString().TrimEnd();
                }

                property.SetValue(obj, value, null);
            }

            obj.IsModified = true;
            return value;
        } 
        #endregion

        #region GetPropery
        /// <summary>Получить значение свойства по строковому имени</summary>
        /// <param name="propertyName">Строковое имя</param>
        /// <returns>Значение свойства</returns>
        public object GetPropery(string propertyName)
        {
            PropertyInfo[] properties = GetType().GetProperties();

            return (from property in properties 
                    where property.Name == propertyName 
                    select property.GetValue(this, null)).FirstOrDefault();
        }
        #endregion

        #region GetProperyType
        /// <summary>Получить тип свойства</summary>
        /// <param name="propertyName">Имя свойства</param>
        /// <returns>Тип свойства</returns>
        public Type GetProperyType(string propertyName)
        {
            return GetProperyType(this, propertyName);
        }

        /// <summary>Получить тип свойства</summary>
        /// <param name="obj">Объект у которого нужно узнать тип</param>
        /// <param name="propertyName">Имя свойства</param>
        /// <returns>Тип свойства</returns>
        public static Type GetProperyType(dbObject obj, string propertyName)
        {
            Type type = obj.GetType();
            PropertyInfo propertyInfo = type.GetProperty(propertyName);

            return propertyInfo.PropertyType;
        } 
        #endregion
    }
}