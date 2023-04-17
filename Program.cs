var squad = new List<Model>();
for(var i = 0; i < 5; i++)
{
    squad.Add(new Model(ref squad));    
}

foreach(var model in squad)
{
    switch(model.Role)
    {
        case Role.Private: Console.WriteLine($"{model.MeleeWeapone}"); break;
        case Role.Sergant: Console.WriteLine($"Justicar with {model.MeleeWeapone}"); break;
        case Role.Specialist: Console.WriteLine($"{model.RangeWeapone}"); break;
    }
}

// See https://aka.ms/new-console-template for more information

