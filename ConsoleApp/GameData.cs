using System.Text.Json.Serialization;

namespace UnitRosterGenerator
{
    class GameData
    {
        public List<Unit> Units { get; set; } = new();
        
        public List<Detach> Detachments { get; set; } = new();

        // Метод для объединения данных из нескольких GameData объектов
        public static GameData Merge(params GameData[] gameDatas)
        {
            var mergedData = new GameData();
            
            foreach (var gameData in gameDatas)
            {
                if (gameData != null)
                {
                    mergedData.Units.AddRange(gameData.Units);
                    
                    // Объединяем detachments, избегая дублирования по имени
                    foreach (var detach in gameData.Detachments)
                    {
                        if (!mergedData.Detachments.Any(d => d.Name == detach.Name))
                        {
                            mergedData.Detachments.Add(detach);
                        }
                    }
                }
            }
            
            return mergedData;
        }
    }
}