using System;

namespace Imprint.Task
{

    public abstract class WorkerJob
    {
        /// <summary>
        /// �ظ�����, -1 ������Զ�ظ�
        /// </summary>
        public int Repeat
        {
            get;
            set;
        } = 0;

        /// <summary>
        /// ����
        /// </summary>
        public object Param
        {
            get;
            set;
        } = null;


        /// <summary>
        /// ִ�м��
        /// </summary>
        public int Interval
        {
            get;
            set;
        } = 0;

        /// <summary>
        /// �ϴ�ִ��ʱ��
        /// </summary>
        public DateTime LastExec
        {
            get; set;
        } = DateTime.MinValue;

        public abstract void Run();
    }
}