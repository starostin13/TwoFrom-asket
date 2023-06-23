namespace ArmyGeneratorMaui
{
    internal class ViewHelper
    {
        internal static string ExtractText(object sender)
        {
            if(sender.GetType() != typeof(Label))
            {
                return string.Empty;
            }
            else
            {
                return ((Label)sender).Text;
            }
        }
    }
}