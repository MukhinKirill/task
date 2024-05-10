namespace Task.Connector.Infrastructure.DataModels;

/// <summary>
/// Модель права.
/// </summary>
public sealed class PermissionDataModel
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// Название.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Описание.
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// Тип.
    /// </summary>
    public string Type { get; set; }
}