internal class Model
{
    public Role Role;
    public RangeWeapone RangeWeapone;
    public MeleeWeapone MeleeWeapone;
    Boolean IsTerminator;

    public Model(ref List<Model> squad)
    {
        var r = new Random();
        if(squad.All(m => m.Role != Role.Sergant))
        {
            this.MeleeWeapone = (MeleeWeapone)r.Next(1, Enum.GetValues(typeof(MeleeWeapone)).Length);
            this.IsTerminator = false;
            this.RangeWeapone = RangeWeapone.Bolter;
            this.Role = Role.Sergant;
        }
        else
        {   
            var role = squad.Any(m => m.Role == Role.Specialist) ? Role.Private : (Role)r.Next(1, Enum.GetValues(typeof(Role)).Length);
            this.Role = role;
            switch (role)
            {
                case Role.Private:
                    this.MeleeWeapone = (MeleeWeapone)r.Next(1, Enum.GetValues(typeof(MeleeWeapone)).Length);
                    this.IsTerminator = false;
                    this.RangeWeapone = RangeWeapone.Bolter;
                    break;
                case Role.Specialist:
                    this.MeleeWeapone = MeleeWeapone.None;
                    this.IsTerminator = false;
                    this.RangeWeapone = (RangeWeapone)r.Next(1, Enum.GetValues(typeof(RangeWeapone)).Length);
                    break;
            }
        }
    }
}

internal enum MeleeWeapone
{
    None = 0, Falcion = 1, Halbert = 2, Sword = 3, Stave = 4, Hammer = 5
}

internal enum RangeWeapone
{
    Bolter = 0, Incinerator = 1, Psilencer = 2, Psycannon = 3
}

internal enum Role
{
    Sergant = 0, Private = 1, Specialist = 2
}