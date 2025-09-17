namespace TR.Connectors.Api.Interfaces;

public interface ILogger
{
    public void Debug(string message);
    public void Error(string message);
    public void Warn(string message);
}