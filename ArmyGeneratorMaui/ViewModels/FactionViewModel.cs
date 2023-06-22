using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ArmyGeneratorMaui.ViewModels
{
    internal class FactionViewModel : INotifyPropertyChanged
    {
        readonly IList<Unit> source;
        Unit selectedUnit;
        int selectionCount = 1;
        private ObservableCollection<Unit> units = new ObservableCollection<Unit>();

        public ObservableCollection<Unit> Units { get => units; private set => units = value; }
        public Unit SelectedUnit
        {
            get { return selectedUnit; }
            set { if(selectedUnit != value)
                {
                    selectedUnit = value;
                }
            }
        }


        public string SelectedUnitMessage { get; private set; }

        public FactionViewModel()
        {
            foreach(var un in Core.MainFaction?.units)
            {
                Units.Add(un);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}