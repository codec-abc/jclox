using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jclox
{
    class AstPrinter : Visitor<string>
    {
        string Print(Expr<string> expr)
        {
            return expr.accept(this);
        }



        public string visitBinaryExpr(Binary<string> expr)
        {
            return parenthesize(expr.operatorToken.lexeme,
                    expr.left, expr.right);
        }

        public string visitGroupingExpr(Grouping<string> expr)
        {
            return parenthesize("group", expr.expression);
        }

        public string visitLiteralExpr(Literal<string> expr)
        {
            if (expr.value == null) return "nil";
            return expr.value.toString();
        }

        public string visitUnaryExpr(Unary<string> expr)
        {
            return parenthesize(expr.operator.lexeme, expr.right);
        }
    }
}
