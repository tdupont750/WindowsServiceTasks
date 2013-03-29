using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsServiceTasks.Base
{
    public abstract class WindowsServiceBase : ServiceBase
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IList<Tuple<IWindowsServiceTask, Task>> _taskPairs;

        private bool _hasOnStopFired;

        protected WindowsServiceBase(params IWindowsServiceTask[] windowsServiceTasks)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _taskPairs = new List<Tuple<IWindowsServiceTask, Task>>(windowsServiceTasks.Length);

            foreach (var windowsServiceTask in windowsServiceTasks)
            {
                var task = new Task(RunServiceTask, windowsServiceTask, _cancellationTokenSource.Token);
                var tuple = new Tuple<IWindowsServiceTask, Task>(windowsServiceTask, task);
                _taskPairs.Add(tuple);
            }
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

            if (_hasOnStopFired) 
                return;

            Console.WriteLine("Stopping...");
            OnStop();
            Console.WriteLine("...Stopping");

            Console.WriteLine();
            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }

        protected override void OnStart(string[] args)
        {
            foreach (var taskPair in _taskPairs)
                taskPair.Item1.OnStart(args);

            foreach (var taskPair in _taskPairs)
                taskPair.Item2.Start();
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

            var tasks = _taskPairs
                .Where(p => p.Item1.WaitOnStop)
                .Select(p => p.Item2)
                .ToArray();

            Task.WaitAll(tasks);

            foreach (var task in tasks)
                task.Dispose();

            foreach (var taskPair in _taskPairs)
                taskPair.Item1.OnStop();

            foreach (var taskPair in _taskPairs)
                taskPair.Item1.Dispose();
        }

        protected abstract void LogException(Exception exception);
    }
}