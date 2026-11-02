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

        private async void OnRosterTapped(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is GeneratedRosterViewModel roster)
            {
                await Clipboard.Default.SetTextAsync(roster.CopyText);
                if (BindingContext is UnitsViewModel viewModel)
                {
                    viewModel.GenerationProgress = $"Copied {roster.Title}.";
                }
            }
        }
    }
}