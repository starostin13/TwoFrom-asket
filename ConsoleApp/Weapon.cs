namespace UnitRosterGenerator
{
    // Класс, представляющий оружие
    public class Weapon
    {
        public required string Name { get; set; }
        public int Cost { get; set; }
        public int MinCount { get; set; }
        public int MaxCount { get; set; }
        public List<Upgrade>? Upgrades { get; set; }
    }
}
