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
        private ObservableCollection<Enchasment> enchasments;
        [ObservableProperty]
        private ObservableCollection<Faction> factions;
        [ObservableProperty]
        private int maxSizeOfRoster;
        [ObservableProperty]
        private ObservableCollection<Faction> selectedFactions;
        [ObservableProperty]
        private ObservableCollection<Unit> units;

        public UnitsViewModel()
        {
            maxSizeOfRoster = 2000;
            Units = new ObservableCollection<Unit>();
            Enchasments = new ObservableCollection<Enchasment>();
            Factions = new ObservableCollection<Faction>();
            PathToFile = Path.Combine(FileSystem.Current.AppDataDirectory, "AllFactions.txt");
            selectedFactions = new ObservableCollection<Faction>();
            uploadIndexFileCommand = new AsyncRelayCommand(UploadIndexFile);
            factions.CollectionChanged += Factions_CollectionChanged;

            foreach (var un in ReadAlreadyParsedFiles())
            {
                factions.Add(un);
            }
        }

        private void SelectedFactions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            units.Clear();
            //foreach(var unit in Factions.Select())
        }

        private void Factions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            File.WriteAllTextAsync(PathToFile, JsonConvert.SerializeObject(Factions));
        }

        [RelayCommand]
        private async void GenerateRoster()
        {

        }

        private List<Faction> ReadAlreadyParsedFiles()
        {
            if (!File.Exists(PathToFile))
                return new List<Faction>();
            //File.WriteAllTextAsync(Path.Combine(FileSystem.Current.AppDataDirectory, "AllFactions.txt"), Serialize(Core.mainFaction));
            using StreamReader r = new(PathToFile);
            string json = r.ReadToEnd();
            List<Faction> items = JsonConvert.DeserializeObject<List<Faction>>(json);
            
            return items;
        }

        [RelayCommand]
        private void SwitchSelection(object sender)
        {
            // todo: looks like currently binding with selected items doesn't work in MAUI
            // so currently I will ignore filter and just show all units from all fractions
            // must be reworked https://github.com/starostin13/TwoFrom-asket/issues/11
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
