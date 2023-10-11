using ArmyGeneratorMaui.ViewModels;

namespace ArmyGeneratorMaui.Views;

public partial class FactionView : ContentView
{
	public FactionView()
	{
		InitializeComponent();

        /*var unitsViewModel = new UnitsViewModel();
        BindingContext = unitsViewModel;*/
    }

    private void WantThisInRoster(object sender, SwipedEventArgs e)
    {
        Frame g = sender as Frame;
        if (g.BackgroundColor == Color.Parse("Red"))
        {
            g.BackgroundColor = Color.Parse("Transparent");
        }
        else
        {
            g.BackgroundColor = Color.Parse("green");
            VerticalStackLayout child = g.Children[0] as VerticalStackLayout;
            HorizontalStackLayout childhor = child.Children[0] as HorizontalStackLayout;
            Label label = childhor.Children[0] as Label;
            Core.AddToWantToRoster(label.Text);
        }
    }

    private void BlockThisInRoster(object sender, SwipedEventArgs e)
    {
        Frame g = sender as Frame;
        if (g.BackgroundColor == Color.Parse("Green"))
        {
            g.BackgroundColor = Color.Parse("Transparent");
        }
        else
        {
            g.BackgroundColor = Color.Parse("Red");
            VerticalStackLayout child = g.Children[0] as VerticalStackLayout;
            HorizontalStackLayout childhor = child.Children[0] as HorizontalStackLayout;
            Label label = childhor.Children[0] as Label;
            Core.AddToBlockForRoster(label.Text);
        }
    }
}