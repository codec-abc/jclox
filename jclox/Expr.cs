using jclox;
using System.Collections.Generic;

public abstract class Expr<R> {
    public abstract R Accept(Visitor<R> visitor);
}

public interface Visitor<R> {
    R VisitBinaryExpr(Binary<R> expr);
    R VisitGroupingExpr(Grouping<R> expr);
    R VisitLiteralExpr(Literal<R> expr);
    R VisitUnaryExpr(Unary<R> expr);
  }

public class Binary<R> : Expr<R> {
    Binary(Expr<R> left, Token operatorToken, Expr<R> right) {
        this.left = left;
        this.operatorToken = operatorToken;
        this.right = right;
    }

    public override R Accept(Visitor<R> visitor) {
        return visitor.VisitBinaryExpr(this);
    }

    public readonly Expr<R> left;
    public readonly Token operatorToken;
    public readonly Expr<R> right;
  }

public class Grouping<R> : Expr<R> {
    Grouping(Expr<R> expression) {
        this.expression = expression;
    }

    public override R Accept(Visitor<R> visitor) {
        return visitor.VisitGroupingExpr(this);
    }

    public readonly Expr<R> expression;
  }

public class Literal<R> : Expr<R> {
    Literal(object value) {
        this.value = value;
    }

    public override R Accept(Visitor<R> visitor) {
        return visitor.VisitLiteralExpr(this);
    }

    public readonly object value;
  }

public class Unary<R> : Expr<R> {
    Unary(Token operatorToken, Expr<R> right) {
        this.operatorToken = operatorToken;
        this.right = right;
    }

    public override R Accept(Visitor<R> visitor) {
        return visitor.VisitUnaryExpr(this);
    }

    public readonly Token operatorToken;
    public readonly Expr<R> right;
  }

