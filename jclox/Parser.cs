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
            Expr<R> expr = Equality();

            if (Match(new TokenType[] { TokenType.EQUAL }))
            {
                Token equals = Previous();
                Expr<R> value = Assignment();

                if (expr is Variable<R>) {
                    Token name = ((Variable<R>)expr).name;
                    return new Assign<R>(name, value);
                }

                Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Stmt<R> Declaration()
        {
            try
            {
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
            if (Match(new TokenType[] { TokenType.PRINT }))
            {
                return PrintStatement();
            }

            if (Match(new TokenType[] { TokenType.LEFT_BRACE } ))
            {
                return new Block<R>(Block());
            }

            return ExpressionStatement();
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

            return Primary();
        }

        private Expr<R> Primary()
        {
            if (Match(new TokenType[] { TokenType.FALSE })) return new Literal<R>(false);
            if (Match(new TokenType[] { TokenType.TRUE })) return new Literal<R>(true);
            if (Match(new TokenType[] { TokenType.NIL })) return new Literal<R>(null);

            if (Match(new TokenType[] { TokenType.NUMBER, TokenType.STRING }))
            {
                return new Literal<R>(Previous().literal);
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
