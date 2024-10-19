using System.Collections.Generic;

namespace UnitRosterGenerator
{
    public class Detach
    {
        public required string Name { get; set; }
        public int MaxDetachUpgrades { get; set; } // Максимальное количество улучшений, которые могут быть применены к одному юниту
        public List<Upgrade> Upgrades { get; set; } // Список доступных улучшений для детача
    }
}
