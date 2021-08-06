using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jclox
{
    public class Interpreter : ExprVisitor<object>, StmtVisitor<object>
    {
        private Environment environment = new Environment();

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
            return environment.Get(expr.name);
        }

        public object VisitAssignExpr(Assign<object> expr)
        {
            object value = Evaluate(expr.value);
            environment.Assign(expr.name, value);
            return value;
        }

        public object VisitBlockStmt(Block<object> stmt)
        {
            ExecuteBlock(stmt.statements, new Environment(environment));
            return null;
        }

        void ExecuteBlock(List<Stmt<object>> statements,
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
                if (IsTruthy(left)) return left;
            } 
            else
            {
                if (!IsTruthy(left)) return left;
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
    }
}
