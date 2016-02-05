using System;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsServiceTasks
{
    /// <summary>
    /// Interface to run a windows service as a task.
    /// Implement this instead of extending System.ServiceProcess.ServiceBase
    /// </summary>
    public interface IWindowsServiceTask
    {
        /// <summary>
        /// Name of the service.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// <value>true</value> if the RunAsync task should be awaited when cancellation is requested.
        /// </summary>
        bool IsWaitOnStop { get; }
        
        /// <summary>
        /// <value>true</value> if all services should shutdown when the RunAsync task completes.
        /// </summary>
        bool IsShutdownOnStop { get; }

        /// <summary>
        /// An async version of System.ServiceProcess.ServiceBase.OnStart
        /// This is different than RunAsync because the process should always stop immediately if this fails.
        /// </summary>
        Task OnStartAsync(string[] args, CancellationToken cancellationToken);

        /// <summary>
        /// The actual task that is running the service.
        /// The returned task will be consumed based on the IsWaitOnStop and IsShutdownOnStop propeties
        /// </summary>
        Task RunAsync(CancellationToken cancellationToken);

        /// <summary>
        /// An async version of System.ServiceProcess.ServiceBase.OnStop
        /// </summary>
        Task OnStopAsync(CancellationToken cancellationToken);
    }
}
