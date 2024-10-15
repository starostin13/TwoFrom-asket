using UnitRosterGenerator;

public class Detach
{
    // Название детача
    public string Name { get; set; }

    // Список улучшений в рамках данного детача
    public List<Upgrade> Upgrades { get; set; }

    // Конструктор класса Detach
    public Detach(string name, List<Upgrade> upgrades)
    {
        Name = name;
        Upgrades = upgrades;
    }
}