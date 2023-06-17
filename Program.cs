var roster = new Roster();

var mainFaction = new Faction();

var r = new Random();
var isRosterHaveFreePoints = true;
while (isRosterHaveFreePoints)
{
    ExemplarUnit exemplar;
    var randomUnit = mainFaction.units[r.Next(0, mainFaction.units.Count())];
    if(randomUnit.LeadedUnits is not null)
    {
        var attachedUnits = new List<Unit>();
        foreach (var lu in randomUnit.LeadedUnits)
            attachedUnits.AddRange(mainFaction.units.Where(mfu => mfu.Name.Contains(lu, StringComparison.InvariantCultureIgnoreCase)));

        exemplar = new ExemplarUnit(randomUnit, attachedUnits[r.Next(0, attachedUnits.Count)]);

        var isAddEnchasment = new Random();
        if(isAddEnchasment.NextDouble() < 50)
        {
            var randomEnch = popRandom(mainFaction.enchasments);
            exemplar.AddEnchasment(randomEnch.Item1, randomEnch.Item2);
        }
    }
    else
    {
        exemplar = new ExemplarUnit(randomUnit);
    }
    
    isRosterHaveFreePoints = roster.AddExemplarIfAcceptable(exemplar);
}

(string, int) popRandom(Stack<(string, int)> enchasments)
{
    var r = new Random();
    var tempList = new List<(string, int)>();
    var randomNumber = r.Next(enchasments.Count);
    for(var i = 0; i <= randomNumber; i++)
    {
        tempList.Add(enchasments.Pop());
    }
    
    var result = tempList.LastOrDefault();
    for (var i = 0; i < randomNumber; i++)
    {
        enchasments.Push(tempList[i]);
    }
    return result;
}

foreach (var unit in roster.ArmyList)
{
    Console.Write($"{unit.Name} cost is {unit.Price}");
    if(unit.Enchasment is not  null)
    {
        Console.WriteLine($"{Environment.NewLine}He also have {unit.Enchasment}");
    }
    if (unit.LeadedUnits is not null)
    {
        Console.WriteLine(" can lead:");
        foreach (var lu in unit.LeadedUnits)
        {
            Console.WriteLine(" " + lu);
        }
    }
    else { Console.Write(Environment.NewLine); }
}

Console.WriteLine($"Total price: {roster.Price}");

// See https://aka.ms/new-console-template for more information

