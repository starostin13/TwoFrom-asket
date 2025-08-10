public class Upgrade
{
    public required string Name { get; set; }
    public int Cost { get; set; }
    public bool Unique { get; set; }
    public int MinCount { get; set; }
    public int MaxCount { get; set; }

    public Upgrade() {}
    public Upgrade(string name, int cost, bool unique, int minCount, int maxCount)
    {
        Name = name;
        Cost = cost;
        Unique = unique;
        MinCount = minCount;
        MaxCount = maxCount;
    }
}
