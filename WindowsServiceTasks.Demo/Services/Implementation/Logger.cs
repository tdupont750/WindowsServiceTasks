using System;
using System.IO;
using System.Text;

namespace WindowsServiceTasks.Demo.Services.Implementation
{
    public class Logger : ILogger
    {
        private readonly string _logFileName;

        public Logger(string logFileName)
        {
            _logFileName = logFileName;
        }

        public void Log(string message)
        {
            var sb = new StringBuilder();
            sb.Append(DateTime.Now);
            sb.Append(" - ");
            sb.AppendLine(message);

            var logLine = sb.ToString();

            if (Environment.UserInteractive)
                Console.Write(logLine);

            File.AppendAllText(_logFileName, logLine);
        }

        public void Log(Exception ex)
        {
            Log(ex.Message);
        }
    }
}