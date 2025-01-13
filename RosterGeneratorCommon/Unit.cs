using System.Collections.Generic;

namespace UnitRosterGenerator
{
    public class Unit
    {
        public required string Name { get; set; }
        public int MinModels { get; set; }
        public int MaxModels { get; set; }
        public List<ExperienceLevelData>? Experience { get; set; } // Список уровней опыта для юнита
        public List<Weapon>? Weapons { get; set; } // Список доступного оружия
        public List<Upgrade>? Upgrade { get; set; } // Список улучшений юнита
        public bool DetachUpgrade { get; set; } // Указывает, может ли юнит использовать апгрейды детачей
        public List<string>? Lead {  get; set; } // Список юнитов которые могут сопровождать юнит

        // Метод для расчета общей стоимости юнита с учетом количества моделей, уровня опыта, выбранного оружия и улучшений
        int CalculateCost(
            int modelCount,
            Dictionary<string, int> selectedWeapons,
            ExperienceLevelData experienceLevel,
            Dictionary<string, int> selectedUpgrades,
            bool weaponUpgradeSelected)
        {
            int totalCost = experienceLevel.BaseCost;

            // Добавляем стоимость за дополнительные модели
            if (modelCount > MinModels)
            {
                totalCost += (modelCount - MinModels) * experienceLevel.AdditionalModelCost;
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
            return $"{Name}, Модели: {MinModels}-{MaxModels}, Опыт: {string.Join(", ", Experience?.Select(e => $"{e.Level} (Base: {e.BaseCost}, Extra: {e.AdditionalModelCost})"))}";
        }
    }
}
