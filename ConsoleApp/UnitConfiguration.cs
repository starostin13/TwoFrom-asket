using System.Collections.Generic;
using System.Linq;

namespace UnitRosterGenerator
{
    public class UnitConfiguration
    {
        public Unit Unit { get; set; } // Базовый юнит
        public int ModelCount { get; set; } // Количество моделей
        public ExperienceLevelData ExperienceLevel { get; set; } // Уровень опыта
        public Dictionary<string, int> SelectedWeapons { get; set; } = new(); // Выбранное оружие и его количество
        public Dictionary<string, int> SelectedUpgrades { get; set; } = new(); // Выбранные апгрейды (как из юнита, так и из детача)
        public bool WeaponUpgradeSelected { get; set; } // Выбраны ли улучшения для оружия
        public int TotalCost { get; private set; } // Общая стоимость юнита

        // Конструктор для создания конфигурации юнита и подсчета его стоимости
        public UnitConfiguration(
            Unit unit,
            int modelCount,
            ExperienceLevelData experienceLevel,
            Dictionary<string, int> selectedWeapons,
            Dictionary<string, int> selectedUpgrades,
            bool weaponUpgradeSelected,
            Detach? selectedDetach) // Добавлено selectedDetach
        {
            Unit = unit;
            ModelCount = modelCount;
            ExperienceLevel = experienceLevel;
            SelectedWeapons = selectedWeapons;
            SelectedUpgrades = selectedUpgrades;
            WeaponUpgradeSelected = weaponUpgradeSelected;

            // Рассчитываем полную стоимость
            CalculateTotalCost(selectedDetach); // Передаем selectedDetach для расчета стоимости
        }

        // Метод для расчета полной стоимости юнита, включая все выбранные апгрейды и модели
    private void CalculateTotalCost(Detach? selectedDetach)
        {
            TotalCost = ExperienceLevel.BaseCost;

            // Добавляем стоимость за дополнительные модели
            if (ModelCount > Unit.MinModels)
            {
                TotalCost += (ModelCount - Unit.MinModels) * ExperienceLevel.AdditionalModelCost;
            }

            // Добавляем стоимость выбранного оружия
            if (SelectedWeapons != null)
            {
                foreach (var weapon in Unit.Weapons ?? Enumerable.Empty<Weapon>())
                {
                    if (SelectedWeapons.ContainsKey(weapon.Name))
                    {
                        int weaponCount = SelectedWeapons[weapon.Name];
                        TotalCost += weaponCount * weapon.Cost;

                        // Если выбрано улучшение оружия, добавляем его стоимость
                        if (WeaponUpgradeSelected && weapon.Upgrades != null)
                        {
                            foreach (var upgrade in weapon.Upgrades)
                            {
                                TotalCost += weaponCount * upgrade.Cost;
                            }
                        }
                    }
                }
            }

            // Добавляем стоимость для каждого выбранного апгрейда из SelectedUpgrades
            if (SelectedUpgrades != null)
            {
                foreach (var upgradeEntry in SelectedUpgrades)
                {
                    string upgradeName = upgradeEntry.Key;
                    int upgradeCount = upgradeEntry.Value;

                    // Ищем апгрейд по имени среди улучшений юнита
                    var unitUpgrade = Unit.Upgrade?.FirstOrDefault(u => u.Name == upgradeName);
                    if (unitUpgrade != null && upgradeCount > 0)
                    {
                        TotalCost += ModelCount * unitUpgrade.Cost;
                    }

                    // Ищем апгрейд по имени среди улучшений выбранного детача
                    var detachUpgrade = selectedDetach?.Upgrades.FirstOrDefault(u => u.Name == upgradeName);
                    if (detachUpgrade != null)
                    {
                        TotalCost += upgradeCount * detachUpgrade.Cost;
                    }
                }
            }
        }
    }
}
