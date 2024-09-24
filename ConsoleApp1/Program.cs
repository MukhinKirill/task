using ConsoleApp1;
using Task.Connector;
using Task.Integration.Data.Models.Models;

FileLoggerTest logger = new("LogFile.txt", "connector");


var connector = new ConnectorDb(logger);
//connector.StartUp("Data Source=.;Initial Catalog=AvanpostService;User ID=sa;Password=cv4Frwtj98uy_m;Trust Server Certificate=True");
string connectionString = "Data Source=.;Initial Catalog=AvanpostService;User ID=sa;Password=cv4Frwtj98uy_m;TrustServerCertificate=True";
Console.WriteLine($"{DateTime.Now.ToString().Replace(":","")}connectorMSSQL.Log");

//connector.StartUp($"ConnectionString='{connectionString}';Provider='SqlServer.2019';SchemaName='AvanpostIntegrationTestTaskSchema';");
connector.StartUp($"ConnectionString='{connectionString}';Provider='SqlServer.2019';SchemaName='TestTaskSchema';");

//connector.StartUp("Data Source=HOME-PC\\SQLEXPRESS;Initial Catalog=TestACAD;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");
//connector.TestMethod();

//connector.TestMethod("ConnectionString='gvbfdbgf'hfggnvbggnngbvgb';Provider='SqlServer.2019';SchemaName='AvanpostIntegrationTestTaskSchema';");

UserToCreate user1 = new("LTest1", "Password1") { Properties = new List<UserProperty>() {  
        new("lastName", "lastNameTest1"),
        new("firstName", "firstNameTest1"),
        new("middleName", "middleNameTest1"),
        new("telephoneNumber", "telephoneNumberTest1"),
        new("isLead", "true")
    } 
};

UserToCreate user2 = new("LTest12", "Password2")
{
    Properties = new List<UserProperty>() {
        new("middleName", "middleNameTest2"),
        new("middleName", "middleNameTest20"),
        new("lastName", "lastNameTest2"),
        new("isLead", "TrUe"),
        new("dgdrsgf", "dgdrsgf2"),
        new("firstName", "firstNameTest2"),
        new("telephoneNumber", "telephoneNumberTest2")
    }
};

UserToCreate userAnyTest = new("LTest1", "Password")
{
    Properties = new List<UserProperty>() {
        new("lastName", "lastNameTest"),
        new("firstName", "firstNameTest"),
        new("middleName", "middleNameTest"),
        new("telephoneNumber", "telephoneNumberTest"),
        new("isLead", "isLead")
    }
};

connector.CreateUser(user2);

//foreach (var item in connector.GetAllProperties())
//{
//    Console.WriteLine($"Свойство {item.Name}, значение/описание {item.Description}");
//}

//foreach (var item in connector.GetUserProperties("LTest1"))
//{
//    Console.WriteLine($"Свойство {item.Name}, значение/описание {item.Value}");
//}

//Console.WriteLine(connector.IsUserExists("LTest0"));

List<UserProperty> userProperties1 = new() { 
    new("lastName", "ChangedTest6"),
    new("lastName", "ChangedTest6"),
    new("middleName", "ChangedTest6"),
    new("middleName", "ChangedTest60"),
    new("isLead", "1")
};

//connector.UpdateUserProperties(userProperties1, "LTest1");

//foreach (var item in connector.GetAllPermissions())
//{
//    Console.WriteLine($"Право {item.Name}, {item.Id}, значение/описание {item.Description}");
//}

//var emptyList = new List<string>();
//connector.AddUserPermissions("LTest1", emptyList);

//connector.AddUserPermissions("LTest2", new List<string>() { "1", "2", "3", "3" });

//connector.RemoveUserPermissions("LTest1", new List<string>() { "fgdfgf", "2", "3", "3" });

foreach (var item in connector.GetUserPermissions("GlavnyyNN"))
{
    Console.WriteLine($"Право {item}");
}

//connector.Logger = new 