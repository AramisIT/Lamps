using WMS_client.Enums;
using WMS_client.Utils;

namespace WMS_client.db
    {
    /// <summary>Лампа</summary>
    public class Lamps : Accessory
        {
        private long _case = -1;

        /// <summary>Корпус</summary>
        public long Case
            {
            get
                {
               // if (_case < 0)
                    {
                    _case = CatalogHelper.FindCaseId(Id, TypeOfAccessories.Lamp);
                    }
                return _case;
                }
            set { _case = value; }
            }

        public override object Write()
            {
            return base.Save<Lamps>();
            }

        public override object Sync()
            {
            return base.Sync<Lamps>();
            }
        }
    }