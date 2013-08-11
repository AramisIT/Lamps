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

//CREATE TABLE Lamps(
//Id int not null,
//BarCode nchar(25) not null,
//Date datetime not null,
//DateOfActuality datetime not null,
//DateOfWarrantyEnd datetime not null,
//DrawdownDate datetime not null,
//HoursOfWork real not null,
//MarkForDeleting bit not null,
//Marking nchar(25) not null,
//Model int not null,
//Number int not null,
//Party int not null,
//Posted bit not null,
//Responsible int not null,
//Status int not null,
//TypeOfWarrantly int not null,
//Description nchar(25) not null,
//LastModified datetime not null,
//Location int not null,
//IsSynced bit not null,
//SyncRef nchar(25) not null
//)