using Imprint.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imprint.Core
{
    public class RandomPicker<T> : Picker<T>
    {
        private Random rand;

        public RandomPicker(List<T> list = null) : base(list)
        {
            rand = new Random();            
            if(list != null)
            {
                this.List = list;
            }
        }

        public override T Pick()
        {
            var index = rand.Next(List.Count - 1);
            return List[index];
        }
    }
}
