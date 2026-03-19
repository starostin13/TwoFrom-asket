using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace UnitRosterGenerator;

/// <summary>
/// Парсит секцию "Options" из текста юнита и преобразует в Weapon[]
/// </summary>
internal static class BoltActionOptionsParser
{
    // Паттерны для разных типов опций
    private static readonly Regex WeaponReplaceRegex = new(
        @"(?:may |can |The NCO and up to \d+ men may |The NCO may |Up to \d+ men may |Up to \d+ man may |Anybody may )?replace (?:their |his |the |its )?(?<oldWeapon>[\w\s-]+?) with (?:a |an )?(?<newWeapon>[\w\s-]+?)(?: in addition to)?(?:,| for[\s\u002B]+)(?<cost>-?\d+)pts?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex WeaponAddRegex = new(
        @"Add up to (?<count>\d+)(?: (?:additional )?(?:men|man|figures?|handlers?))? (?:with |armed with |carrying )?(?:a |an )?(?<weapon>[\w\s-]+?)(?: at |for |at a cost of | )+(?:\+|ƒ|∆|\u002B)?(?<cost>-?\d+)pts?(?: each)?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex WeaponTakeRegex = new(
        @"(?:may |can )?take (?:a |an )?(?<weapon>[\w\s-]+?) (?:in addition to|for) (?:other weapons for )?\+?(?<cost>-?\d+)pts?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex AntiTankGrenadesRegex = new(
        @"(?:may be )?given anti-tank grenades for \+?(?<cost>\d+)pts? per figure",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex PintleMountedRegex = new(
        @"(?:Add a|Take an additional|May add a) pintle-mounted (?<weapon>MMG|HMG|machine gun) (?:on (?:top of )?the turret )?for \+?(?<cost>\d+)pts?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static List<Weapon> ParseOptions(string fullUnitText, string unitName)
    {
        var weapons = new List<Weapon>();

        if (string.IsNullOrWhiteSpace(fullUnitText))
        {
            return weapons;
        }

        // Извлекаем секцию Options
        var optionsMatch = Regex.Match(
            fullUnitText,
            @"Options\s*-?\s*(?<options>.*?)(?=Special Rules|Damage Value|$)",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        if (!optionsMatch.Success)
        {
            return weapons;
        }

        var optionsText = optionsMatch.Groups["options"].Value;

        // Разбиваем на отдельные опции (каждая начинается с "-" или "•")
        var optionLines = Regex.Split(optionsText, @"(?=^\s*[-•])", RegexOptions.Multiline)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Trim())
            .ToList();

        foreach (var line in optionLines)
        {
            // Пропускаем опции "By Air, Land, and Sea" и другие special rules
            if (line.Contains("By Air, Land, and Sea", StringComparison.OrdinalIgnoreCase) ||
                line.Contains("If taken as", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Парсим замену оружия
            var replaceMatch = WeaponReplaceRegex.Match(line);
            if (replaceMatch.Success)
            {
                var weaponName = NormalizeWeaponName(replaceMatch.Groups["newWeapon"].Value);
                var cost = int.Parse(replaceMatch.Groups["cost"].Value);

                weapons.Add(new Weapon
                {
                    Name = weaponName,
                    Cost = cost,
                    MinCount = 0,
                    MaxCount = ExtractMaxCount(line)
                });
                continue;
            }

            // Парсим добавление моделей с оружием
            var addMatch = WeaponAddRegex.Match(line);
            if (addMatch.Success)
            {
                var count = int.Parse(addMatch.Groups["count"].Value);
                var weaponName = NormalizeWeaponName(addMatch.Groups["weapon"].Value);
                var costPerModel = int.Parse(addMatch.Groups["cost"].Value);

                // Если это добавление моделей с rifles (базовое оружие)
                if (weaponName.Contains("rifle", StringComparison.OrdinalIgnoreCase) && 
                    !weaponName.Contains("automatic", StringComparison.OrdinalIgnoreCase))
                {
                    // Это расширение отряда, не оружие
                    continue;
                }

                weapons.Add(new Weapon
                {
                    Name = weaponName,
                    Cost = costPerModel,
                    MinCount = 0,
                    MaxCount = count
                });
                continue;
            }

            // Парсим "take" опции (demolition charge, etc.)
            var takeMatch = WeaponTakeRegex.Match(line);
            if (takeMatch.Success)
            {
                var weaponName = NormalizeWeaponName(takeMatch.Groups["weapon"].Value);
                var cost = int.Parse(takeMatch.Groups["cost"].Value);

                weapons.Add(new Weapon
                {
                    Name = weaponName,
                    Cost = cost,
                    MinCount = 0,
                    MaxCount = 1
                });
                continue;
            }

            // Anti-tank grenades (особый случай - cost per figure)
            var atGrenadesMatch = AntiTankGrenadesRegex.Match(line);
            if (atGrenadesMatch.Success)
            {
                var costPerFigure = int.Parse(atGrenadesMatch.Groups["cost"].Value);

                weapons.Add(new Weapon
                {
                    Name = "Anti-tank grenades",
                    Cost = costPerFigure,
                    MinCount = 0,
                    MaxCount = 99 // Per figure, так что max довольно большой
                });
                continue;
            }

            // Pintle-mounted weapons
            var pintleMatch = PintleMountedRegex.Match(line);
            if (pintleMatch.Success)
            {
                var weaponType = pintleMatch.Groups["weapon"].Value;
                var cost = int.Parse(pintleMatch.Groups["cost"].Value);

                var weaponName = weaponType.ToUpperInvariant() switch
                {
                    "HMG" or "HEAVY MACHINE GUN" => "Pintle-mounted HMG",
                    "MMG" or "MACHINE GUN" => "Pintle-mounted MMG",
                    _ => $"Pintle-mounted {weaponType}"
                };

                weapons.Add(new Weapon
                {
                    Name = weaponName,
                    Cost = cost,
                    MinCount = 0,
                    MaxCount = 1
                });
            }
        }

        return weapons;
    }

    private static string NormalizeWeaponName(string rawName)
    {
        var name = rawName.Trim();

        // Нормализуем известные сокращения
        name = name.Replace("submachine gun", "SMG", StringComparison.OrdinalIgnoreCase);
        name = name.Replace("sub-machine gun", "SMG", StringComparison.OrdinalIgnoreCase);
        name = name.Replace("automatic rifle", "BAR", StringComparison.OrdinalIgnoreCase);
        name = name.Replace("light machine gun", "LMG", StringComparison.OrdinalIgnoreCase);
        name = name.Replace("medium machine gun", "MMG", StringComparison.OrdinalIgnoreCase);
        name = name.Replace("heavy machine gun", "HMG", StringComparison.OrdinalIgnoreCase);

        // Делаем первую букву заглавной
        if (name.Length > 0)
        {
            name = char.ToUpper(name[0]) + name.Substring(1);
        }

        return name;
    }

    private static int ExtractMaxCount(string text)
    {
        // Пытаемся найти "up to X"
        var upToMatch = Regex.Match(text, @"(?:up to|Up to) (\d+)", RegexOptions.IgnoreCase);
        if (upToMatch.Success)
        {
            return int.Parse(upToMatch.Groups[1].Value);
        }

        // Пытаемся найти "The NCO and up to X men"
        var ncoAndMatch = Regex.Match(text, @"NCO and up to (\d+)", RegexOptions.IgnoreCase);
        if (ncoAndMatch.Success)
        {
            return int.Parse(ncoAndMatch.Groups[1].Value) + 1; // +1 for NCO
        }

        // По умолчанию
        return 1;
    }

    /// <summary>
    /// Извлекает полный текст блока юнита из страницы
    /// </summary>
    public static string ExtractUnitBlock(string pageText, CandidateUnitExtract candidate)
    {
        if (string.IsNullOrWhiteSpace(pageText))
        {
            return string.Empty;
        }

        // Ищем начало блока юнита (название)
        var nameIndex = pageText.IndexOf(candidate.Name, StringComparison.OrdinalIgnoreCase);
        if (nameIndex == -1)
        {
            return string.Empty;
        }

        // Ищем конец блока (следующий юнит или конец страницы)
        var endMarkers = new[]
        {
            "\nCost-", "\nCost -", "\nCost ",
            "BOLT Action"
        };

        var minEndIndex = pageText.Length;
        foreach (var marker in endMarkers)
        {
            var endIndex = pageText.IndexOf(marker, nameIndex + candidate.Name.Length + 50, StringComparison.OrdinalIgnoreCase);
            if (endIndex > nameIndex && endIndex < minEndIndex)
            {
                minEndIndex = endIndex;
            }
        }

        // Берем текст блока
        var blockLength = Math.Min(2000, minEndIndex - nameIndex);
        return pageText.Substring(nameIndex, blockLength);
    }
}
