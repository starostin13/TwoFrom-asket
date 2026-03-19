using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WeaponParserTool
{
    public class DraftData
    {
        public List<Page> Pages { get; set; }
        public List<CandidateUnit> CandidateUnits { get; set; }
    }

    public class Page
    {
        public int PageNumber { get; set; }
        public string Text { get; set; }
    }

    public class CandidateUnit
    {
        public string Name { get; set; }
        public int PageNumber { get; set; }
    }

    public class GameData
    {
        public List<Unit> Units { get; set; }
    }

    public class Unit
    {
        public string Name { get; set; }
        public int MinModels { get; set; }
        public int MaxModels { get; set; }
        public List<Experience> Experience { get; set; }
        public List<Weapon> Weapons { get; set; }
        public bool DetachUpgrade { get; set; }
    }

    public class Experience
    {
        public string Level { get; set; }
        public int BaseCost { get; set; }
        public int AdditionalModelCost { get; set; }
    }

    public class Weapon
    {
        public string Name { get; set; }
        public int Cost { get; set; }
        public int MinCount { get; set; }
        public int MaxCount { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var draftPath = @"C:\Users\staro\Projects\RosterBuilder\TwoFrom-asket\ConsoleApp\Data\BoltAction\USA.extracted.draft.json";
            var usaPath = @"C:\Users\staro\Projects\RosterBuilder\TwoFrom-asket\ConsoleApp\Data\BoltAction\USA.json";

            var draftJson = File.ReadAllText(draftPath);
            var draftData = JsonSerializer.Deserialize<DraftData>(draftJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var usaJson = File.ReadAllText(usaPath);
            var usaData = JsonSerializer.Deserialize<GameData>(usaJson);

            // Создаем словарь для быстрого поиска текста по странице
            var pageTexts = draftData.Pages.ToDictionary(p => p.PageNumber, p => p.Text);

            int weaponsAdded = 0;
            int manualReviewCount = 0;

            foreach (var unit in usaData.Units)
            {
                var candidate = draftData.CandidateUnits.FirstOrDefault(c => 
                    c.Name.Equals(unit.Name, StringComparison.OrdinalIgnoreCase) ||
                    c.Name.Contains(unit.Name, StringComparison.OrdinalIgnoreCase) ||
                    unit.Name.Contains(c.Name, StringComparison.OrdinalIgnoreCase));

                if (candidate == null) continue;

                if (pageTexts.TryGetValue(candidate.PageNumber, out var pageText))
                {
                    var weapons = ParseWeapons(pageText, candidate.Name);
                    
                    if (weapons.Count > 0)
                    {
                        Console.WriteLine($"\n{unit.Name} (page {candidate.PageNumber}):");
                        foreach (var weapon in weapons)
                        {
                            if (!unit.Weapons.Any(w => w.Name == weapon.Name))
                            {
                                unit.Weapons.Add(weapon);
                                Console.WriteLine($"  + {weapon.Name} (cost {weapon.Cost}, max {weapon.MaxCount})");
                                weaponsAdded++;
                                
                                if (weapon.Name.Contains("MANUAL_REVIEW"))
                                    manualReviewCount++;
                            }
                        }
                    }
                }
            }

            // Сохраняем обновленный файл
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var updatedJson = JsonSerializer.Serialize(usaData, options);
            File.WriteAllText(usaPath, updatedJson);

            Console.WriteLine($"\n\n=== COMPLETED ===");
            Console.WriteLine($"Weapons added: {weaponsAdded}");
            Console.WriteLine($"Manual review items: {manualReviewCount}");
            Console.WriteLine($"File saved to {usaPath}");
        }

        static List<Weapon> ParseWeapons(string pageText, string unitName)
        {
            var weapons = new List<Weapon>();

            // Ищем секцию Options для данного юнита
            var optionsMatch = Regex.Match(pageText, @"Options(.*?)(?:Special Rules|$)", 
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            if (!optionsMatch.Success)
            {
                return weapons;
            }

            var optionsText = optionsMatch.Groups[1].Value;

            // Паттерны для парсинга оружия
            var patterns = new[]
            {
                // "replace rifle with submachine gun for +4pts"
                new { Pattern = @"replace.*?with\s+(?:an?\s+)?(.+?)\s+for\s+\+(\d+)pts", Type = "replace", WeaponPos = 1, CostPos = 2 },
                // "with a submachine gun for +4pts each"
                new { Pattern = @"with\s+(?:an?\s+)?(.+?)\s+for\s+\+(\d+)pts\s+each", Type = "add", WeaponPos = 1, CostPos = 2 },
                // "given anti-tank grenades for +2pts per figure"
                new { Pattern = @"given\s+(.+?)\s+for\s+\+(\d+)pts\s+per", Type = "unit", WeaponPos = 1, CostPos = 2 },
            };

            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(optionsText, pattern.Pattern, RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    if (match.Success && match.Groups.Count >= 3)
                    {
                        var weaponName = CleanWeaponName(match.Groups[1].Value);
                        if (string.IsNullOrWhiteSpace(weaponName)) continue;

                        var cost = int.Parse(match.Groups[2].Value);
                        var (minCount, maxCount) = ExtractWeaponCount(optionsText, weaponName);

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

            // Ищем сложные случаи, которые требуют ручной обработки
            var complexPatterns = new[]
            {
                ("Intelligence training", @"Intelligence training.*?\+(\d+)pts"),
                ("Demolition charge", @"demolition charge.*?\+(\d+)pts"),
                ("Flamethrower", @"flamethrower.*?\+(\d+)pts"),
                ("Motorcycles", @"motorcycles.*?\+(\d+)pts"),
                ("Horses", @"horses.*?\+(\d+)pts"),
                ("Gun shield", @"gun shield.*?\+(\d+)points?"),
                ("Spotter", @"spotter.*?\+(\d+)pts"),
            };

            foreach (var (itemName, costPattern) in complexPatterns)
            {
                if (optionsText.IndexOf(itemName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (!weapons.Any(w => w.Name.Contains(itemName, StringComparison.OrdinalIgnoreCase)))
                    {
                        var costMatch = Regex.Match(optionsText, costPattern, RegexOptions.IgnoreCase);
                        var cost = costMatch.Success ? int.Parse(costMatch.Groups[1].Value) : 0;

                        weapons.Add(new Weapon
                        {
                            Name = $"MANUAL_REVIEW: {itemName}",
                            Cost = cost,
                            MinCount = 0,
                            MaxCount = 1
                        });
                    }
                }
            }

            return weapons;
        }

        static string CleanWeaponName(string name)
        {
            name = name.Trim();
            name = Regex.Replace(name, @"[,\.\)].*$", ""); // Убираем то что после пунктуации
            name = Regex.Replace(name, @"^(a|an|the|up to|one|two|three)\s+", "", RegexOptions.IgnoreCase);
            
            var replacements = new Dictionary<string, string>
            {
                { "submachine gun|Thompson|M3|SMG", "SMG" },
                { "automatic rifle|BAR|M1919", "BAR" },
                { "light machine gun|LMG", "LMG" },
                { "medium machine gun|MMG", "MMG" },
                { "heavy machine gun|HMG", "HMG" },
                { "anti-tank grenades?|AT grenades?", "AT Grenades" },
                { "shotgun", "Shotgun" },
                { "pistol|carbine", "Pistol" },
            };

            foreach (var kvp in replacements)
            {
                name = Regex.Replace(name, kvp.Key, kvp.Value, RegexOptions.IgnoreCase);
            }

            return name;
        }

        static (int minCount, int maxCount) ExtractWeaponCount(string text, string weaponName)
        {
            // Ищем "up to X men may"
            var upToMatch = Regex.Match(text, $@"up to (\d+)\s+men?\s+may.*?(?:{Regex.Escape(weaponName)}|[,\.])", 
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            
            if (upToMatch.Success)
            {
                return (0, int.Parse(upToMatch.Groups[1].Value));
            }

            // Ищем "up to X men can"
            upToMatch = Regex.Match(text, $@"up to (\d+)\s+men?\s+can.*?(?:{Regex.Escape(weaponName)}|[,\.])",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (upToMatch.Success)
            {
                return (0, int.Parse(upToMatch.Groups[1].Value));
            }

            // Ищем просто "1 man may"
            upToMatch = Regex.Match(text, $@"one\s+man?\s+(?:may|can).*?(?:{Regex.Escape(weaponName)}|[,\.])",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (upToMatch.Success)
            {
                return (0, 1);
            }

            return (0, 0);
        }
    }
}
