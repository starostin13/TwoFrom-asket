using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArmyGeneratorMaui.ViewModels
{
    internal partial class RosterViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<ExemplarUnit> units;

        [ObservableProperty]
        private ObservableCollection<SavedRosterInfo> savedRosters;

        [ObservableProperty]
        private SavedRosterInfo selectedRoster;

        public RosterViewModel()
        {
            units = new ObservableCollection<ExemplarUnit>();
            savedRosters = new ObservableCollection<SavedRosterInfo>();
            
            LoadSavedRosters();
            LoadCurrentRoster();
        }

        private void LoadSavedRosters()
        {
            SavedRosters.Clear();
            var saved = StorageHelper.LoadSavedRosters();
            foreach (var roster in saved)
            {
                SavedRosters.Add(roster);
            }
        }

        private void LoadCurrentRoster()
        {
            Units.Clear();
            if (Core.Roster?.ArmyList != null)
            {
                foreach (var unit in Core.Roster.ArmyList)
                {
                    Units.Add(unit);
                }
            }
        }

        [RelayCommand]
        private void RosterSelected(SavedRosterInfo roster)
        {
            if (roster != null)
            {
                SelectedRoster = roster;
                var loadedRoster = StorageHelper.LoadRoster(roster.FilePath);
                if (loadedRoster != null)
                {
                    Core.Roster = loadedRoster;
                    LoadCurrentRoster();
                }
            }
        }
    }
}

