using UglyToad.PdfPig;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace ArmyGeneratorMaui
{
    public class Faction
    {
        public List<Unit> units = new List<Unit>();
        public Stack<(string, int)> enchasments = new Stack<(string, int)>();
        public string FactionName { get; set; }
        public string factionSide;
        private string fullPath;

        public Faction() { }

        public Faction(string fullPath, DataResourceType dataResourceType)
        {
            var fileName = Path.GetFileName(fullPath);
            switch (dataResourceType)
            {
                case DataResourceType.CAT:
                    {
                        var splittedFileName = fileName.Split('-', StringSplitOptions.TrimEntries);
                        factionSide = splittedFileName[0];
                        FactionName = splittedFileName[1];

                        XmlSerializer serializer = new XmlSerializer();
                        var xml1 = XmlSerializer.Deserialize(r);
                    }
                    break;
                case DataResourceType.PDF:
                default:
                {
                    factionSide = "Imperium";
                    this.fullPath = fullPath;
                    FactionName = fileName;

                    var skipPages = 6;

                    var pricesResult = getPrices("GREY KNIGHTS");
                    Dictionary<string, int> prices = pricesResult.Item1;
                    foreach (var ench in pricesResult.Item2)
                    {
                        enchasments.Push((ench.Key, ench.Value));
                    }

                    using (PdfDocument pdfDocument = PdfDocument.Open(fullPath))
                    {
                        var pages = pdfDocument.GetPages();
                        var sheet = pages.Skip(skipPages).Take(2);

                        for (var i = skipPages; i < pages.Count(); i += 2)
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
                                    if (letters2.FontName != currentFontName && letters != lines.FirstOrDefault())
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
                                if (sentences?.LastOrDefault()?.FontName == sentence.FontName && sentences?.LastOrDefault()?.TextSequence - sentence.TextSequence < -1)
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
                                foreach (var line in leadingList)
                                {
                                    if (shouldBeNextLineBeAdded)
                                    {
                                        unit.AddLeadedUnit(line.Value);
                                        shouldBeNextLineBeAdded = false;
                                    }

                                    if (line.Value == "■")
                                        shouldBeNextLineBeAdded = true;
                                }
                            }

                            var namepatter = @"\b" + unit.Name + @"\d+\smodel[s]*\b";

                            foreach (var price in prices.Where(p => Regex.IsMatch(p.Key, namepatter, RegexOptions.IgnoreCase))) // p.Key.Contains(unit.Name, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                var newUnit = (Unit)unit.Clone();
                                newUnit.Price = price.Value;
                                units.Add(newUnit);
                            }

                        }
                    };
                }
                break;
            }
        }

        private (Dictionary<string, int>, Dictionary<string, int>) getPrices(string factionName)
        {
            var factionPrices = new Dictionary<string, int>();
            var enchasmentPrices = new Dictionary<string, int>();
            using (PdfDocument pdfDocument = PdfDocument.Open("C:/Users/al-gerasimov/YandexDisk/WH40K/MUNITORUM.pdf"))
            {
                var pages = pdfDocument.GetPages();
                var page = pages.Skip(1).SkipWhile(p => !p.Text.Contains(factionName)).Take(1).FirstOrDefault();
                var reg = @"[\w\s()-]+ \.*\d+\s*pts";
                var units = page.Text;
                const string DETACHMENTENHANCEMENTS = "DETACHMENT ENHANCEMENTS";
                MatchCollection matches = Regex.Matches(units, reg);

                factionPrices = getMatches(matches);

                var enchasments = page.Text.Substring(page.Text.IndexOf(DETACHMENTENHANCEMENTS) + DETACHMENTENHANCEMENTS.Length + "Teleport Strike Force".Length);
                MatchCollection matchesEnch = Regex.Matches(enchasments, @"[\w\s()-]+\.*\d+\s*pts");

                enchasmentPrices = getMatches(matchesEnch);
            }

            return (factionPrices, enchasmentPrices);
        }

        private Dictionary<string, int> getMatches(MatchCollection matches)
        {
            var factionPrices = new Dictionary<string, int>();
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

            return factionPrices;
        }
    }
}