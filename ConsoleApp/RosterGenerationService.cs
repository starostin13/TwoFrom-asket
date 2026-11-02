namespace UnitRosterGenerator
{
    public static class RosterGenerationService
    {
        private const double MultiDetachChance = 0.4;

        public static List<Roster> GenerateTopRosters(
            GameData gameData,
            int maxPoints,
            IReadOnlyDictionary<string, int>? maxUnitQuantities = null,
            int candidateCount = 100,
            IProgress<int>? progress = null)
        {
            ArgumentNullException.ThrowIfNull(gameData);
            ArgumentNullException.ThrowIfNull(gameData.Units);
            ArgumentNullException.ThrowIfNull(gameData.Detachments);

            var allowMultiDetach = gameData.Detachments.Count > 0 &&
                                  gameData.Detachments.All(detach => detach.GetDetachmentCost().HasValue);
            var maxDetachmentPoints = maxPoints <= 1000 ? 2 : 3;
            var candidates = new List<Roster>();

            for (var index = 0; index < candidateCount; index++)
            {
                candidates.Add(RandomRosterBuilder.BuildRandomRoster(
                    gameData.Units,
                    gameData.Detachments,
                    maxPoints,
                    allowMultiDetach,
                    maxDetachmentPoints,
                    MultiDetachChance,
                    maxUnitQuantities));
                progress?.Report(index + 1);
            }

            return candidates
                .OrderByDescending(roster => roster.CalculateTotalCost())
                .Take(5)
                .ToList();
        }
    }
}
