namespace UnitRosterGenerator
{
    // Класс, представляющий оружие
    public class Weapon
    {
        public required string Name { get; set; }
        public int Cost { get; set; }
        public int MinCount { get; set; }
        /// <summary>Use -1 in JSON to mean "up to model count".</summary>
        public int MaxCount { get; set; }
        public List<Upgrade>? Upgrades { get; set; }

        public int ResolveMaxCount(int modelCount) =>
            MaxCount == -1 ? modelCount : MaxCount;
    }
}
