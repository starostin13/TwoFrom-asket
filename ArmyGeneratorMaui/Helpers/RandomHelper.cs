namespace ArmyGeneratorMaui.Helpers
{
    internal static class RandomHelper
    {
        internal static (string, int) PopRandom(Stack<(string, int)> enchasments)
        {
            var r = new Random();
            var tempList = new List<(string, int)>();
            var randomNumber = r.Next(enchasments.Count);
            for (var i = 0; i <= randomNumber; i++)
            {
                bool isHaveNode = enchasments.TryPop(out var node);
                if (isHaveNode) tempList.Add(node);
            }

            var result = tempList.LastOrDefault();
            for (var i = 0; i < randomNumber; i++)
            {
                enchasments.Push(tempList[i]);
            }
            return result;
        }
    }
}
