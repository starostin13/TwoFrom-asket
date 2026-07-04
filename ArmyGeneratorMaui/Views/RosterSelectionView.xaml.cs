using ArmyGeneratorMaui.ViewModels;

namespace ArmyGeneratorMaui.Views;

public partial class RosterSelectionView : ContentPage
{
	public RosterSelectionView()
	{
		InitializeComponent();
		BindingContext = new RosterSelectionViewModel();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		if (BindingContext is RosterSelectionViewModel vm)
		{
			vm.OnPageAppearing();
		}
	}
}
