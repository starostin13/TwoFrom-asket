using System.Text.Json;
using System.Text.Json.Serialization;

public class UnitLimit
{
    [JsonPropertyName("ModelName")]
    public required string ModelName { get; set; }

    [JsonPropertyName("MaxQuantity")]
    public int MaxQuantity { get; set; }
}

public class UnitsLimits
{
    private static string filePath = "UnitsLimits.json";

    public List<UnitLimit> Limits { get; set; } = new List<UnitLimit>();

    public UnitsLimits()
    {
        if (File.Exists(filePath))
        {
            var json = File.ReadAllText(filePath);
            var unitsLimits = JsonSerializer.Deserialize<UnitsLimits>(json);
            if (unitsLimits != null)
            {
                Limits = unitsLimits.Limits;
            }
        }
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }

    public void AddOrUpdateLimit(string modelName, int maxQuantity)
    {
        var existingLimit = Limits.Find(limit => limit.ModelName == modelName);
        if (existingLimit != null)
        {
            existingLimit.MaxQuantity = maxQuantity;
        }
        else
        {
            Limits.Add(new UnitLimit { ModelName = modelName, MaxQuantity = maxQuantity });
        }
    }

    public int? GetLimit(string modelName)
    {
        var limit = Limits.Find(limit => limit.ModelName == modelName);
        
        return limit != null ? limit.MaxQuantity : null;
    }

    public static UnitsLimits? LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return new UnitsLimits();
        }

        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<UnitsLimits>(json);
    }

    public void SaveToFile(string filePath)
    {
        var json = ToJson();
        File.WriteAllText(filePath, json);
    }
}