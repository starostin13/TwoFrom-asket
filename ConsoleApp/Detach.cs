using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UnitRosterGenerator
{
    public class Detach
    {
        public required string Name { get; set; }
        public int? Cost { get; set; }
        [JsonPropertyName("DP")]
        public int? DP { get; set; }
        public int MaxDetachUpgrades { get; set; } // Максимальное количество улучшений, которые могут быть применены к одному юниту
        public List<Upgrade> Upgrades { get; set; } = new(); // Список доступных улучшений для детача

        public int? GetDetachmentCost()
        {
            return Cost ?? DP;
        }
    }
}
