namespace TR.Connectors.Api.Interfaces;

public interface IStatusExtensions
{
    void Lock(string login);
    void Unlock(string login);
}