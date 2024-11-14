
namespace Task.Connector.ConnectorVerified
{
	internal class Providers
	{
		public enum ProvidersSupported
		{
			SqlServer2019
		}

		public static readonly Dictionary<ProvidersSupported, string> providers = new Dictionary<ProvidersSupported, string>
		{
			{ ProvidersSupported.SqlServer2019, "SqlServer.2019" }
		};
	}
}
