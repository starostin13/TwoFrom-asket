using ArmyGeneratorMaui.Helpers;

namespace ArmyGeneratorMaui
{
    public static class Core
    {
        private static Faction mainFaction;
        private static Roster roster;
        private static Stack<string> wantToRoster = new ();
        private static List<string> blockForRoster = new ();

        public static Stack<string> WantToRoster { get => wantToRoster; private set => wantToRoster = value; }
        public static List<string> BlockForRoster { get => blockForRoster; private set => blockForRoster = value; }
        internal static Faction MainFaction { get => mainFaction; set => mainFaction = value; }
        internal static Roster Roster { get => roster; set => roster = value; }

        internal static void AddToWantToRoster(string text)
        {
            WantToRoster.Push(text);
            if(blockForRoster.Contains(text))
            {
                blockForRoster.Remove(text);
            }
        }
        internal static void AddToBlockForRoster(string text)
        {
            BlockForRoster.Add(text);
        }

        internal static void GenerateRoster()
        {
            roster = new Roster();
            var r = new Random();
            var isRosterHaveFreePoints = true;
            while (isRosterHaveFreePoints)
            {
                ExemplarUnit exemplar;
                var isHaveWantedUnit = WantToRoster.TryPop(out var wantedUnit);
                var unitsWithoutBlocked = mainFaction.units.Where(except => !BlockForRoster.Contains(except.Name)).ToList();
                Unit randomUnit = isHaveWantedUnit
                    ? mainFaction.units.Where(u => u.Name == wantedUnit).FirstOrDefault()
                    : unitsWithoutBlocked[r.Next(0, unitsWithoutBlocked.Count())];
                if (randomUnit.LeadedUnits is not null)
                {
                    var attachedUnits = new List<Unit>();
                    foreach (var lu in randomUnit.LeadedUnits)
                        attachedUnits.AddRange(mainFaction.units.Where(mfu => mfu.Name.Contains(lu, StringComparison.InvariantCultureIgnoreCase)));

                    exemplar = new ExemplarUnit(randomUnit, attachedUnits[r.Next(0, attachedUnits.Count)]);

                    var isAddEnchasment = new Random();
                    if (isAddEnchasment.NextDouble() < 50)
                    {
                        var randomEnch = RandomHelper.PopRandom(mainFaction.enchasments);
                        exemplar.AddEnchasment(randomEnch.Item1, randomEnch.Item2);
                    }
                }
                else
                {
                    exemplar = new ExemplarUnit(randomUnit);
                }

                isRosterHaveFreePoints = roster.AddExemplarIfAcceptable(exemplar);
            }
        }

        internal static void SetCurrentFaction(Faction faction)
        {
            MainFaction = faction;
        }

        internal static void SaveAllFactions()
        {
            File.WriteAllTextAsync(Path.Combine(FileSystem.Current.AppDataDirectory, "AllFactions.txt"), Serialize(Core.mainFaction));
        }

        private static string Serialize(Faction mainFaction)
        {
            throw new NotImplementedException();
        }
    }
}
