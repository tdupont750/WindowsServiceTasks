using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Common.Logging.Simple;

namespace WindowsServiceTasks
{
    public class ConsoleWindowsService : ServiceBase
    {
        private static readonly TimeSpan StartStopTimeoutMin = TimeSpan.FromSeconds(15);
        private static readonly TimeSpan StartStopTimeoutMax = TimeSpan.FromSeconds(25);

        private readonly Lazy<ConsoleOutLogger> _lazyConsoleLogger;
        private readonly object _startStopLock;
        private readonly CancellationTokenSource _runCancelSource;

        private readonly ILog _registeredlogger;
        private readonly IList<ServiceTaskPair> _taskPairs;

        public ConsoleWindowsService(ILog registeredlogger, params IWindowsServiceTask[] windowsServiceTasks)
        {
            // Ensure that we have service tasks to run.
            if (windowsServiceTasks.Length == 0)
                throw new ArgumentException("WindowsServiceTasks Required", nameof(windowsServiceTasks));

            _startStopLock = new object();
            _runCancelSource = new CancellationTokenSource();
            _lazyConsoleLogger = new Lazy<ConsoleOutLogger>(() => new ConsoleOutLogger("ConsoleWindowsService", LogLevel.All, true, true, true, string.Empty, true));

            _registeredlogger = registeredlogger;
            _taskPairs = windowsServiceTasks.Select(ServiceTaskPair.Create).ToList();
        }

        public void Start(params string[] args)
        {
            // If not user interactive then run the service normally.
            if (!Environment.UserInteractive)
            {
                Run(new ServiceBase[] { this });
                return;
            }

            // Inform user about console mode.
            _lazyConsoleLogger.Value.Info("Running on console mode, press enter to stop.");

            // Running in console mode, call OnStart.
            OnStart(args);

            // Wait for user input to before shutting down.
            Console.ReadLine();

            if (_runCancelSource.IsCancellationRequested)
                // Something already stopped the services.
                _lazyConsoleLogger.Value.Warn("Service was already stopped.");
            else
            // Call OnStop before to initiate shutdown.
                OnStop();

            // Let user read output before shutting down.
            _lazyConsoleLogger.Value.Info("Press enter to exit.");
            Console.ReadLine();
        }

        protected override void OnStart(string[] args)
        {

            LockAndTry(() =>
            {
                _lazyConsoleLogger.Value.Debug("Starting...");

                // Call OnStart for each service, same way windows would.
                using (var startCancelSource = new CancellationTokenSource(StartStopTimeoutMin))
                {
                    var startTasks = new Task[_taskPairs.Count];

                    for (var i = 0; i < _taskPairs.Count; i++)
                        startTasks[i] = _taskPairs[i].ServiceTask.OnStartAsync(args, startCancelSource.Token);

                    Task.WaitAll(startTasks, StartStopTimeoutMax);
                }

                // Call RunAsync for each service.
                foreach (var pair in _taskPairs)
                    pair.Task = RunServiceAsync(pair.ServiceTask);

                _lazyConsoleLogger.Value.Debug("...Started");
            });
        }

        private async Task RunServiceAsync(IWindowsServiceTask serviceTask)
        {
            Exception exception = null;

            try
            {
                await serviceTask.RunAsync(_runCancelSource.Token).ConfigureAwait(false);
            }
            catch (TaskCanceledException ex)
            {
                if (!_runCancelSource.IsCancellationRequested)
                    exception = ex;
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (exception != null)
            {
                if (Environment.UserInteractive)
                    _lazyConsoleLogger.Value.Error(exception);

                // ReSharper disable once InconsistentlySynchronizedField
                _registeredlogger.ErrorFormat("ConsoleWindowsService.RunServiceAsync - Unhandled Exception - Name: {0}", exception, serviceTask.Name);
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
            LockAndTry(() =>
            {
                // Exit if another thread has already initiated shutdown.
                if (_runCancelSource.IsCancellationRequested)
                    return true;

                _lazyConsoleLogger.Value.Debug("Stopping...");

                // Signal shutdown.
                _runCancelSource.Cancel();

                // Call OnStop for each service, same way windows would.
                using (var stopCancelSource = new CancellationTokenSource(StartStopTimeoutMin))
                {
                    var stopTasks = new Task[_taskPairs.Count];

                    for (var i = 0; i < _taskPairs.Count; i++)
                        stopTasks[i] = _taskPairs[i].ServiceTask.OnStopAsync(stopCancelSource.Token);

                    Task.WaitAll(stopTasks, StartStopTimeoutMax);
                }

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
                Task.WaitAll(tasks, StartStopTimeoutMax);

                _lazyConsoleLogger.Value.Debug("...Stopped");

                // Success if normal OnStop call from ServiceProcess, otherwise
                // this was unexpected and should cause a shutdown. 
                return serviceTask == null;
            });
        }

        private void LockAndTry(Action action)
        {
            LockAndTry(() =>
            {
                action();
                return true;
            });
        }

        private void LockAndTry(Func<bool> func)
        {
            lock (_startStopLock)
            {
                bool isSuccess;

                try
                {
                    isSuccess = func();
                }
                catch (Exception ex)
                {
                    _lazyConsoleLogger.Value.Fatal(ex);
                    _registeredlogger.Fatal(ex);

                    isSuccess = false;
                }

                if (isSuccess)
                    return;

                if (Environment.UserInteractive)
                    _lazyConsoleLogger.Value.Fatal("Service has stopped unexpectedly.");
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

            public IWindowsServiceTask ServiceTask { get; }

            public Task Task { get; set; }
        }
    }
}