using System.Threading;
using System.Threading.Tasks;
using Common.Logging;

namespace WindowsServiceTasks.Demo.ServiceTasks
{
    public class EmailServiceTask : IWindowsServiceTask
    {
        private readonly ILog _logger;

        public EmailServiceTask(ILog logger)
        {
            _logger = logger;
        }

        public string Name
        {
            get { return GetType().Name; }
        }

        public bool IsWaitOnStop
        {
            get { return true; }
        }

        public bool IsShutdownOnStop
        {
            get { return true; }
        }

        public void OnStart(params string[] args)
        {
            _logger.Info("EmailServiceTask.OnStart");
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken);

                _logger.Info("EmailServiceTask.RunAsync - Send Email");
            }
        }

        public void OnStop()
        {
            _logger.Info("EmailServiceTask.OnStop");
        }
    }
}