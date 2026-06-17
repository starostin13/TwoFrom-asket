namespace UnitRosterGenerator
{
    public class Roster
    {
        public List<UnitConfiguration> UnitConfigurations { get; set; } = new();
        public Detach? SelectedDetach { get; set; } // Выбранный детач, если он есть
        public List<Detach> SelectedDetaches { get; set; } = new();

        public Roster(List<UnitConfiguration> unitConfigurations, Detach? selectedDetach)
        {
            UnitConfigurations = unitConfigurations;
            SelectedDetach = selectedDetach;
            if (selectedDetach != null)
            {
                SelectedDetaches = new List<Detach> { selectedDetach };
            }
        }

        public Roster(List<UnitConfiguration> unitConfigurations, Detach? selectedDetach, List<Detach>? selectedDetaches)
        {
            UnitConfigurations = unitConfigurations;
            SelectedDetach = selectedDetach;
            SelectedDetaches = selectedDetaches ?? new List<Detach>();

            if (SelectedDetach == null && SelectedDetaches.Count > 0)
            {
                SelectedDetach = SelectedDetaches[0];
            }
        }

        // Метод для вычисления общей стоимости ростера
        public int CalculateTotalCost()
        {
            int totalCost = 0;
            foreach (var unitConfig in UnitConfigurations)
            {
                totalCost += unitConfig.TotalCost;
            }
            return totalCost;
        }
    }
}
