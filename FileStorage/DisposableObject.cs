using System;
using System.Threading;

namespace FileStorage
{
    
    public abstract class DisposableObject : IDisposable
    {
        private long _isDispose = 0;

        protected abstract void OnDisposed();

        public bool IsNotDisposed
        {
            get { return Interlocked.Read(ref _isDispose) == 0; }
        }

        public bool IsDisposed
        {
            get { return Interlocked.Read(ref _isDispose) != 0; }
        }


        public void Dispose()
        {
            if (Interlocked.Exchange(ref _isDispose, 1) == 0)
            {
                try
                {
                    OnDisposed();
                }
                catch
                {
                }
                finally
                {
                    GC.SuppressFinalize(this);
                }
            }
        }

        ~DisposableObject()
        {
            Dispose();
        }
    }
}