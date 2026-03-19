using System.Text.RegularExpressions;

namespace UnitRosterGenerator;

internal static class BoltActionDraftMatcher
{
    public static BoltActionMatchReport BuildReport(
        BoltActionPdfExtractionResult draft,
        IReadOnlyList<Unit> baselineUnits)
    {
        var report = new BoltActionMatchReport
        {
            GeneratedAtUtc = DateTime.UtcNow,
            SourcePdf = draft.SourcePdf,
            FromPage = draft.FromPage,
            BaselineUnits = baselineUnits.Count,
            CandidateUnits = draft.CandidateUnits.Count
        };

        var usedCandidates = new HashSet<int>();

        foreach (var baseline in baselineUnits)
        {
            var best = FindBestCandidate(baseline.Name, draft.CandidateUnits, usedCandidates);
            var match = new UnitMatchEntry
            {
                BaselineName = baseline.Name,
                BaselineMinModels = baseline.MinModels,
                BaselineMaxModels = baseline.MaxModels,
                SuggestedEra = best?.Era,
                CandidateName = best?.Candidate.Name,
                CandidatePage = best?.Candidate.PageNumber,
                Score = best?.Score ?? 0
            };

            if (best != null)
            {
                usedCandidates.Add(best.Index);
            }

            report.UnitMatches.Add(match);
        }

        for (var i = 0; i < draft.CandidateUnits.Count; i++)
        {
            if (usedCandidates.Contains(i))
            {
                continue;
            }

            var candidate = draft.CandidateUnits[i];
            report.UnmatchedCandidates.Add(new UnmatchedCandidateEntry
            {
                Name = candidate.Name,
                PageNumber = candidate.PageNumber,
                EraHints = candidate.EraHints,
                ProfileLine = candidate.ProfileLine
            });
        }

        return report;
    }

    private static CandidateMatchResult? FindBestCandidate(
        string baselineName,
        IReadOnlyList<CandidateUnitExtract> candidates,
        HashSet<int> usedCandidates)
    {
        var bestScore = 0.0;
        CandidateMatchResult? bestResult = null;

        var baselineTokens = Tokenize(baselineName);
        var baselineNormalized = NormalizeName(baselineName);

        for (var i = 0; i < candidates.Count; i++)
        {
            if (usedCandidates.Contains(i))
            {
                continue;
            }

            var candidate = candidates[i];
            var candidateNormalized = NormalizeName(candidate.Name);
            var candidateTokens = Tokenize(candidate.Name);

            var score = ComputeScore(baselineNormalized, candidateNormalized, baselineTokens, candidateTokens);
            if (score <= bestScore)
            {
                continue;
            }

            var era = candidate.EraHints.FirstOrDefault();
            bestScore = score;
            bestResult = new CandidateMatchResult
            {
                Candidate = candidate,
                Index = i,
                Score = score,
                Era = era
            };
        }

        // Weak threshold to avoid noisy mapping.
        if (bestResult is null || bestResult.Score < 0.52)
        {
            return null;
        }

        return bestResult;
    }

    private static double ComputeScore(
        string baselineNormalized,
        string candidateNormalized,
        HashSet<string> baselineTokens,
        HashSet<string> candidateTokens)
    {
        if (string.IsNullOrWhiteSpace(baselineNormalized) || string.IsNullOrWhiteSpace(candidateNormalized))
        {
            return 0;
        }

        if (baselineNormalized == candidateNormalized)
        {
            return 1;
        }

        var containsBonus =
            (baselineNormalized.Contains(candidateNormalized, StringComparison.Ordinal) ||
             candidateNormalized.Contains(baselineNormalized, StringComparison.Ordinal))
                ? 0.2
                : 0.0;

        var intersect = baselineTokens.Intersect(candidateTokens).Count();
        var union = baselineTokens.Union(candidateTokens).Count();
        var jaccard = union == 0 ? 0 : (double)intersect / union;

        return Math.Min(1, jaccard + containsBonus);
    }

    private static HashSet<string> Tokenize(string value)
    {
        return Regex.Matches(value.ToLowerInvariant(), @"[a-z0-9]+")
            .Select(m => m.Value)
            .Where(t => t.Length > 1)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static string NormalizeName(string value)
    {
        var cleaned = value.ToLowerInvariant();
        cleaned = cleaned.Replace("medium", "");
        cleaned = cleaned.Replace("light", "");
        cleaned = cleaned.Replace("heavy", "");
        cleaned = cleaned.Replace("team", "");
        cleaned = cleaned.Replace("squad", "");
        cleaned = Regex.Replace(cleaned, @"[^a-z0-9]", "");
        return cleaned.Trim();
    }

    private sealed class CandidateMatchResult
    {
        public required CandidateUnitExtract Candidate { get; init; }
        public required int Index { get; init; }
        public required double Score { get; init; }
        public string? Era { get; init; }
    }
}

internal sealed class BoltActionMatchReport
{
    public DateTime GeneratedAtUtc { get; set; }
    public string SourcePdf { get; set; } = string.Empty;
    public int FromPage { get; set; }
    public int BaselineUnits { get; set; }
    public int CandidateUnits { get; set; }
    public List<UnitMatchEntry> UnitMatches { get; set; } = new();
    public List<UnmatchedCandidateEntry> UnmatchedCandidates { get; set; } = new();
}

internal sealed class UnitMatchEntry
{
    public string BaselineName { get; set; } = string.Empty;
    public int BaselineMinModels { get; set; }
    public int BaselineMaxModels { get; set; }
    public string? SuggestedEra { get; set; }
    public string? CandidateName { get; set; }
    public int? CandidatePage { get; set; }
    public double Score { get; set; }
}

internal sealed class UnmatchedCandidateEntry
{
    public string Name { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public List<string> EraHints { get; set; } = new();
    public string ProfileLine { get; set; } = string.Empty;
}
