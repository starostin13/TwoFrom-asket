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
            for(int i=0; i<100;i++)
            {
                var list = new List<UnitConfiguration>();
                // Вызываем метод для генерации случайного ростера
                RandomRosterBuilder.BuildRandomRoster(units, maxPoints, list, allRosters);
            }

            // Выводим результаты
            foreach (var r in allRosters.Select(roster => new
            {
                Roster = roster,
                TotalCost = roster.Sum(unitConfig => unitConfig.TotalCost) // Суммируем стоимость всех юнитов в ростере
            })
    .OrderByDescending(r => r.TotalCost) // Сортируем ростеры по общей стоимости, от большего к меньшему
    .Take(5).ToList())
            {
                int totalRosterCost = 0; // Переменная для хранения общей стоимости ростера
                foreach (var unitConfig in r.Roster)
                {
                    Console.Write($"{unitConfig.Unit.Name} (Опыт: {unitConfig.ExperienceLevel.Level}, Модели: {unitConfig.ModelCount},");
                    if (unitConfig.SelectedWeapons.Count > 0)
                    {
                        Console.Write($"Оружие: {string.Join(", ", unitConfig.SelectedWeapons.Select(w => $"{w.Key} x{w.Value}"))},");

                        if(unitConfig.WeaponUpgradeSelected)
                        {
                            Console.Write($"Улучшение оружия: {unitConfig.WeaponUpgradeSelected}, ");
                        }
                    }
                    if (unitConfig.UnitUpgradeSelected)
                    {
                        Console.Write($"Улучшение юнита: {unitConfig.UnitUpgradeSelected}, ");
                    }

                    Console.Write($"Общая стоимость: {unitConfig.TotalCost}){Environment.NewLine}");
                    
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
