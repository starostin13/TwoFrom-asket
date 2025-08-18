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


            string? userFile = args.FirstOrDefault(a => !a.StartsWith("-"));
            string targetFileName = string.IsNullOrWhiteSpace(userFile) ? "BlackTemplars.json" : userFile;

            // Парсим аргумент очков: --points=2000 или -p=2000 (по умолчанию 2000)
            int maxPoints = 2000;
            var pointsArg = args.FirstOrDefault(a => a.StartsWith("--points=") || a.StartsWith("-p="));
            if (pointsArg != null)
            {
                var valuePart = pointsArg.Split('=', 2).Last();
                if (int.TryParse(valuePart, out int parsed) && parsed > 0)
                {
                    maxPoints = parsed;
                }
            }

            // Фильтр по тегу: --tag=E или -t=E (один тег). Если указан, оставляем только юниты содержащие этот тег
            string? tagFilter = null;
            var tagArg = args.FirstOrDefault(a => a.StartsWith("--tag=") || a.StartsWith("-t="));
            if (tagArg != null)
            {
                tagFilter = tagArg.Split('=', 2).Last().Trim();
                if (string.IsNullOrWhiteSpace(tagFilter)) tagFilter = null;
            }

            // Если пользователь передал полный путь, используем его напрямую
            string? selected = null;
            if (userFile != null && File.Exists(userFile))
            {
                selected = userFile;
            }
            else
            {
                string[] candidates = new[]
                {
                    Path.Combine("Data", targetFileName),
                    Path.Combine("ConsoleApp","Data", targetFileName),
                    Path.Combine(AppContext.BaseDirectory, "Data", targetFileName),
                    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "ConsoleApp", "Data", targetFileName))
                };

                selected = candidates.FirstOrDefault(File.Exists);
                if (selected == null)
                {
                    Console.Error.WriteLine($"Файл {targetFileName} не найден в папке Data. Проверьте расположение.");
                    return;
                }
            }

            Console.WriteLine($"Загружаю данные из: {selected}");

            GameData? gameData = LoadGameDataFromJson(selected);
            if (gameData == null)
            {
                Console.WriteLine(args.Length > 0 ? args[0] : "Не удалось загрузить данные");
                return;
            }

            // Предполагаем, что новые файлы данных уже используют Corps; legacy поля игнорируются если Corps присутствует.

            List<Unit> units = gameData.Units;
            if (tagFilter != null)
            {
                units = units.Where(u => u.Tags != null && u.Tags.Contains(tagFilter, StringComparer.OrdinalIgnoreCase)).ToList();
                if (units.Count == 0)
                {
                    Console.WriteLine($"Нет юнитов с тегом '{tagFilter}'.");
                    return;
                }
            }
            List<Detach> detaches = gameData.Detaches;

            // maxPoints уже получен из аргументов
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
                    var corpsLabel = unitConfig.SelectedCorps?.Name != null ? $" ({unitConfig.SelectedCorps.Name})" : string.Empty;
                    Console.Write($"{unitConfig.Unit.Name}{corpsLabel} Опыт: {unitConfig.ExperienceLevel.Level}, Модели: {unitConfig.ModelCount}, ");
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
