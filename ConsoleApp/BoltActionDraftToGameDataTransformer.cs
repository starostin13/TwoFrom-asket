using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System;

namespace UnitRosterGenerator;

/// <summary>
/// Трансформирует BoltActionPdfExtractionResult в полный GameData с weapons и upgrades
/// </summary>
internal static class BoltActionDraftToGameDataTransformer
{
    private static readonly Regex ExperienceCostRegex = new(
        @"(?<cost>\d+)pts?\s*\((?<exp>Inexperienced|Regular|Veteran)\)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex CompositionRegex = new(
        @"Composition\s*(?<minModels>\d+)\s*(?:NCO and |officer and |medic and |man and )?(?<additionalModels>\d+)?\s*(?:men|man|figures?|handlers?)?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex TeamCompositionRegex = new(
        @"Team\s*(?<teamSize>\d+)\s*(?:men|man)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static GameData TransformToGameData(
        BoltActionPdfExtractionResult draft,
        string factionName = "United States")
    {
        var units = new List<Unit>();

        foreach (var candidate in draft.CandidateUnits)
        {
            // Находим страницу с этим юнитом
            var page = draft.Pages.FirstOrDefault(p => p.PageNumber == candidate.PageNumber);
            if (page == null)
            {
                continue;
            }

            // Извлекаем полный текст блока юнита
            var unitBlock = BoltActionOptionsParser.ExtractUnitBlock(page.Text, candidate);
            if (string.IsNullOrWhiteSpace(unitBlock))
            {
                continue;
            }

            var unit = TransformCandidateToUnit(candidate, unitBlock);
            if (unit != null)
            {
                units.Add(unit);
            }
        }

        return new GameData
        {
            Units = units,
            Detachments = new List<Detach>()
        };
    }

    private static Unit? TransformCandidateToUnit(
        CandidateUnitExtract candidate,
        string unitBlock)
    {
        // Парсим Experience levels и costs
        var experiences = ParseExperienceLevels(candidate.ProfileLine);
        if (experiences.Count == 0)
        {
            Console.WriteLine($"[warn] Could not parse experience for: {candidate.Name}");
            return null;
        }

        // Парсим Composition (Min/Max models)
        var (minModels, maxModels) = ParseComposition(unitBlock);

        // Парсим Weapons из секции Options
        var weapons = BoltActionOptionsParser.ParseOptions(unitBlock, candidate.Name);

        return new Unit
        {
            Name = candidate.Name,
            MinModels = minModels,
            MaxModels = maxModels,
            Experience = experiences,
            Weapons = weapons
        };
    }

    private static List<ExperienceLevelData> ParseExperienceLevels(string profileLine)
    {
        var levels = new List<ExperienceLevelData>();
        var matches = ExperienceCostRegex.Matches(profileLine);

        foreach (Match match in matches)
        {
            var level = match.Groups["exp"].Value;
            var cost = int.Parse(match.Groups["cost"].Value);

            levels.Add(new ExperienceLevelData
            {
                Level = level,
                BaseCost = cost,
                AdditionalModelCost = 0 // Будет вычислено позже
            });
        }

        // Упорядочиваем: Inexperienced -> Regular -> Veteran
        levels = levels
            .OrderBy(l => l.Level switch
            {
                "Inexperienced" => 0,
                "Regular" => 1,
                "Veteran" => 2,
                _ => 3
            })
            .ToList();

        return levels;
    }

    private static (int MinModels, int MaxModels) ParseComposition(string unitBlock)
    {
        // Ищем "Composition"
        var compositionMatch = CompositionRegex.Match(unitBlock);
        if (compositionMatch.Success)
        {
            var minModels = int.Parse(compositionMatch.Groups["minModels"].Value);
            var additionalText = compositionMatch.Groups["additionalModels"].Value;

            int maxModels = minModels;
            if (!string.IsNullOrWhiteSpace(additionalText) && int.TryParse(additionalText, out var additional))
            {
                maxModels = minModels + additional;
            }

            // Ищем "Add up to X men" в Options
            var addUpToMatch = Regex.Match(unitBlock, @"Add up to (\d+) (?:additional )?(?:men|man)", RegexOptions.IgnoreCase);
            if (addUpToMatch.Success)
            {
                var addUpTo = int.Parse(addUpToMatch.Groups[1].Value);
                maxModels = minModels + addUpTo;
            }

            return (minModels, maxModels);
        }

        // Ищем "Team"
        var teamMatch = TeamCompositionRegex.Match(unitBlock);
        if (teamMatch.Success)
        {
            var teamSize = int.Parse(teamMatch.Groups["teamSize"].Value);
            return (teamSize, teamSize);
        }

        // По умолчанию
        return (1, 1);
    }

    /// <summary>
    /// Вычисляет AdditionalModelCost на основе базовых затрат
    /// </summary>
    public static void CalculateAdditionalModelCosts(GameData gameData)
    {
        foreach (var unit in gameData.Units)
        {
            if (unit.MinModels >= unit.MaxModels)
            {
                // Нет дополнительных моделей
                continue;
            }

            foreach (var exp in unit.Experience)
            {
                // Ищем опции добавления моделей
                var addRifles = unit.Weapons
                    .FirstOrDefault(w => w.Name.Contains("rifle", StringComparison.OrdinalIgnoreCase) &&
                                         !w.Name.Contains("automatic", StringComparison.OrdinalIgnoreCase));

                if (addRifles != null && addRifles.Cost > 0)
                {
                    exp.AdditionalModelCost = addRifles.Cost;
                }
            }
        }
    }
}
