using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace UnitRosterGenerator;

internal static class BoltActionPdfExtractor
{
    private static readonly Regex ProfileRegex = new(
        @"(Inexperienced|Regular|Veteran).{0,30}?\d+",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex CostPeriodBlockRegex = new(
        @"Cost\s*-?\s*(?<cost>.*?)(?:Period\s*)(?<era>(?:E|M|L)(?:\s*/\s*(?:E|M|L))*)",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex UnitBlockRegex = new(
        @"(?<name>[A-Z][A-Z0-9\u0026/\-()'\s]{3,90})\s+[A-Z][a-z][\s\S]{20,1200}?Cost\s*-?\s*(?<cost>.*?)(?:Period\s*)(?<era>(?:E|M|L)(?:\s*/\s*(?:E|M|L))*)",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly string[] UnitNameKeywords =
    [
        "COMMANDER", "MEDIC", "CHAPLAIN", "OBSERVER", "OFFICER", "SQUAD", "TEAM",
        "MORTAR", "HOWITZER", "GUN", "TANK", "DESTROYER", "JEEP", "TRUCK", "HALF-TRACK",
        "SCOUT", "CAR", "ARTILLERY", "SNIPER", "BAZOOKA", "FLAMETHROWER", "ENGINEER"
    ];

    private static readonly Regex EraRegex = new(
        @"\b(?:E|M|L)(?:\s*/\s*(?:E|M|L))*\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static BoltActionPdfExtractionResult ExtractDraft(string pdfPath, int fromPage)
    {
        if (!File.Exists(pdfPath))
        {
            throw new FileNotFoundException("PDF file not found", pdfPath);
        }

        using var pdfDocument = PdfDocument.Open(pdfPath);
        var pageCount = pdfDocument.NumberOfPages;
        if (fromPage < 1 || fromPage > pageCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fromPage),
                $"fromPage must be in range 1..{pageCount}");
        }

        var pages = new List<PdfPageExtract>();
        var candidates = new List<CandidateUnitExtract>();

        for (int pageNumber = fromPage; pageNumber <= pageCount; pageNumber++)
        {
            var page = pdfDocument.GetPage(pageNumber);
            var text = page.Text ?? string.Empty;
            var lines = NormalizeToLines(text)
                .Split('\n', StringSplitOptions.TrimEntries)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToList();

            pages.Add(new PdfPageExtract
            {
                PageNumber = pageNumber,
                LineCount = lines.Count,
                Text = text
            });

            ExtractCandidateUnitsFromPage(pageNumber, text, lines, candidates);
        }

        return new BoltActionPdfExtractionResult
        {
            SourcePdf = pdfPath,
            FromPage = fromPage,
            GeneratedAtUtc = DateTime.UtcNow,
            TotalPagesInPdf = pageCount,
            Pages = pages,
            CandidateUnits = candidates
                .DistinctBy(c => $"{c.PageNumber}|{c.Name}|{c.ProfileLine}")
                .OrderBy(c => c.PageNumber)
                .ThenBy(c => c.Name)
                .ToList()
        };
    }

    private static void ExtractCandidateUnitsFromPage(
        int pageNumber,
        string pageText,
        List<string> lines,
        List<CandidateUnitExtract> candidates)
    {
        var unitMatches = UnitBlockRegex.Matches(pageText);
        foreach (Match unitMatch in unitMatches)
        {
            if (!unitMatch.Success)
            {
                continue;
            }

            var rawName = Regex.Replace(unitMatch.Groups["name"].Value, @"\s+", " ").Trim();
            if (!IsValidUnitHeading(rawName))
            {
                continue;
            }

            var profileLine = unitMatch.Groups["cost"].Value.Trim();
            var era = unitMatch.Groups["era"].Value.Trim();
            var eras = new List<string>();
            if (!string.IsNullOrWhiteSpace(era))
            {
                eras.Add(Regex.Replace(era.ToUpperInvariant(), @"\s+", string.Empty));
            }

            candidates.Add(new CandidateUnitExtract
            {
                Name = rawName,
                PageNumber = pageNumber,
                ProfileLine = profileLine,
                EraHints = eras
            });
        }

        var blockMatches = CostPeriodBlockRegex.Matches(pageText);
        foreach (Match blockMatch in blockMatches)
        {
            if (!blockMatch.Success)
            {
                continue;
            }

            var name = FindCandidateNameFromText(pageText, blockMatch.Index);
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var profileLine = blockMatch.Groups["cost"].Value.Trim();
            var era = blockMatch.Groups["era"].Value.Trim();
            var eras = new List<string>();
            if (!string.IsNullOrWhiteSpace(era))
            {
                eras.Add(Regex.Replace(era.ToUpperInvariant(), @"\s+", string.Empty));
            }

            candidates.Add(new CandidateUnitExtract
            {
                Name = name,
                PageNumber = pageNumber,
                ProfileLine = profileLine,
                EraHints = eras
            });
        }

        // Fallback when block parser misses entries.
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (!ProfileRegex.IsMatch(line))
            {
                continue;
            }

            var name = FindCandidateName(lines, i);
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            if (!IsValidUnitHeading(name))
            {
                continue;
            }

            var eras = CollectEraHints(lines, i);

            candidates.Add(new CandidateUnitExtract
            {
                Name = name,
                PageNumber = pageNumber,
                ProfileLine = line,
                EraHints = eras
            });
        }
    }

    private static string NormalizeToLines(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var normalized = text;
        var markers = new[]
        {
            " Cost",
            " Period",
            " Composition",
            " Team",
            " Weapons",
            " Options",
            " Special Rules",
            " Selector",
            " Transport",
            " Damage Value"
        };

        foreach (var marker in markers)
        {
            normalized = normalized.Replace(marker, "\n" + marker.TrimStart(), StringComparison.OrdinalIgnoreCase);
        }

        normalized = Regex.Replace(normalized, @"\s{2,}", " ");
        return normalized;
    }

    private static string FindCandidateName(List<string> lines, int profileLineIndex)
    {
        for (var i = profileLineIndex - 1; i >= Math.Max(0, profileLineIndex - 6); i--)
        {
            var candidate = lines[i].Trim();
            if (candidate.Length is < 2 or > 90)
            {
                continue;
            }

            if (ProfileRegex.IsMatch(candidate) || candidate.StartsWith("•") || candidate.StartsWith("-") || candidate.StartsWith("+"))
            {
                continue;
            }

            return candidate;
        }

        return string.Empty;
    }

    private static string FindCandidateNameFromText(string text, int costIndex)
    {
        var backtrackLength = Math.Min(240, costIndex);
        if (backtrackLength <= 0)
        {
            return string.Empty;
        }

        var beforeCost = text.Substring(costIndex - backtrackLength, backtrackLength);
        var headingMatches = Regex.Matches(
            beforeCost,
            @"(?<name>[A-Z][A-Z0-9\u0026/\-()'\s]{2,80})(?=\s+[A-Z][a-z])");

        for (var i = headingMatches.Count - 1; i >= 0; i--)
        {
            var heading = headingMatches[i].Groups["name"].Value.Trim();
            if (IsValidUnitHeading(heading))
            {
                return heading;
            }
        }

        var nameMatches = Regex.Matches(beforeCost, @"[A-Z0-9][A-Z0-9\u0026/\-()'\s]{2,}");

        for (var i = nameMatches.Count - 1; i >= 0; i--)
        {
            var candidate = nameMatches[i].Value.Trim();
            if (candidate.Length is < 3 or > 70)
            {
                continue;
            }

            if (!IsValidUnitHeading(candidate))
            {
                continue;
            }

            return candidate;
        }

        return string.Empty;
    }

    private static bool IsValidUnitHeading(string candidate)
    {
        if (candidate.Length is < 3 or > 70)
        {
            return false;
        }

        if (candidate.Any(char.IsLower))
        {
            return false;
        }

        if (candidate.Contains("BOLT ACTION", StringComparison.OrdinalIgnoreCase) ||
            candidate.Contains("THE ARMY LIST", StringComparison.OrdinalIgnoreCase) ||
            candidate.Contains("LAYOUTS", StringComparison.OrdinalIgnoreCase) ||
            candidate.Contains("INDD", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!Regex.IsMatch(candidate, @"[A-Za-z]"))
        {
            return false;
        }

        if (!UnitNameKeywords.Any(keyword =>
                candidate.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return true;
    }

    private static List<string> CollectEraHints(List<string> lines, int centerIndex)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var i = Math.Max(0, centerIndex - 3); i <= Math.Min(lines.Count - 1, centerIndex + 3); i++)
        {
            foreach (Match match in EraRegex.Matches(lines[i]))
            {
                var normalized = Regex.Replace(match.Value.ToUpperInvariant(), @"\s+", string.Empty);
                if (normalized.Length > 0)
                {
                    result.Add(normalized);
                }
            }
        }

        return result.OrderBy(x => x).ToList();
    }
}

internal sealed class BoltActionPdfExtractionResult
{
    public string SourcePdf { get; set; } = string.Empty;
    public int FromPage { get; set; }
    public DateTime GeneratedAtUtc { get; set; }
    public int TotalPagesInPdf { get; set; }
    public List<PdfPageExtract> Pages { get; set; } = new();
    public List<CandidateUnitExtract> CandidateUnits { get; set; } = new();
}

internal sealed class PdfPageExtract
{
    public int PageNumber { get; set; }
    public int LineCount { get; set; }
    public string Text { get; set; } = string.Empty;
}

internal sealed class CandidateUnitExtract
{
    public string Name { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public string ProfileLine { get; set; } = string.Empty;
    public List<string> EraHints { get; set; } = new();
}