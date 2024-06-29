namespace Task.Connector.Records
{
    public record UserProperties(
        string FirstNamePropertyName,
        string MiddleNamePropertyName,
        string LastNamePropertyName,
        string TelephoneNumberPropertyName,
        string IsLeadPropertyName)
    {
    }
}
