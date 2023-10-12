using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace ArmyGeneratorMaui.ViewModels
{
    partial class UnitsViewModel : ObservableObject
    {
        private readonly string PathToFile;

        [ObservableProperty]
        private ObservableCollection<Unit> units;
        [ObservableProperty]
        private ObservableCollection<Enchasment> enchasments;
        [ObservableProperty]
        private int maxSizeOfRoster;
        [ObservableProperty]
        private ObservableCollection<Faction> factions;

        /*public IAsyncRelayCommand UploadIndexFileCommand { get; }*/

        public UnitsViewModel()
        {
            maxSizeOfRoster = 2000;
            Units = new ObservableCollection<Unit>();
            Enchasments = new ObservableCollection<Enchasment>();
            Factions = new ObservableCollection<Faction>();
            PathToFile = Path.Combine(FileSystem.Current.AppDataDirectory, "AllFactions.txt");
            uploadIndexFileCommand = new AsyncRelayCommand(UploadIndexFile);

            foreach (var un in ReadAlreadyParsedFiles())
            {
                units.Add(un);
            }
        }

        private async void GenerateRoster()
        { 
            
        }

        private List<Unit> ReadAlreadyParsedFiles()
        {
            if (!File.Exists(PathToFile))
                return new List<Unit>();
            //File.WriteAllTextAsync(Path.Combine(FileSystem.Current.AppDataDirectory, "AllFactions.txt"), Serialize(Core.mainFaction));
            using StreamReader r = new(PathToFile);
            string json = r.ReadToEnd();
            List<Unit> items = JsonConvert.DeserializeObject<List<Unit>>(json);
            
            return items;
        }

        [RelayCommand]
        private void SwitchSelection(object sender)
        { 
            
        }

        [RelayCommand]
        private async Task<Faction> UploadIndexFile()
        {
            var result = await FileManagerHelper.GetFactionFromPdfAsync();
            
            if (result != null)
            {
                if (!Factions.Select(f => f.FactionName).Contains(result.FactionName))
                {
                    Factions.Add(result);
                    foreach (var unit in result.units)
                    {
                        Units.Add(unit);
                    }
                }
            }

            return result;
        }
    }
}
