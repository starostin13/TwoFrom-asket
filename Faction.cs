using UglyToad.PdfPig;
using System.Text.RegularExpressions;

internal class Faction
{
    public Faction()
    {
        /*string path = "C:/Users/al-gerasimov/YandexDisk/WH40K/Tau/10ed.pdf";
        var skipPages = 8;*/
        string path = "C:/Users/al-gerasimov/YandexDisk/WH40K/Grey Knights/Grey_knights.pdf";
        var skipPages = 6;

        Dictionary<string, int> prices = getPrices("GREY KNIGHTS");
        var units = new List<Unit>();

        using (PdfDocument pdfDocument = PdfDocument.Open(path))
        {
            var pages = pdfDocument.GetPages();
            var sheet = pages.Skip(skipPages).Take(2);

            for (var i = skipPages; i < pages.Count() ; i += 2)
            {
                sheet = pages.Skip(i).Take(2);
                var sentences = new List<Sentence>();
                var sheetLetters = sheet.SelectMany(l => l.Letters);

                var lines = sheetLetters.GroupBy(line => line.StartBaseLine.Y).ToList();
                var currentFontName = lines.FirstOrDefault()?.FirstOrDefault()?.FontName;
                foreach (var letters in lines)
                {
                    var sentence = new Sentence(string.Empty);
                    foreach (var letters2 in letters)
                    {
                        if(letters2.FontName != currentFontName && letters != lines.FirstOrDefault())
                        {
                            sentences.Add(sentence);
                            sentence = new Sentence(string.Empty);
                            currentFontName = letters2.FontName;
                        }

                        currentFontName = letters2.FontName;
                        sentence.Value += letters2.Value;
                        sentence.FontName = currentFontName;
                        sentence.TextSequence = letters2.TextSequence;
                    }
                    if(sentences?.LastOrDefault()?.FontName == sentence.FontName && sentences?.LastOrDefault()?.TextSequence - sentence.TextSequence < -1)
                    {
                        var lastSentense = sentences.LastOrDefault();
                        var tempSentense = sentence;
                        sentences.Remove(sentences.LastOrDefault());
                        sentence.Value = lastSentense.Value + tempSentense.Value;
                    }
                    sentences.Add(sentence);
                }

                var unit = new Unit();
                unit.SetName(sentences.FirstOrDefault()?.Value);

                var leadingList = sentences.SkipWhile(s => !s.Value.Contains("This model can be attached to the following units:"));
                if (leadingList is not null)
                {
                    var shouldBeNextLineBeAdded = false;
                    foreach ( var line in leadingList)
                    {
                        if(shouldBeNextLineBeAdded)
                        {
                            unit.AddLeadedUnit(line.Value);
                            shouldBeNextLineBeAdded = false;
                        }

                        if (line.Value == "■")
                            shouldBeNextLineBeAdded = true;
                    }
                }

                var namepatter = @"\b" + unit.Name + @"\d+\smodel\b";

                foreach (var price in prices.Where(p => Regex.IsMatch(p.Key, namepatter, RegexOptions.IgnoreCase))) // p.Key.Contains(unit.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    var newUnit = unit;
                    newUnit.Price = price.Value;
                    units.Add(unit);

                    Console.Write($"{unit.Name} cost is {unit.Price}");
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

            }
        };

    }

    private Dictionary<string, int> getPrices(string factionName)
    {
        var factionPrices = new Dictionary<string, int>();
        using (PdfDocument pdfDocument = PdfDocument.Open("C:/Users/al-gerasimov/YandexDisk/WH40K/MUNITORUM.pdf"))
        {
            var pages = pdfDocument.GetPages();
            var page = pages.Skip(1).SkipWhile(p => !p.Text.Contains(factionName)).Take(1).FirstOrDefault();
            var reg = @"[\w\s()-]+ \.*\d+\s*pts";
            MatchCollection matches = Regex.Matches(page.Text, reg);

            foreach (Match match in matches)
            {
                var note = match.Groups[0].Value.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                var key = note.FirstOrDefault();
                //case with different number of models
                if (Regex.IsMatch(key.Trim(' '), @"^\d+\s+models$"))
                {
                    factionPrices.Add(
                        factionPrices.LastOrDefault().Key.Substring(0, factionPrices.LastOrDefault().Key.Length - "5 models".Length) + key,
                        Convert.ToInt32(note.LastOrDefault().Substring(0, note.LastOrDefault().Length - 4)));
                }
                else
                {
                    factionPrices.Add(note.FirstOrDefault().Trim(' '), Convert.ToInt32(note.LastOrDefault().Substring(0, note.LastOrDefault().Length - 4)));
                }
            }
        }

        return factionPrices;
    }
}