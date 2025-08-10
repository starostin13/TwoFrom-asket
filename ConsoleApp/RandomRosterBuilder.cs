namespace UnitRosterGenerator
{
    class RandomRosterBuilder
    {
    private static readonly Random random = new();
    private static List<UnitConfiguration> currentRoster = new();
    private static readonly UnitsLimits unitsLimits = new();
    private static readonly List<string> bannedUnits = new();

        public static Roster BuildRandomRoster(
            List<Unit> availableUnits,
            List<Detach> availableDetaches,
            int maxPoints)
        {
            int currentPoints = 0;
            currentRoster = new List<UnitConfiguration>();

            Detach? selectedDetach = ChooseRandomDetach(availableDetaches);

            while (true)
            {
                var unit = GetRandomUnit(availableUnits);
                if (unit == null || bannedUnits.Contains(unit.Name)) continue;

                // Pre-generate a prospective model count for limit checking
                int prospectiveModels = unit.MinModels == unit.MaxModels ? unit.MinModels : random.Next(unit.MinModels, unit.MaxModels + 1);
                if (IsUnitLimitExceed(unit.Name, prospectiveModels)) continue;

                UnitConfiguration unitConfig = GetUnitconfig(selectedDetach, unit, prospectiveModels);
                UnitConfiguration? attachedUnitconfig = null;

                Unit? thisUnitLeadUnit = GetLeadedUnits(unit, availableUnits);
                if (thisUnitLeadUnit != null)
                {
                    attachedUnitconfig = GetUnitconfig(selectedDetach, thisUnitLeadUnit);

                    if (currentPoints + unitConfig.TotalCost + attachedUnitconfig.TotalCost > maxPoints)
                    {
                        break;
                    }
                }

                if (currentPoints + unitConfig.TotalCost > maxPoints)
                {
                    break;
                }

                currentRoster.Add(unitConfig);
                if(unit.MutualExclude != null)
                {
                    foreach (var unitName in unit.MutualExclude)
                    {
                        bannedUnits.Add(unitName);
                    }
                }
                currentPoints += unitConfig.TotalCost;
                if (attachedUnitconfig != null)
                {
                    currentRoster.Add(attachedUnitconfig);
                    currentPoints += attachedUnitconfig.TotalCost;
                }
            }

            return new Roster(currentRoster, selectedDetach);
        }

        static bool IsUnitLimitExceed(string name, int additionalModels = 0)
        {
            int? limit = unitsLimits.GetMaxLimit(name);
            if (limit == null) return false;

            int currentModels = currentRoster
                .Where(unitConfig => unitConfig.Unit.Name == name)
                .Sum(unitConfig => unitConfig.ModelCount);

            return currentModels + additionalModels > limit;
        }

    private static UnitConfiguration GetUnitconfig(Detach? selectedDetach, Unit unit, int? predefinedModelCount = null)
        {
            var experienceLevel = GetRandomExperienceLevel(unit);
            int modelCount = predefinedModelCount ?? random.Next(unit.MinModels, unit.MaxModels + 1);
            var selectedWeapons = GetRandomWeapons(unit.Weapons);
            var selectedUnitUpgrades = GetRandomUnitUpgrades(unit.Upgrade);

            if (selectedDetach != null && unit.DetachUpgrade)
            {
                var selectedDetachUpgrades = GetRandomDetachUpgrades(selectedDetach);

                int detachUpgradesAdded = 0;
                foreach (var upgrade in selectedDetachUpgrades)
                {
                    if (detachUpgradesAdded >= selectedDetach.MaxDetachUpgrades) break;

                    if (selectedUnitUpgrades.ContainsKey(upgrade.Key))
                    {
                        selectedUnitUpgrades[upgrade.Key] += upgrade.Value;
                    }
                    else
                    {
                        selectedUnitUpgrades[upgrade.Key] = upgrade.Value;
                    }
                    detachUpgradesAdded++;
                }
            }

            var unitConfig = new UnitConfiguration(
                unit, modelCount, experienceLevel, selectedWeapons, selectedUnitUpgrades, false, selectedDetach);
            return unitConfig;
        }

        private static Unit? GetLeadedUnits(Unit unit, List<Unit> availableUnits)
        {
            if (unit.Lead is not null)
            {
                return availableUnits.FirstOrDefault(u => u.Name == unit.Lead[random.Next(unit.Lead.Count)]);
            }

            return null;
        }

    private static Unit? GetRandomUnit(List<Unit> availableUnits)
        {
            if (availableUnits.Count == 0) return null;

            var mandatoryUnits = unitsLimits.Limits
                .Where(limit => limit.MinQuantity > 0 && availableUnits.Any(unit => unit.Name == limit.ModelName))
                .Select(limit => new
                {
                    Unit = availableUnits.First(unit => unit.Name == limit.ModelName),
                    Limit = limit
                })
                .Where(x => currentRoster
                    .Where(cfg => cfg.Unit.Name == x.Unit.Name)
                    .Sum(cfg => cfg.ModelCount) < x.Limit.MinQuantity)
                .Select(x => x.Unit)
                .ToList();

            if (mandatoryUnits.Count > 0)
            {
                return mandatoryUnits[random.Next(mandatoryUnits.Count)];
            }

            return availableUnits[random.Next(availableUnits.Count)];
        }

        private static ExperienceLevelData GetRandomExperienceLevel(Unit unit)
        {
            if (unit.Experience.Count == 0)
            {
                return new ExperienceLevelData { Level = "Regular", BaseCost = 0, AdditionalModelCost = 0 };
            }
            return unit.Experience[random.Next(unit.Experience.Count)];
        }

        private static Dictionary<string, int> GetRandomWeapons(List<Weapon>? weapons)
        {
            var selectedWeapons = new Dictionary<string, int>();

            if (weapons != null && weapons.Count > 0)
            {
                foreach (var weapon in weapons)
                {
                    if (random.Next(0, 2) == 1)
                    {
                        int weaponCount = random.Next(weapon.MinCount, weapon.MaxCount + 1);
                        selectedWeapons[weapon.Name] = weaponCount;

                        if (weapon.Upgrades != null)
                        {
                            foreach (var upgrade in weapon.Upgrades)
                            {
                                if (random.Next(0, 2) == 1)
                                {
                                    selectedWeapons[upgrade.Name] = 1;
                                }
                            }
                        }
                    }
                }
            }

            return selectedWeapons;
        }

    private static Dictionary<string, int> GetRandomUnitUpgrades(List<Upgrade>? upgrades)
        {
            var selectedUpgrades = new Dictionary<string, int>();

            if (upgrades != null)
            {
                foreach (var upgrade in upgrades)
                {
                    int upgradeCount = random.Next(upgrade.MinCount, upgrade.MaxCount + 1);
                    if (upgradeCount > 0)
                    {
                        selectedUpgrades[upgrade.Name] = upgradeCount;
                    }
                }
            }

            return selectedUpgrades;
        }

    private static Dictionary<string, int> GetRandomDetachUpgrades(Detach detach)
        {
            var selectedUpgrades = new Dictionary<string, int>();
            int upgradesAdded = 0;

            // можно добавлять только те апгреды которые ещё не добавлены в ростер
            var allSelectedUpgradeKeys = currentRoster.SelectMany(unitConfig => unitConfig.SelectedUpgrades.Keys).ToList();

            var filteredUpgrades = detach.Upgrades.Where(upgrade => !allSelectedUpgradeKeys.Contains(upgrade.Name)).ToList();
            filteredUpgrades.Shuffle();


            foreach (var upgrade in filteredUpgrades)
            {
                if (upgradesAdded >= detach.MaxDetachUpgrades) break;

                if (random.Next(0, 2) == 1)
                {
                    int upgradeCount = random.Next(1, upgrade.MaxCount + 1);
                    if (upgradeCount > 0)
                    {
                        selectedUpgrades[upgrade.Name] = upgradeCount;
                        upgradesAdded++;
                    }
                }
            }

            return selectedUpgrades;
        }

    private static Detach? ChooseRandomDetach(List<Detach> detaches)
        {
            if (detaches == null || detaches.Count == 0) return null;
            return detaches[random.Next(detaches.Count)];
        }
    }

    static class Extensions
    {
        private static Random rng = new Random();

        public static void Shuffle<T>(this List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = list[n];
                list[n] = list[k];
                list[k] = temp;
            }
        }
    }
}
