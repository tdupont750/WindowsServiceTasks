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
        
        public abstract void OnStart(string[] args);

        public abstract void Run(CancellationToken cancellationToken);

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

        public virtual bool WaitOnStop
        {
            get { return true; }
        }
    }
}