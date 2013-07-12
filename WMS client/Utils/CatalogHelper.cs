using System;
using System.Data.SqlServerCe;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using WMS_client.Enums;
using WMS_client.db;

namespace WMS_client.Utils
    {
    class CatalogHelper
        {
        private static Dictionary<Type, SyncRefsDict> cache = new Dictionary<Type, SyncRefsDict>();

        internal static long GetModelId<T>(object syncRef) where T : CatalogObject
            {
            string strSyncRef = ((syncRef ?? string.Empty) as string).TrimEnd();
            if (string.IsNullOrEmpty(strSyncRef))
                {
                return 0;
                }

            long id;

            SyncRefsDict itemsCache = getItemsCache<T>();
            if (!itemsCache.TryGetValue(strSyncRef, out id))
                {
                id = Convert.ToInt64(BarcodeWorker.GetIdByRef(typeof(T), strSyncRef));
                itemsCache.Add(strSyncRef, id);
                }

            return id;
            }

        private static SyncRefsDict getItemsCache<T>()
            {
            SyncRefsDict syncRefsDict;
            if (!cache.TryGetValue(typeof(T), out syncRefsDict))
                {
                syncRefsDict = new SyncRefsDict();
                cache.Add(typeof(T), syncRefsDict);
                }

            return syncRefsDict;
            }

        internal class SyncRefsDict : Dictionary<string, long>
        {
        }

        public static long FindCaseId(long accessoryId, TypeOfAccessories typeOfAccessories)
            {
            string sql = string.Format(@"select Id from cases where {0} = @accessoryId", typeOfAccessories);
            SqlCeCommand query = dbWorker.NewQuery(sql);
            query.AddParameter("accessoryId", accessoryId);
            object idObj = query.ExecuteScalar();

            return idObj == null ? 0 : Convert.ToInt64(idObj);
            }

        }
    }
