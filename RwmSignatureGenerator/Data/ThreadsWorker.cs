using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RwmSignatureGenerator.Data
{
    internal sealed class ThreadsWorker
    {
        CancellationTokenSource _cancellationTokenSource;
        Thread[] _mainThreadPool;
        int _maxThreadsCount = 0;
        int _busyThreadCount = 0;

        ThreadsWorker()
        {
            _maxThreadsCount = Environment.ProcessorCount;
            _mainThreadPool = new Thread[_maxThreadsCount];
            _cancellationTokenSource = new CancellationTokenSource();
        }

        object locker = new object();
        private bool _isWorkCanbeFinished;
        internal bool IsWorkCanBeFinished
        {
            get
            {
                lock (locker)
                {
                    return _isWorkCanbeFinished;
                }
            }
            set
            {
                lock (locker)
                {
                    _isWorkCanbeFinished = value;
                }
            }
        }

        private static readonly Lazy<ThreadsWorker> lazy = new Lazy<ThreadsWorker>(() => new ThreadsWorker());
        internal static ThreadsWorker Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        internal void StopWork()
        {
            _cancellationTokenSource.Cancel();
            WaitWorksFinished();
        }

        internal bool NeedStopWork()
        {
            return _cancellationTokenSource.Token.IsCancellationRequested;
        }

        internal void WaitWorksFinished()
        {
            IsWorkCanBeFinished = true;
            for (var i = 0; i < _mainThreadPool.Length; i++)
            {
                if (_mainThreadPool[i] != null && _mainThreadPool[i].IsAlive)
                {
                    _mainThreadPool[i].Join();
                }
            }
        }

        internal void Run(Action action, ref long launchedJobsCount)
        {
            if (_busyThreadCount == _maxThreadsCount)
            {
                launchedJobsCount = 0;
                return;
            }
            if (launchedJobsCount > _maxThreadsCount)
            {
                launchedJobsCount = _maxThreadsCount;
            }

            for (var i = 0; i < launchedJobsCount; i++)
            {
                _busyThreadCount++;
                var thread = new Thread(() => { action(); });
                _mainThreadPool[i] = thread;
                thread.Start();
            }
        }
    }
}
