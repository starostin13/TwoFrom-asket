namespace UnitRosterGenerator
{
    public class CorpsOption
    {
        public string? Name { get; set; }
        public int MinModels { get; set; }
        public int MaxModels { get; set; }
        public List<ExperienceLevelData> Experience { get; set; } = new();
    }
}
