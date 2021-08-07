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

            return 0;
        }

        private static void RunFile(string path)
        {
            var text = System.IO.File.ReadAllText(path);
            Run(text);

            if (hadError)
            {
                System.Environment.Exit(65);
            }

            if (hadRuntimeError)
            {
                System.Environment.Exit(70);
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
            List<Stmt<object>> statements = parser.Parse();

            // Stop if there was a syntax error.
            if (hadError)
            {
                return;
            }

            Resolver resolver = new Resolver(interpreter);
            resolver.Resolve(statements);

            if (hadError)
            {
                return;
            }

            interpreter.Interpret(statements);

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
