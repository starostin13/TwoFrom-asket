namespace UnitRosterGenerator
{
    class Roster
    {
        public List<UnitConfiguration> UnitConfigurations { get; set; }
        public Detach? SelectedDetach { get; set; } // Выбранный детач, если он есть

        public Roster(List<UnitConfiguration> unitConfigurations, Detach? selectedDetach)
        {
            UnitConfigurations = unitConfigurations;
            SelectedDetach = selectedDetach;
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
