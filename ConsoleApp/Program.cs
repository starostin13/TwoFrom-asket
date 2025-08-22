using System.Text.Json;
using System.Text;

namespace UnitRosterGenerator
{
    class Program
    {
        static GameData? LoadGameDataFromJson(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<GameData>(json);
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            string[] candidates = new[]
            {
                // Relative from repo root when running via solution
                Path.Combine("ConsoleApp", "Data", "AELDARI.json"),
                // Relative from project directory (working dir)
                Path.Combine("Data", "AELDARI.json"),
                // In output/bin folder after build copy
                Path.Combine(AppContext.BaseDirectory, "Data", "AELDARI.json"),
                // Fallback: navigate up from bin/Debug/netX to project Data
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "ConsoleApp", "Data", "AELDARI.json"))
            };

            string? selected = candidates.FirstOrDefault(File.Exists);
            if (selected == null)
            {
                Console.Error.WriteLine("\\Data\\AELDARI.json not found. Checked:\n" + string.Join("\n", candidates));
                return;
            }

            GameData? gameData = LoadGameDataFromJson(selected);
            if (gameData == null)
            {
                Console.WriteLine(args.Length > 0 ? args[0] : "Не удалось загрузить данные");
                return;
            }

            List<Unit> units = gameData.Units;
            List<Detach> detaches = gameData.Detaches;

            int maxPoints = 500;
            List<Roster> allRosters = [];

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
                    Console.Write($"{unitConfig.Unit.Name} Опыт: {unitConfig.ExperienceLevel.Level}, Модели: {unitConfig.ModelCount}, ");
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

            // Не блокируем CI / пайпы если вывод перенаправлен
            if (!Console.IsInputRedirected && Environment.GetEnvironmentVariable("CI") == null)
            {
                Console.ReadKey();
            }
        }
    }
}
