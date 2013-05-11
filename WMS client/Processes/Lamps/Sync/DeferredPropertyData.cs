using System;

namespace WMS_client
    {
    /// <summary>Данные о отложенном свойстве</summary>
    public class DataAboutDeferredProperty
        {
        /// <summary>ID комплектующего</summary>
        public long Id { get; set; }
        /// <summary>Тип данных комплектующего</summary>
        public Type AccessoryType { get; set; }
        /// <summary>Тип данных отложенного свойства</summary>
        public Type PropertyType { get; set; }
        /// <summary>Имя свойства</summary>
        public string PropertyName { get; set; }
        /// <summary>Значение свойства</summary>
        public object Value { get; set; }

        /// <summary>Данные о отложенном свойстве</summary>
        /// <param name="accessoryType">Тип данных комплектующего</param>
        /// <param name="propertyType">Тип данных отложенного свойства</param>
        /// <param name="property">Имя свойства</param>
        /// <param name="value">Значение свойства</param>
        public DataAboutDeferredProperty(Type accessoryType, Type propertyType, string property, object value)
            {
            AccessoryType = accessoryType;
            PropertyType = propertyType;
            PropertyName = property;
            Value = value;
            }
        }
    }