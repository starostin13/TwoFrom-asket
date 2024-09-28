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
        public Upgrade Upgrades { get; set; } // Новое поле для улучшений
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
        public Upgrade Upgrades { get; set; }

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
                totalCost += modelCount * Upgrades.Cost;
            }

            // Добавляем стоимость за правило Tough Fighters, если уровень опыта - Veteran и правило активно
            if (experienceLevel.Level == ExperienceLevel.Veteran && experienceLevel.Upgrades != null)
            {
                totalCost += modelCount; // Увеличение стоимости на 1 за каждую модель
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

            // Удаляем текущее оружие из выбора
            if (selectedWeapons.ContainsKey(weapon.Name))
            {
                selectedWeapons.Remove(weapon.Name);
            }

            // Продолжаем без выбора текущего оружия
            GenerateWeaponCombinations(weapons, selectedWeapons, weaponIndex + 1, unit, modelCount, experienceLevel, currentPoints, maxPoints, currentRoster, allRosters);
        }

        static void Main(string[] args)
        {
            // Загрузка юнитов из JSON файла
            List<Unit> units = LoadUnitsFromJson("units.json");

            // Максимальная стоимость для отрядов
            int maxPoints = 1000;

            // Генерация всех возможных списков юнитов
            List<List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, bool)>> allRosters = new List<List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, bool)>>();
            GenerateAllRosters(units, maxPoints, new List<(Unit, int, Dictionary<string, int>, ExperienceLevelData, bool)>(), 0, allRosters);

            // Вывод всех сгенерированных списков юнитов
            foreach (var roster in allRosters)
            {
                foreach (var (unit, modelCount, selectedWeapons, experienceLevel, upgradeSelected) in roster)
                {
                    Console.WriteLine($"{unit.Name} (Опыт: {experienceLevel.Level}, Модели: {modelCount}, Оружие: {string.Join(", ", selectedWeapons.Select(w => $"{w.Key} x{w.Value}"))}, Улучшение: {upgradeSelected})");
                }
                Console.WriteLine("-----");
            }
        }
    }
}
