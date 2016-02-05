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

        public string Name => GetType().Name;

        public bool IsWaitOnStop => true;

        public bool IsShutdownOnStop => true;

        public Task OnStartAsync(string[] args, CancellationToken cancelToken)
        {
            _logger.Info("EmailServiceTask.OnStart");
            return Task.FromResult(true);
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                _logger.Info("EmailServiceTask.RunAsync - Send Email");
            }
        }

        public Task OnStopAsync(CancellationToken cancellationToken)
        {
            _logger.Info("EmailServiceTask.OnStop");
            return Task.FromResult(true);
        }
    }
}