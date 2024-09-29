using System.Collections.Generic;
using UnitRosterGenerator;

class UnitConfiguration
{
    public Unit Unit { get; set; } // Базовый юнит
    public int ModelCount { get; set; } // Количество моделей
    public ExperienceLevelData ExperienceLevel { get; set; } // Уровень опыта
    public Dictionary<string, int> SelectedWeapons { get; set; } // Выбранное оружие и его количество
    public Dictionary<string, int> SelectedUnitUpgrades { get; set; } // Выбранные апгрейды для юнита и их количество
    public bool WeaponUpgradeSelected { get; set; } // Выбраны ли улучшения для оружия
    public int TotalCost { get; set; } // Общая стоимость юнита

    // Конструктор для создания конфигурации юнита
    public UnitConfiguration(Unit unit, int modelCount, ExperienceLevelData experienceLevel, Dictionary<string, int> selectedWeapons, Dictionary<string, int> selectedUnitUpgrades, bool weaponUpgradeSelected)
    {
        Unit = unit;
        ModelCount = modelCount;
        ExperienceLevel = experienceLevel;
        SelectedWeapons = selectedWeapons;
        SelectedUnitUpgrades = selectedUnitUpgrades; // Список выбранных апгрейдов для юнита
        WeaponUpgradeSelected = weaponUpgradeSelected;

        // Рассчитываем полную стоимость юнита в зависимости от выбранных параметров
        TotalCost = unit.CalculateCost(modelCount, selectedWeapons, experienceLevel, selectedUnitUpgrades, weaponUpgradeSelected);
    }
}
