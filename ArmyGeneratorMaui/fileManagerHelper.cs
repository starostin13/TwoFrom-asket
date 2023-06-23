namespace ArmyGeneratorMaui
{
    internal class fileManagerHelper
    {
        internal static async Task PickTheFileAsync()
        {
            try
            {
                var result = await FilePicker.Default.PickAsync();
                if (result != null)
                {
                    if (result.FileName.EndsWith("pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        using var stream = await result.OpenReadAsync();
                    }
                }

                var faction = new Faction(result.FullPath);
                StorageHelper.SaveFaction(faction);
                Core.SetCurrentFaction(faction);
            }
            catch (Exception ex)
            {
                // The user canceled or something went wrong
            }
        }
    }
}