using System.Text.Json;

namespace Task.Connector.Tests
{
    internal class Task
    {
        public static void Main(string[] args)
        {
            UnitTest1 unitTest1 = new UnitTest1();
            unitTest1.CreateUser("MSSQL");
            unitTest1.IsUserExists("MSSQL");
            unitTest1.GetAllProperties("MSSQL");
            unitTest1.GetUserProperties("MSSQL");
            unitTest1.UpdateUserProperties("MSSQL");
            unitTest1.GetAllPermissions("MSSQL");
            unitTest1.AddUserPermissions("MSSQL");
            unitTest1.RemoveUserPermissions("MSSQL");
            unitTest1.GetUserPermissions("MSSQL");

            Console.WriteLine("Все тесты прошли!");
        }

    }
}
