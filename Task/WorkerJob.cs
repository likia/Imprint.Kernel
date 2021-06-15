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
        } = 0;

        /// <summary>
        /// 参数
        /// </summary>
        public object Param
        {
            get;
            set;
        } = null;


        /// <summary>
        /// 执行间隔
        /// </summary>
        public int Interval
        {
            get;
            set;
        } = 0;

        /// <summary>
        /// 上次执行时间
        /// </summary>
        public DateTime LastExec
        {
            get; set;
        } = DateTime.MinValue;

        public abstract void Run();
    }
}