using WindowsServiceTasks.Demo.ServiceTasks;
using WindowsServiceTasks.Demo.Services.Implementation;

namespace WindowsServiceTasks.Demo
{
    public static class Program
    {
        public static void Main(params string[] args)
        {
            var logger = new Logger("Demo.log");
            var emailTask = new EmailServiceTask(logger);
            var failTask = new FailServiceTask(logger);

            var service = new Service1(logger, emailTask, failTask);
            service.Start(args);
        }
    }
}
