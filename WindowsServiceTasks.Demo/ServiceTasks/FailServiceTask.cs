using System;
using System.Threading;
using WindowsServiceTasks.Base;
using WindowsServiceTasks.Demo.Services;

namespace WindowsServiceTasks.Demo.ServiceTasks
{
    public class FailServiceTask : WindowsServiceTaskBase
    {
        private readonly ILogger _logger;
        
        public FailServiceTask(ILogger logger)
        {
            _logger = logger;
        }

        public override void Run(CancellationToken cancellationToken)
        {
            cancellationToken.WaitHandle.WaitOne(4500);

            if (!cancellationToken.IsCancellationRequested)
                throw new Exception("FailServiceTask is failing!");
        }

        public override void OnStart(string[] args)
        {
            _logger.Log("FailServiceTask.OnStart");
        }

        public override void OnStop()
        {
            _logger.Log("FailServiceTask.OnStop");
        }

        protected override void DisposeResources()
        {
        }
    }
}