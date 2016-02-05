using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;

namespace WindowsServiceTasks.Demo.ServiceTasks
{
    public class FailServiceTask : IWindowsServiceTask
    {
        private readonly ILog _logger;
        
        public FailServiceTask(ILog logger)
        {
            _logger = logger;
        }

        public string Name => GetType().Name;

        public bool IsWaitOnStop => true;

        public bool IsShutdownOnStop => true;

        public Task OnStartAsync(string[] args, CancellationToken cancelToken)
        {
            _logger.Info("FailServiceTask.OnStart");
            return Task.FromResult(true);
        }

        public async Task RunAsync(CancellationToken cancelToken)
        {
            await Task.Delay(2500, cancelToken).ConfigureAwait(false);
            
            if (!cancelToken.IsCancellationRequested)
                throw new Exception("FailServiceTask is failing!");
        }

        public Task OnStopAsync(CancellationToken cancelToken)
        {
            _logger.Info("FailServiceTask.OnStop");
            return Task.FromResult(true);
        }
    }
}