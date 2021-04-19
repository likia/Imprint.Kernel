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
        }

        /// <summary>
        /// ����
        /// </summary>
        public object Param
        {
            get;
            set;
        }

        public abstract void Run();
    }
}