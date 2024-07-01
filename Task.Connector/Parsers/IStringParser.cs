namespace Task.Connector.Parsers
{
    public interface IStringParser<TOut>
    {
        TOut Parse(string input);
    }
}
