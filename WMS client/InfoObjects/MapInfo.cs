using System.Drawing;

namespace WMS_client
{
    /// <summary>���������� ��� �����</summary>
    public struct MapInfo
    {
        /// <summary>����� ������</summary>
        public bool IsSelected { get; private set; }
        /// <summary>�����</summary>
        public string Description { get; private set; }
        /// <summary>Id �����</summary>
        public object Id { get; private set; }
        /// <summary>������� �������</summary>
        public Point Range { get; private set; }

        /// <summary>���������� ��� �����</summary>
        /// <param name="id">Id �����</param>
        /// <param name="description">�����</param>
        /// <param name="start">������ ��������</param>
        /// <param name="finish">���������� ��������</param>
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