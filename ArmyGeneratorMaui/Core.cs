namespace ArmyGeneratorMaui
{
    public static class Core
    {
        private static Faction mainFaction;

        public static List<string> WantToRoster { get; private set; }
        internal static Faction MainFaction { get => mainFaction; set => mainFaction = value; }

        internal static void AddToWantToRoster(string text)
        {
            WantToRoster.Add(text);
        }

        internal static void SetCurrentFaction(Faction faction)
        {
            MainFaction = faction;
        }
    }
}
