using System;
using WindowsServiceTasks.Base;
using WindowsServiceTasks.Demo.Services;

namespace WindowsServiceTasks.Demo.ServiceTasks
{
    public class FailServiceTask : WindowsServiceTaskBase
    {
        private readonly ILogger _logger;

        private int _failCount;

        public FailServiceTask(ILogger logger)
        {
            _logger = logger;
        }

        protected override int LoopMilliseconds
        {
            get { return 5000; }
        }

        protected override void HandleException(Exception exception)
        {
            _failCount++;

            // Rethrowing here will cause the WindowsService running this to stop.
            if (_failCount >= 3)
                throw exception;

            _logger.Log(exception);
        }

        protected override void RunLoop()
        {
            // This method will throw an exception, and after 3 failures the
            // HandleException method will rethrow and cause the service to stop.
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