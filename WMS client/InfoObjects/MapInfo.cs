using System.Drawing;

namespace WMS_client
{
    /// <summary>Інформація про карту</summary>
    public struct MapInfo
    {
        /// <summary>Карта обрана</summary>
        public bool IsSelected { get; private set; }
        /// <summary>Назва</summary>
        public string Description { get; private set; }
        /// <summary>Id карти</summary>
        public object Id { get; private set; }
        /// <summary>Іапазон регістрів</summary>
        public Point Range { get; private set; }

        /// <summary>Інформація про карту</summary>
        /// <param name="id">Id карти</param>
        /// <param name="description">Назва</param>
        /// <param name="start">Начало діапазону</param>
        /// <param name="finish">Завершення діапазону</param>
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