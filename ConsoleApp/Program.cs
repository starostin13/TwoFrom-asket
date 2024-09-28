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
    }

    // Класс дополнительного вооружения
    class Weapon
    {
        public string Name { get; set; }
        public int Cost { get; set; }
        public int MinCount { get; set; }
        public int MaxCount { get; set; }
    }

    // Класс улучшения
    class Upgrade
    {
        public string Name { get; set; }
        public int Cost { get; set; }
        public int MinCount { get; set; }
        public int MaxCount { get; set; }
    }

    // Класс юнита
    class Unit
    {
        public string Name { get; set; }
        public int MinModels { get; set; }
        public int MaxModels { get; set; }
        public List<ExperienceLevelData> ExperienceLevels { get; set; }
        public List<Weapon> Weapons { get; set; }
        public Upgrade Upgrade { get; set; }

        // Метод для расчета стоимости юнита с учетом количества моделей, уровня опыта, выбранного вооружения и улучшений
        public int CalculateCost(int modelCount, Dictionary<string, int> selectedWeapons, ExperienceLevelData experienceLevel, bool upgradeSelected)
        {
            // Базовая стоимость за минимальное количество моделей
            int totalCost = experienceLevel.BaseCost;

            // Добавляем стоимость за дополнительные модели
            if (modelCount > MinModels)
            {
                totalCost += (modelCount - MinModels) * experienceLevel.AdditionalModelCost;
            }

            // Добавляем стоимость за дополнительное вооружение
            foreach (var weapon in Weapons)
            {
                if (selectedWeapons.ContainsKey(weapon.Name))
                {
                    int weaponCount = selectedWeapons[weapon.Name];
                    totalCost += weaponCount * weapon.Cost;
                }
            }

            // Добавляем стоимость за улучшение
            if (upgradeSelected)
            {
                totalCost += modelCount * Upgrade.Cost;
            }

            return totalCost;
        }

        public override string ToString()
        {
            return $"{Name}, Модели: {MinModels}-{MaxModels}, Опыт: {string.Join(", ", ExperienceLevels.Select(e => $"{e.Level} (Base: {e.BaseCost}, Extra: {e.AdditionalModelCost})"))}";
        }
    }

    class Program
    {
        // Метод для загрузки юнитов из JSON файла
        static List<Unit> LoadUnitsFromJson(string filePath)
        {
            string json = File.ReadAllText(filePath);
            var units = JsonSerializer.Deserialize<List<Unit>>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

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
        static void GenerateAllRosters(List<Unit> availableUnits, int maxPoints, List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, bool)> currentRoster, int currentPoints, List<List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, bool)>> allRosters)
        {
            if (currentPoints > maxPoints)
                return;

            allRosters.Add(new List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, bool)>(currentRoster));

            foreach (var unit in availableUnits)
            {
                foreach (var experienceLevel in unit.ExperienceLevels)
                {
                    // Перебираем все возможные варианты количества моделей для текущего юнита
                    for (int modelCount = unit.MinModels; modelCount <= unit.MaxModels; modelCount++)
                    {
                        // Перебираем все возможные комбинации дополнительного вооружения
                        var selectedWeapons = new Dictionary<string, int>();
                        GenerateWeaponCombinations(unit.Weapons, selectedWeapons, 0, unit, modelCount, experienceLevel, currentPoints, maxPoints, currentRoster, allRosters);
                    }
                }
            }
        }

        // Метод генерации всех возможных комбинаций дополнительного вооружения
        static void GenerateWeaponCombinations(List<Weapon> weapons, Dictionary<string, int> selectedWeapons, int weaponIndex, Unit unit, int modelCount, ExperienceLevelData experienceLevel, int currentPoints, int maxPoints, List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, bool)> currentRoster, List<List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, bool)>> allRosters)
        {
            if (weaponIndex == weapons.Count)
            {
                // Генерация всех возможных вариантов выбора улучшения
                for (bool upgradeSelected = false; upgradeSelected <= true; upgradeSelected = !upgradeSelected)
                {
                    int unitCost = unit.CalculateCost(modelCount, selectedWeapons, experienceLevel, upgradeSelected);
                    if (currentPoints + unitCost <= maxPoints)
                    {
                        currentRoster.Add((unit, modelCount, new Dictionary<string, int>(selectedWeapons), experienceLevel, upgradeSelected));
                        GenerateAllRosters(new List<Unit> { unit }, maxPoints, currentRoster, currentPoints + unitCost, allRosters);
                        currentRoster.RemoveAt(currentRoster.Count - 1);
                    }
                }
                return;
            }

            Weapon weapon = weapons[weaponIndex];
            for (int weaponCount = weapon.MinCount; weaponCount <= weapon.MaxCount; weaponCount++)
            {
                selectedWeapons[weapon.Name] = weaponCount;
                GenerateWeaponCombinations(weapons, selectedWeapons, weaponIndex + 1, unit, modelCount, experienceLevel, currentPoints, maxPoints, currentRoster, allRosters);
            }
        }

        static void Main(string[] args)
        {
            // Чтение юнитов из JSON файла
            string filePath = "units.json";
            List<Unit> availableUnits = LoadUnitsFromJson(filePath);

            int maxPoints = 1000;

            // Список для хранения всех возможных ростеров
            List<List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, bool)>> allRosters = new List<List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, bool)>>();

            // Генерация всех ростеров
            GenerateAllRosters(availableUnits, maxPoints, new List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, bool)>(), 0, allRosters);

            // Отбираем лучший ростер
            var bestRoster = GetBestRoster(allRosters, maxPoints);
            if (bestRoster != null)
            {
                Console.WriteLine("Лучший ростер:");
                foreach (var (unit, modelCount, selectedWeapons, experienceLevel, upgradeSelected) in bestRoster)
                {
                    Console.WriteLine($"{unit.Name} (Модели: {modelCount}, Уровень: {experienceLevel.Level}, Улучшение: {upgradeSelected})");
                    foreach (var weapon in selectedWeapons)
                    {
                        if (weapon.Value > 0)
                        {
                            Console.WriteLine($" - {weapon.Key}: {weapon.Value}");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Ростер не найден.");
            }
        }

        static List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, bool)> GetBestRoster(List<List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, bool)>> allRosters, int maxPoints)
        {
            // Отбираем только те ростеры, у которых стоимость максимально приближена к maxPoints
            return allRosters.OrderByDescending(roster => roster.Sum(unit => unit.Item1.CalculateCost(unit.Item2, unit.Item3, unit.Item4, unit.Item5)))
                             .FirstOrDefault();
        }
    }
}
