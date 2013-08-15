using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using WMS_client.Enums;
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
          
        Case FintCaseByLamp(int lampId);

        Case FintCaseByUnit(int unitId);

        Unit ReadUnitByBarcode(int barcode);

        Lamp ReadLampByBarcode(int barcode);

        List<Case> ReadCases(List<int> idsList);

        List<Unit> ReadUnits(List<int> idsList);

        List<Lamp> ReadLamps(List<int> idsList);

        bool WriteMap(Map map);

        List<List<int>> GetUpdateTasks(TypeOfAccessories accessoryType, int recordsQuantityInTask);

        DataTable GetAccessoriesTable(List<int> task, TypeOfAccessories typeOfAccessories);
        }
    }
