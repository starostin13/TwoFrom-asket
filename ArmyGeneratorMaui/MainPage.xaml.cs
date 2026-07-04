using ArmyGeneratorMaui.ViewModels;

namespace ArmyGeneratorMaui
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            BindingContext = new UnitsViewModel();
            RosterView.BindingContext = new RosterViewModel();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            CounterBtn.IsEnabled = false;
            BusyIndicator.IsRunning = true;
            //var r = FileManagerHelper.PickTheFileAsync();

            /*r.GetAwaiter().OnCompleted(() =>
            {                
                //unitsViewModel.UploadIndexFileCommand.Execute(Core.MainFaction);
                CounterBtn.IsEnabled = true;
                BusyIndicator.IsRunning = false;
            });*/
            SemanticScreenReader.Announce(CounterBtn.Text);
        }

        private async void OnGenerateClick(object sender, EventArgs e)
        {
            BusyIndicator.IsRunning = true;
            GenerateBtn.IsEnabled = false;
            
            await Task.Run(() =>
            {
                Core.GenerateRosters(3);
            });
            
            GenerateBtn.IsEnabled = true;
            BusyIndicator.IsRunning = false;
            
            await Shell.Current.GoToAsync("roster-selection");
        }
    }
}