using System.Text.Json;

namespace UnitRosterGenerator
{
    class Program
    {
        static GameData LoadGameDataFromJson(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<GameData>(json);
        }

        static void Main(string[] args)
        {
            GameData gameData = LoadGameDataFromJson("USA-BoltAction.json");
            List<Unit> units = gameData.Units;
            List<Detach> detaches = gameData.Detaches;

            int maxPoints = 561;
            List<Roster> allRosters = new List<Roster>();

            for (int i = 0; i < 100; i++)
            {
                var roster = RandomRosterBuilder.BuildRandomRoster(units, detaches, maxPoints);
                allRosters.Add(roster);
            }

            var topRosters = allRosters
                .Select(roster => new
                {
                    Roster = roster,
                    TotalCost = roster.CalculateTotalCost()
                })
                .OrderByDescending(r => r.TotalCost)
                .Take(5)
                .ToList();

            foreach (var r in topRosters)
            {
                Console.WriteLine($"Общая стоимость ростера: {r.TotalCost}");

                if (r.Roster.SelectedDetach != null)
                {
                    Console.WriteLine($"Выбранный детач: {r.Roster.SelectedDetach.Name}");
                }

                foreach (var unitConfig in r.Roster.UnitConfigurations)
                {
                    Console.Write($"{unitConfig.Unit.Name} (Опыт: {unitConfig.ExperienceLevel.Level}, Модели: {unitConfig.ModelCount}, ");
                    if (unitConfig.SelectedWeapons.Count > 0)
                    {
                        Console.Write($"Оружие: {string.Join(", ", unitConfig.SelectedWeapons.Where(weapon => weapon.Value > 0).Select(w => $"{w.Key} x{w.Value}"))}, ");
                    }
                    if (unitConfig.SelectedUpgrades.Count > 0)
                    {
                        Console.Write($"Апгрейды: {string.Join(", ", unitConfig.SelectedUpgrades.Where(upgrd => upgrd.Value > 0).Select(u => $"{u.Key} x{u.Value}"))}, ");
                    }
                    Console.WriteLine($"Стоимость: {unitConfig.TotalCost}");
                }

                Console.WriteLine("-----");
            }

            Console.ReadKey();
        }
    }
}
