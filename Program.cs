using System;
using System.Collections.Generic;

namespace jclox
{
    class Program
    {
        public static int Main(String[] args)
        {
            if (args.Length > 1) {
                Console.WriteLine("Usage: jlox [script]");
                return 64;
            } else if (args.Length == 1) {
                RunFile(args[0]);
            } else {
                RunPrompt();
            }

            return 0;
        }

        private static void RunFile(String path) 
        {
            var text = System.IO.File.ReadAllText(path);
            Run(text);
        }

        private static void RunPrompt()
        {
            for (;;) {
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

        static void Error(int line, String message)
        {
            Report(line, "", message);
        }

        static bool hadError = false;

        private static void Report(
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

        private static void Run(String source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();

            // For now, just print the tokens.
            foreach(var token in tokens)
            {
                Console.WriteLine(token);
            }
        }
    }
}
