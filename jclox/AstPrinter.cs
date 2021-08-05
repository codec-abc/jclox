using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jclox
{
    class AstPrinter : Visitor<string>
    {
        public string Print(Expr<string> expr)
        {
            return expr.Accept(this);
        }

        public string VisitBinaryExpr(Binary<string> expr)
        {
            return Parenthesize(expr.operatorToken.lexeme, new Expr<string>[] {
                    expr.left, expr.right });
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

        public string VisitUnaryExpr(Unary<string> expr)
        {
            return Parenthesize(expr.operatorToken.lexeme, new Expr<string>[] { expr.right });
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
