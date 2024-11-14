using Task.Integration.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector
{
    internal class NullLogger : ILogger
    {
        public void Debug(string message)
        {
            
        }

        public void Error(string message)
        {
            
        }

        public void Warn(string message)
        {
            
        }
    }
}
