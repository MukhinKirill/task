namespace Task.Connector.Domain;

internal class Instruction
{
    public string Text { get; set; }

    public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
}