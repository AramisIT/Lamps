using System;
using System.Data.SqlServerCe;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using WMS_client.Models;

namespace WMS_client.Repositories
    {
    class CatalogCache<ID, C> where C : ICatalog<ID>, new()
        {
        public List<C> CatalogList
            {
            get
                {
                return catalogList;
                }
            }

        public List<CatalogItem> ItemsList
            {
            get
                {
                var result = new List<CatalogItem>();

                foreach (var item in catalogList)
                    {
                    var catalogItem = new CatalogItem() { Description = item.Description };
                    catalogItem.Id = Convert.ToInt64(item.Id);
                    result.Add(catalogItem);
                    }

                return result;
                }
            }

        public C GetCatalogItem(ID id)
            {
            C foundedCatalogItem;
            if (catalogDictionary.TryGetValue(id, out foundedCatalogItem))
                {
                return foundedCatalogItem;
                }

            return new C();
            }

        public void Load(string sqlCommand, Func<SqlCeConnection> getConnection, Action<SqlCeDataReader, C> fillCatalog)
            {
            catalogList = new List<C>();

            using (var conn = getConnection())
                {
                using (var cmd = conn.CreateCommand())
                    {
                    cmd.CommandText = sqlCommand;
                    using (var reader = cmd.ExecuteReader())
                        {
                        while (reader.Read())
                            {
                            var newCatalog = new C();
                            fillCatalog(reader, newCatalog);
                            catalogList.Add(newCatalog);
                            }
                        }
                    }
                }

            fillDictionary();
            }

        private List<C> catalogList;
        private Dictionary<ID, C> catalogDictionary;

        private void fillDictionary()
            {
            catalogDictionary = new Dictionary<ID, C>();
            catalogList.ForEach(catalog => catalogDictionary.Add(catalog.Id, catalog));
            }


        }
    }
