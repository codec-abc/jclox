using System;
using System.Collections.Generic;

namespace jclox
{
    class Program
    {
        public static int main(String[] args)
        {
            if (args.Length > 1) {
                Console.WriteLine("Usage: jlox [script]");
                return 64;
            } else if (args.Length == 1) {
                runFile(args[0]);
            } else {
                runPrompt();
            }

            return 0;
        }

        private static void runFile(String path) 
        {
            var text = System.IO.File.ReadAllText(path);
            run(text);
        }

        private static void runPrompt()
        {

            for (;;) {
                Console.WriteLine("> ");
                string line = Console.ReadLine();
                if (line == null)
                {
                    break;
                }
                run(line);

                hadError = false;
            }
        }

        static void error(int line, String message)
        {
            report(line, "", message);
        }

        static bool hadError = false;

        private static void report(
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

        private static void run(String source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.scanTokens();

            // For now, just print the tokens.
            foreach(var token in tokens)
            {
                Console.WriteLine(token);
            }
        }
    }
}
