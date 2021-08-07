using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jclox
{
    public class Resolver : ExprVisitor<object>, StmtVisitor<object>
    {
        private readonly Interpreter interpreter;
        private readonly Stack<Dictionary<string, bool>> scopes = new Stack<Dictionary<string, bool>>();
        private FunctionType currentFunction = FunctionType.NONE;

        private enum FunctionType
        {
            NONE,
            FUNCTION,
            INITIALIZER,
            METHOD
        }

        private enum ClassType
        {
            NONE,
            CLASS
        }

        private ClassType currentClass = ClassType.NONE;

        public Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        public object VisitBlockStmt(Block<object> stmt)
        {
            BeginScope();
            Resolve(stmt.statements);
            EndScope();
            return null;
        }

        public void Resolve(List<Stmt<object>> statements)
        {
            foreach (Stmt<object> statement in statements)
            {
                Resolve(statement);
            }
        }

        private void Resolve(Stmt<object> stmt)
        {
            stmt.Accept(this);
        }

        private void Resolve(Expr<object> expr)
        {
            expr.Accept(this);
        }

        private void BeginScope()
        {
            scopes.Push(new Dictionary<string, bool>());
        }

        private void EndScope()
        {
            scopes.Pop();
        }

        public object VisitExpressionStmt(Expression<object> stmt)
        {
            Resolve(stmt.expression);
            return null;
        }

        public object VisitFunctionStmt(Function<object> stmt)
        {
            Declare(stmt.name);
            Define(stmt.name);

            ResolveFunction(stmt, FunctionType.FUNCTION);
            return null;
        }

        public object VisitIfStmt(If<object> stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.thenBranch);
            if (stmt.elseBranch != null)
            {
                Resolve(stmt.elseBranch);
            }
            return null;
        }

        public object VisitPrintStmt(Print<object> stmt)
        {
            Resolve(stmt.expression);
            return null;
        }

        public object VisitReturnStmt(Return<object> stmt)
        {
            if (currentFunction == FunctionType.NONE)
            {
                Lox.Error(stmt.keyword, "Can't return from top-level code.");
            }

            if (stmt.value != null)
            {
                if (currentFunction == FunctionType.INITIALIZER)
                {
                    Lox.Error(stmt.keyword, "Can't return a value from an initializer.");
                }

                Resolve(stmt.value);
            }

            return null;
        }

        public object VisitVarStmt(Var<object> stmt)
        {
            Declare(stmt.name);
            if (stmt.initializer != null)
            {
                Resolve(stmt.initializer);
            }
            Define(stmt.name);
            return null;
        }

        private void Declare(Token name)
        {
            if (scopes.Count == 0)
            {
                return;
            }

            Dictionary<string, bool> scope = scopes.Peek();

            if (scope.ContainsKey(name.lexeme))
            {
                Lox.Error(name,"Already a variable with this name in this scope.");
            }

            scope[name.lexeme] = false;
        }

        private void Define(Token name)
        {
            if (scopes.Count == 0)
            {
                return;
            }

            scopes.Peek()[name.lexeme] =  true;
        }

        public object VisitWhileStmt(While<object> stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.body);
            return null;
        }

        public object VisitAssignExpr(Assign<object> expr)
        {
            Resolve(expr.value);
            ResolveLocal(expr, expr.name);
            return null;
        }

        public object VisitBinaryExpr(Binary<object> expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        public object VisitCallExpr(Call<object> expr)
        {
            Resolve(expr.callee);

            foreach (Expr<object> argument in expr.arguments)
            {
                Resolve(argument);
            }

            return null;
        }

        public object VisitGroupingExpr(Grouping<object> expr)
        {
            Resolve(expr.expression);
            return null;
        }

        public object VisitLiteralExpr(Literal<object> expr)
        {
            return null;
        }

        public object VisitLogicalExpr(Logical<object> expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        public object VisitUnaryExpr(Unary<object> expr)
        {
            Resolve(expr.right);
            return null;
        }

        public object VisitVariableExpr(Variable<object> expr)
        {
            if (scopes.Count != 0)
            {
                var previousScope = scopes.Peek();
                if (previousScope.ContainsKey(expr.name.lexeme) && previousScope[expr.name.lexeme] == false)
                {
                    Lox.Error(expr.name,
                        "Can't read local variable in its own initializer.");
                }
            }

            ResolveLocal(expr, expr.name);
            return null;
        }

        private void ResolveLocal(Expr<object> expr, Token name)
        {
            var allScopes = scopes.ToArray();
            for (int i = 0; i < scopes.Count; i++)
            {
                if (allScopes[i].ContainsKey(name.lexeme))
                {
                    interpreter.Resolve(expr, i);
                    return;
                }
            }
        }

        private void ResolveFunction(Function<object> function, FunctionType type)
        {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;

            BeginScope();
            foreach (Token param in function.funcParams)
            {
                Declare(param);
                Define(param);
            }
            Resolve(function.body);
            EndScope();
            currentFunction = enclosingFunction;
        }

        public object VisitClassStmt(Class<object> stmt)
        {
            ClassType enclosingClass = currentClass;
            currentClass = ClassType.CLASS;

            Declare(stmt.name);
            Define(stmt.name);

            BeginScope();
            scopes.Peek()["this"] = true;

            foreach (Function<object> method in stmt.methods)
            {
                FunctionType declaration = FunctionType.METHOD;
                ResolveFunction(method, declaration);
            }

            EndScope();

            currentClass = enclosingClass;
            return null;
        }

        public object VisitGetExpr(Get<object> expr)
        {
            Resolve(expr.obj);
            return null;
        }

        public object VisitSetExpr(Set<object> expr)
        {
            Resolve(expr.value);
            Resolve(expr.obj);
            return null;
        }

        public object VisitThisExpr(This<object> expr)
        {
            if (currentClass == ClassType.NONE)
            {
                Lox.Error(expr.keyword, "Can't use 'this' outside of a class.");
                return null;
            }

            ResolveLocal(expr, expr.keyword);
            return null;
        }
    }
}
