namespace ArmyGeneratorMaui
{
    internal class FileManagerHelper
    {
        internal static async Task<Faction> GetFactionFromPdfAsync()
        {
            var result = await FilePicker.Default.PickAsync();
            if (result != null)
            {
                if (result.FileName.EndsWith("pdf", StringComparison.OrdinalIgnoreCase))
                    return new Faction(result.FullPath, DataResourceType.PDF);
                if (result.FileName.EndsWith("cat", StringComparison.OrdinalIgnoreCase))
                    return new Faction(result.FullPath, DataResourceType.CAT);
            }

            return null;
        }
    }
}