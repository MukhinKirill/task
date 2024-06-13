using Task.Connector.Config;
using Task.Connector.Config.Parse;
using Xunit.Abstractions;

namespace Task.Connector.Tests.ConfigParser;

public class UrlParserTest
{
    private readonly ITestOutputHelper _helper;

    public UrlParserTest(ITestOutputHelper h)
    {
        _helper = h;
    }
    
    [Theory]
    [MemberData(nameof(Urls))]
    public void ShouldParseUrls(string url)
    {
        ConnectionScheme? connUrl = null;
        try
        {
            connUrl = ConnectionUrlParser.Default.Parse(url);
        }
        catch (Exception e)
        {
            Assert.Fail($"did not want an exception, but got one {e}");
            return;
        }
        
        Assert.NotNull(connUrl);
        _helper.WriteLine(connUrl.ToString());
    }

    [Theory]
    [MemberData(nameof(InvalidUrls))]
    public void ShouldNotParseUrls(string url)
    {
        ConnectionScheme? connUrl = null;
        Assert.Throws<UrlParserFormatException>(() =>
        {
            connUrl = ConnectionUrlParser.Default.Parse(url);
        });
        Assert.Null(connUrl);
    }

    public static IEnumerable<object[]> Urls()
    {
        yield return new object[] { "posgres://user:pass@localhost:1234/db?param1=value1&param2=value2" };
        yield return new object[] { "posgres://user:pass@localhost/db" };
        yield return new object[] { "posgres://user@localhost:1234/db?param1=value1" };
        yield return new object[] { "mysql://user@localhost:1234?param1=value1" };
        yield return new object[] { "sqlite://c/users/test/db.sqlite" };
    }

    public static IEnumerable<object[]> InvalidUrls()
    {
        yield return new object[] { "" };
        yield return new object[] { "garbage" };
        yield return new object[] { "://" };
        yield return new object[] { "www.example.com" };
        yield return new object[] { "user:pass@localhost" };
        yield return new object[] { "://pass:p@host:1234/?w=x" };
    }
}