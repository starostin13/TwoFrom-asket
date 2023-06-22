using System;



public class Roster
{
    public Subfraction subfraction;
    public CompulsoryType compulsoryType;

    public int MaxSize { get; }

    private List<ExemplarUnit> armyList;

    public Roster(int size = 2000)
    {
        MaxSize = size;
        armyList = new List<ExemplarUnit>();
    }

    public int Price
    {
        get
        {
            var cost = 0;
            if (armyList is null) return 0;
            foreach (var unit in armyList)
            {
                cost += unit.Price;
            }
            return cost;
        }
    }

    public List<ExemplarUnit> ArmyList { get => armyList; private set => armyList = value; }

    internal bool AddExemplarIfAcceptable(ExemplarUnit exemplar)
    {
        if (this.Price + exemplar.Price > MaxSize)
            return false;
        else
        {
            armyList.Add(exemplar);
            return true;
        }
    }
}

public enum Subfraction
{
    Swordbeares,
    BladesOfVictory,
    Wardmakers,
    PrescientBrethern,
    Preservers,
    Rapiers,
    Exactors,
    SilverBlades
}

public enum CompulsoryType
{
    FastAttack, HeavySupport, Troops
}
