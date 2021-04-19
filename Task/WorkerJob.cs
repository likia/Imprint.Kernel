using System;

namespace Imprint.Task
{

    public abstract class WorkerJob
    {
        /// <summary>
        /// 重复次数, -1 代表永远重复
        /// </summary>
        public int Repeat
        {
            get;
            set;
        }

        /// <summary>
        /// 参数
        /// </summary>
        public object Param
        {
            get;
            set;
        }

        public abstract void Run();
    }
}