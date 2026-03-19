using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace UnitRosterGenerator
{
    class Program
    {
        private sealed class ExtractionCliOptions
        {
            public required string PdfPath { get; init; }
            public required string OutputPath { get; init; }
            public int FromPage { get; init; }
        }

        private sealed class MatchCliOptions
        {
            public required string DraftJsonPath { get; init; }
            public required string BaselineJsonPath { get; init; }
            public required string OutputPath { get; init; }
        }

        private sealed class TransformCliOptions
        {
            public required string DraftJsonPath { get; init; }
            public required string OutputPath { get; init; }
        }

        private sealed class EnhanceWeaponsCliOptions
        {
            public required string DraftJsonPath { get; init; }
            public required string GameDataJsonPath { get; init; }
        }

        static GameData? LoadGameDataFromJson(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<GameData>(json);
        }

        private static ExtractionCliOptions? TryParseExtractionArgs(string[] args)
        {
            if (!args.Contains("--extract-bolt-pdf", StringComparer.OrdinalIgnoreCase))
            {
                return null;
            }

            var pdfPath = string.Empty;
            var outputPath = Path.Combine("ConsoleApp", "Data", "BoltAction", "USA.extracted.draft.json");
            var fromPage = 28;

            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--extract-bolt-pdf":
                        if (i + 1 >= args.Length)
                        {
                            throw new ArgumentException("Expected PDF path after --extract-bolt-pdf");
                        }

                        pdfPath = args[++i];
                        break;
                    case "--from-page":
                        if (i + 1 >= args.Length || !int.TryParse(args[++i], out fromPage))
                        {
                            throw new ArgumentException("Expected integer value after --from-page");
                        }

                        break;
                    case "--out":
                        if (i + 1 >= args.Length)
                        {
                            throw new ArgumentException("Expected output path after --out");
                        }

                        outputPath = args[++i];
                        break;
                }
            }

            if (string.IsNullOrWhiteSpace(pdfPath))
            {
                throw new ArgumentException("PDF path is required when using --extract-bolt-pdf");
            }

            return new ExtractionCliOptions
            {
                PdfPath = pdfPath,
                OutputPath = outputPath,
                FromPage = fromPage
            };
        }

        private static MatchCliOptions? TryParseMatchArgs(string[] args)
        {
            if (!args.Contains("--match-bolt-draft", StringComparer.OrdinalIgnoreCase))
            {
                return null;
            }

            string draftJsonPath = string.Empty;
            var baselineJsonPath = Path.Combine("ConsoleApp", "Data", "BoltAction", "USA.json");
            var outputPath = Path.Combine("ConsoleApp", "Data", "BoltAction", "USA.extracted.match-report.json");

            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--match-bolt-draft":
                        if (i + 1 >= args.Length)
                        {
                            throw new ArgumentException("Expected draft JSON path after --match-bolt-draft");
                        }

                        draftJsonPath = args[++i];
                        break;
                    case "--baseline-json":
                        if (i + 1 >= args.Length)
                        {
                            throw new ArgumentException("Expected baseline JSON path after --baseline-json");
                        }

                        baselineJsonPath = args[++i];
                        break;
                    case "--out":
                        if (i + 1 >= args.Length)
                        {
                            throw new ArgumentException("Expected output path after --out");
                        }

                        outputPath = args[++i];
                        break;
                }
            }

            if (string.IsNullOrWhiteSpace(draftJsonPath))
            {
                throw new ArgumentException("Draft JSON path is required when using --match-bolt-draft");
            }

            return new MatchCliOptions
            {
                DraftJsonPath = draftJsonPath,
                BaselineJsonPath = baselineJsonPath,
                OutputPath = outputPath
            };
        }

        private static TransformCliOptions? TryParseTransformArgs(string[] args)
        {
            if (!args.Contains("--transform-draft-to-full", StringComparer.OrdinalIgnoreCase))
            {
                return null;
            }

            string draftJsonPath = string.Empty;
            var outputPath = Path.Combine("ConsoleApp", "Data", "BoltAction", "USA.json");

            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--transform-draft-to-full":
                        if (i + 1 >= args.Length)
                        {
                            throw new ArgumentException("Expected draft JSON path after --transform-draft-to-full");
                        }

                        draftJsonPath = args[++i];
                        break;
                    case "--out":
                        if (i + 1 >= args.Length)
                        {
                            throw new ArgumentException("Expected output path after --out");
                        }

                        outputPath = args[++i];
                        break;
                }
            }

            if (string.IsNullOrWhiteSpace(draftJsonPath))
            {
                throw new ArgumentException("Draft JSON path is required when using --transform-draft-to-full");
            }

            return new TransformCliOptions
            {
                DraftJsonPath = draftJsonPath,
                OutputPath = outputPath
            };
        }

        private static int RunPdfExtractionMode(ExtractionCliOptions options)
        {
            Console.WriteLine($"[extract] Source PDF: {options.PdfPath}");
            Console.WriteLine($"[extract] Start page: {options.FromPage}");

            var result = BoltActionPdfExtractor.ExtractDraft(options.PdfPath, options.FromPage);
            WriteJson(options.OutputPath, result);

            Console.WriteLine($"[extract] Pages parsed: {result.Pages.Count}");
            Console.WriteLine($"[extract] Candidate unit entries: {result.CandidateUnits.Count}");
            Console.WriteLine($"[extract] Draft saved to: {options.OutputPath}");
            Console.WriteLine("[extract] This is a draft extraction report (no overwrite of USA.json).");

            return 0;
        }

        private static int RunDraftMatchMode(MatchCliOptions options)
        {
            Console.WriteLine($"[match] Draft JSON: {options.DraftJsonPath}");
            Console.WriteLine($"[match] Baseline JSON: {options.BaselineJsonPath}");

            var draftJson = File.ReadAllText(options.DraftJsonPath);
            var draft = JsonSerializer.Deserialize<BoltActionPdfExtractionResult>(draftJson);
            if (draft == null)
            {
                throw new InvalidOperationException("Failed to deserialize draft extraction JSON");
            }

            var baseline = LoadGameDataFromJson(options.BaselineJsonPath);
            if (baseline == null)
            {
                throw new InvalidOperationException("Failed to deserialize baseline USA.json");
            }

            var report = BoltActionDraftMatcher.BuildReport(draft, baseline.Units);
            WriteJson(options.OutputPath, report);

            var matched = report.UnitMatches.Count(m => m.CandidateName != null);
            Console.WriteLine($"[match] Baseline units: {report.BaselineUnits}");
            Console.WriteLine($"[match] Matched units: {matched}");
            Console.WriteLine($"[match] Unmatched candidates: {report.UnmatchedCandidates.Count}");
            Console.WriteLine($"[match] Report saved to: {options.OutputPath}");

            return 0;
        }

        private static int RunTransformMode(TransformCliOptions options)
        {
            Console.WriteLine($"[transform] Draft JSON: {options.DraftJsonPath}");
            Console.WriteLine($"[transform] Output: {options.OutputPath}");

            var draftJson = File.ReadAllText(options.DraftJsonPath);
            var draft = JsonSerializer.Deserialize<BoltActionPdfExtractionResult>(draftJson);
            if (draft == null)
            {
                throw new InvalidOperationException("Failed to deserialize draft extraction JSON");
            }

            Console.WriteLine($"[transform] Transforming {draft.CandidateUnits.Count} units...");

            var gameData = BoltActionDraftToGameDataTransformer.TransformToGameData(draft);
            BoltActionDraftToGameDataTransformer.CalculateAdditionalModelCosts(gameData);

            WriteJson(options.OutputPath, gameData);

            Console.WriteLine($"[transform] Units transformed: {gameData.Units.Count}");
            Console.WriteLine($"[transform] Total weapons/options extracted: " +
                            $"{gameData.Units.Sum(u => u.Weapons.Count)}");
            Console.WriteLine($"[transform] Output saved to: {options.OutputPath}");
            Console.WriteLine("[transform] COMPLETE - USA.json has been replaced with parsed data including weapons!");

            return 0;
        }

        private static EnhanceWeaponsCliOptions? TryParseEnhanceWeaponsArgs(string[] args)
        {
            if (!args.Contains("--enhance-weapons", StringComparer.OrdinalIgnoreCase))
            {
                return null;
            }

            var draftJsonPath = Path.Combine("Data", "BoltAction", "USA.extracted.draft.json");
            var gameDataJsonPath = Path.Combine("Data", "BoltAction", "USA.json");

            for (var i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], "--draft-json", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    draftJsonPath = args[i + 1];
                }
                else if (string.Equals(args[i], "--gamedata-json", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    gameDataJsonPath = args[i + 1];
                }
            }

            return new EnhanceWeaponsCliOptions
            {
                DraftJsonPath = draftJsonPath,
                GameDataJsonPath = gameDataJsonPath
            };
        }

        private static int RunEnhanceWeaponsMode(EnhanceWeaponsCliOptions options)
        {
            Console.WriteLine($"[enhance-weapons] Draft JSON: {options.DraftJsonPath}");
            Console.WriteLine($"[enhance-weapons] Game Data JSON: {options.GameDataJsonPath}");

            var draftJson = File.ReadAllText(options.DraftJsonPath);
            var draft = JsonSerializer.Deserialize<BoltActionPdfExtractionResult>(draftJson, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (draft == null)
            {
                Console.Error.WriteLine("Failed to deserialize draft JSON");
                return -1;
            }

            var gameDataJson = File.ReadAllText(options.GameDataJsonPath);
            var gameData = JsonSerializer.Deserialize<GameData>(gameDataJson);
            
            if (gameData == null)
            {
                Console.Error.WriteLine("Failed to deserialize game data JSON");
                return -1;
            }

            var pageTexts = draft.Pages.ToDictionary(p => p.PageNumber, p => p.Text);
            int weaponsAdded = 0;
            int manualReviewCount = 0;

            foreach (var unit in gameData.Units)
            {
                var candidate = draft.CandidateUnits.FirstOrDefault(c => 
                    c.Name.Equals(unit.Name, StringComparison.OrdinalIgnoreCase) ||
                    c.Name.Contains(unit.Name, StringComparison.OrdinalIgnoreCase) ||
                    unit.Name.Contains(c.Name, StringComparison.OrdinalIgnoreCase));

                if (candidate == null) continue;

                if (pageTexts.TryGetValue(candidate.PageNumber, out var pageText))
                {
                    var weapons = ExtractWeaponsFromPageText(pageText, candidate.Name);
                    
                    if (weapons.Count > 0)
                    {
                        Console.WriteLine($"\n[enhance] {unit.Name} (page {candidate.PageNumber}):");
                        foreach (var weapon in weapons)
                        {
                            if (!unit.Weapons.Any(w => w.Name == weapon.Name))
                            {
                                unit.Weapons.Add(weapon);
                                Console.WriteLine($"  + {weapon.Name} (cost: {weapon.Cost}, max: {weapon.MaxCount})");
                                weaponsAdded++;
                                
                                if (weapon.Name.Contains("MANUAL_REVIEW"))
                                    manualReviewCount++;
                            }
                        }
                    }
                }
            }

            WriteJson(options.GameDataJsonPath, gameData);

            Console.WriteLine($"\n[enhance-weapons] COMPLETE");
            Console.WriteLine($"  Weapons/upgrades added: {weaponsAdded}");
            Console.WriteLine($"  Items needing manual review: {manualReviewCount}");
            Console.WriteLine($"  File saved to: {options.GameDataJsonPath}");

            return 0;
        }

        private static List<Weapon> ExtractWeaponsFromPageText(string pageText, string unitName)
        {
            var weapons = new List<Weapon>();

            // Ищем секцию Options
            var optionsMatch = System.Text.RegularExpressions.Regex.Match(pageText, @"Options(.*?)(?:Special Rules|$)", 
                System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (!optionsMatch.Success)
                return weapons;

            var optionsText = optionsMatch.Groups[1].Value;

            // Паттерны для парсинга
            var patterns = new[]
            {
                new { Regex = @"replace.*?with\s+(?:an?\s+)?(.+?)\s+for\s+\+(\d+)pts", WeaponIdx = 1, CostIdx = 2 },
                new { Regex = @"with\s+(?:an?\s+)?(.+?)\s+for\s+\+(\d+)pts\s+each", WeaponIdx = 1, CostIdx = 2 },
                new { Regex = @"given\s+(.+?)\s+for\s+\+(\d+)pts\s+per", WeaponIdx = 1, CostIdx = 2 },
            };

            foreach (var pattern in patterns)
            {
                var matches = System.Text.RegularExpressions.Regex.Matches(optionsText, pattern.Regex, 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    if (match.Success && match.Groups.Count >= 3)
                    {
                        var weaponName = CleanAndNormalizeWeaponName(match.Groups[1].Value);
                        if (string.IsNullOrWhiteSpace(weaponName)) continue;

                        if (int.TryParse(match.Groups[2].Value, out var cost))
                        {
                            var (minCount, maxCount) = ExtractWeaponCountLimits(optionsText, weaponName);
                            
                            if (!weapons.Any(w => w.Name.Equals(weaponName, StringComparison.OrdinalIgnoreCase)))
                            {
                                weapons.Add(new Weapon 
                                { 
                                    Name = weaponName, 
                                    Cost = cost, 
                                    MinCount = minCount, 
                                    MaxCount = maxCount 
                                });
                            }
                        }
                    }
                }
            }

            // Сложные случаи для ручной обработки
            var complexItems = new[]
            {
                "Intelligence training", "Demolition charge", "Flamethrower", 
                "Motorcycles", "Horses", "Gun shield", "Spotter"
            };

            foreach (var item in complexItems)
            {
                if (optionsText.IndexOf(item, StringComparison.OrdinalIgnoreCase) >= 0 && 
                    !weapons.Any(w => w.Name.Contains(item, StringComparison.OrdinalIgnoreCase)))
                {
                    var costMatch = System.Text.RegularExpressions.Regex.Match(optionsText, 
                        $@"{System.Text.RegularExpressions.Regex.Escape(item)}.*?\+(\d+)", 
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                    
                    var cost = costMatch.Success ? int.Parse(costMatch.Groups[1].Value) : 0;

                    weapons.Add(new Weapon
                    {
                        Name = $"MANUAL_REVIEW: {item}",
                        Cost = cost,
                        MinCount = 0,
                        MaxCount = 1
                    });
                }
            }

            return weapons;
        }

        private static string CleanAndNormalizeWeaponName(string name)
        {
            name = name.Trim();
            name = System.Text.RegularExpressions.Regex.Replace(name, @"[,\.\)].*$", "");
            name = System.Text.RegularExpressions.Regex.Replace(name, @"^(a|an|the|up to|one|two)\s+", "", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // Убираем лишние слова в конце
            name = System.Text.RegularExpressions.Regex.Replace(name, @"\s+(each|at \+\d+pts.*)?$", "", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            var replacements = new Dictionary<string, string>
            {
                { "submachine gun|Thompson|M3|SMG", "SMG" },
                { "automatic rifle|BAR|M1919A6", "BAR" },
                { "light machine gun|LMG", "LMG" },
                { "medium machine gun|MMG", "MMG" },
                { "heavy machine gun|HMG", "HMG" },
                { "anti-tank grenades?|AT grenades?", "AT Grenades" },
                { "shotgun", "Shotgun" },
                { "pistol|carbine", "Pistol" },
                { "rifles?", "Rifle" },
            };

            foreach (var kvp in replacements)
            {
                name = System.Text.RegularExpressions.Regex.Replace(name, kvp.Key, kvp.Value, 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }

            return name;
        }

        private static (int minCount, int maxCount) ExtractWeaponCountLimits(string optionsText, string weaponSearchTerm)
        {
            var searchPattern = System.Text.RegularExpressions.Regex.Escape(weaponSearchTerm);
            
            // Patern: "up to X men may replace their rifle with..."
            var upToMatch = System.Text.RegularExpressions.Regex.Match(optionsText, 
                $@"up to (\d+)\s+men?\s+(?:may|can).*?(?:{searchPattern}|replace|with)", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);

            if (upToMatch.Success && int.TryParse(upToMatch.Groups[1].Value, out var count))
            {
                return (0, count);
            }

            // Pattern: "Add up to X men"
            upToMatch = System.Text.RegularExpressions.Regex.Match(optionsText, 
                $@"add up to (\d+)\s+men?\s+with.*?(?:{searchPattern}|[,\.])", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);

            if (upToMatch.Success && int.TryParse(upToMatch.Groups[1].Value, out count))
            {
                return (0, count);
            }

            // Pattern: "One man may"
            upToMatch = System.Text.RegularExpressions.Regex.Match(optionsText, 
                $@"one\s+man?\s+(?:may|can).*?(?:{searchPattern}|[,\.])", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);

            if (upToMatch.Success)
            {
                return (0, 1);
            }

            return (0, 1);  // Возвращаем 1 по умолчанию, а не 0
        }

        private static void WriteJson<T>(string outputPath, T payload)
        {
            var serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var outputDirectory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrWhiteSpace(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            var json = JsonSerializer.Serialize(payload, serializerOptions);
            File.WriteAllText(outputPath, json, Encoding.UTF8);
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

            var extractionOptions = TryParseExtractionArgs(args);
            if (extractionOptions != null)
            {
                RunPdfExtractionMode(extractionOptions);
                return;
            }

            var matchOptions = TryParseMatchArgs(args);
            if (matchOptions != null)
            {
                RunDraftMatchMode(matchOptions);
                return;
            }

            var transformOptions = TryParseTransformArgs(args);
            if (transformOptions != null)
            {
                RunTransformMode(transformOptions);
                return;
            }

            var enhanceWeaponsOptions = TryParseEnhanceWeaponsArgs(args);
            if (enhanceWeaponsOptions != null)
            {
                RunEnhanceWeaponsMode(enhanceWeaponsOptions);
                return;
            }

            // Парсим аргументы командной строки
            var factionFiles = new List<string>();
            int maxPoints = 500;
            
            if (args.Length == 0)
            {
                // Значение по умолчанию
                factionFiles.Add("ChaosDaemons - Nurgle.json");
            }
            else
            {
                // Ищем аргумент с очками
                for (int i = 0; i < args.Length; i++)
                {
                    if (int.TryParse(args[i], out int parsedPoints))
                    {
                        maxPoints = parsedPoints;
                    }
                    else
                    {
                        // Это файл фракции
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

            List<Unit> units = gameData.Units;
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
