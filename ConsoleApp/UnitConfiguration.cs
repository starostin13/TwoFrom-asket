using System.Collections.Generic;
using UnitRosterGenerator;

class UnitConfiguration
{
    public Unit Unit { get; set; } // Базовый юнит
    public int ModelCount { get; set; } // Количество моделей
    public ExperienceLevelData ExperienceLevel { get; set; } // Уровень опыта
    public Dictionary<string, int> SelectedWeapons { get; set; } // Выбранное оружие и его количество
    public bool UnitUpgradeSelected { get; set; } // Выбраны ли улучшения для юнита
    public bool WeaponUpgradeSelected { get; set; } // Выбраны ли улучшения для оружия
    public int TotalCost { get; set; } // Общая стоимость юнита

    // Конструктор для создания конфигурации юнита
    public UnitConfiguration(Unit unit, int modelCount, ExperienceLevelData experienceLevel, Dictionary<string, int> selectedWeapons, bool unitUpgradeSelected, bool weaponUpgradeSelected)
    {
        Unit = unit;
        ModelCount = modelCount;
        ExperienceLevel = experienceLevel;
        SelectedWeapons = selectedWeapons;
        UnitUpgradeSelected = unitUpgradeSelected;
        WeaponUpgradeSelected = weaponUpgradeSelected;

        // Рассчитываем полную стоимость юнита в зависимости от выбранных параметров
        TotalCost = unit.CalculateCost(modelCount, selectedWeapons, experienceLevel, unitUpgradeSelected, weaponUpgradeSelected);
    }
}
