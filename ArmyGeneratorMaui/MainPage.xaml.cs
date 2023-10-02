using ArmyGeneratorMaui.ViewModels;

namespace ArmyGeneratorMaui
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            CounterBtn.IsEnabled = false;
            BusyIndicator.IsRunning = true;
            var r = FileManagerHelper.PickTheFileAsync();

            r.GetAwaiter().OnCompleted(() =>
            {
                var unitsViewModel = new UnitsViewModel();
                unitsViewModel.UploadIndexFileCommand.Execute(Core.MainFaction);
                FW.BindingContext = unitsViewModel;
                CounterBtn.IsEnabled = true;
                BusyIndicator.IsRunning = false;
            });
            SemanticScreenReader.Announce(CounterBtn.Text);
        }

        private void OnGenerateClick(object sender, EventArgs e)
        {
            Core.GenerateRoster();
            RosterView.BindingContext = new RosterViewModel();
        }
    }
}