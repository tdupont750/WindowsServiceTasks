using System;
using WindowsServiceTasks.Base;
using WindowsServiceTasks.Demo.Services;

namespace WindowsServiceTasks.Demo.ServiceTasks
{
    public class EmailServiceTask : WindowsServiceTaskBase
    {
        private readonly ILogger _logger;

        public EmailServiceTask(ILogger logger)
        {
            _logger = logger;
        }

        protected override int LoopMilliseconds
        {
            get { return 10000; }
        }

        protected override void HandleException(Exception exception)
        {
            _logger.Log(exception);
        }

        protected override void RunLoop()
        {
            // TODO Send Email!
        }

        public override void OnStart(string[] args)
        {
            _logger.Log("EmailServiceTask.OnStart");
        }

        public override void OnStop()
        {
            _logger.Log("EmailServiceTask.OnStop");
        }

        protected override void DisposeResources()
        {
        }
    }
}