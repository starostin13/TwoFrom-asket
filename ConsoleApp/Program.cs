using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace UnitRosterGenerator
{
    // Категория юнита: неопытный, регулярный или ветеран
    enum ExperienceLevel
    {
        Inexperienced,
        Regular,
        Veteran
    }

    // Класс, представляющий уровень опыта юнита
    class ExperienceLevelData
    {
        public ExperienceLevel Level { get; set; }
        public int BaseCost { get; set; }
        public int AdditionalModelCost { get; set; }
        public Upgrade? Upgrades { get; set; } // Поле для улучшений
    }

    // Класс, представляющий оружие
    class Weapon
    {
        public required string Name { get; set; }
        public int Cost { get; set; }
        public int MinCount { get; set; }
        public int MaxCount { get; set; }
        public List<Upgrade>? Upgrades { get; set; } // Поле для улучшений
    }

    // Класс улучшения
    class Upgrade
    {
        public required string Name { get; set; }
        public int Cost { get; set; }
        public int MinCount { get; set; }
        public int MaxCount { get; set; }
    }

    // Класс несовместимого оружия
    class IncompatibleWeaponGroup
    {
        public required string Group { get; set; }
        public List<string>? WeaponNames { get; set; }
    }

    class Program
    {
        // Метод для загрузки юнитов из JSON файла
        static List<Unit> LoadUnitsFromJson(string filePath)
        {
            string json = File.ReadAllText(filePath);
            var units = JsonSerializer.Deserialize<List<Unit>>(json);

            // Преобразуем строки уровня опыта в enum ExperienceLevel
            foreach (var unit in units)
            {
                foreach (var expLevel in unit.ExperienceLevels)
                {
                    expLevel.Level = Enum.Parse<ExperienceLevel>(expLevel.Level.ToString(), ignoreCase: true);
                }
            }

            return units;
        }

        // Метод для генерации всех возможных комбинаций юнитов
        static void GenerateAllRosters(List<Unit> availableUnits, int maxPoints, List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, bool, bool)> currentRoster, int currentPoints, List<List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, bool, bool)>> allRosters)
        {
            if (currentPoints > maxPoints)
                return;

            allRosters.Add(new List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, bool, bool)>(currentRoster));

            foreach (var unit in availableUnits)
            {
                foreach (var experienceLevel in unit.ExperienceLevels)
                {
                    // Перебираем все возможные варианты количества моделей для текущего юнита
                    for (int modelCount = unit.MinModels; modelCount <= unit.MaxModels; modelCount++)
                    {
                        // Перебираем все возможные комбинации дополнительного вооружения
                        var selectedWeapons = new Dictionary<string, int>();
                        bool upgradeSelected = unit.Upgrades != null && unit.Upgrades.MinCount > 0;

                        // Генерация всех возможных комбинаций оружия
                        GenerateWeaponCombinations(unit.Weapons, selectedWeapons, 0, unit, modelCount, experienceLevel, currentPoints, maxPoints, currentRoster, allRosters, upgradeSelected);
                    }
                }
            }
        }

        // Рекурсивный метод для генерации всех возможных комбинаций оружия
        static void GenerateWeaponCombinations(List<Weapon> weapons, Dictionary<string, int> selectedWeapons, int weaponIndex, Unit unit, int modelCount, ExperienceLevelData experienceLevel, int currentPoints, int maxPoints, List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, bool, bool)> currentRoster, List<List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, bool, bool)>> allRosters, bool upgradeSelected)
        {
            if (weaponIndex >= weapons.Count)
            {
                // Проверка всех вариантов с улучшениями оружия
                bool weaponUpgradeSelected = false;

                foreach (var weapon in weapons)
                {
                    weaponUpgradeSelected = selectedWeapons.ContainsKey(weapon.Name) && weapon.Upgrades.Count > 0;
                    int cost = unit.CalculateCost(modelCount, selectedWeapons, experienceLevel, upgradeSelected, weaponUpgradeSelected);

                    if (cost <= maxPoints && currentPoints + cost <= maxPoints)
                    {
                        currentRoster.Add((unit, modelCount, new Dictionary<string, int>(selectedWeapons), experienceLevel, upgradeSelected, weaponUpgradeSelected));
                    }
                }

                return;
            }

            var currentWeapon = weapons[weaponIndex]; // Изменено имя переменной

            // Генерация всех возможных количеств для текущего оружия
            for (int weaponCount = currentWeapon.MinCount; weaponCount <= currentWeapon.MaxCount; weaponCount++)
            {
                selectedWeapons[currentWeapon.Name] = weaponCount;

                // Генерация всех возможных апгрейдов для текущего оружия
                foreach (var upgrade in currentWeapon.Upgrades)
                {
                    // Проверка минимального количества
                    if (upgrade.MinCount > 0)
                    {
                        selectedWeapons[upgrade.Name] = 1; // выбираем один апгрейд
                    }

                    // Рекурсивный вызов для следующего типа оружия
                    GenerateWeaponCombinations(weapons, selectedWeapons, weaponIndex + 1, unit, modelCount, experienceLevel, currentPoints, maxPoints, currentRoster, allRosters, upgradeSelected);
                }

                // Удаляем текущее оружие из выбора
                selectedWeapons.Remove(currentWeapon.Name);
            }

            // Продолжаем без выбора текущего оружия
            GenerateWeaponCombinations(weapons, selectedWeapons, weaponIndex + 1, unit, modelCount, experienceLevel, currentPoints, maxPoints, currentRoster, allRosters, upgradeSelected);
        }


        static void Main(string[] args)
        {
            // Загрузка юнитов из JSON файла
            List<Unit> units = LoadUnitsFromJson("units.json");

            // Максимальная стоимость для отрядов
            int maxPoints = 1000;

            // Генерация всех возможных списков юнитов
            List<List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, bool, bool)>> allRosters = new List<List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, bool, bool)>>();
            GenerateAllRosters(units, maxPoints, new List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, bool, bool)>(), 0, allRosters);

            // Вывод всех сгенерированных списков юнитов
            foreach (var roster in allRosters)
            {
                foreach (var (unit, modelCount, selectedWeapons, experienceLevel, upgradeSelected, weaponUpgradeSelected) in roster)
                {
                    Console.WriteLine($"{unit.Name} (Опыт: {experienceLevel.Level}, Модели: {modelCount}, Оружие: {string.Join(", ", selectedWeapons.Select(w => $"{w.Key} x{w.Value}"))}, Улучшение юнита: {upgradeSelected}, Улучшение оружия: {weaponUpgradeSelected})");
                }
                Console.WriteLine("-----");
            }
        }
    }
}
