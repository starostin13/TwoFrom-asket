using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ArmyGeneratorMaui.ViewModels
{
    internal class RosterViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ExemplarUnit> Units { get => units; private set => units = value; }
        public RosterViewModel()
        {
            foreach (var unit in Core.Roster.ArmyList)
            {
                Units.Add(unit);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private ObservableCollection<ExemplarUnit> units = new ObservableCollection<ExemplarUnit>();
    }
}
