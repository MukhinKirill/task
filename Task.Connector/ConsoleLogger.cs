using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task.Integration.Data.Models;

namespace Task.Connector
{
    public class ConsoleLogger : ILogger
    {
        public void LogInformation(string message, params object[] args)
        {
            Console.WriteLine("Info: " + message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            Console.WriteLine("Warning: " + message, args);
        }

        public void LogError(Exception exception, string message, params object[] args)
        {
            Console.WriteLine("Error: " + message + "\nException: " + exception.Message, args);
        }
    }
}
