using System.Collections.Generic;
using System.Text.Json;

namespace UnitRosterGenerator
{
    public class Unit
    {
        public required string Name { get; set; }
        // Top-level Min/Max/Experience kept for backward compatibility. If Corps present, they are ignored.
        public int? MinModels { get; set; }
        public int? MaxModels { get; set; }
        public List<ExperienceLevelData>? Experience { get; set; } = new();
    public List<CorpsOption>? Corps { get; set; }
    // Arbitrary extra properties (e.g. TransportCapacity, DamageValue, Tow, etc.)
    public Dictionary<string, JsonElement>? Properties { get; set; }
    // Optional arbitrary tags (e.g. Period tags like E, M, L)
    public List<string>? Tags { get; set; }
        public List<Weapon>? Weapons { get; set; }
        public List<Upgrade>? Upgrade { get; set; }
        public bool DetachUpgrade { get; set; }
        public List<string>? Lead {  get; set; }
        public List<string>? MutualExclude { get; set; }

        // Метод для расчета общей стоимости юнита с учетом количества моделей, уровня опыта, выбранного оружия и улучшений
        public int CalculateCost(
            int modelCount,
            Dictionary<string, int> selectedWeapons,
            ExperienceLevelData experienceLevel,
            Dictionary<string, int> selectedUpgrades,
            bool weaponUpgradeSelected)
        {
            int totalCost = experienceLevel.BaseCost;

            // Добавляем стоимость за дополнительные модели
            int baseMin = Corps != null && Corps.Count > 0 ? Corps[0].MinModels : (MinModels ?? 0);
            if (modelCount > baseMin)
            {
                totalCost += (modelCount - baseMin) * experienceLevel.AdditionalModelCost;
            }

            // Добавляем стоимость выбранного оружия
            if (Weapons != null)
            {
                foreach (var weapon in Weapons ?? Enumerable.Empty<Weapon>())
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
                    var unitUpgrade = Upgrade?.FirstOrDefault(u => u.Name == upgradeName);
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
            var primaryCorps = Corps?.FirstOrDefault();
            int min = primaryCorps?.MinModels ?? (MinModels ?? 0);
            int max = primaryCorps?.MaxModels ?? (MaxModels ?? min);
            var expSource = primaryCorps?.Experience ?? Experience ?? new List<ExperienceLevelData>();
            var expList = expSource.Select(e => $"{e.Level} (Base: {e.BaseCost}, Extra: {e.AdditionalModelCost})");
            return $"{Name}, Модели: {min}-{max}, Опыт: {string.Join(", ", expList)}";
        }
    }
}
