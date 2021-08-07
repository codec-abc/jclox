using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jclox
{
    class Parser<R>
    {
        private readonly List<Token> tokens;
        private int current = 0;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public List<Stmt<R>> Parse()
        {
            var statements = new List<Stmt<R>>();
            while (!IsAtEnd())
            {
                statements.Add(Declaration());
            }

            return statements;
        }

        private Expr<R> Expression()
        {
            return Assignment();
        }

        private Expr<R> Assignment()
        {
            Expr<R> expr = Or();

            if (Match(new TokenType[] { TokenType.EQUAL }))
            {
                Token equals = Previous();
                Expr<R> value = Assignment();

                if (expr is Variable<R>)
                {
                    Token name = ((Variable<R>)expr).name;
                    return new Assign<R>(name, value);
                }
                else if (expr is Get<R>)
                {
                    Get<R> get = (Get<R>)expr;
                    return new Set<R>(get.obj, get.name, value);
                }

                Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Expr<R> Or()
        {
            Expr<R> expr = And();

            while (Match(new TokenType[] { TokenType.OR }))
            {
                Token operatorToken = Previous();
                Expr<R> right = And();
                expr = new Logical<R>(expr, operatorToken, right);
            }

            return expr;
        }

        private Expr<R> And()
        {
            Expr<R> expr = Equality();

            while (Match(new TokenType[] { TokenType.AND }))
            {
                Token operatorToken = Previous();
                Expr<R> right = Equality();
                expr = new Logical<R>(expr, operatorToken, right);
            }

            return expr;
        }

        private Stmt<R> Declaration()
        {
            try
            {
                if (Match(new TokenType[] { TokenType.CLASS }))
                {
                    return ClassDeclaration();
                }

                if (Match(new TokenType[] { TokenType.FUN }))
                {
                    return Function("function");
                }

                if (Match(new TokenType[] { TokenType.VAR }))
                {
                    return VarDeclaration();
                }

                return Statement();
            }
            catch (ParseError error)
            {
                Synchronize();
                return null;
            }
        }

        private Stmt<R> ClassDeclaration()
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expect class name.");

            Variable<R> superclass = null;
            if (Match(new TokenType[] { TokenType.LESS }))
            {
                Consume(TokenType.IDENTIFIER, "Expect superclass name.");
                superclass = new Variable<R>(Previous());
            }

            Consume(TokenType.LEFT_BRACE, "Expect '{' before class body.");

            List<Function<R>> methods = new List<Function<R>>();
            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            {
                methods.Add(Function("method"));
            }

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after class body.");

            return new Class<R>(name, superclass, methods);
        }

        private Function<R> Function(string kind)
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expect " + kind + " name.");
            Consume(TokenType.LEFT_PAREN, "Expect '(' after " + kind + " name.");
            List<Token> parameters = new List<Token>();
            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (parameters.Count >= 255)
                    {
                        Error(Peek(), "Can't have more than 255 parameters.");
                    }

                    parameters.Add
                    (
                        Consume(TokenType.IDENTIFIER, "Expect parameter name.")
                    );

                } while (Match(new TokenType[] { TokenType.COMMA }));
            }
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");

            Consume(TokenType.LEFT_BRACE, "Expect '{' before " + kind + " body.");
            List<Stmt<R>> body = Block();
            return new Function<R>(name, parameters, body);
        }

        private Stmt<R> VarDeclaration()
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

            Expr<R> initializer = null;
            if (Match(new TokenType[] { TokenType.EQUAL }))
            {
                initializer = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
            return new Var<R>(name, initializer);
        }

        private Stmt<R> Statement()
        {
            if (Match(new TokenType[] { TokenType.FOR }))
            {
                return ForStatement();
            }

            if (Match(new TokenType[] { TokenType.IF }))
            {
                return IfStatement();
            }

            if (Match(new TokenType[] { TokenType.PRINT }))
            {
                return PrintStatement();
            }

            if (Match(new TokenType[] { TokenType.RETURN }))
            {
                return ReturnStatement();
            }

            if (Match(new TokenType[] { TokenType.WHILE }))
            {
                return WhileStatement();
            }

            if (Match(new TokenType[] { TokenType.LEFT_BRACE }))
            {
                return new Block<R>(Block());
            }

            return ExpressionStatement();
        }

        private Stmt<R> ReturnStatement()
        {
            Token keyword = Previous();
            Expr<R> value = null;
            if (!Check(TokenType.SEMICOLON))
            {
                value = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after return value.");
            return new Return<R>(keyword, value);
        }

        private Stmt<R> ForStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");

            Stmt<R> initializer;
            if (Match(new TokenType[] { TokenType.SEMICOLON }))
            {
                initializer = null;
            }
            else if (Match(new TokenType[] { TokenType.VAR }))
            {
                initializer = VarDeclaration();
            }
            else
            {
                initializer = ExpressionStatement();
            }

            Expr<R> condition = null;
            if (!Check(TokenType.SEMICOLON))
            {
                condition = Expression();
            }
            Consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

            Expr<R> increment = null;
            if (!Check(TokenType.RIGHT_PAREN))
            {
                increment = Expression();
            }
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");

            Stmt<R> body = Statement();

            if (increment != null)
            {
                body =
                    new Block<R>
                    (
                        new List<Stmt<R>>()
                        {
                            body,
                            new Expression<R>(increment)
                        }
                    );
            }

            if (condition == null)
            {
                condition = new Literal<R>(true);
            }

            body = new While<R>(condition, body);

            if (initializer != null)
            {
                body = new Block<R>(new List<Stmt<R>> { initializer, body });
            }

            return body;
        }

        private Stmt<R> WhileStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
            Expr<R> condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
            Stmt<R> body = Statement();

            return new While<R>(condition, body);
        }

        private Stmt<R> IfStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
            Expr<R> condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

            Stmt<R> thenBranch = Statement();
            Stmt<R> elseBranch = null;

            if (Match(new TokenType[] { TokenType.ELSE }))
            {
                elseBranch = Statement();
            }

            return new If<R>(condition, thenBranch, elseBranch);
        }

        private List<Stmt<R>> Block()
        {
            List<Stmt<R>> statements = new List<Stmt<R>>();

            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            {
                statements.Add(Declaration());
            }

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
            return statements;
        }

        private Stmt<R> PrintStatement()
        {
            Expr<R> value = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after value.");
            return new Print<R>(value);
        }

        private Stmt<R> ExpressionStatement()
        {
            Expr<R> expr = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
            return new Expression<R>(expr);
        }

        private Expr<R> Equality()
        {
            Expr<R> expr = Comparison();

            while (Match(new TokenType[] { TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL }))
            {
                Token operatorToken = Previous();
                Expr<R> right = Comparison();
                expr = new Binary<R>(expr, operatorToken, right);
            }

            return expr;
        }

        private Expr<R> Comparison()
        {
            Expr<R> expr = Term();

            while (Match(new TokenType[] { TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL }))
            {
                Token operatorToken = Previous();
                Expr<R> right = Term();
                expr = new Binary<R>(expr, operatorToken, right);
            }

            return expr;
        }

        private Expr<R> Term()
        {
            Expr<R> expr = Factor();

            while (Match(new TokenType[] { TokenType.MINUS, TokenType.PLUS }))
            {
                Token operatorToken = Previous();
                Expr<R> right = Factor();
                expr = new Binary<R>(expr, operatorToken, right);
            }

            return expr;
        }

        private Expr<R> Factor()
        {
            Expr<R> expr = Unary();

            while (Match(new TokenType[] { TokenType.SLASH, TokenType.STAR }))
            {
                Token operatorToken = Previous();
                Expr<R> right = Unary();
                expr = new Binary<R>(expr, operatorToken, right);
            }

            return expr;
        }

        private Expr<R> Unary()
        {
            if (Match(new TokenType[] { TokenType.BANG, TokenType.MINUS }))
            {
                Token operatorToken = Previous();
                Expr<R> right = Unary();
                return new Unary<R>(operatorToken, right);
            }

            return Call();
        }

        private Expr<R> Call()
        {
            Expr<R> expr = Primary();

            while (true)
            {
                if (Match(new TokenType[] { TokenType.LEFT_PAREN }))
                {
                    expr = FinishCall(expr);
                }
                else if (Match(new TokenType[] { TokenType.DOT }))
                {
                    Token name = Consume(TokenType.IDENTIFIER, "Expect property name after '.'.");
                    expr = new Get<R>(expr, name);
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        private Expr<R> FinishCall(Expr<R> callee)
        {
            List<Expr<R>> arguments = new List<Expr<R>>();

            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (arguments.Count >= 255)
                    {
                        Error(Peek(), "Can't have more than 255 arguments.");
                    }

                    arguments.Add(Expression());
                } while (Match(new TokenType[] { TokenType.COMMA }));
            }

            Token paren = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");

            return new Call<R>(callee, paren, arguments);
        }

        private Expr<R> Primary()
        {
            if (Match(new TokenType[] { TokenType.FALSE }))
            {
                return new Literal<R>(false);
            }

            if (Match(new TokenType[] { TokenType.TRUE }))
            {
                return new Literal<R>(true);
            }

            if (Match(new TokenType[] { TokenType.NIL }))
            {
                return new Literal<R>(null);
            }

            if (Match(new TokenType[] { TokenType.NUMBER, TokenType.STRING }))
            {
                return new Literal<R>(Previous().literal);
            }

            if (Match(new TokenType[] { TokenType.SUPER }))
            {
                Token keyword = Previous();
                Consume(TokenType.DOT, "Expect '.' after 'super'.");
                Token method = Consume(TokenType.IDENTIFIER, "Expect superclass method name.");
                return new Super<R>(keyword, method);
            }

            if (Match(new TokenType[] { TokenType.THIS }))
            {
                return new Variable<R>(Previous());
            }

            if (Match(new TokenType[] { TokenType.IDENTIFIER }))
            {
                return new Variable<R>(Previous());
            }

            if (Match(new TokenType[] { TokenType.LEFT_PAREN }))
            {
                Expr<R> expr = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Grouping<R>(expr);
            }


            throw Error(Peek(), "Expect expression.");
        }

        private bool Match(TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private Token Consume(TokenType type, string message)
        {
            if (Check(type)) { return Advance(); }

            throw Error(Peek(), message);
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd())
            {
                return false;
            }
            return Peek().type == type;
        }

        private Token Advance()
        {
            if (!IsAtEnd())
            {
                current++;
            }
            return Previous();
        }

        private bool IsAtEnd()
        {
            return Peek().type == TokenType.EOF;
        }

        private Token Peek()
        {
            return tokens[current];
        }

        private Token Previous()
        {
            return tokens[current - 1];
        }

        private ParseError Error(Token token, string message)
        {
            Lox.Error(token, message);
            return new ParseError();
        }

        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
            {
                if (Previous().type == TokenType.SEMICOLON)
                {
                    return;
                }

                switch (Peek().type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                        return;
                }

                Advance();
            }
        }

        private class ParseError : Exception
        {
        }

    }


}
