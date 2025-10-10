using System.Text.Json;
using System.Text.Json.Serialization;

public class UnitLimit
{
    [JsonPropertyName("ModelName")]
    public required string ModelName { get; set; }

    [JsonPropertyName("MaxQuantity")]
    public int? MaxQuantity
    {
        get; set;
    }

    [JsonPropertyName("MinQuantity")]
    public int? MinQuantity
    {
        get; set;
    }
}

public class UnitsLimits
{
    private static string filePath = "ModelLimits.json";

    public List<UnitLimit> Limits { get; set; } = new List<UnitLimit>();

    public UnitsLimits()
    {
        try
        {
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                var unitsLimits = JsonSerializer.Deserialize<List<UnitLimit>>(json);
                if (unitsLimits != null)
                {
                    Limits = unitsLimits;
                }
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error deserializing JSON: {ex.Message}");
            // Handle the error or set default values
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

    public int? GetMaxLimit(string modelName)
    {
        var limit = Limits.Find(limit => limit.ModelName == modelName);
        
        return limit != null ? limit.MaxQuantity : null;
    }

    public int? GetMaxLimit(string modelName, string? model)
    {
        // Если задано поле Model, используем его для поиска лимита, иначе используем имя юнита
        string lookupKey = !string.IsNullOrEmpty(model) ? model : modelName;
        var limit = Limits.Find(limit => limit.ModelName == lookupKey);
        
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