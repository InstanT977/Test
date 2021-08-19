using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RwmSignatureGenerator.Data
{
    internal class RThreadPoolItem
    {
        internal RThreadPoolItem()
        {
            _threadWorkWaiter = new ManualResetEvent(false);
            _thread = new Thread(Execution);
            _thread.Start();
        }
        private Thread _thread { get; set; }
        private object _threadWorkLocker = new object();
        private Action _threadWork;
        internal void SetItemWork(Action work)
        {
            if(IsFree())
              _threadWork = work;
        }

        internal bool IsFree()
        {
            return _threadWorkWaiter.WaitOne(0);
        }

        internal int Id { get; set; }
        private ManualResetEvent _threadWorkWaiter;

        private void Execution()
        {
            while (true)
            {
                _threadWork?.Invoke();
                _threadWorkWaiter.WaitOne();
            }
        }
    }


    internal sealed class RThreadPool
    {
        CancellationTokenSource _cancellationTokenSource;
        RThreadPoolItem[] _mainThreadPool;
        int _maxThreadsCount = 0;
        int _freeThreadsCount = 0;
        
        RThreadPool()
        {
            _maxThreadsCount = Environment.ProcessorCount;
            _freeThreadsCount = Environment.ProcessorCount;
            _mainThreadPool = new RThreadPoolItem[_maxThreadsCount];
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

        private static readonly Lazy<RThreadPool> lazy = new Lazy<RThreadPool>(() => new RThreadPool());
        internal static RThreadPool Instance
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

        internal List<RThreadPoolItem> GetFreeThreadItems(int count)
        {
            var rThreadPoolItemsList = new List<RThreadPoolItem>();
            for(var i=0;i< _mainThreadPool.Length;i++)
            {
                if(_mainThreadPool[i] == null)
                {
                    _mainThreadPool[i] = new RThreadPoolItem();
                }
                if(_mainThreadPool[i].IsFree())
                {
                    rThreadPoolItemsList.Add(_mainThreadPool[i]);
                }
            }
            return rThreadPoolItemsList;
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

    internal class RBackgroundWorker
    {
        ConcurrentDictionary<Guid, TWork> _tWorks = new ConcurrentDictionary<Guid, TWork>();
        internal RBackgroundWorkCreationState AddWork(Action<IWorkNotifyer> work, int desiredWorkersCount)
        {
            var threadItems = RThreadPool.Instance.GetFreeThreadItems(desiredWorkersCount);
            if (threadItems.Count == 0)
            {
                return new RBackgroundWorkCreationState(false);
            }
            var workItem = new TWork()
        }

    }

    internal class RBackgroundWorkCreationState
    {
        internal bool IsRunnings { get; private set; }
        internal Guid? CreatedItemGuid { get; private set; }
        internal RBackgroundWorkCreationState(bool isRunning, Guid? createdItemGuid)
        {
            IsRunnings = isRunning;
            CreatedItemGuid = createdItemGuid;
        }
        internal RBackgroundWorkCreationState(bool isRunning)
        {
            IsRunnings = isRunning;
        }
    }

    internal class TWork
    {
        internal Guid Guid { get; private set; }
        internal IWorkNotifyer Notifyer { get; private set; }
        internal RThreadPoolItem[] ThreadItems { get; private set; }
        internal TWorkStatus Status { get; private set; }
        private int _requiredThreadsCount;

        internal TWork(RThreadPoolItem[] threadItems, Action<IWorkNotifyer> work)
        {
            Guid = Guid.NewGuid();
            Notifyer = new WorkNotifyer(Guid);
            ThreadItems = threadItems;
            Status = TWorkStatus.WaitThreads;
            StartWork(work);
        }

        private void StartWork(Action<IWorkNotifyer> work)
        {
            if (Status == TWorkStatus.WaitThreads)
            {
                for (var i = 0; i < ThreadItems.Length; i++)
                {
                    ThreadItems[i].SetItemWork(() => { work(Notifyer); });
                }
            }
        }

        internal void Stop()
        {
            Notifyer.CancellationToken.Cancel
        }


    }

    internal enum Work

    internal enum TWorkStatus
    {
        Running,
        WaitThreads,
        Stopped
    }
    interface IWorkNotifyer
    {
        CancellationToken CancellationToken { get;}
        void WaitNextWorkData();
        bool IsWorkCanBeFinished();
    }
    internal class WorkNotifyer : IWorkNotifyer
    {
        public Guid TWorkIdentifier => _tWorkIdentifyer;

        public CancellationToken CancellationToken => throw new NotImplementedException();

        AutoResetEvent _event;
        internal WorkNotifyer()
        {
            _event = new AutoResetEvent(false);
        }

        public void WaitWork()
        {
            throw new NotImplementedException();
        }

        public void WaitNextWorkData()
        {
            throw new NotImplementedException();
        }

        public bool IsWorkCanBeFinished()
        {
            throw new NotImplementedException();
        }
    }
}
