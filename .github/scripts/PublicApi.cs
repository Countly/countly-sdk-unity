using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Prints the public C# API declared in the source files under one or more directories as sorted lines, so two
/// versions can be compared. It reads syntax only, so the sources do not have to compile and Unity is not needed.
/// Usage: PublicApi "&lt;symbol set&gt;;&lt;symbol set&gt;..." &lt;directory&gt; [&lt;directory&gt; ...]
/// Each symbol set is a comma separated list of preprocessor symbols; the files are parsed once per set and the
/// lines of every set are merged, so API that only exists for one platform is listed too.
/// </summary>
internal static class PublicApi
{
    private static readonly Regex Whitespace = new Regex(@"\s+");

    /// <summary>
    /// Parses the sources once per symbol set and prints the merged, sorted public API lines.
    /// </summary>
    /// <param name="args">[symbol sets] followed by the source directories.</param>
    /// <returns>0 when lines were printed, 1 when no public declaration was found, 2 on bad arguments.</returns>
    private static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: PublicApi \"<symbol set>;...\" <directory> [<directory> ...]");
            return 2;
        }

        string[][] symbolSets = args[0].Split(';').Select(set => set.Split(',', StringSplitOptions.RemoveEmptyEntries)).ToArray();
        List<string> files = args.Skip(1).SelectMany(dir => Directory.EnumerateFiles(dir, "*.cs", SearchOption.AllDirectories)).Where(f => !IsBuildOutput(f)).OrderBy(f => f, StringComparer.Ordinal).ToList();
        SortedSet<string> lines = new SortedSet<string>(StringComparer.Ordinal);
        foreach (string[] symbols in symbolSets)
        {
            CSharpParseOptions options = new CSharpParseOptions(LanguageVersion.Latest, preprocessorSymbols: symbols);
            foreach (string file in files)
            {
                CompilationUnitSyntax root = (CompilationUnitSyntax)CSharpSyntaxTree.ParseText(File.ReadAllText(file), options).GetRoot();
                foreach (MemberDeclarationSyntax member in root.Members)
                {
                    Visit(member, "", true, false, lines);
                }
            }
        }

        if (lines.Count == 0)
        {
            Console.Error.WriteLine("No public declaration was found.");
            return 1;
        }
        foreach (string line in lines)
        {
            Console.WriteLine(line);
        }
        return 0;
    }

    /// <summary>
    /// Adds the lines of a declaration and of the public declarations it contains.
    /// </summary>
    /// <param name="member">The declaration to describe.</param>
    /// <param name="scope">The namespace or type that contains it, empty at the top level.</param>
    /// <param name="scopeVisible">Whether code outside the assembly can see the containing scope.</param>
    /// <param name="inInterface">Whether the declaration is an interface member, which is public by default.</param>
    /// <param name="lines">The lines collected so far.</param>
    private static void Visit(MemberDeclarationSyntax member, string scope, bool scopeVisible, bool inInterface, ISet<string> lines)
    {
        string prefix = scope.Length == 0 ? "" : scope + ".";
        switch (member)
        {
            case BaseNamespaceDeclarationSyntax ns:
                foreach (MemberDeclarationSyntax child in ns.Members)
                {
                    Visit(child, prefix + ns.Name, scopeVisible, false, lines);
                }
                return;
            case EnumDeclarationSyntax enumDeclaration:
                if (!IsVisible(enumDeclaration.Modifiers, scopeVisible, inInterface))
                {
                    return;
                }
                string enumName = prefix + enumDeclaration.Identifier.Text;
                lines.Add($"{enumName} enum{Suffix(enumDeclaration.BaseList)}");
                foreach (EnumMemberDeclarationSyntax value in enumDeclaration.Members)
                {
                    lines.Add($"{enumName}.{value.Identifier.Text} enum value");
                }
                return;
            case TypeDeclarationSyntax type:
                if (!IsVisible(type.Modifiers, scopeVisible, inInterface))
                {
                    return;
                }
                string typeName = prefix + type.Identifier.Text + Flatten(type.TypeParameterList);
                lines.Add($"{typeName} {type.Keyword.Text}{Keywords(type.Modifiers, "static", "abstract", "sealed")}{Suffix(type.BaseList)}");
                bool isInterface = type is InterfaceDeclarationSyntax;
                foreach (MemberDeclarationSyntax child in type.Members)
                {
                    Visit(child, typeName, true, isInterface, lines);
                }
                return;
            case DelegateDeclarationSyntax del:
                if (IsVisible(del.Modifiers, scopeVisible, inInterface))
                {
                    lines.Add($"{prefix}{del.Identifier.Text}{Flatten(del.TypeParameterList)} delegate {Flatten(del.ReturnType)}{Parameters(del.ParameterList)}");
                }
                return;
            default:
                // Type members are listed below.
                break;
        }

        if (!scopeVisible || !IsVisible(member.Modifiers, true, inInterface))
        {
            return;
        }
        string kinds = Keywords(member.Modifiers, "protected", "static", "abstract", "virtual", "override", "sealed", "readonly", "const");
        switch (member)
        {
            case MethodDeclarationSyntax method when method.ExplicitInterfaceSpecifier == null:
                lines.Add($"{prefix}{method.Identifier.Text}{Flatten(method.TypeParameterList)}{Parameters(method.ParameterList)} method {Flatten(method.ReturnType)}{kinds}");
                break;
            case ConstructorDeclarationSyntax constructor when !constructor.Modifiers.Any(SyntaxKind.StaticKeyword):
                lines.Add($"{prefix}{constructor.Identifier.Text}{Parameters(constructor.ParameterList)} constructor{kinds}");
                break;
            case PropertyDeclarationSyntax property when property.ExplicitInterfaceSpecifier == null:
                lines.Add($"{prefix}{property.Identifier.Text} property {Flatten(property.Type)}{Accessors(property.AccessorList, property.ExpressionBody)}{kinds}");
                break;
            case IndexerDeclarationSyntax indexer when indexer.ExplicitInterfaceSpecifier == null:
                lines.Add($"{prefix}this{Parameters(indexer.ParameterList)} indexer {Flatten(indexer.Type)}{Accessors(indexer.AccessorList, indexer.ExpressionBody)}{kinds}");
                break;
            case FieldDeclarationSyntax field:
                foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
                {
                    lines.Add($"{prefix}{variable.Identifier.Text} field {Flatten(field.Declaration.Type)}{kinds}");
                }
                break;
            case EventFieldDeclarationSyntax eventField:
                foreach (VariableDeclaratorSyntax variable in eventField.Declaration.Variables)
                {
                    lines.Add($"{prefix}{variable.Identifier.Text} event {Flatten(eventField.Declaration.Type)}{kinds}");
                }
                break;
            case EventDeclarationSyntax eventDeclaration when eventDeclaration.ExplicitInterfaceSpecifier == null:
                lines.Add($"{prefix}{eventDeclaration.Identifier.Text} event {Flatten(eventDeclaration.Type)}{kinds}");
                break;
            case OperatorDeclarationSyntax op:
                lines.Add($"{prefix}operator {op.OperatorToken.Text}{Parameters(op.ParameterList)} operator {Flatten(op.ReturnType)}");
                break;
            case ConversionOperatorDeclarationSyntax conversion:
                lines.Add($"{prefix}{conversion.ImplicitOrExplicitKeyword.Text} operator {Flatten(conversion.Type)}{Parameters(conversion.ParameterList)} conversion");
                break;
            default:
                // Explicit interface implementations, static constructors and finalizers are left out.
                break;
        }
    }

    /// <summary>
    /// Returns whether a source file lies under a build output folder, obj or bin, whose generated files are not API.
    /// </summary>
    /// <param name="path">The source file's path.</param>
    /// <returns>True for a file under a folder named obj or bin.</returns>
    private static bool IsBuildOutput(string path)
    {
        return path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }).Any(part => part == "obj" || part == "bin");
    }

    /// <summary>
    /// Returns whether a declaration with these modifiers can be seen from outside the assembly.
    /// </summary>
    /// <param name="modifiers">The declaration's modifiers.</param>
    /// <param name="scopeVisible">Whether its containing scope can be seen from outside the assembly.</param>
    /// <param name="inInterface">Whether it is an interface member, which is public unless marked otherwise.</param>
    /// <returns>True for public and protected declarations in a visible scope.</returns>
    private static bool IsVisible(SyntaxTokenList modifiers, bool scopeVisible, bool inInterface)
    {
        if (!scopeVisible || modifiers.Any(SyntaxKind.PrivateKeyword) || (modifiers.Any(SyntaxKind.InternalKeyword) && !modifiers.Any(SyntaxKind.ProtectedKeyword)))
        {
            return false;
        }
        return inInterface || modifiers.Any(SyntaxKind.PublicKeyword) || modifiers.Any(SyntaxKind.ProtectedKeyword);
    }

    /// <summary>
    /// Returns the parameter list with the types, modifiers and optional markers, without parameter names.
    /// </summary>
    /// <param name="list">The parameter list to describe.</param>
    /// <returns>The parameters as "(type, ref type, type = ...)".</returns>
    private static string Parameters(BaseParameterListSyntax list)
    {
        IEnumerable<string> described = list.Parameters.Select(parameter =>
            string.Join(" ", parameter.Modifiers.Select(modifier => modifier.Text).Append(Flatten(parameter.Type)))
            + (parameter.Default != null ? " = ..." : ""));
        return "(" + string.Join(", ", described) + ")";
    }

    /// <summary>
    /// Returns the accessors of a property or indexer that code outside the assembly can call.
    /// </summary>
    /// <param name="list">The accessor list, or null for an expression-bodied member.</param>
    /// <param name="expressionBody">The expression body, which means a getter only.</param>
    /// <returns>The visible accessors as " { get; set; }".</returns>
    private static string Accessors(AccessorListSyntax list, ArrowExpressionClauseSyntax expressionBody)
    {
        if (list == null)
        {
            return expressionBody != null ? " { get; }" : "";
        }
        IEnumerable<string> visible = list.Accessors
            .Where(accessor => !accessor.Modifiers.Any(SyntaxKind.PrivateKeyword) && !(accessor.Modifiers.Any(SyntaxKind.InternalKeyword) && !accessor.Modifiers.Any(SyntaxKind.ProtectedKeyword)))
            .Select(accessor => accessor.Keyword.Text + ";");
        return " { " + string.Join(" ", visible) + " }";
    }

    /// <summary>
    /// Returns the listed keywords that the modifiers contain, each preceded by a space.
    /// </summary>
    /// <param name="modifiers">The declaration's modifiers.</param>
    /// <param name="wanted">The keywords to report, in the order they are printed.</param>
    /// <returns>For example " static abstract", or an empty string.</returns>
    private static string Keywords(SyntaxTokenList modifiers, params string[] wanted)
    {
        return string.Concat(wanted.Where(keyword => modifiers.Any(modifier => modifier.Text == keyword)).Select(keyword => " " + keyword));
    }

    /// <summary>
    /// Returns a syntax node's text in Roslyn's normal formatting on one line, or an empty string for null.
    /// </summary>
    /// <param name="node">The node to print.</param>
    /// <returns>The node's text with single spaces and no line breaks.</returns>
    private static string Flatten(SyntaxNode node)
    {
        return node == null ? "" : Whitespace.Replace(node.NormalizeWhitespace().ToString(), " ").Trim();
    }

    /// <summary>
    /// Returns a syntax node's flattened text preceded by a space, or an empty string for null.
    /// </summary>
    /// <param name="node">The node to print, such as a base type list.</param>
    /// <returns>" " followed by the node's text, or an empty string.</returns>
    private static string Suffix(SyntaxNode node)
    {
        return node == null ? "" : " " + Flatten(node);
    }
}
