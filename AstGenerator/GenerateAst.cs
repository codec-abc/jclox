using System;
using System.Collections.Generic;
using System.Text;

namespace AstGenerator
{
    class GenerateAst
    {
        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: generate_ast <output directory>");
                return 64;
            }

            string outputDir = args[0];

            DefineAst
            (
                outputDir, "Expr", new List<string> {
                  "Binary   : Expr left, Token operator, Expr right",
                  "Grouping : Expr expression",
                  "Literal  : Object value",
                  "Unary    : Token operator, Expr right"
                }
            );

            return 0;
        }


        private static void DefineAst
        (
            string outputDir,
            string baseName,
            List<string> types
        )
        {
            string path = outputDir + "/" + baseName + ".cs";

            var writer = new StringBuilder();

            //writer.Append("namespace com.craftinginterpreters.lox;" + "\n");
            //writer.Append("\n");
            writer.AppendLine("using System.Collections.Generic;");
            writer.AppendLine("");
            writer.AppendLine("abstract class " + baseName + " {");

            DefineVisitor(writer, baseName, types);

            writer.AppendLine("}");

            foreach(var type in types)
            {
                string className = type.Split(":")[0].Trim();
                string fields = type.Split(":")[1].Trim();
                DefineType(writer, baseName, className, fields);
            }

            // The base accept() method.
            writer.AppendLine();
            writer.AppendLine("  abstract <R> R accept(Visitor<R> visitor);");

            System.IO.File.WriteAllText(path, writer.ToString());
        }

        private static void DefineType
        (
            StringBuilder writer, 
            string baseName,
            string className, 
            string fieldList
        )
        {
            writer.AppendLine("  static class " + className + " extends " +
                baseName + " {");

            // Constructor.
            writer.AppendLine("    " + className + "(" + fieldList + ") {");

            // Store parameters in fields.
            string[] fields = fieldList.Split(", ");
            foreach (var field in fields)
            {
                string name = field.Split(" ")[1];
                writer.AppendLine("      this." + name + " = " + name + ";");
            }

            writer.AppendLine("    }");

            // Visitor pattern.
            writer.AppendLine();
            writer.AppendLine("    @Override");
            writer.AppendLine("    <R> R accept(Visitor<R> visitor) {");
            writer.AppendLine("      return visitor.visit" +
                className + baseName + "(this);");
            writer.AppendLine("    }");

            // Fields.
            writer.AppendLine();
            foreach (var field in fields)
            {
                writer.AppendLine("    final " + field + ";");
            }

            writer.AppendLine("  }");
        }

        private static void DefineVisitor
        (
            StringBuilder writer, 
            string baseName,
            List<string> types
        )
        {
            writer.AppendLine("  interface Visitor<R> {");

            foreach (var type in types)
            {
                string typeName = type.Split(":")[0].Trim();
                writer.AppendLine("    R visit" + typeName + baseName + "(" +
                    typeName + " " + baseName.ToLower() + ");");
            }

            writer.AppendLine("  }");
        }
    }
}
