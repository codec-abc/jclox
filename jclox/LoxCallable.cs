using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jclox
{
    public interface LoxCallable
    {
        object Call(Interpreter interpreter, List<object> arguments);
        int Arity();
    }
}
