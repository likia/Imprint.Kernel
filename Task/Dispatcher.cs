using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Threading;

namespace Imprint.Task
{
    public interface Dispatchable
    {
        void Execute(object sender, int ThreadIndex, object Parameter);
    }

    public enum DispatcherState
    {
        IDLE = 0,
        INIT = 1,
        BUSY = 2,
        HALTING = 3
    }

    class JobItem
    {
        public int Repeats;
        public bool Wait;
        public Dispatchable Job;
        public object Parameter;
    }

    public delegate void DispatchFinishedHandler(object sender);

    class ThreadProxyArgs
    {
        public JobItem Job;
        public int ThreadIndex;
    }

    class WorkerArgs
    {
        public int ThreadIndex;
    }

    /// <summary>
    /// 綫程池調度器
    /// </summary>
    public class Dispatcher
    {
        private Queue DispatchQueue;
        public DispatcherState State;
        private Thread[] WorkingThread;
        private int Living;
        private object LivingLock;
        private int ExecuteInterval;
        public object Tag;
        public event DispatchFinishedHandler DispatchFinished;
        public event DispatchFinishedHandler QueueEmptied;
        bool finishFlag = false;

        public static void AwaitBackgroundThread(ThreadStart _delegate, Action finished)
        {
            var thread = new Thread(() => {
                _delegate.Invoke();
                finished.Invoke();
            })
            { IsBackground = true };
        }

        /// <summary>
        /// 开启新的后台线程
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public static Thread BackgroundThread(ThreadStart _delegate)
        {
            var thread = new Thread(_delegate) { IsBackground = true };
            thread.Start();
            return thread;
        }

        public static Timer StartTicking(TimerCallback callback, int interval = 1000, object param = null, int delay = 0)
        {
            return new Timer(callback, param, delay, interval);
        }

        public int Count
        {
            get
            {
                return DispatchQueue.Count;
            }
        }

        public Dispatcher()
        {
            LivingLock = new object();

            Tag = null;
            State = DispatcherState.IDLE;
            DispatchQueue = Queue.Synchronized(new Queue());
        }

        public void Clear()
        {
            DispatchQueue.Clear();
        }

        public void Remove(Dispatchable Job)
        {
            lock (DispatchQueue.SyncRoot)
            {
                var array = DispatchQueue.ToArray();
                var list = new ArrayList(array);
                list.Remove(Job);
                DispatchQueue = new Queue(list);
            }
        }

        public void Append(Dispatchable Job, object Parameter, int Repeats, bool Wait)
        {
            JobItem NewJob = new JobItem();
            NewJob.Repeats = Repeats;
            NewJob.Wait = Wait;
            NewJob.Job = Job;
            NewJob.Parameter = Parameter;
            lock (DispatchQueue.SyncRoot)
            {
                DispatchQueue.Enqueue(NewJob);
            }
        }

        public bool Dispatch(int ThreadCount, int ExecuteInt)
        {
            if (State != DispatcherState.IDLE) return false;
            State = DispatcherState.INIT;
            Living = ThreadCount;
            ExecuteInterval = ExecuteInt;
            WorkingThread = new Thread[ThreadCount];
            State = DispatcherState.BUSY;
            for (int i = 0; i < WorkingThread.Length; i++)
            {
                WorkerArgs Args = new WorkerArgs() { ThreadIndex = i };
                WorkingThread[i] = new Thread(new ParameterizedThreadStart(ThreadWork)) { IsBackground = true };
                WorkingThread[i].Start(Args);
            }
            return true;
        }

        public bool Halt()
        {
            if (State != DispatcherState.BUSY) return false;
            State = DispatcherState.HALTING;
            new System.Threading.Thread(new System.Threading.ThreadStart(delegate ()
            {
                for (int i = 0; i < WorkingThread.Length; i++)
                {
                    try
                    {
                        WorkingThread[i].Abort();
                    }
                    catch
                    {
                    }
                }
                State = DispatcherState.IDLE;
                if (DispatchFinished != null) DispatchFinished(this);
            }))
            { IsBackground = true }.Start();
            return true;
        }

        private void ExecuteProxy(object Obj)
        {
            ThreadProxyArgs Args = (ThreadProxyArgs)Obj;
            JobItem Job = Args.Job;
            Job.Job.Execute(this, Args.ThreadIndex, Job.Parameter);
        }

        private void ThreadWork(object Param)
        {
            WorkerArgs WorkerArg = (WorkerArgs)Param;
            int i = WorkerArg.ThreadIndex;
            DateTime LastExecute = new DateTime(1970, 1, 1);
            JobItem Job;
            while (State == DispatcherState.BUSY)
            {
                lock (DispatchQueue.SyncRoot)
                {
                    if ((DateTime.Now - LastExecute).TotalMilliseconds < ExecuteInterval)
                    {
                        goto workerWait;
                    }
                    if (DispatchQueue.Count == 0)
                    {
                        if (!finishFlag)
                        {
                            // call finished event
                            // and set flag
                            if (QueueEmptied != null)
                            {
                                finishFlag = true;
                                QueueEmptied(this);
                            }
                        }
                        goto workerWait;
                    }
                    Job = (JobItem)DispatchQueue.Dequeue();
                    finishFlag = false;
                    if (Job.Repeats == -1 || (Job.Repeats--) > 0)
                    {
                        DispatchQueue.Enqueue(Job);
                    }
                }
                LastExecute = DateTime.Now;
                ThreadProxyArgs Args = new ThreadProxyArgs() { ThreadIndex = i, Job = Job };
                if (Job.Wait)
                {
                    ExecuteProxy(Args);
                }
                else
                {
                    Thread Worker = new Thread(new ParameterizedThreadStart(ExecuteProxy));
                    try
                    {
                        Worker.Start(Args);
                    }
                    catch
                    {

                    }
                }
                workerWait:
                Thread.Sleep(32);
                continue;
            }
        }
    }
}