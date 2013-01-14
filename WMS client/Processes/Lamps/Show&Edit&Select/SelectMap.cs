using System.Data;
using System;
using System.Data.SqlServerCe;
using WMS_client.db;

namespace WMS_client
{
    /// <summary>Выбор карты</summary>
    public class SelectMap : BusinessProcess
    {
        /// <summary>Информация по карте</summary>
        public MapInfo MapInfo { get; set; }
        /// <summary>Таблица для MobileTable...</summary>
        private readonly DataTable sourceTable;
        /// <summary>Отображаемый контрол с данными о картах</summary>
        private MobileTable visualTable;
        /// <summary>Штрихкод светильника</summary>
        private readonly string LampBarCode;
        private readonly long CurrentMapId;

        /// <summary>Выбор карты</summary>
        /// <param name="MainProcess">Основной процесс</param>
        /// <param name="currentMapId"> </param>
        /// <param name="lampBarCode">Штрихкод светильника</param>
        public SelectMap(WMSClient MainProcess, long currentMapId, string lampBarCode)
            : base(MainProcess, 1)
        {
            FormNumber = 1;
            BusinessProcessType = ProcessType.Registration;
            MainProcess.ToDoCommand = "Оберіть карту";
            LampBarCode = lampBarCode;
            CurrentMapId = currentMapId;

            sourceTable = new DataTable();
            sourceTable.Columns.AddRange(new[]
                                               {
                                                   new DataColumn("Description", typeof (string)),
                                                   new DataColumn("Id", typeof (short))
                                               });

            IsLoad = true;
            DrawControls();
        }

        #region Override methods
        public override sealed void DrawControls()
        {
            if (IsLoad)
            {
                visualTable = MainProcess.CreateTable("Maps", 259, onRowSelected);
                visualTable.DT = sourceTable;
                visualTable.AddColumn("Карта", "Description", 214);
                SqlCeDataReader reader = GetMapsList();

                while(reader.Read())
                {
                    visualTable.AddRow(reader["Description"], reader["Id"]);
                }

                visualTable.Focus();
            }
        }

        /// <summary>Выбор строки в таблице</summary>
        private void onRowSelected(object sender, OnRowSelectedEventArgs e)
        {
            long mapId = Convert.ToInt64(e.SelectedRow["Id"]);

            //Если таблица - значит есть вложенные карты. Опять отобразить
            if (checkIncludeMapOrInfo(mapId))
            {
                MainProcess.ClearControls();
                MainProcess.Process = new SelectMap(MainProcess, mapId, LampBarCode);
            }
            //Иначе - результат (выбранная карта)
            else
            {
                object[] array = getMapInfo(mapId);
                int start = Convert.ToInt32(array[2]);
                int finish = Convert.ToInt32(array[3]);
                MapInfo = new MapInfo(array[0], array[1].ToString(), start, finish);

                MainProcess.ClearControls();
                MainProcess.Process = new InstallingNewLighter(MainProcess, LampBarCode) { MapInfo = MapInfo };
            }
        }

        public override void OnBarcode(string Barcode)
        {
        }

        public override void OnHotKey(KeyAction TypeOfAction)
        {
            switch (TypeOfAction)
            {
                case KeyAction.Esc:
                    MainProcess.ClearControls();
                    MainProcess.Process = new InstallingNewLighter(MainProcess, LampBarCode) { MapInfo = MapInfo };
                    break;
            }
        }
        #endregion

        #region Query
        private SqlCeDataReader GetMapsList()
        {
            SqlCeCommand query = dbWorker.NewQuery("SELECT Id,Description FROM Maps WHERE ParentId=@Id ORDER BY Description");
            query.AddParameter("Id", CurrentMapId);
            return query.ExecuteReader();
        }

        private bool checkIncludeMapOrInfo(long id)
        {
            SqlCeCommand query = dbWorker.NewQuery("SELECT Count(1) FROM Maps WHERE ParentId=@Id");
            query.AddParameter("Id", id);
            object countObj = query.ExecuteScalar();

            return Convert.ToInt32(countObj) != 0;
        }

        private object[] getMapInfo(long id)
        {
            SqlCeCommand query = dbWorker.NewQuery(@"SELECT m.Id,m.Description,m.RegisterFrom,m.RegisterTo
FROM Maps m
WHERE Id=@Id");
            query.AddParameter("Id", id);

            return query.SelectArray();
        }
        #endregion
    }
}