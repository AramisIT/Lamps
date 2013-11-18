﻿using System;
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
        bool IsIntactDatabase(out bool lowPower);

        bool LoadingDataFromGreenhouse { get; set; }

        int GetNextUnitId();

        int GetNextLampId();

        bool UpdateMaps(List<Map> maps);

        bool UpdateParties(List<PartyModel> parties);

        bool UpdateModels(List<Model> models);

        bool UpdateLamps(List<Lamp> list, bool justInsert);

        bool UpdateUnits(List<Unit> list, bool justInsert);

        bool UpdateCases(List<Case> cases, bool justInsert);

        List<CatalogItem> ModelsList { get; }

        List<CatalogItem> PartiesList { get; }

        List<CatalogItem> MapsList { get; }

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

        List<List<int>> GetUpdateTasks(TypeOfAccessories accessoryType, int recordsQuantityInTask);

        bool ResetUpdateLog(TypeOfAccessories accessoriesType);

        long GetLastDownloadedId(TypeOfAccessories accessoryType);

        void SetLastDownloadedId(TypeOfAccessories accessoryType, long lastDownloadedId);

        List<int> GetCasesIds();

        long GetLastDownloadedId(Type catalogType);

        void SetLastDownloadedId(Type catalogType, long lastAcceptedRowId);

        void ResetModels();

        void ResetMaps();

        void ResetParties();

        bool UpdateBrokenLightsRecord(BrokenLightsRecord brokenLightsRecord);
        }
    }
