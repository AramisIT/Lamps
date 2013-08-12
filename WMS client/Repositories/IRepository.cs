using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WMS_client.Models;

namespace WMS_client.Repositories
    {
    public interface IRepository
        {
        bool WriteModel(Model model);

        bool WriteParty(PartyModel party);

        List<Model> ModelsList { get; }

        List<PartyModel> PartiesList { get; }

        Model GetModel(short modelId);

        Map GetMap(int id);

        PartyModel GetParty(int partyId);

        IAccessory FindAccessory(int accessoryBarcode);

        Case ReadCase(int id);

        Unit ReadUnitByBarcode(int barcode);

        Lamp ReadLampByBarcode(int barcode);

        Unit ReadUnit(int id);

        Lamp ReadLamp(int id);

        bool WriteMap(Map map);
        }
    }
