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

        public Expr<R> Parse()
        {
            try
            {
                return Expression();
            }
            catch (ParseError error)
            {
                return null;
            }
        }

        private Expr<R> Expression()
        {
            return Equality();
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
