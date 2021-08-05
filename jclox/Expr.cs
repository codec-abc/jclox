using jclox;
using System.Collections.Generic;

public abstract class Expr<R> {
    public abstract R Accept(Visitor<R> visitor);
}

public interface Visitor<R> {
    R VisitBinaryExpr<R>(Binary<R> expr);
    R VisitGroupingExpr<R>(Grouping<R> expr);
    R VisitLiteralExpr<R>(Literal<R> expr);
    R VisitUnaryExpr<R>(Unary<R> expr);
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

    readonly Expr<R> left;
    readonly Token operatorToken;
    readonly Expr<R> right;
  }

public class Grouping<R> : Expr<R> {
    Grouping(Expr<R> expression) {
        this.expression = expression;
    }

    public override R Accept(Visitor<R> visitor) {
        return visitor.VisitGroupingExpr(this);
    }

    readonly Expr<R> expression;
  }

public class Literal<R> : Expr<R> {
    Literal(object value) {
        this.value = value;
    }

    public override R Accept(Visitor<R> visitor) {
        return visitor.VisitLiteralExpr(this);
    }

    readonly object value;
  }

public class Unary<R> : Expr<R> {
    Unary(Token operatorToken, Expr<R> right) {
        this.operatorToken = operatorToken;
        this.right = right;
    }

    public override R Accept(Visitor<R> visitor) {
        return visitor.VisitUnaryExpr(this);
    }

    readonly Token operatorToken;
    readonly Expr<R> right;
  }

