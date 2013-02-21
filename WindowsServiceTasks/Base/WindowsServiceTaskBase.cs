using System;
using System.Threading;

namespace WindowsServiceTasks.Base
{
    public abstract class WindowsServiceTaskBase : IWindowsServiceTask
    {
        private bool _isDisposed;

        ~WindowsServiceTaskBase()
        {
            Dispose(true);
        }

        protected abstract int LoopMilliseconds { get; }

        protected abstract void HandleException(Exception exception);

        protected abstract void RunLoop();

        public abstract void OnStart(string[] args);

        public void Run(CancellationToken cancellationToken)
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

        public abstract void OnStop();

        public void Dispose()
        {
            Dispose(false);
        }

        private void Dispose(bool isFinalizing)
        {
            if (_isDisposed)
                return;

            DisposeResources();

            if (!isFinalizing)
                GC.SuppressFinalize(this);

            _isDisposed = true;
        }

        protected abstract void DisposeResources();
    }
}