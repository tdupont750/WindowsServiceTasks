using System;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsServiceTasks
{
    public interface IWindowsServiceTask
    {
        string Name { get; }

        bool IsWaitOnStop { get; }
        
        bool IsShutdownOnStop { get; }

        void OnStart(params string[] args);

        Task RunAsync(CancellationToken cancellationToken);

        void OnStop();
    }
}
