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

        bool UpdateLamps(List<Lamp> list, bool justInsert);

        bool UpdateUnits(List<Unit> list, bool justInsert);

        bool UpdateCases(List<Case> cases, bool justInsert);

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

        bool WriteMap(Map map);

        bool SaveAccessoriesSet(Case _Case, Lamp lamp, Unit unit);

        bool SaveGroupOfSets(Case _Case, Lamp lamp, Unit unit, List<int> barcodes);
        }
    }
