using WindowsServiceTasks.Demo.ServiceTasks;
using Common.Logging;
using Common.Logging.Simple;

namespace WindowsServiceTasks.Demo
{
    public static class Program
    {
        public static void Main(params string[] args)
        {
            var logger = new ConsoleOutLogger("Default", LogLevel.All, true, true, true, string.Empty, true);
            var emailTask = new EmailServiceTask(logger);
            var failTask = new FailServiceTask(logger);

            using (var service = new ConsoleWindowsService(logger, emailTask, failTask))
                service.Start(args);
        }
    }
}
