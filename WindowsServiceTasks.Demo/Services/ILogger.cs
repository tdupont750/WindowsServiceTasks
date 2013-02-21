using System;

namespace WindowsServiceTasks.Demo.Services
{
    public interface ILogger
    {
        void Log(string message);

        void Log(Exception ex);
    }
}