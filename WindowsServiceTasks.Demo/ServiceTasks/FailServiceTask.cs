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
            _logger.Info("FailServiceTask.OnStart");
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(2500, cancellationToken);
            
            if (!cancellationToken.IsCancellationRequested)
                throw new Exception("FailServiceTask is failing!");
        }

        public void OnStop()
        {
            _logger.Info("FailServiceTask.OnStop");
        }
    }
}