using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Imprint.Task
{
    /// <summary>
    /// 任务线程池
    /// </summary>
    public class WorkerDispatcher
    {
        /// <summary>
        /// 同步锁
        /// </summary>
        private object syncLock = new object();

        private volatile ConcurrentQueue<WorkerJob> queue = new ConcurrentQueue<WorkerJob>();

        private volatile DispatcherState state = DispatcherState.IDLE;

        private Thread[] threads;

        // 已结束任务的线程数量
        private volatile int exitedThread = 0;

        /// <summary>
        /// 任务结束
        /// </summary>
        public event DispatchFinishedHandler DispatchFinished;

        /// <summary>
        /// 队列为空
        /// </summary>
        public event DispatchFinishedHandler QueueEmptied;

        /// <summary>
        /// 线程数
        /// </summary>
        public int ThreadCount
        {
            get;
            private set;
        }

        /// <summary>
        /// 当前状态
        /// </summary>
        public DispatcherState State
        {
            get
            {
                return state;
            }
        }

        /// <summary>
        /// 任务数量
        /// </summary>
        public int Count
        {
            get
            {
                return queue.Count;
            }
        }

        /// <summary>
        /// 清空队列
        /// </summary>
        public void Clear()
        {
            queue = new ConcurrentQueue<WorkerJob>();
        }

        /// <summary>
        /// 任务入队
        /// </summary>
        /// <param name="job"></param>
        public void Append(WorkerJob job)
        {
            queue.Enqueue(job);
        }

        /// <summary>
        /// 开启线程池调度
        /// </summary>
        /// <param name="threadCount"></param>
        /// <param name="execInterv"></param>
        public void Start(int threadCount)
        {
            if (state == DispatcherState.BUSY)
            {
                return;
            }
            ThreadCount = threadCount;
            exitedThread = 0;
            state = DispatcherState.BUSY;
            threads = new Thread[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                var thread = new Thread(threadWork);
                thread.IsBackground = true;
                thread.Start();
                threads[i] = threads[i];
            }

            // 监视线程, 负责触发事件
            var daemon = new Thread(() =>
            {
                while (true)
                {
                    // 运行线程已经全部退出
                    if (exitedThread == ThreadCount)
                    {
                        DispatchFinished?.Invoke(this);
                        return;
                    }
                    Thread.Sleep(100);
                }
            })
            {
                IsBackground = true
            };
            daemon.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Halt()
        {
            if (state != DispatcherState.BUSY)
            {
                return;
            }
            state = DispatcherState.HALTING;
        }

       void threadWork()
        {
            while (state == DispatcherState.BUSY)
            {
                if (queue.TryDequeue(out WorkerJob job))
                {
                    if (job.Interval > 0)
                    {
                        lock (syncLock)
                        {
                            var delta = (int)DateTime.Now.Subtract(job.LastExec).TotalMilliseconds;
                            // 还没到执行的时候
                            if (delta > 0 && delta < job.Interval)
                            {
                                Task<object>.Run(async () =>
                                {
                                    await Task<object>.Delay(job.Interval - delta);

                                    queue.Enqueue(job);
                                });
                                continue;
                            }
                        }
                    }
                    if (job.Interval > 0)
                    {
                        lock (syncLock)
                        {
                            job.LastExec = DateTime.Now;
                        }
                    }
                    try
                    {
                        // 执行任务
                        job.Run();
                    }
                    catch (Exception ex)
                    {
                    }
                    finally
                    {
                        lock (syncLock)
                        {
                            if (job.Repeat == -1 || job.Repeat-- > 0)
                            {
                                queue.Enqueue(job);
                            }
                        }
                    }
                }
                else
                {
                    wait();
                }
            }
            // 原子计数线程退出个数
            Interlocked.Increment(ref exitedThread);
        }

        void wait()
        {
            Thread.Sleep(16);
        }
    }
}
