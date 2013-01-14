using System.Drawing;

namespace WMS_client
{
    /// <summary>Информация по карте</summary>
    public struct MapInfo
    {
        /// <summary>Карта выбрана</summary>
        public bool IsSelected { get; private set; }
        /// <summary>Название карты</summary>
        public string Description { get; private set; }
        /// <summary>Id карты</summary>
        public object Id { get; private set; }
        /// <summary>Диапазон регистров</summary>
        public Point Range { get; private set; }

        /// <summary>Информация по карте</summary>
        /// <param name="id">Id карты</param>
        /// <param name="description">Название карты</param>
        /// <param name="start">Начало диапазона</param>
        /// <param name="finish">Окончание диапазона</param>
        public MapInfo(object id, string description, int start, int finish)
            : this()
        {
            IsSelected = true;
            Id = id;
            Description = description;
            Range = new Point(start, finish);
        }
    }
}