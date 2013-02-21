using System;
using System.Threading;

namespace WindowsServiceTasks
{
    public interface IWindowsServiceTask : IDisposable
    {
        void OnStart(string[] args);

        void Run(CancellationToken cancellationToken);

        void OnStop();
    }
}
