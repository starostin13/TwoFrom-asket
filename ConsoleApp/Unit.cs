using System.Collections.Generic;
using System.Linq;

namespace UnitRosterGenerator
{
    public class UnitVariant
    {
        public required string Name { get; set; }
        public int? MinModels { get; set; }
        public int? MaxModels { get; set; }
        public List<ExperienceLevelData>? Experience { get; set; }
        public List<Weapon>? Weapons { get; set; }
        public List<Upgrade>? Upgrade { get; set; }
        public bool? DetachUpgrade { get; set; }
        public List<string>? Lead { get; set; }
        public List<string>? MutualExclude { get; set; }
    }

    public class Unit
    {
        public required string Name { get; set; }
        public string? ModelType { get; set; } // Optional: groups units that use same physical models
        public int MinModels { get; set; }
        public int MaxModels { get; set; }
        public List<ExperienceLevelData> Experience { get; set; } = new();
        public List<Weapon>? Weapons { get; set; }
        public List<Upgrade>? Upgrade { get; set; }
        public bool DetachUpgrade { get; set; }
        public List<string>? Lead {  get; set; }
        public List<string>? MutualExclude { get; set; }
        public List<UnitVariant>? Variants { get; set; }
        public List<string>? Tags { get; set; }

        public bool HasVariants => Variants != null && Variants.Count > 0;

        public UnitVariant ResolveVariant(string? variantName)
        {
            if (HasVariants)
            {
                if (!string.IsNullOrWhiteSpace(variantName))
                {
                    var byName = Variants!.FirstOrDefault(v =>
                        v.Name.Equals(variantName, StringComparison.InvariantCultureIgnoreCase));
                    if (byName != null)
                    {
                        return byName;
                    }
                }

                return Variants!.First();
            }

            return new UnitVariant
            {
                Name = Name,
                MinModels = MinModels,
                MaxModels = MaxModels,
                Experience = Experience,
                Weapons = Weapons,
                Upgrade = Upgrade,
                DetachUpgrade = DetachUpgrade,
                Lead = Lead,
                MutualExclude = MutualExclude
            };
        }

        public int GetMinModels(UnitVariant variant)
            => variant.MinModels ?? MinModels;

        public int GetMaxModels(UnitVariant variant)
            => variant.MaxModels ?? MaxModels;

        public List<ExperienceLevelData> GetExperience(UnitVariant variant)
            => variant.Experience ?? Experience;

        public List<Weapon>? GetWeapons(UnitVariant variant)
            => variant.Weapons ?? Weapons;

        public List<Upgrade>? GetUpgrades(UnitVariant variant)
            => variant.Upgrade ?? Upgrade;

        public bool GetDetachUpgrade(UnitVariant variant)
            => variant.DetachUpgrade ?? DetachUpgrade;

        public List<string>? GetLead(UnitVariant variant)
            => variant.Lead ?? Lead;

        public List<string>? GetMutualExclude(UnitVariant variant)
            => variant.MutualExclude ?? MutualExclude;

        public string GetDisplayName(UnitVariant variant)
        {
            if (!HasVariants)
            {
                return Name;
            }

            return $"{Name} ({variant.Name})";
        }

        public int GetGlobalMinModels()
        {
            if (!HasVariants)
            {
                return MinModels;
            }

            return Variants!
                .Select(v => v.MinModels ?? MinModels)
                .DefaultIfEmpty(MinModels)
                .Min();
        }

        public int GetGlobalMaxModels()
        {
            if (!HasVariants)
            {
                return MaxModels;
            }

            return Variants!
                .Select(v => v.MaxModels ?? MaxModels)
                .DefaultIfEmpty(MaxModels)
                .Max();
        }

        // Метод для расчета общей стоимости юнита с учетом количества моделей, уровня опыта, выбранного оружия и улучшений
        public int CalculateCost(
            int modelCount,
            Dictionary<string, int> selectedWeapons,
            ExperienceLevelData experienceLevel,
            Dictionary<string, int> selectedUpgrades,
            bool weaponUpgradeSelected)
        {
            var variant = ResolveVariant(null);
            int totalCost = experienceLevel.BaseCost;
            var minModels = GetMinModels(variant);
            var weapons = GetWeapons(variant);
            var upgrades = GetUpgrades(variant);

            // Добавляем стоимость за дополнительные модели
            if (modelCount > minModels)
            {
                totalCost += (modelCount - minModels) * experienceLevel.AdditionalModelCost;
            }

            // Добавляем стоимость выбранного оружия
            if (weapons != null)
            {
                foreach (var weapon in weapons)
                {
                    if (selectedWeapons.ContainsKey(weapon.Name))
                    {
                        int weaponCount = selectedWeapons[weapon.Name];
                        totalCost += weaponCount * weapon.Cost;

                        // Если выбрано улучшение оружия, добавляем его стоимость
                        if (weaponUpgradeSelected && weapon.Upgrades != null)
                        {
                            foreach (var upgrade in weapon.Upgrades)
                            {
                                totalCost += weaponCount * upgrade.Cost;
                            }
                        }
                    }
                }
            }

            // Добавляем стоимость для каждого выбранного апгрейда из SelectedUpgrades
            if (selectedUpgrades != null)
            {
                foreach (var upgradeEntry in selectedUpgrades)
                {
                    string upgradeName = upgradeEntry.Key;
                    int upgradeCount = upgradeEntry.Value;

                    // Ищем апгрейд по имени среди улучшений юнита
                    var unitUpgrade = upgrades?.FirstOrDefault(u => u.Name == upgradeName);
                    if (unitUpgrade != null)
                    {
                        totalCost += upgradeCount * unitUpgrade.Cost;
                    }
                }
            }

            return totalCost;
        }

        public override string ToString()
        {
            var expList = Experience.Select(e => $"{e.Level} (Base: {e.BaseCost}, Extra: {e.AdditionalModelCost})");
            return $"{Name}, Модели: {MinModels}-{MaxModels}, Опыт: {string.Join(", ", expList)}";
        }
    }
}
