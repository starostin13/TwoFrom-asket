﻿using UglyToad.PdfPig.Content;
using UglyToad.PdfPig;

internal class Faction
{
    public Faction()
    {

        using (PdfDocument pdfDocument = PdfDocument.Open("C:/Users/al-gerasimov/YandexDisk/WH40K/Grey Knights/Grey_knights.pdf"))
        {
            foreach (Page page in pdfDocument.GetPages().Skip(6))
            {
                string pageText = page.Text;
                var sentences = new List<Sentence>();

                var lines = page.Letters.GroupBy(line => line.StartBaseLine.Y).ToList();
                var currentFontName = lines.FirstOrDefault()?.FirstOrDefault()?.FontName;
                foreach (var letters in lines)
                {
                    var sentence = new Sentence(string.Empty);
                    foreach (var letters2 in letters)
                    {
                        if(letters2.FontName != currentFontName) 
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
                    if(sentences?.LastOrDefault()?.FontName == sentence.FontName && sentences?.LastOrDefault()?.TextSequence - sentence.TextSequence < 1)
                    //■
                    {
                        var lastSentense = sentences.LastOrDefault();
                        var tempSentense = sentence;
                        sentences.Remove(sentences.LastOrDefault());
                        sentence.Value = lastSentense.Value + tempSentense.Value;
                    }
                    sentences.Add(sentence);
                }

                var unit = new Unit();
                foreach (var s in sentences.OrderBy(q => q.TextSequence))
                {
                    /*if (string.IsNullOrEmpty(Unit.Name))
                        Unit.Name = s.ToString();

                    if (s == "This model can be attached to the following units:")
                    {

                    }
                    */
                    Console.WriteLine(s);
                }
            }
        };

    }
}