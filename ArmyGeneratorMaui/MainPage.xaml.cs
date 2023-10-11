using ArmyGeneratorMaui.ViewModels;

namespace ArmyGeneratorMaui
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            BindingContext = new UnitsViewModel();
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

        private void OnGenerateClick(object sender, EventArgs e)
        {
            Core.GenerateRoster();
            RosterView.BindingContext = new RosterViewModel();
        }
    }
}