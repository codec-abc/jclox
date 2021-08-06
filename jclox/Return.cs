using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jclox
{
    public class Return : Exception
    {
        public readonly object value;

        public Return(object value) : base(null, null)
        {
            this.value = value;
        }
    }
}
