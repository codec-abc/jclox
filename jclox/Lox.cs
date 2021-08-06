using System;
using System.Collections.Generic;

namespace jclox
{
    class Lox
    {
        static bool hadError = false;

        public static int Main(String[] args)
        {
            //if (args.Length > 1) 
            //{
            //    Console.WriteLine("Usage: jlox [script]");
            //    return 64;
            //} 
            //else if (args.Length == 1) 
            //{
            //    RunFile(args[0]);
            //} 
            //else 
            //{
            //    RunPrompt();
            //}

            Expr<string> expression = new Binary<string>(
                new Unary<string>(
                    new Token(TokenType.MINUS, "-", null, 1),
                    new Literal<string>(123)
                ),
                new Token(TokenType.STAR, "*", null, 1),
                new Grouping<string>(
                    new Literal<string>(45.67)
                )
            );

            Console.WriteLine(new AstPrinter().Print(expression));

            return 0;
        }

        private static void RunFile(String path)
        {
            var text = System.IO.File.ReadAllText(path);
            Run(text);
        }

        private static void RunPrompt()
        {
            for (; ; )
            {
                Console.WriteLine("> ");
                string line = Console.ReadLine();
                if (line == null)
                {
                    break;
                }
                Run(line);

                hadError = false;
            }
        }

        public static void Error(int line, String message)
        {
            Report(line, "", message);
        }

        private static void Report
        (
            int line,
            String where,
            String message
        )
        {
            Console.WriteLine(
                "[line " + line + "] Error" + where + ": " + message
            );

            hadError = true;
        }

        private static void Run(string source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();

            Parser<string> parser = new Parser<string>(tokens);
            Expr<string> expression = parser.Parse();

            // Stop if there was a syntax error.
            if (hadError)
            {
                return;
            }

            Console.WriteLine(new AstPrinter().Print(expression));
        }

        public static void Error(Token token, string message)
        {
            if (token.type == TokenType.EOF)
            {
                Report(token.line, " at end", message);
            }
            else
            {
                Report(token.line, " at '" + token.lexeme + "'", message);
            }
        }
    }
}
