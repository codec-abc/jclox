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
        private readonly bool isInitializer;

        public LoxFunction
        (
            Function<object> declaration,
            Environment closure,
            bool isInitializer
        )
        {
            this.isInitializer = isInitializer;
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

            for (int i = 0; i < declaration.funcParams.Count; i++)
            {
                environment.Define
                (
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
                if (isInitializer)
                {
                    return closure.GetAt(0, "this");
                }
                return returnValue.value;
            }

            if (isInitializer)
            {
                return closure.GetAt(0, "this");
            }
            return null;
        }

        public LoxFunction Bind(LoxInstance instance)
        {
            Environment environment = new Environment(closure);
            environment.Define("this", instance);
            return new LoxFunction(declaration, environment, isInitializer);
        }

        public override string ToString()
        {
            return "<fn " + declaration.name.lexeme + ">";
        }
    }
}
