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

        static GameData? LoadMultipleGameData(string[] fileNames)
        {
            var gameDatas = new List<GameData>();
            
            foreach (string fileName in fileNames)
            {
                string[] candidates = new[]
                {
                    fileName,
                    Path.Combine("ConsoleApp", fileName),
                    Path.Combine("ConsoleApp","Data", fileName),
                    Path.Combine(AppContext.BaseDirectory, fileName),
                    Path.Combine(AppContext.BaseDirectory, "Data", fileName),
                    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "ConsoleApp", fileName)),
                    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "ConsoleApp", "Data", fileName))
                };

                string? selected = candidates.FirstOrDefault(File.Exists);
                if (selected == null)
                {
                    Console.Error.WriteLine($"{fileName} not found. Checked:\n" + string.Join("\n", candidates));
                    return null;
                }

                GameData? gameData = LoadGameDataFromJson(selected);
                if (gameData == null)
                {
                    Console.Error.WriteLine($"Не удалось загрузить данные из {fileName}");
                    return null;
                }
                
                gameDatas.Add(gameData);
                Console.WriteLine($"Загружен файл: {fileName} ({gameData.Units.Count} юнитов, {gameData.Detachments.Count} detachments)");
            }
            
            return GameData.Merge(gameDatas.ToArray());
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            // Парсим аргументы командной строки
            var factionFiles = new List<string>();
            var requiredTags = new List<string>();
            int maxPoints = 500;
            
            if (args.Length == 0)
            {
                // Значение по умолчанию
                factionFiles.Add("ChaosDaemons - Nurgle.json");
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "--tags" && i + 1 < args.Length)
                    {
                        // --tags E,M,L  или  --tags E  (можно повторять)
                        i++;
                        requiredTags.AddRange(args[i].Split(',', StringSplitOptions.RemoveEmptyEntries));
                    }
                    else if (int.TryParse(args[i], out int parsedPoints))
                    {
                        maxPoints = parsedPoints;
                    }
                    else
                    {
                        factionFiles.Add(args[i]);
                    }
                }
            }

            // Если файлы не указаны, используем значение по умолчанию
            if (factionFiles.Count == 0)
            {
                factionFiles.Add("ChaosDaemons - Nurgle.json");
            }

            Console.WriteLine($"Загружаем фракции: {string.Join(", ", factionFiles)}");
            Console.WriteLine($"Максимальные очки: {maxPoints}");

            GameData? gameData = LoadMultipleGameData(factionFiles.ToArray());
            if (gameData == null)
            {
                Console.WriteLine("Не удалось загрузить данные");
                return;
            }

            Console.WriteLine($"Всего загружено: {gameData.Units.Count} юнитов, {gameData.Detachments.Count} detachments");

            // Фильтрация юнитов по тэгам
            List<Unit> units = requiredTags.Count > 0
                ? gameData.Units.Where(u => u.Tags != null && requiredTags.All(t => u.Tags.Contains(t))).ToList()
                : gameData.Units;

            if (requiredTags.Count > 0)
                Console.WriteLine($"Фильтр тэгов [{string.Join(", ", requiredTags)}]: {units.Count} юнитов");
            List<Detach> detaches = gameData.Detachments;
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
                    Console.Write($"{unitConfig.DisplayName} Опыт: {unitConfig.ExperienceLevel.Level}, Модели: {unitConfig.ModelCount}, ");
                    if (unitConfig.SelectedWeapons.Any(w => w.Value > 0))
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
