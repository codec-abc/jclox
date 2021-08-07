using jclox;
using System.Collections.Generic;

public abstract class Expr<R>
{
    public abstract R Accept(ExprVisitor<R> visitor);
}

public interface ExprVisitor<R>
{
    R VisitAssignExpr(Assign<R> expr);
    R VisitBinaryExpr(Binary<R> expr);
    R VisitCallExpr(Call<R> expr);
    R VisitGetExpr(Get<R> expr);
    R VisitGroupingExpr(Grouping<R> expr);
    R VisitLiteralExpr(Literal<R> expr);
    R VisitLogicalExpr(Logical<R> expr);
    R VisitSetExpr(Set<R> expr);
    R VisitSuperExpr(Super<R> expr);
    R VisitThisExpr(This<R> expr);
    R VisitUnaryExpr(Unary<R> expr);
    R VisitVariableExpr(Variable<R> expr);
}

public class Assign<R> : Expr<R>
{
    public Assign(Token name, Expr<R> value)
    {
        this.name = name;
        this.value = value;
    }

    public override R Accept(ExprVisitor<R> visitor)
    {
        return visitor.VisitAssignExpr(this);
    }

    public readonly Token name;
    public readonly Expr<R> value;
}

public class Binary<R> : Expr<R>
{
    public Binary(Expr<R> left, Token operatorToken, Expr<R> right)
    {
        this.left = left;
        this.operatorToken = operatorToken;
        this.right = right;
    }

    public override R Accept(ExprVisitor<R> visitor)
    {
        return visitor.VisitBinaryExpr(this);
    }

    public readonly Expr<R> left;
    public readonly Token operatorToken;
    public readonly Expr<R> right;
}

public class Call<R> : Expr<R>
{
    public Call(Expr<R> callee, Token paren, List<Expr<R>> arguments)
    {
        this.callee = callee;
        this.paren = paren;
        this.arguments = arguments;
    }

    public override R Accept(ExprVisitor<R> visitor)
    {
        return visitor.VisitCallExpr(this);
    }

    public readonly Expr<R> callee;
    public readonly Token paren;
    public readonly List<Expr<R>> arguments;
}

public class Get<R> : Expr<R>
{
    public Get(Expr<R> obj, Token name)
    {
        this.obj = obj;
        this.name = name;
    }

    public override R Accept(ExprVisitor<R> visitor)
    {
        return visitor.VisitGetExpr(this);
    }

    public readonly Expr<R> obj;
    public readonly Token name;
}

public class Grouping<R> : Expr<R>
{
    public Grouping(Expr<R> expression)
    {
        this.expression = expression;
    }

    public override R Accept(ExprVisitor<R> visitor)
    {
        return visitor.VisitGroupingExpr(this);
    }

    public readonly Expr<R> expression;
}

public class Literal<R> : Expr<R>
{
    public Literal(object value)
    {
        this.value = value;
    }

    public override R Accept(ExprVisitor<R> visitor)
    {
        return visitor.VisitLiteralExpr(this);
    }

    public readonly object value;
}

public class Logical<R> : Expr<R>
{
    public Logical(Expr<R> left, Token operatorToken, Expr<R> right)
    {
        this.left = left;
        this.operatorToken = operatorToken;
        this.right = right;
    }

    public override R Accept(ExprVisitor<R> visitor)
    {
        return visitor.VisitLogicalExpr(this);
    }

    public readonly Expr<R> left;
    public readonly Token operatorToken;
    public readonly Expr<R> right;
}

public class Set<R> : Expr<R>
{
    public Set(Expr<R> obj, Token name, Expr<R> value)
    {
        this.obj = obj;
        this.name = name;
        this.value = value;
    }

    public override R Accept(ExprVisitor<R> visitor)
    {
        return visitor.VisitSetExpr(this);
    }

    public readonly Expr<R> obj;
    public readonly Token name;
    public readonly Expr<R> value;
}

public class Super<R> : Expr<R>
{
    public Super(Token keyword, Token method)
    {
        this.keyword = keyword;
        this.method = method;
    }

    public override R Accept(ExprVisitor<R> visitor)
    {
        return visitor.VisitSuperExpr(this);
    }

    public readonly Token keyword;
    public readonly Token method;
}

public class This<R> : Expr<R>
{
    public This(Token keyword)
    {
        this.keyword = keyword;
    }

    public override R Accept(ExprVisitor<R> visitor)
    {
        return visitor.VisitThisExpr(this);
    }

    public readonly Token keyword;
}

public class Unary<R> : Expr<R>
{
    public Unary(Token operatorToken, Expr<R> right)
    {
        this.operatorToken = operatorToken;
        this.right = right;
    }

    public override R Accept(ExprVisitor<R> visitor)
    {
        return visitor.VisitUnaryExpr(this);
    }

    public readonly Token operatorToken;
    public readonly Expr<R> right;
}

public class Variable<R> : Expr<R>
{
    public Variable(Token name)
    {
        this.name = name;
    }

    public override R Accept(ExprVisitor<R> visitor)
    {
        return visitor.VisitVariableExpr(this);
    }

    public readonly Token name;
}

