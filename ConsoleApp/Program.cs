using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace UnitRosterGenerator
{
    //// Категория юнита: неопытный, регулярный или ветеран
    //enum ExperienceLevel
    //{
    //    Inexperienced,
    //    Regular,
    //    Veteran
    //}


    // Класс, представляющий уровень опыта юнита
    class ExperienceLevelData
    {
        public string Level { get; set; }
        public int BaseCost { get; set; }
        public int AdditionalModelCost { get; set; }
        public Upgrade? Upgrades { get; set; } // Можно также использовать List<Upgrade>
    }


    // Класс, представляющий оружие
    class Weapon
    {
        public required string Name { get; set; }
        public int Cost { get; set; }
        public int MinCount { get; set; }
        public int MaxCount { get; set; }
        public List<Upgrade>? Upgrades { get; set; }
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

            return units;
        }

        // Метод для генерации всех возможных комбинаций юнитов
        static void GenerateAllRosters(
    List<Unit> availableUnits,
    int maxPoints,
    List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, bool, bool)> currentRoster,
    int currentPoints,
    List<List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, bool, bool)>> allRosters)
        {
            // Добавляем текущий ростер к списку возможных ростеров, если он подходит по очкам
            if (currentPoints <= maxPoints)
            {
                allRosters.Add(new List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, bool, bool)>(currentRoster));
            }

            // Если текущий ростер уже превысил лимит по очкам, дальнейший перебор не нужен
            if (currentPoints > maxPoints)
                return;

            // Проходим по всем доступным юнитам
            foreach (var unit in availableUnits)
            {
                // Для каждого юнита перебираем уровни опыта
                foreach (var experienceLevel in unit.Experience)
                {
                    // Перебираем все возможные варианты количества моделей для текущего юнита
                    for (int modelCount = unit.MinModels; modelCount <= unit.MaxModels; modelCount++)
                    {
                        // Перебираем все возможные комбинации дополнительного вооружения
                        var selectedWeapons = new Dictionary<string, int>();
                        bool upgradeSelected = unit.Upgrades != null && unit.Upgrades.MinCount > 0;

                        // Копия текущего ростера для добавления новых вариантов
                        var currentRosterCopy = new List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, bool, bool)>(currentRoster);

                        // Генерация всех возможных комбинаций оружия и добавление в ростер
                        GenerateWeaponCombinations(
                            unit.Weapons,
                            selectedWeapons,
                            0,
                            unit,
                            modelCount,
                            experienceLevel,
                            currentPoints,
                            maxPoints,
                            currentRosterCopy,
                            allRosters,
                            upgradeSelected);
                    }
                }
            }
        }


        // Рекурсивный метод для генерации всех возможных комбинаций оружия
        static void GenerateWeaponCombinations(
    List<Weapon> weapons,
    Dictionary<string, int> selectedWeapons,
    int weaponIndex,
    Unit unit,
    int modelCount,
    ExperienceLevelData experienceLevel,
    int currentPoints,
    int maxPoints,
    List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, bool, bool)> currentRoster,
    List<List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, bool, bool)>> allRosters,
    bool upgradeSelected)
        {
            // Проверяем, есть ли у юнита оружие
            if (weapons == null || weapons.Count == 0)
            {
                // Если оружия нет, добавляем текущий состав (без оружия)
                int cost = unit.CalculateCost(modelCount, selectedWeapons, experienceLevel, upgradeSelected, false);
                if (cost <= maxPoints && currentPoints + cost <= maxPoints)
                {
                    currentRoster.Add((unit, modelCount, new Dictionary<string, int>(selectedWeapons), experienceLevel, upgradeSelected, false));
                }
                return;
            }

            if (weaponIndex >= weapons.Count)
            {
                // Проверка всех вариантов с выбранным оружием и без апгрейдов
                int cost = unit.CalculateCost(modelCount, selectedWeapons, experienceLevel, upgradeSelected, false);
                if (cost <= maxPoints && currentPoints + cost <= maxPoints)
                {
                    currentRoster.Add((unit, modelCount, new Dictionary<string, int>(selectedWeapons), experienceLevel, upgradeSelected, false));
                }
                return;
            }

            var currentWeapon = weapons[weaponIndex]; // Изменено имя переменной

            // Генерация всех возможных количеств для текущего оружия
            for (int weaponCount = currentWeapon.MinCount; weaponCount <= currentWeapon.MaxCount; weaponCount++)
            {
                selectedWeapons[currentWeapon.Name] = weaponCount;

                // Генерация всех возможных апгрейдов для текущего оружия (если они есть)
                if (currentWeapon.Upgrades != null)
                {
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
                }

                // Рекурсивный вызов без выбора улучшений для текущего оружия
                GenerateWeaponCombinations(weapons, selectedWeapons, weaponIndex + 1, unit, modelCount, experienceLevel, currentPoints, maxPoints, currentRoster, allRosters, upgradeSelected);

                // Удаляем текущее оружие из выбора
                selectedWeapons.Remove(currentWeapon.Name);
            }
        }



        static void Main(string[] args)
        {
            // Загрузка юнитов из JSON файла
            List<Unit> units = LoadUnitsFromJson("units.json");

            // Максимальная стоимость для отрядов
            int maxPoints = 1000;

            List<List<UnitConfiguration>> allRosters = new List<List<UnitConfiguration>>();

            // Вызываем метод для генерации случайного ростера
            RandomRosterBuilder.BuildRandomRoster(units, maxPoints, new List<UnitConfiguration>(), allRosters);

            // Выводим результаты
            foreach (var roster in allRosters)
            {
                int totalRosterCost = 0; // Переменная для хранения общей стоимости ростера
                foreach (var unitConfig in roster)
                {
                    Console.WriteLine($"{unitConfig.Unit.Name} (Опыт: {unitConfig.ExperienceLevel.Level}, Модели: {unitConfig.ModelCount}, Оружие: {string.Join(", ", unitConfig.SelectedWeapons.Select(w => $"{w.Key} x{w.Value}"))}, Улучшение юнита: {unitConfig.UnitUpgradeSelected}, Улучшение оружия: {unitConfig.WeaponUpgradeSelected}, Общая стоимость: {unitConfig.TotalCost})");
                    // Добавляем стоимость данного юнита к общей стоимости ростера
                    totalRosterCost += unitConfig.TotalCost;
                }
                Console.WriteLine($"Общая стоимость ростера: {totalRosterCost}");
                Console.WriteLine("-----");
            }

            Console.ReadKey();
        }
    }
}
