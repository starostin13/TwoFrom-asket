public class Unit
{
    public Unit()
    {
    }

    public string? Name { get; internal set; }
    public List<string>? LeadedUnits { get; internal set; }
    public int Price { get; internal set; }

    internal void AddLeadedUnit(string value)
    {
        LeadedUnits ??= new List<string>();
        LeadedUnits.Add(value);
    }

    internal void SetName(string? value)
    {
        if (value.Substring(0, value.Length / 2) == value.Substring((value.Length / 2))) { value = value.Substring(0, value.Length / 2); }
        Name = value;
    }
}