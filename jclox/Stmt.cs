using jclox;
using System.Collections.Generic;

public abstract class Stmt<R> {
    public abstract R Accept(StmtVisitor<R> visitor);
}

public interface StmtVisitor<R> {
    R VisitBlockStmt(Block<R> stmt);
    R VisitExpressionStmt(Expression<R> stmt);
    R VisitPrintStmt(Print<R> stmt);
    R VisitVarStmt(Var<R> stmt);
  }

public class Block<R> : Stmt<R> {
    public Block(List<Stmt<R>> statements) {
        this.statements = statements;
    }

    public override R Accept(StmtVisitor<R> visitor) {
        return visitor.VisitBlockStmt(this);
    }

    public readonly List<Stmt<R>> statements;
  }

public class Expression<R> : Stmt<R> {
    public Expression(Expr<R> expression) {
        this.expression = expression;
    }

    public override R Accept(StmtVisitor<R> visitor) {
        return visitor.VisitExpressionStmt(this);
    }

    public readonly Expr<R> expression;
  }

public class Print<R> : Stmt<R> {
    public Print(Expr<R> expression) {
        this.expression = expression;
    }

    public override R Accept(StmtVisitor<R> visitor) {
        return visitor.VisitPrintStmt(this);
    }

    public readonly Expr<R> expression;
  }

public class Var<R> : Stmt<R> {
    public Var(Token name, Expr<R> initializer) {
        this.name = name;
        this.initializer = initializer;
    }

    public override R Accept(StmtVisitor<R> visitor) {
        return visitor.VisitVarStmt(this);
    }

    public readonly Token name;
    public readonly Expr<R> initializer;
  }

