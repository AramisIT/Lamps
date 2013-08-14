using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WMS_client.Models;

namespace WMS_client.Repositories
    {
    public interface IRepository
        {
        int GetNextUnitId();

        int GetNextLampId();

        bool WriteModel(Model model);

        bool WriteParty(PartyModel party);

        bool InsertCases(List<Case> list);

        bool InsertLamps(List<Lamp> list);

        bool InsertUnits(List<Unit> list);

        List<Model> ModelsList { get; }

        List<PartyModel> PartiesList { get; }

        Model GetModel(short modelId);

        Map GetMap(int id);

        PartyModel GetParty(int partyId);

        IAccessory FindAccessory(int accessoryBarcode);

        Case ReadCase(int id);

        Case FintCaseByLamp(int lampId);

        Case FintCaseByUnit(int unitId);

        Unit ReadUnitByBarcode(int barcode);

        Lamp ReadLampByBarcode(int barcode);

        Unit ReadUnit(int id);

        Lamp ReadLamp(int id);

        bool UpdateCase(Case _Case);

        bool WriteMap(Map map);

        bool SaveAccessoriesSet(Case _Case, Lamp lamp, Unit unit);

        bool SaveGroupOfSets(Case _Case, Lamp lamp, Unit unit, List<int> barcodes);
        }
    }
