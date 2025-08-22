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
    // Primary file name inside output directory (copied via csproj)
    private const string FileName = "ModelLimits.json";
    private static readonly string[] CandidatePaths = new[]
    {
        // 1. Output/bin folder
        Path.Combine(AppContext.BaseDirectory, FileName),
        // 2. Working directory (if started inside project folder)
        FileName,
        // 3. Project relative when run from repo root
        Path.Combine("ConsoleApp", FileName)
    };

    public List<UnitLimit> Limits { get; set; } = new List<UnitLimit>();

    public UnitsLimits()
    {
        foreach (var path in CandidatePaths)
        {
            try
            {
                if (!File.Exists(path)) continue;
                var json = File.ReadAllText(path);
                var unitsLimits = JsonSerializer.Deserialize<List<UnitLimit>>(json);
                if (unitsLimits != null)
                {
                    Limits = unitsLimits;
                    return; // Loaded successfully
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[UnitsLimits] JSON error in '{path}': {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UnitsLimits] Error reading '{path}': {ex.Message}");
            }
        }

        Console.WriteLine("[UnitsLimits] ModelLimits.json not found in any candidate path. Limits disabled.");
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