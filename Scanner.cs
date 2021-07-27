using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jclox
{
    public class Scanner
    {
        private readonly string source;
        private readonly List<Token> tokens = new();

        private int start = 0;
        private int current = 0;
        private int line = 1;

        public Scanner(string source)
        {
            this.source = source;
        }

        private static readonly Dictionary<string, TokenType> keywords = new()
        {
            { "and",    TokenType.AND},
            { "class",  TokenType.CLASS},
            { "else",   TokenType.ELSE},
            { "false",  TokenType.FALSE},
            { "for",    TokenType.FOR},
            { "fun",    TokenType.FUN},
            { "if",     TokenType.IF},
            { "nil",    TokenType.NIL},
            { "or",     TokenType.OR},
            { "print",  TokenType.PRINT},
            { "return", TokenType.RETURN},
            { "super",  TokenType.SUPER},
            { "this",   TokenType.THIS},
            { "true",   TokenType.TRUE},
            { "var",    TokenType.VAR},
            { "while",  TokenType.WHILE},
        };

        private bool IsAtEnd()
        {
            return current >= source.Length;
        }

        public List<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                // We are at the beginning of the next lexeme.
                start = current;
                ScanToken();
            }

            tokens.Add(new Token(TokenType.EOF, "", null, line));
            return tokens;
        }

        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*': AddToken(TokenType.STAR); break;
                case '!':
                    AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                    break;
                case '=':
                    AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                    break;
                case '<':
                    AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                    break;
                case '>':
                    AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                    break;
                case '/':
                    if (Match('/'))
                    {
                        // A comment goes until the end of the line.
                        while (Peek() != '\n' && !IsAtEnd())
                        {
                            Advance();
                        }
                    }
                    else
                    {
                        AddToken(TokenType.SLASH);
                    }
                    break;

                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace.
                    break;

                case '\n':
                    line++;
                    break;
                case '"': 
                    ScanString(); 
                    break;

                default:
                    if (IsDigit(c))
                    {
                        ScanNumber();
                    }
                    else if (IsAlpha(c))
                    {
                        ScanIdentifier();
                    }
                    else
                    {
                        Lox.Error(line, "Unexpected character.");
                    }
                    break;
            }
        }

        private void ScanIdentifier()
        {
            while (IsAlphaNumeric(Peek()))
            {
                Advance();
            }

            string text = source.Substring(start, current - start);

            if (!keywords.ContainsKey(text)) 
            {
                AddToken(TokenType.IDENTIFIER);
            }
            else
            {
                AddToken(keywords[text]);
            }
        }

        private static bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                    c == '_';
        }

        private static bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        private static bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private void ScanNumber()
        {
            while (IsDigit(Peek()))
            {
                Advance();
            }

            // Look for a fractional part.
            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                // Consume the "."
                Advance();

                while (IsDigit(Peek()))
                {
                    Advance();
                }
            }

            AddToken
            (
                TokenType.NUMBER,
                double.Parse(source.Substring(start, current - start))
            );
        }

        private char PeekNext()
        {
            if (current + 1 >= source.Length)
            {
                return '\0';
            }
            return source[current + 1];
        }

        private void ScanString() 
        {
            while (Peek() != '"' && !IsAtEnd()) 
            {
                if (Peek() == '\n') 
                { 
                    line++; 
                }
                Advance();
            }

            if (IsAtEnd()) 
            {
              Lox.Error(line, "Unterminated string.");
              return;
            }

            // The closing ".
            Advance();

            // Trim the surrounding quotes.
            string value = source.Substring(start + 1, current - 1 - (start + 1));
            AddToken(TokenType.STRING, value);
        }

        private char Peek()
        {
            if (IsAtEnd())
            {
                return '\0';
            }
            return source[current];
        }

        private bool Match(char expected)
        {
            if (IsAtEnd())
            {
                return false;
            }
            if (source[current] != expected) 
            {
                return false;
            }

            current++;
            return true;
        }

        private char Advance()
        {
            return source[current++];
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, Object literal)
        {
            string text = source.Substring(start, current - start);
            tokens.Add(new Token(type, text, literal, line));
        }
    }
}
