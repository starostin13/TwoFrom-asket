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
            var r = fileManagerHelper.PickTheFileAsync();

            r.GetAwaiter().OnCompleted(() =>
            {
                FW.BindingContext = new FactionViewModel();
                CounterBtn.IsEnabled = true;
                BusyIndicator.IsRunning = false;
            });
            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }
}