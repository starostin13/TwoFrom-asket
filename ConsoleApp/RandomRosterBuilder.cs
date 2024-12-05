using System.Collections.Generic;

namespace UnitRosterGenerator
{
    class RandomRosterBuilder
    {
        private static Random random = new Random();

        public static Roster BuildRandomRoster(
            List<Unit> availableUnits,
            List<Detach> availableDetaches,
            int maxPoints)
        {
            int currentPoints = 0;
            List<UnitConfiguration> currentRoster = new List<UnitConfiguration>();

            Detach selectedDetach = ChooseRandomDetach(availableDetaches);

            while (true)
            {
                var unit = GetRandomUnit(availableUnits);
                if (unit == null) break;
                UnitConfiguration unitConfig = GetUnitconfig(selectedDetach, unit);
                UnitConfiguration attachedUnitconfig = null;
                                
                Unit thisUnitLeadUnit = GetLeadedUnits(unit, availableUnits);
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
                currentPoints += unitConfig.TotalCost;
                if (attachedUnitconfig != null)
                {
                    currentRoster.Add(attachedUnitconfig);
                    currentPoints += attachedUnitconfig.TotalCost;
                }

            }

            return new Roster(currentRoster, selectedDetach);
        }

        private static UnitConfiguration GetUnitconfig(Detach selectedDetach, Unit unit)
        {
            var experienceLevel = GetRandomExperienceLevel(unit);
            if (experienceLevel == null) continue;

            int modelCount = random.Next(unit.MinModels, unit.MaxModels + 1);
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

        private static Unit GetRandomUnit(List<Unit> availableUnits)
        {
            return availableUnits.Count > 0 ? availableUnits[random.Next(availableUnits.Count)] : null;
        }

        private static ExperienceLevelData GetRandomExperienceLevel(Unit unit)
        {
            return unit.Experience?.Count > 0 ? unit.Experience[random.Next(unit.Experience.Count)] : null;
        }

        private static Dictionary<string, int> GetRandomWeapons(List<Weapon> weapons)
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

        private static Dictionary<string, int> GetRandomUnitUpgrades(List<Upgrade> upgrades)
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

            var shuffledUpgrades = new List<Upgrade>(detach.Upgrades);
            shuffledUpgrades.Shuffle();

            foreach (var upgrade in shuffledUpgrades)
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

        private static Detach ChooseRandomDetach(List<Detach> detaches)
        {
            if (detaches == null) return null;

            return detaches.Count > 0 ? detaches[random.Next(detaches.Count)] : null;
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
