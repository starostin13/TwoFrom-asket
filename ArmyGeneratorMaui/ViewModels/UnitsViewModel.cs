using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace ArmyGeneratorMaui.ViewModels
{
    partial class UnitsViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Unit> units;

        public UnitsViewModel()
        {
            ReadAlreadyParsedFiles();
        }

        private static void ReadAlreadyParsedFiles()
        {
            //File.WriteAllTextAsync(Path.Combine(FileSystem.Current.AppDataDirectory, "AllFactions.txt"), Serialize(Core.mainFaction));
            using StreamReader r = new(Path.Combine(FileSystem.Current.AppDataDirectory, "AllFactions.txt"));
            string json = r.ReadToEnd();
            List<Unit> items = JsonConvert.DeserializeObject<List<Unit>>(json);
            foreach (Unit item in items)
            {
                Units.Add(item);
            }
        }

        [RelayCommand]
        private void UploadIndexFile(Faction faction)
        { 
            
        }
    }
}
