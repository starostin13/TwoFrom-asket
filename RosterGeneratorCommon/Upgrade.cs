public class Upgrade
{
    // Название улучшения
    public string Name { get; set; }

    // Стоимость улучшения
    public int Cost { get; set; }

    // Уникальность улучшения
    public bool Unique { get; set; }

    // Минимальное количество
    public int MinCount { get; set; }

    // Максимальное количество
    public int MaxCount { get; set; }

    // Конструктор класса Upgrade
    public Upgrade(string name, int cost, bool unique, int minCount, int maxCount)
    {
        Name = name;
        Cost = cost;
        Unique = unique;
        MinCount = minCount;
        MaxCount = maxCount;
    }
}
