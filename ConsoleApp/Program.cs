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
    public class Upgrade
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
    List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, Dictionary<string, int>, bool)> currentRoster, // Изменено
    List<List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, Dictionary<string, int>, bool)>> allRosters, // Изменено
    Dictionary<string, int> selectedUnitUpgrades) // Добавлено для обработки апгрейдов юнита
        {
            // Проверяем, есть ли у юнита оружие
            if (weapons == null || weapons.Count == 0)
            {
                // Если оружия нет, добавляем текущий состав (без оружия)
                int cost = unit.CalculateCost(modelCount, selectedWeapons, experienceLevel, selectedUnitUpgrades, false); // Изменено
                if (cost <= maxPoints && currentPoints + cost <= maxPoints)
                {
                    currentRoster.Add((unit, modelCount, new Dictionary<string, int>(selectedWeapons), experienceLevel, new Dictionary<string, int>(selectedUnitUpgrades), false)); // Изменено
                }
                return;
            }

            // Если индекс оружия превышает количество оружия, добавляем конфигурацию в текущий состав
            if (weaponIndex >= weapons.Count)
            {
                int cost = unit.CalculateCost(modelCount, selectedWeapons, experienceLevel, selectedUnitUpgrades, false); // Изменено
                if (cost <= maxPoints && currentPoints + cost <= maxPoints)
                {
                    currentRoster.Add((unit, modelCount, new Dictionary<string, int>(selectedWeapons), experienceLevel, new Dictionary<string, int>(selectedUnitUpgrades), false)); // Изменено
                }
                return;
            }

            var currentWeapon = weapons[weaponIndex]; // Текущее оружие

            // Генерация всех возможных количеств для текущего оружия
            for (int weaponCount = currentWeapon.MinCount; weaponCount <= currentWeapon.MaxCount; weaponCount++)
            {
                selectedWeapons[currentWeapon.Name] = weaponCount;

                // Генерация всех возможных апгрейдов для текущего оружия (если они есть)
                if (currentWeapon.Upgrades != null && currentWeapon.Upgrades.Count > 0)
                {
                    foreach (var upgrade in currentWeapon.Upgrades)
                    {
                        // Генерация всех возможных комбинаций с апгрейдом оружия
                        for (int upgradeCount = upgrade.MinCount; upgradeCount <= upgrade.MaxCount; upgradeCount++)
                        {
                            if (upgradeCount > 0)
                            {
                                selectedWeapons[upgrade.Name] = upgradeCount; // Устанавливаем количество апгрейдов
                            }

                            // Рекурсивный вызов для следующего типа оружия с выбранными апгрейдами
                            GenerateWeaponCombinations(weapons, selectedWeapons, weaponIndex + 1, unit, modelCount, experienceLevel, currentPoints, maxPoints, currentRoster, allRosters, selectedUnitUpgrades);

                            // Удаляем апгрейд после использования, чтобы не оставался в других комбинациях
                            if (selectedWeapons.ContainsKey(upgrade.Name))
                            {
                                selectedWeapons.Remove(upgrade.Name);
                            }
                        }
                    }
                }

                // Рекурсивный вызов для следующего типа оружия без апгрейдов
                GenerateWeaponCombinations(weapons, selectedWeapons, weaponIndex + 1, unit, modelCount, experienceLevel, currentPoints, maxPoints, currentRoster, allRosters, selectedUnitUpgrades);

                // Удаляем текущее оружие из выбора после обработки
                selectedWeapons.Remove(currentWeapon.Name);
            }
        }



        static void Main(string[] args)
        {
            // Загрузка юнитов из JSON файла
            List<Unit> units = LoadUnitsFromJson("units.json");

            // Максимальная стоимость для отрядов
            int maxPoints = 382;

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
                    if (unitConfig.SelectedUnitUpgrades != null)
                    {
                        Console.Write($"Улучшение юнита: {string.Join(", ", unitConfig.SelectedUnitUpgrades.Select(w => $"{w.Key} x{w.Value}"))}, ");
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
