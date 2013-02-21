using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsServiceTasks.Base
{
    public abstract class WindowsServiceBase : ServiceBase
    {
        private readonly Task[] _runTasks;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IWindowsServiceTask[] _windowsServiceTasks;

        private bool _hasOnStopFired;

        protected WindowsServiceBase(params IWindowsServiceTask[] windowsServiceTasks)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _windowsServiceTasks = windowsServiceTasks;
            _runTasks = new Task[windowsServiceTasks.Length];
        }

        public void Start(string[] args)
        {
            if (!Environment.UserInteractive)
            {
                Run(new[] { (ServiceBase)this });
                return;
            }

            Console.WriteLine("Starting...");
            OnStart(args);
            Console.WriteLine("...Started");

            Console.WriteLine();
            Console.WriteLine("Press enter to stop.");
            Console.ReadLine();

            Console.WriteLine("Stopping...");
            OnStop();
            Console.WriteLine("...Stopping");
        }

        protected override void OnStart(string[] args)
        {
            foreach (var serviceTask in _windowsServiceTasks)
                serviceTask.OnStart(args);

            for (var i = 0; i < _windowsServiceTasks.Length; i++)
            {
                var serviceTask = _windowsServiceTasks[i];
                _runTasks[i] = Task.Factory.StartNew(RunServiceTask, serviceTask, _cancellationTokenSource.Token);
            }
        }

        private void RunServiceTask(object state)
        {
            try
            {
                var serviceTask = (IWindowsServiceTask)state;
                serviceTask.Run(_cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // Ok
            }
            catch (Exception ex)
            {
                if (Environment.UserInteractive)
                    Console.WriteLine(ex.ToString());

                LogException(ex);

                // Shut down service (use another thread to prevent deadlock)
                if (!_hasOnStopFired)
                    Task.Factory.StartNew(Stop);
            }
        }

        protected override void OnStop()
        {
            _hasOnStopFired = true;

            _cancellationTokenSource.Cancel();

            Task.WaitAll(_runTasks);

            foreach (var runTask in _runTasks)
                runTask.Dispose();

            foreach (var serviceTask in _windowsServiceTasks)
                serviceTask.OnStop();

            foreach (var serviceTask in _windowsServiceTasks)
                serviceTask.Dispose();
        }

        protected abstract void LogException(Exception exception);
    }
}