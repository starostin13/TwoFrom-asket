namespace UnitRosterGenerator
{
    // Класс несовместимого оружия
    class IncompatibleWeaponGroup
    {
        public required string Group { get; set; }
        public List<string>? WeaponNames { get; set; }
    }
}
