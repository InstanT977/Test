using RwmSignatureGenerator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RwmSignatureGenerator.Data.Loggers
{
    public class ConsoleLogger : ILogger
    {
        private void Log(string message)
        {
            Console.WriteLine(message);
        }

        public void Info(string message)
        {
            Log(message);
        }

        public void Error(string message)
        {
            Log($"Error while execution! Message: {message}");
        }

        public void Error(Exception exception)
        {
            var message = $"Error while execution! Message: {exception.Message} {Environment.NewLine} StackTrace : {exception.StackTrace}";
            Log(message);
        }
    }
}
