using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imprint.Core
{
    public class LoopPicker<T> : Picker<T>
    {
        private object Lock = new object();
        public LoopPicker(List<T> list = null) : base(list)
        {
            Index = 0;
            if (list != null)
            {
                this.List = list;
            }
        }
        protected int Index;

        public override T Pick()
        {
            T obj;
            lock (Lock)
            {
                if (Index >= List.Count)
                    Index = 0;

                obj = List[Index];
                ++Index;
            }
            return obj;
        }
    }
}
