using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imprint.Core
{
    public abstract class Picker<T>
    {
        public Picker(List<T> list)
        {
            this.List = list;
        }


        protected List<T> List;

        public void Update(List<T> lst)
        {
            List = lst;
        }

        public abstract T Pick();

        public T[] PickMultiple(int cnt)
        {
            List<T> lst = new List<T>();
            for (int i = 0; i < cnt; i++)
            {
                T obj = Pick();
                lst.Add(obj);
            }
            return lst.ToArray();
        }       
    }
}
