using System.Text.Json;
using System.Linq;
using System;

namespace UnitRosterGenerator
{
    class Program
    {
        // Метод для загрузки данных из JSON файла
        static GameData LoadGameDataFromJson(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<GameData>(json);
        }

        static void Main(string[] args)
        {
            GameData gameData = LoadGameDataFromJson("Tau.json");
            List<Unit> units = gameData.Units;
            List<Detach> detaches = gameData.Detaches;

            int maxPoints = 330;
            List<List<UnitConfiguration>> allRosters = new List<List<UnitConfiguration>>();

            for (int i = 0; i < 100; i++)
            {
                var currentRoster = new List<UnitConfiguration>();
                RandomRosterBuilder.BuildRandomRoster(units, detaches, maxPoints, currentRoster, allRosters);
            }

            var topRosters = allRosters.Select(roster => new
            {
                Roster = roster,
                TotalCost = roster.Sum(unitConfig => unitConfig.TotalCost)
            })
            .OrderByDescending(r => r.TotalCost)
            .Take(5)
            .ToList();

            foreach (var r in topRosters)
            {
                Console.WriteLine($"Общая стоимость ростера: {r.TotalCost}");
                foreach (var unitConfig in r.Roster)
                {
                    Console.Write($"{unitConfig.Unit.Name} (Опыт: {unitConfig.ExperienceLevel.Level}, Модели: {unitConfig.ModelCount}, ");
                    if (unitConfig.SelectedWeapons.Count > 0)
                    {
                        Console.Write($"Оружие: {string.Join(", ", unitConfig.SelectedWeapons.Select(w => $"{w.Key} x{w.Value}"))}, ");
                    }
                    if (unitConfig.SelectedUpgrades.Count > 0)
                    {
                        Console.Write($"Апгрейды: {string.Join(", ", unitConfig.SelectedUpgrades.Select(u => $"{u.Key} x{u.Value}"))}, ");
                    }
                    Console.WriteLine($"Стоимость: {unitConfig.TotalCost}");
                }
                Console.WriteLine("-----");
            }

            Console.ReadKey();
        }
    }
}
