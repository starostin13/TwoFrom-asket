public class ExemplarUnit : Unit
{
    public string Enchasment { get; set; }
    public ExemplarUnit(Unit unit)
    {
        AttachedSquad = unit;
    }

    public ExemplarUnit(Unit mainUnit, Unit attachedSquad)
    {
        MainUnit = mainUnit;
        AttachedSquad = attachedSquad;
    }

    public void AddEnchasment(string enchasmentName, int price)
    {
        Enchasment = enchasmentName;
        MainUnit.Price += price;
    }

    public Unit MainUnit { get; }
    public Unit AttachedSquad { get; }

    public string Name { get
        {
            return MainUnit is not null ? $"{AttachedSquad.Name} lead by {MainUnit.Name}" : $"{AttachedSquad.Name}";
        }
    }

    public int Price { get { return MainUnit is not null ? AttachedSquad.Price + MainUnit.Price : AttachedSquad.Price; }}
}