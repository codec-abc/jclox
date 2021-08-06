using System;
using System.Collections.Generic;

namespace jclox
{
    class Lox
    {
        private static readonly Interpreter interpreter = new Interpreter();
        static bool hadError = false;
        static bool hadRuntimeError = false;

        public static int Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: jlox [script]");
                return 64;
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }

            //Expr<string> expression = new Binary<string>(
            //    new Unary<string>(
            //        new Token(TokenType.MINUS, "-", null, 1),
            //        new Literal<string>(123)
            //    ),
            //    new Token(TokenType.STAR, "*", null, 1),
            //    new Grouping<string>(
            //        new Literal<string>(45.67)
            //    )
            //);

            //Console.WriteLine(new AstPrinter().Print(expression));

            return 0;
        }

        private static void RunFile(string path)
        {
            var text = System.IO.File.ReadAllText(path);
            Run(text);

            if (hadError)
            {
                Environment.Exit(65);
            }

            if (hadRuntimeError)
            {
                Environment.Exit(70);
            }

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

            Parser<object> parser = new Parser<object>(tokens);
            Expr<object> expression = parser.Parse();

            // Stop if there was a syntax error.
            if (hadError)
            {
                return;
            }

            interpreter.Interpret(expression);

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

        public static void RuntimeError(RuntimeError error)
        {
            Console.WriteLine(error.Message + "\n[line " + error.token.line + "]");
            hadRuntimeError = true;
        }
    }
}
