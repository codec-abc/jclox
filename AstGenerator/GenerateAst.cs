﻿using System;
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
                  "Binary   : Expr<R> left, Token operatorToken, Expr<R> right",
                  "Grouping : Expr<R> expression",
                  "Literal  : object value",
                  "Unary    : Token operatorToken, Expr<R> right"
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
            writer.AppendLine("using jclox;");
            writer.AppendLine("using System.Collections.Generic;");
            writer.AppendLine("");
            writer.AppendLine("public abstract class " + baseName + "<R> {");
            writer.AppendLine("    public abstract R Accept(Visitor<R> visitor);");
            writer.AppendLine("}");
            writer.AppendLine("");

            DefineVisitor(writer, baseName, types);

            foreach (var type in types)
            {
                string className = type.Split(":")[0].Trim();
                string fields = type.Split(":")[1].Trim();
                DefineType(writer, baseName, className, fields);
            }

            // The base accept() method.
            //writer.AppendLine();
            //writer.AppendLine("  abstract <R> R accept(Visitor<R> visitor);");

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
            writer.AppendLine("public class " + className + "<R> : " +
                baseName + "<R> {");

            // Constructor.
            writer.AppendLine("    public " + className + "(" + fieldList + ") {");

            // Store parameters in fields.
            string[] fields = fieldList.Split(", ");
            foreach (var field in fields)
            {
                string name = field.Split(" ")[1];
                writer.AppendLine("        this." + name + " = " + name + ";");
            }

            writer.AppendLine("    }");

            // Visitor pattern.
            writer.AppendLine();
            //writer.AppendLine("    @Override");
            //writer.AppendLine("    <R> R accept(Visitor<R> visitor) {");
            writer.AppendLine("    public override R Accept(Visitor<R> visitor) {");
            writer.AppendLine("        return visitor.Visit" +
                className + baseName + "(this);");
            writer.AppendLine("    }");

            // Fields.
            writer.AppendLine();
            foreach (var field in fields)
            {
                writer.AppendLine("    public readonly " + field + ";");
            }

            writer.AppendLine("  }");
            writer.AppendLine("");
        }

        private static void DefineVisitor
        (
            StringBuilder writer, 
            string baseName,
            List<string> types
        )
        {
            writer.AppendLine("public interface Visitor<R> {");

            foreach (var type in types)
            {
                string typeName = type.Split(":")[0].Trim();
                writer.AppendLine("    R Visit" + typeName + baseName + "(" +
                    typeName + "<R> " + baseName.ToLower() + ");");
            }

            writer.AppendLine("  }");
            writer.AppendLine("");
        }
    }
}
