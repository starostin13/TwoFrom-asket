using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ArmyGeneratorMaui.ViewModels
{
    internal partial class RosterSelectionViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Roster> rosters;

        [ObservableProperty]
        private Roster selectedRoster;

        private int selectedIndex = 0;

        public RosterSelectionViewModel()
        {
            rosters = new ObservableCollection<Roster>();
        }

        public void OnPageAppearing()
        {
            Rosters.Clear();
            foreach (var roster in Core.GeneratedRosters)
            {
                Rosters.Add(roster);
            }
            if (Rosters.Count > 0)
            {
                SelectedRoster = Rosters[0];
            }
        }

        [RelayCommand]
        private void RosterSelected(Roster roster)
        {
            SelectedRoster = roster;
        }

        [RelayCommand]
        private async Task SaveRoster()
        {
            if (SelectedRoster != null)
            {
                Core.Roster = SelectedRoster;
                await Shell.Current.GoToAsync("..");
            }
        }

        [RelayCommand]
        private async Task SaveAndNameRoster()
        {
            if (SelectedRoster == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No roster selected", "OK");
                return;
            }

            string rosterName = await Application.Current.MainPage.DisplayPromptAsync(
                "Save Roster",
                "Enter roster name:",
                "Save",
                "Cancel",
                "My Roster",
                -1,
                Keyboard.Text);

            if (!string.IsNullOrWhiteSpace(rosterName))
            {
                StorageHelper.SaveRoster(SelectedRoster, rosterName);
                Core.Roster = SelectedRoster;
                await Shell.Current.GoToAsync("..");
            }
        }

        [RelayCommand]
        private async Task Cancel()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
