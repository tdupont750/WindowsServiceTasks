using System;
using System.Threading;

namespace WindowsServiceTasks.Base
{
    public abstract class WindowsServiceLoopBase : WindowsServiceTaskBase
    {
        protected abstract int LoopMilliseconds { get; }

        protected abstract void RunLoop();

        protected abstract void HandleException(Exception exception);

        public override void Run(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    RunLoop();
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }

                cancellationToken.WaitHandle.WaitOne(LoopMilliseconds);
            }
        }
    }
}