using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jclox
{
    public class LoxFunction : LoxCallable
    {
        private readonly Function<object> declaration;
        private readonly Environment closure;

        public LoxFunction(Function<object> declaration, Environment closure)
        {
            this.closure = closure;
            this.declaration = declaration;
        }

        public int Arity()
        {
            return declaration.funcParams.Count;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            Environment environment = new Environment(closure);
            for (int i = 0; i < declaration.funcParams.Count; i++) {
                environment.Define(
                    declaration.funcParams[i].lexeme,
                    arguments[i]
                );
            }

            try
            {
                interpreter.ExecuteBlock(declaration.body, environment);
            }
            catch (Return returnValue)
            {
                return returnValue.value;
            }
            return null;
        }

        public override string ToString()
        {
            return "<fn " + declaration.name.lexeme + ">";
        }
    }
}
