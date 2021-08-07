using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jclox
{
    class AstPrinter : ExprVisitor<string>
    {
        public string Print(Expr<string> expr)
        {
            return expr.Accept(this);
        }

        public string VisitAssignExpr(Assign<string> expr)
        {
            return Parenthesize(expr.name.lexeme, new Expr<string>[] { expr.value });
        }

        public string VisitBinaryExpr(Binary<string> expr)
        {
            return Parenthesize(expr.operatorToken.lexeme, new Expr<string>[] {
                    expr.left, expr.right });
        }

        public string VisitCallExpr(Call<string> expr)
        {
            throw new NotImplementedException();
        }

        public string VisitGetExpr(Get<string> expr)
        {
            throw new NotImplementedException();
        }

        public string VisitGroupingExpr(Grouping<string> expr)
        {
            return Parenthesize("group", new Expr<string>[] { expr.expression });
        }

        public string VisitLiteralExpr(Literal<string> expr)
        {
            if (expr.value == null) return "nil";
            return expr.value.ToString();
        }

        public string VisitLogicalExpr(Logical<string> expr)
        {
            throw new NotImplementedException();
        }

        public string VisitSetExpr(Set<string> expr)
        {
            throw new NotImplementedException();
        }

        public string VisitSuperExpr(Super<string> expr)
        {
            throw new NotImplementedException();
        }

        public string VisitThisExpr(This<string> expr)
        {
            throw new NotImplementedException();
        }

        public string VisitUnaryExpr(Unary<string> expr)
        {
            return Parenthesize(expr.operatorToken.lexeme, new Expr<string>[] { expr.right });
        }

        public string VisitVariableExpr(Variable<string> expr)
        {
            return "(var " + expr.name + ")"; 
        }

        private string Parenthesize(string name, Expr<string>[] exprs)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach (var expr in exprs)
            {
                builder.Append(" ");
                builder.Append(expr.Accept(this));
            }
            builder.Append(")");

            return builder.ToString();
        }
    }
}
