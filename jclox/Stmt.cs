using jclox;
using System.Collections.Generic;

public abstract class Stmt<R> {
    public abstract R Accept(StmtVisitor<R> visitor);
}

public interface StmtVisitor<R> {
    R VisitBlockStmt(Block<R> stmt);
    R VisitClassStmt(Class<R> stmt);
    R VisitExpressionStmt(Expression<R> stmt);
    R VisitFunctionStmt(Function<R> stmt);
    R VisitIfStmt(If<R> stmt);
    R VisitPrintStmt(Print<R> stmt);
    R VisitReturnStmt(Return<R> stmt);
    R VisitVarStmt(Var<R> stmt);
    R VisitWhileStmt(While<R> stmt);
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

public class Class<R> : Stmt<R> {
    public Class(Token name, List<Function<R>> methods) {
        this.name = name;
        this.methods = methods;
    }

    public override R Accept(StmtVisitor<R> visitor) {
        return visitor.VisitClassStmt(this);
    }

    public readonly Token name;
    public readonly List<Function<R>> methods;
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

public class Function<R> : Stmt<R> {
    public Function(Token name, List<Token> funcParams, List<Stmt<R>> body) {
        this.name = name;
        this.funcParams = funcParams;
        this.body = body;
    }

    public override R Accept(StmtVisitor<R> visitor) {
        return visitor.VisitFunctionStmt(this);
    }

    public readonly Token name;
    public readonly List<Token> funcParams;
    public readonly List<Stmt<R>> body;
  }

public class If<R> : Stmt<R> {
    public If(Expr<R> condition, Stmt<R> thenBranch, Stmt<R> elseBranch) {
        this.condition = condition;
        this.thenBranch = thenBranch;
        this.elseBranch = elseBranch;
    }

    public override R Accept(StmtVisitor<R> visitor) {
        return visitor.VisitIfStmt(this);
    }

    public readonly Expr<R> condition;
    public readonly Stmt<R> thenBranch;
    public readonly Stmt<R> elseBranch;
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

public class Return<R> : Stmt<R> {
    public Return(Token keyword, Expr<R> value) {
        this.keyword = keyword;
        this.value = value;
    }

    public override R Accept(StmtVisitor<R> visitor) {
        return visitor.VisitReturnStmt(this);
    }

    public readonly Token keyword;
    public readonly Expr<R> value;
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

public class While<R> : Stmt<R> {
    public While(Expr<R> condition, Stmt<R> body) {
        this.condition = condition;
        this.body = body;
    }

    public override R Accept(StmtVisitor<R> visitor) {
        return visitor.VisitWhileStmt(this);
    }

    public readonly Expr<R> condition;
    public readonly Stmt<R> body;
  }

