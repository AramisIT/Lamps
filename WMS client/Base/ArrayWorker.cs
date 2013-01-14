using System.Collections.Generic;

namespace WMS_client.Base
{
    /// <summary>Работа с масивами</summary>
    public static class ArrayWorker
    {
        /// <summary>Удалить первый элемент</summary>
        /// <param name="array">Масив элементов</param>
        /// <returns>Обработанный масив</returns>
        public static object[] RemoveFirstElement(object[] array)
        {
            List<object> list = new List<object>(array);
            list.RemoveAt(0);

            return list.ToArray();
        }
    }
}
