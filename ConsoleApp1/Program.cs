using Task.Connector;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var connector = new ConnectorDb();
            var connectionString = "Host=localhost;Port=5432;Database=DBForTestTask;Username=postgres;Password=1234;SearchPath=TestTaskSchema";

            connector.StartUp(connectionString);

            var a = connector.GetAllProperties();

            foreach (var a1 in a)
            {
                Console.WriteLine(a1.Name);
            }
        }
    }
}
