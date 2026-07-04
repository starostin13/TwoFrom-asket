using Newtonsoft.Json;

namespace ArmyGeneratorMaui
{
    internal class StorageHelper
    {
        private static readonly string RostersPath = Path.Combine(FileSystem.Current.AppDataDirectory, "Rosters");

        public static void InitializeStorage()
        {
            if (!Directory.Exists(RostersPath))
            {
                Directory.CreateDirectory(RostersPath);
            }
        }

        internal static void SaveRoster(Roster roster, string name)
        {
            InitializeStorage();
            
            var rosterData = new
            {
                Name = name,
                Timestamp = DateTime.Now,
                MaxSize = roster.MaxSize,
                Price = roster.Price,
                Units = roster.ArmyList.Select(u => new
                {
                    u.Name,
                    u.Price,
                    u.Enchasment,
                    MainUnitName = u.MainUnit?.Name,
                    AttachedSquadName = u.AttachedSquad?.Name
                }).ToList()
            };

            string fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{name}.json";
            string filePath = Path.Combine(RostersPath, fileName);
            
            string json = JsonConvert.SerializeObject(rosterData, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        internal static List<SavedRosterInfo> LoadSavedRosters()
        {
            InitializeStorage();
            
            var rosters = new List<SavedRosterInfo>();
            
            if (!Directory.Exists(RostersPath))
            {
                return rosters;
            }

            var files = Directory.GetFiles(RostersPath, "*.json");
            foreach (var file in files)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var rosterData = JsonConvert.DeserializeObject<dynamic>(json);
                    
                    rosters.Add(new SavedRosterInfo
                    {
                        FilePath = file,
                        Name = rosterData["Name"],
                        Price = (int)rosterData["Price"],
                        Timestamp = DateTime.Parse((string)rosterData["Timestamp"]),
                        UnitCount = rosterData["Units"].Count
                    });
                }
                catch
                {
                    // Skip invalid files
                }
            }

            return rosters.OrderByDescending(r => r.Timestamp).ToList();
        }

        internal static Roster LoadRoster(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            try
            {
                string json = File.ReadAllText(filePath);
                var rosterData = JsonConvert.DeserializeObject<dynamic>(json);
                
                var roster = new Roster((int)rosterData["MaxSize"]);
                var units = rosterData["Units"] as Newtonsoft.Json.Linq.JArray;
                
                foreach (var unitData in units)
                {
                    var exemplarUnit = new ExemplarUnit(new Unit
                    {
                        Name = (string)unitData["AttachedSquadName"],
                        Price = (int)unitData["Price"]
                    });
                    
                    if (!string.IsNullOrEmpty((string)unitData["Enchasment"]))
                    {
                        exemplarUnit.Enchasment = (string)unitData["Enchasment"];
                    }
                    
                    roster.AddExemplarIfAcceptable(exemplarUnit);
                }
                
                return roster;
            }
            catch
            {
                return null;
            }
        }
    }

    public class SavedRosterInfo
    {
        public string FilePath { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public DateTime Timestamp { get; set; }
        public int UnitCount { get; set; }
        
        public string DisplayName => $"{Name} ({Price} pts)";
        public string DisplayInfo => $"{UnitCount} units - {Timestamp:dd.MM.yyyy HH:mm}";
    }
}
