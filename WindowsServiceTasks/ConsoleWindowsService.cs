using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;

namespace WindowsServiceTasks
{
    public class ConsoleWindowsService : ServiceBase
    {
        private readonly ILog _logger;
        private readonly object _startStopLock;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IList<ServiceTaskPair> _taskPairs;

        public ConsoleWindowsService(ILog logger, params IWindowsServiceTask[] windowsServiceTasks)
        {
            // Ensure that we have service tasks to run.
            if (!windowsServiceTasks.Any())
                throw new ArgumentException("WindowsServiceTasks Required", "windowsServiceTasks");
            
            _logger = logger;
            _startStopLock = new object();
            _cancellationTokenSource = new CancellationTokenSource();

            // Bind service tasks into pairs to assiciate them with tasks.
            _taskPairs = windowsServiceTasks.Select(ServiceTaskPair.Create).ToList();
        }

        public void Start(params string[] args)
        {
            // If not user interactive then run the service normally.
            if (!Environment.UserInteractive)
            {
                Run(new[] { (ServiceBase)this });
                return;
            }

            // Running in console mode, call OnStart.
            Console.WriteLine("Starting...");
            OnStart(args);
            Console.WriteLine("...Started");

            // Wait for user input to before shutting down.
            Console.WriteLine();
            Console.WriteLine("Press enter to stop.");
            Console.ReadLine();

            if (_cancellationTokenSource.IsCancellationRequested)
            {
                // Something already stopped the services.
                Console.WriteLine("Already stopped.");
            }
            else
            {
                // Call OnStop before to initiate shutdown.
                Console.WriteLine("Stopping...");
                OnStop();
                Console.WriteLine("...Stopping");
            }
            
            // Let user read output before shutting down.
            Console.WriteLine();
            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }

        protected override void OnStart(string[] args)
        {
            lock (_startStopLock)
            {
                // Call OnStart for each service, same way windows would.
                foreach (var pair in _taskPairs)
                    pair.ServiceTask.OnStart(args);

                // Call RunAsync for each service.
                foreach (var pair in _taskPairs)
                    pair.Task = RunServiceAsync(pair.ServiceTask);
            }
        }

        private async Task RunServiceAsync(IWindowsServiceTask serviceTask)
        {
            try
            {
                await serviceTask.RunAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                // Ignore task canceled exceptions, it should me that we are shutting down.
            }
            catch (Exception ex)
            {
                if (Environment.UserInteractive)
                    Console.WriteLine(ex.ToString());

                // There was an error, log it!
                _logger.ErrorFormat("ConsoleWindowsService.RunServiceAsync - Unhandled Exception - Name: {0}", ex, serviceTask.Name);
            }

            // If specified, shutdown all serviecs.
            if (serviceTask.IsShutdownOnStop)
                OnStop(serviceTask);
        }

        protected override void OnStop()
        {
            OnStop(null);
        }

        protected void OnStop(IWindowsServiceTask serviceTask)
        {
            lock (_startStopLock)
            {
                // Exit if another thread has already initiated shutdown.
                if (_cancellationTokenSource.IsCancellationRequested)
                    return;

                // Signal shutdown.
                _cancellationTokenSource.Cancel();

                // Call OnStop.
                foreach (var pair in _taskPairs)
                    pair.ServiceTask.OnStop();

                // Find...
                // 1) All OTHER service tasks (to avoid deadlock) 
                // 2) that we should wait on stop
                // 3) and have tasks to wait on.
                // ...and then select their tasks into an array.
                var tasks = _taskPairs
                    .Where(p => !ReferenceEquals(p.ServiceTask, serviceTask))
                    .Where(p => p.ServiceTask.IsWaitOnStop)
                    .Where(p => p.Task != null)
                    .Select(p => p.Task)
                    .ToArray();

                // Wait on the other tasks.
                Task.WaitAll(tasks, TimeSpan.FromSeconds(30));

                // If normal OnStop call, just exit.
                if (serviceTask == null)
                    return;

                // This OnStop was fired by a failure, force shutdown.
                if (Environment.UserInteractive)
                {
                    Console.WriteLine(); 
                    Console.WriteLine("Service has stopped!");
                }
                else
                    Environment.Exit(1);
            }
        }

        private class ServiceTaskPair
        {
            public static ServiceTaskPair Create(IWindowsServiceTask serviceTask)
            {
                return new ServiceTaskPair(serviceTask);
            }

            private ServiceTaskPair(IWindowsServiceTask serviceTask)
            {
                ServiceTask = serviceTask;
            }

            public IWindowsServiceTask ServiceTask { get; private set; }

            public Task Task { get; set; }
        }
    }
}