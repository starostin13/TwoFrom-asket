using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace UnitRosterGenerator
{
    public class BoltActionUnitParser
    {
        public static List<Unit> ParseFromExtractedData(string jsonPath)
        {
            var jsonText = System.IO.File.ReadAllText(jsonPath);
            var doc = JsonDocument.Parse(jsonText);
            var root = doc.RootElement;
            
            var units = new List<Unit>();
            var pages = root.GetProperty("Pages").EnumerateArray().ToList();
            var candidates = root.GetProperty("CandidateUnits").EnumerateArray().ToList();
            
            foreach (var candidate in candidates)
            {
                var name = candidate.GetProperty("Name").GetString();
                var pageNum = candidate.GetProperty("PageNumber").GetInt32();
                var profileLine = candidate.GetProperty("ProfileLine").GetString();
                
                // Найти соответствующую страницу
                var page = pages.FirstOrDefault(p => p.GetProperty("PageNumber").GetInt32() == pageNum);
                if (page.ValueKind == JsonValueKind.Undefined) continue;
                
                var pageText = page.GetProperty("Text").GetString();
                
                var unit = ParseUnit(name, profileLine, pageText);
                if (unit != null)
                {
                    units.Add(unit);
                }
            }
            
            return units;
        }
        
        private static Unit ParseUnit(string name, string profileLine, string pageText)
        {
            var unit = new Unit { Name = CleanName(name) };
            
            // Парсим цены из ProfileLine
            var experience = ParseExperience(profileLine);
            if (experience.Count == 0) return null;
            
            unit.Experience = experience;
            
            // Парсим состав (Composition)
            var (minModels, maxModels) = ParseComposition(pageText, name);
            unit.MinModels = minModels;
            unit.MaxModels = maxModels;
            
            // Парсим оружие (если есть)
            unit.Weapons = ParseWeapons(pageText, name);
            
            return unit;
        }
        
        private static string CleanName(string name)
        {
            // Очистка имени от артефактов парсинга
            name = name.Replace("S MARAUDERS", "Merrill's Marauders");
            name = name.Replace("MM M2", "90mm M2");
            name = name.Replace("MM ANTI-TANK GUN M1", "57mm Anti-tank Gun M1");
            name = name.Replace("INCH ANTI-TANK GUN M5", "3-inch Anti-tank Gun M5");
            name = name.Replace("M AA GUN MK 4", "Twin 20mm AA Gun Mk 4");
            
            // Преобразуем в Title Case если все заглавные
            if (name == name.ToUpper())
            {
                name = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name.ToLower());
            }
            
            return name;
        }
        
        private static List<ExperienceLevelData> ParseExperience(string profileLine)
        {
            var experience = new List<ExperienceLevelData>();
            
            // Регулярки для парсинга цен
            var patterns = new[]
            {
                @"(\d+)pts\s*\((Inexperienced|Regular|Veteran)\)",
                @"(Inexperienced|Regular|Veteran)\s*\((\d+)pts\)"
            };
            
            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(profileLine, pattern, RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    string level;
                    int cost;
                    
                    if (int.TryParse(match.Groups[1].Value, out cost))
                    {
                        level = match.Groups[2].Value;
                    }
                    else
                    {
                        level = match.Groups[1].Value;
                        cost = int.Parse(match.Groups[2].Value);
                    }
                    
                    experience.Add(new ExperienceLevelData 
                    { 
                        Level = level, 
                        BaseCost = cost 
                    });
                }
            }
            
            return experience;
        }
        
        private static (int min, int max) ParseComposition(string pageText, string unitName)
        {
            // Ищем секцию Composition
            var compositionMatch = Regex.Match(pageText, @"Composition\s*(\d+)\s+(\w+)", RegexOptions.IgnoreCase);
            if (!compositionMatch.Success)
            {
                return (1, 1); // По умолчанию
            }
            
            int minModels = int.Parse(compositionMatch.Groups[1].Value);
            int maxModels = minModels;
            
            // Ищем опции для добавления моделей
            var optionsMatch = Regex.Match(pageText, @"up to (\d+) (?:additional )?men", RegexOptions.IgnoreCase);
            if (optionsMatch.Success)
            {
                maxModels += int.Parse(optionsMatch.Groups[1].Value);
            }
            
            // Для squad-юнитов ищем "Add up to X men"
            var addMatch = Regex.Match(pageText, @"Add up to (\d+) men", RegexOptions.IgnoreCase);
            if (addMatch.Success)
            {
                maxModels = minModels + int.Parse(addMatch.Groups[1].Value);
            }
            
            return (minModels, maxModels);
        }
        
        private static List<Weapon> ParseWeapons(string pageText, string unitName)
        {
            var weapons = new List<Weapon>();
            
            // Это упрощённая версия - полный парсинг оружия требует более сложной логики
            // Пока возвращаем пустой список
            
            return weapons;
        }
    }
}
