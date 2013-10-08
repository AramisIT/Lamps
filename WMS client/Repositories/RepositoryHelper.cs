using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using WMS_client.Models;
using WMS_client.Repositories;

namespace WMS_client
    {
    public static class RepositoryHelper
        {
        public static Lamp ReadLamp(this IRepository repository, int id)
            {
            List<Lamp> lamps = repository.ReadLamps(new List<int>() { id });
            return lamps.Count > 0 ? lamps[0] : null;
            }

        public static Unit ReadUnit(this IRepository repository, int id)
            {
            List<Unit> units = repository.ReadUnits(new List<int>() { id });
            return units.Count > 0 ? units[0] : null;
            }

        public static Case ReadCase(this IRepository repository, int id)
            {
            List<Case> cases = repository.ReadCases(new List<int>() { id });
            return cases.Count > 0 ? cases[0] : null;
            }

        public static bool WriteCase(this IRepository repository, Case _Case)
            {
            return repository.UpdateCases(new List<Case>() { _Case }, false);
            }

        public static bool WriteLamp(this IRepository repository, Lamp lamp)
            {
            return repository.UpdateLamps(new List<Lamp>() { lamp }, false);
            }

        public static bool WriteUnit(this IRepository repository, Unit unit)
            {
            return repository.UpdateUnits(new List<Unit>() { unit }, false);
            }
        }
    }
