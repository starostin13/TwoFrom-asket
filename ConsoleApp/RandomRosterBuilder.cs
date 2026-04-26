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
            List<Detach> availableDetachments,
            int maxPoints)
        {
            int currentPoints = 0;
            currentRoster = new List<UnitConfiguration>();

            Detach? selectedDetach = ChooseRandomDetach(availableDetachments);

            while (true)
            {
                var unit = GetRandomUnit(availableUnits);
                if (unit == null || bannedUnits.Contains(unit.Name)) continue;

                // Check limits and get adapted model count
                int? prospectiveModels = GetAdaptedModelCount(unit);
                if (!prospectiveModels.HasValue) continue; // Limit exhausted, skip unit

                UnitConfiguration unitConfig = GetUnitconfig(selectedDetach, unit, prospectiveModels.Value);
                UnitConfiguration? attachedUnitconfig = null;

                Unit? thisUnitLeadUnit = GetLeadedUnits(unitConfig, availableUnits);
                if (thisUnitLeadUnit != null)
                {
                    // Check limits and get adapted model count for attached unit too
                    int? attachedProspectiveModels = GetAdaptedModelCount(thisUnitLeadUnit);
                    if (attachedProspectiveModels.HasValue)
                    {
                        attachedUnitconfig = GetUnitconfig(selectedDetach, thisUnitLeadUnit, attachedProspectiveModels.Value);
                    }
                
                    if (attachedUnitconfig != null && currentPoints + unitConfig.TotalCost + attachedUnitconfig.TotalCost > maxPoints)
                    {
                        break;
                    }
                }

                if (currentPoints + unitConfig.TotalCost > maxPoints)
                {
                    break;
                }

                currentRoster.Add(unitConfig);
                if (unitConfig.EffectiveMutualExclude.Count > 0)
                {
                    foreach (var unitName in unitConfig.EffectiveMutualExclude)
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

    private static UnitConfiguration GetUnitconfig(Detach? selectedDetach, Unit unit, int? modelCountCap = null)
        {
            var selectedVariant = GetRandomVariant(unit);
            var minModels = unit.GetMinModels(selectedVariant);
            var maxModels = unit.GetMaxModels(selectedVariant);
            var experienceLevel = GetRandomExperienceLevel(unit, selectedVariant);

            // Apply the cap from model limits
            int effectiveMax = (modelCountCap.HasValue && modelCountCap.Value != int.MaxValue)
                ? Math.Min(maxModels, modelCountCap.Value)
                : maxModels;
            effectiveMax = Math.Max(minModels, effectiveMax);
            int modelCount = minModels == effectiveMax ? minModels : random.Next(minModels, effectiveMax + 1);

            var selectedWeapons = GetRandomWeapons(unit.GetWeapons(selectedVariant), modelCount);
            var selectedUnitUpgrades = GetRandomUnitUpgrades(unit.GetUpgrades(selectedVariant));

            if (selectedDetach != null && unit.GetDetachUpgrade(selectedVariant))
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
                unit, selectedVariant, modelCount, experienceLevel, selectedWeapons, selectedUnitUpgrades, false, selectedDetach);
            return unitConfig;
        }

        /// <summary>
        /// Gets adapted model count for a unit based on current limits.
        /// Returns null if limit is exhausted and unit should be skipped.
        /// </summary>
        private static int? GetAdaptedModelCount(Unit unit)
        {
            int currentModelsOfThisType = 0;
            int? maxLimit = null;

            // Check ModelType limit first (shared models)
            if (!string.IsNullOrEmpty(unit.ModelType))
            {
                maxLimit = unitsLimits.GetMaxLimitByModelType(unit.ModelType);
                if (maxLimit.HasValue)
                {
                    currentModelsOfThisType = currentRoster
                        .Where(unitConfig => unitConfig.Unit.ModelType == unit.ModelType)
                        .Sum(unitConfig => unitConfig.ModelCount);
                }
            }
            
            // If no ModelType limit, check unit-specific limit
            if (!maxLimit.HasValue)
            {
                maxLimit = unitsLimits.GetMaxLimit(unit.Name);
                currentModelsOfThisType = currentRoster
                    .Where(unitConfig => unitConfig.Unit.Name == unit.Name)
                    .Sum(unitConfig => unitConfig.ModelCount);
            }

            if (maxLimit.HasValue)
            {
                int remainingSlots = maxLimit.Value - currentModelsOfThisType;
                if (remainingSlots <= 0) return null; // Limit exhausted, skip unit

                // Return remaining slots as a cap; actual random pick happens in GetUnitconfig after bonus is applied
                int unitMin = unit.GetGlobalMinModels();
                if (remainingSlots < unitMin) return null; // Can't fit even a min-size unit
                return remainingSlots;
            }
            else
            {
                // No limit — return int.MaxValue so GetUnitconfig does uncapped random pick
                return int.MaxValue;
            }
        }

        private static Unit? GetLeadedUnits(UnitConfiguration unitConfig, List<Unit> availableUnits)
        {
            var leadList = unitConfig.EffectiveLead;
            if (leadList.Count > 0)
            {
                string target = leadList[random.Next(leadList.Count)];
                var candidates = availableUnits.Where(u =>
                    u.Name.Equals(target, StringComparison.InvariantCultureIgnoreCase) ||
                    (u.Variants?.Any(v => v.Name.Equals(target, StringComparison.InvariantCultureIgnoreCase)) ?? false) ||
                    (u.Variants?.Any(v => $"{u.Name} ({v.Name})".Equals(target, StringComparison.InvariantCultureIgnoreCase)) ?? false)
                ).ToList();

                if (candidates.Count > 0)
                {
                    return candidates[random.Next(candidates.Count)];
                }
            }

            return null;
        }

    private static Unit? GetRandomUnit(List<Unit> availableUnits)
        {
            if (availableUnits.Count == 0) return null;

            var mandatoryUnits = unitsLimits.Limits
                .Where(limit => limit.MinQuantity > 0 && availableUnits.Any(unit => UnitMatchesLimitName(unit, limit.ModelName)))
                .Select(limit => new
                {
                    Unit = availableUnits.First(unit => UnitMatchesLimitName(unit, limit.ModelName)),
                    Limit = limit
                })
                .Where(x => currentRoster
                    .Where(cfg =>
                        cfg.Unit.Name == x.Unit.Name ||
                        cfg.SelectedVariant.Name.Equals(x.Limit.ModelName, StringComparison.InvariantCultureIgnoreCase) ||
                        cfg.DisplayName.Equals(x.Limit.ModelName, StringComparison.InvariantCultureIgnoreCase))
                    .Sum(cfg => cfg.ModelCount) < x.Limit.MinQuantity)
                .Select(x => x.Unit)
                .ToList();

            if (mandatoryUnits.Count > 0)
            {
                return mandatoryUnits[random.Next(mandatoryUnits.Count)];
            }

            return availableUnits[random.Next(availableUnits.Count)];
        }

        private static ExperienceLevelData GetRandomExperienceLevel(Unit unit, UnitVariant variant)
        {
            var levels = unit.GetExperience(variant);
            if (levels.Count == 0)
            {
                return new ExperienceLevelData { Level = "Regular", BaseCost = 0, AdditionalModelCost = 0 };
            }

            return levels[random.Next(levels.Count)];
        }

        private static UnitVariant GetRandomVariant(Unit unit)
        {
            if (unit.Variants == null || unit.Variants.Count == 0)
            {
                return unit.ResolveVariant(null);
            }

            return unit.Variants[random.Next(unit.Variants.Count)];
        }

        private static bool UnitMatchesLimitName(Unit unit, string? modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
            {
                return false;
            }

            if (unit.Name.Equals(modelName, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (unit.Variants == null)
            {
                return false;
            }

            return unit.Variants.Any(v =>
                v.Name.Equals(modelName, StringComparison.InvariantCultureIgnoreCase) ||
                $"{unit.Name} ({v.Name})".Equals(modelName, StringComparison.InvariantCultureIgnoreCase));
        }

        private static Dictionary<string, int> GetRandomWeapons(List<Weapon>? weapons, int modelCount)
        {
            var selectedWeapons = new Dictionary<string, int>();

            if (weapons != null && weapons.Count > 0)
            {
                foreach (var weapon in weapons)
                {
                    if (random.Next(0, 2) == 1)
                    {
                        int resolvedMax = weapon.ResolveMaxCount(modelCount);
                        int weaponCount = random.Next(weapon.MinCount, resolvedMax + 1);
                        if (weaponCount > 0)
                        {
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

    private static Detach? ChooseRandomDetach(List<Detach> detachments)
        {
            if (detachments == null || detachments.Count == 0) return null;
            return detachments[random.Next(detachments.Count)];
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
