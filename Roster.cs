using System;



public class Roster
{
    public Subfraction subfraction;
    public CompulsoryType compulsoryType;
    public List<Unit> armyList;

    public Roster(int size = 2000)
    {
        Array values = Enum.GetValues(typeof(Subfraction));
        var random = new Random();
        this.subfraction = (Subfraction)random.Next(Enum.GetValues(typeof(Subfraction)).Length);
        this.compulsoryType = (CompulsoryType)random.Next(Enum.GetValues(typeof(CompulsoryType)).Length);
        var currentSize = 0;
        
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
