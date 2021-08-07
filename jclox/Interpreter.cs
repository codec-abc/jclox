using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jclox
{
    public class Interpreter : ExprVisitor<object>, StmtVisitor<object>
    {
        public readonly Environment globals = new Environment();
        private readonly Dictionary<Expr<object>, int> locals = new Dictionary<Expr<object>, int>();
        private Environment environment;

        public Interpreter()
        {
            environment = globals;

            globals.Define("clock", new ClockFunction());
        }

        class ClockFunction : LoxCallable
        {
            public int Arity()
            {
                return 0;
            }

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return (double)DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000.0;
            }

            public override string ToString() { return "<native fn>"; }
        }

        public object VisitBinaryExpr(Binary<object> expr)
        {
            object left = Evaluate(expr.left);
            object right = Evaluate(expr.right);

            switch (expr.operatorToken.type)
            {
                case TokenType.GREATER:
                    CheckNumberOperands(expr.operatorToken, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperands(expr.operatorToken, left, right);
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    CheckNumberOperands(expr.operatorToken, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    CheckNumberOperands(expr.operatorToken, left, right);
                    return (double)left <= (double)right;
                case TokenType.MINUS:
                    CheckNumberOperand(expr.operatorToken, right);
                    return (double)left - (double)right;
                case TokenType.PLUS:
                    if (left is double && right is double)
                    {
                        return (double)left + (double)right;
                    }

                    if (left is string && right is string)
                    {
                        return (string)left + (string)right;
                    }
                    throw new RuntimeError(expr.operatorToken, "Operands must be two numbers or two strings.");
                case TokenType.SLASH:
                    CheckNumberOperands(expr.operatorToken, left, right);
                    return (double)left / (double)right;
                case TokenType.STAR:
                    CheckNumberOperands(expr.operatorToken, left, right);
                    return (double)left * (double)right;
                case TokenType.BANG_EQUAL: 
                    return !IsEqual(left, right);
                case TokenType.EQUAL_EQUAL: 
                    return IsEqual(left, right);
            }

            // Unreachable.
            return null;
        }

        private void CheckNumberOperands(Token operatorToken,
                                   object left, object right)
        {
            if (left is double && right is double) {
                return;
            }

            throw new RuntimeError(operatorToken, "Operands must be numbers.");
        }

        private void CheckNumberOperand(Token opeartorToken, object operand)
        {
            if (operand is double) return;
            throw new RuntimeError(opeartorToken, "Operand must be a number.");
        }

        private bool IsEqual(object a, object b)
        {
            if (a == null && b == null)
            {
                return true;
            }
            if (a == null)
            {
                return false;
            }

            return a.Equals(b);
        }

        public object VisitGroupingExpr(Grouping<object> expr)
        {
            return Evaluate(expr.expression);
        }

        public object VisitLiteralExpr(Literal<object> expr)
        {
            return expr.value;
        }

        public object VisitUnaryExpr(Unary<object> expr)
        {
            object right = Evaluate(expr.right);

            switch (expr.operatorToken.type)
            {
                case TokenType.MINUS:
                    return -(double)right;
                case TokenType.BANG:
                    return !IsTruthy(right);
            }

            // Unreachable.
            return null;
        }

        private bool IsTruthy(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is bool)
            {
                return (bool)obj;
            }
            return true;
        }

        private object Evaluate(Expr<object> expr)
        {
            return expr.Accept(this);
        }

        public void Interpret(List<Stmt<object>> statements)
        {
            try
            {
                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeError error)
            {
                Lox.RuntimeError(error);
            }
        }

        private void Execute(Stmt<object> stmt)
        {
            stmt.Accept(this);
        }

        public void Resolve(Expr<object> expr, int depth)
        {
            locals[expr] = depth;
        }

        private string Stringify(object obj)
        {
            if (obj == null)
            {
                return "nil";
            }

            if (obj is double) 
            {
                string text = obj.ToString();
                if (text.EndsWith(".0"))
                {
                    text = text.Substring(text.Length - 2);
                }
                return text;
            }

            return obj.ToString();
        }

        public object VisitExpressionStmt(Expression<object> stmt)
        {
            Evaluate(stmt.expression);
            return null;
        }

        public object VisitPrintStmt(Print<object> stmt)
        {
            object value = Evaluate(stmt.expression);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public object VisitVarStmt(Var<object> stmt)
        {
            object value = null;
            if (stmt.initializer != null)
            {
                value = Evaluate(stmt.initializer);
            }

            environment.Define(stmt.name.lexeme, value);
            return null;
        }

        public object VisitVariableExpr(Variable<object> expr)
        {
            return LookUpVariable(expr.name, expr);
        }

        private object LookUpVariable(Token name, Expr<object> expr)
        {
            if (locals.ContainsKey(expr))
            {
                int distance = locals[expr];
                return environment.GetAt(distance, name.lexeme);
            }
            else
            {
                return globals.Get(name);
            }
        }

        public object VisitAssignExpr(Assign<object> expr)
        {
            object value = Evaluate(expr.value);

            if (locals.ContainsKey(expr))
            {
                int distance = locals[expr];
                environment.AssignAt(distance, expr.name, value);
            }
            else
            {
                globals.Assign(expr.name, value);
            }
            return value;
        }

        public object VisitBlockStmt(Block<object> stmt)
        {
            ExecuteBlock(stmt.statements, new Environment(environment));
            return null;
        }

        public void ExecuteBlock(List<Stmt<object>> statements,
                    Environment environment)
        {
            Environment previous = this.environment;
            try
            {
                this.environment = environment;

                foreach (Stmt<object> statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                this.environment = previous;
            }
        }

        public object VisitIfStmt(If<object> stmt)
        {
            if (IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.thenBranch);
            }
            else if (stmt.elseBranch != null)
            {
                Execute(stmt.elseBranch);
            }
            return null;
        }

        public object VisitLogicalExpr(Logical<object> expr)
        {
            object left = Evaluate(expr.left);

            if (expr.operatorToken.type == TokenType.OR) 
            {
                if (IsTruthy(left))
                {
                    return left;
                }
            } 
            else
            {
                if (!IsTruthy(left))
                {
                    return left;
                }
            }

            return Evaluate(expr.right);
        }

        public object VisitWhileStmt(While<object> stmt)
        {
            while (IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.body);
            }
            return null;
        }

        public object VisitCallExpr(Call<object> expr)
        {
            object callee = Evaluate(expr.callee);

            List<object> arguments = new List<object>();
            foreach (Expr<object> argument in expr.arguments)
            {
                arguments.Add(Evaluate(argument));
            }

            if (!(callee is LoxCallable)) 
            {
                throw new RuntimeError(expr.paren, "Can only call functions and classes.");
            }

            LoxCallable function = (LoxCallable)callee;

            if (arguments.Count != function.Arity())
            {
                throw new RuntimeError(expr.paren, "Expected " + function.Arity() + " arguments but got " + arguments.Count + ".");
            }

            return function.Call(this, arguments);
        }

        public object VisitFunctionStmt(Function<object> stmt)
        {
            LoxFunction function = new LoxFunction(stmt, environment, false);
            environment.Define(stmt.name.lexeme, function);
            return null;
        }

        public object VisitReturnStmt(Return<object> stmt)
        {
            object value = null;
            if (stmt.value != null)
            {
                value = Evaluate(stmt.value);
            }

            throw new Return(value);
        }

        public object VisitClassStmt(Class<object> stmt)
        {
            object superclass = null;

            if (stmt.superclass != null)
            {
                superclass = Evaluate(stmt.superclass);
                if (!(superclass is LoxClass)) 
                {
                    throw new RuntimeError(stmt.superclass.name, "Superclass must be a class.");
                }
            }

            environment.Define(stmt.name.lexeme, null);

            if (stmt.superclass != null)
            {
                environment = new Environment(environment);
                environment.Define("super", superclass);
            }

            Dictionary<string, LoxFunction> methods = new Dictionary<string, LoxFunction>();
            foreach (Function<object> method in stmt.methods)
            {
                LoxFunction function = new LoxFunction(method, environment, method.name.lexeme == "init");
                methods[method.name.lexeme] = function;
            }

            LoxClass klass = new LoxClass(stmt.name.lexeme, (LoxClass)superclass, methods);

            if (superclass != null)
            {
                environment = environment.enclosing;
            }

            environment.Assign(stmt.name, klass);
            return null;
        }

        public object VisitGetExpr(Get<object> expr)
        {
            object obj = Evaluate(expr.obj);
            if (obj is LoxInstance) 
            {
                LoxInstance loxInstance = ((LoxInstance)obj);
                return loxInstance.Get(expr.name);
            }

            throw new RuntimeError(expr.name, "Only instances have properties.");
        }

        public object VisitSetExpr(Set<object> expr)
        {
            object obj = Evaluate(expr.obj);

            if (!(obj is LoxInstance)) 
            {
                throw new RuntimeError(expr.name, "Only instances have fields.");
            }

            object value = Evaluate(expr.value);
            LoxInstance loxInstane = ((LoxInstance)obj);
            loxInstane.Set(expr.name, value);
            return value;
        }

        public object VisitThisExpr(This<object> expr)
        {
            return LookUpVariable(expr.keyword, expr);
        }

        public object VisitSuperExpr(Super<object> expr)
        {
            int distance = locals[expr];
            LoxClass superclass = (LoxClass)environment.GetAt(distance, "super");

            LoxInstance obj = (LoxInstance)environment.GetAt(distance - 1, "this");

            LoxFunction method = superclass.FindMethod(expr.method.lexeme);

            if (method == null)
            {
                throw new RuntimeError(expr.method, "Undefined property '" + expr.method.lexeme + "'.");
            }

            return method.Bind(obj);
        }
    }
}
