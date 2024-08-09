namespace Task.Connector;

public class UserPropInfo
{
  public required string Name { get; init; }
  public required string Type { get; init; }
  public required bool IsNotNull { get; init; }
  public string? DefaultValue { get; init; }

}