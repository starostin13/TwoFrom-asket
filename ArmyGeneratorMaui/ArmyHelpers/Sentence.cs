internal class Sentence
{
    private string value;

    public Sentence(string empty) => this.Value = empty;

    public string Value { get => value; set => this.value = value; }
    public string FontName { get; internal set; }
    public int TextSequence { get; internal set; }

    public override string ToString()
    {
        return value;
    }
}