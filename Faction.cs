using UglyToad.PdfPig.Content;
using UglyToad.PdfPig;

internal class Faction
{
    public Faction()
    {

        using (PdfDocument pdfDocument = PdfDocument.Open("C:/Users/al-gerasimov/YandexDisk/WH40K/Grey Knights/Grey_knights.pdf"))
        {
            var pages = pdfDocument.GetPages();
            var skipPages = 6;
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
            }
        };

    }
}