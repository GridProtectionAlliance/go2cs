//******************************************************************************************************
//  GeneratorExecutionContextExtensions.cs - Gbtc
//
//  Copyright © 2026, J. Ritchie Carroll.  All Rights Reserved.
//
//  Licensed under the MIT License (MIT), the "License"; you may not use this file except in compliance
//  with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  01/27/2025 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static go2cs.Symbols;

namespace go2cs;

public static class GeneratorExecutionContextExtensions
{
    public static string GetUnderlyingTypeName(string typeName)
    {
        // If type is a pointer, i.e., ж<T>, then get the underlying type. Match the pointer prefix as
        // `ж<` specifically — a bare `ж` scan mis-fires on the marker glyph EMBEDDED in an identifier
        // (crypto/elliptic's `P256PointжnistPoint` self-referential-constraint proxy), slicing the
        // name into garbage so the struct declaration it names can never resolve (a struct embedding
        // a generic instantiation over such a proxy then promotes nothing — CS1061/CS1929/CS1501).
        int startIndex = typeName.IndexOf($"{PointerPrefix}<", StringComparison.Ordinal);

        if (startIndex > -1 && typeName.EndsWith(">"))
            typeName = typeName[(startIndex + 2)..^1];

        return typeName;
    }

    public static ClassDeclarationSyntax? FindPackageClass(this Compilation compilation)
    {
        return compilation.SyntaxTrees
            .SelectMany(tree => tree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>())
            .FirstOrDefault(classDecl =>
            {
                ISymbol? symbol = compilation
                    .GetSemanticModel(classDecl.SyntaxTree)
                    .GetDeclaredSymbol(classDecl);

                string symbolName = symbol?.ToDisplayString() ?? "";
                return symbolName.EndsWith(PackageSuffix);
            });
    }

    public static StructDeclarationSyntax? FindStructDeclaration(this Compilation compilation, string structTypeName)
    {
        structTypeName = GetUnderlyingTypeName(structTypeName);

        Debug.WriteLine($"Finding struct '{structTypeName}'...");

        // A GENERIC INSTANTIATION request (`Foo<Concrete>`) can never string-match a generic
        // struct DECLARATION, whose display name carries its type PARAMETERS (`Foo<T>`); reduce the
        // request to its base name + arity so a same-named, same-arity generic declaration resolves.
        // Needed to promote a struct that embeds a generic instantiation — crypto/elliptic's
        // p256Curve embeds nistCurve<P256PointжnistPoint>, which must resolve to nistCurve<Point>.
        string? genericBaseName = null;
        int genericArity = 0;
        int ltIndex = structTypeName.IndexOf('<');

        if (ltIndex > 0 && structTypeName.EndsWith(">"))
        {
            string baseName = structTypeName[..ltIndex];
            int lastDot = baseName.LastIndexOf('.');
            genericBaseName = lastDot >= 0 ? baseName[(lastDot + 1)..] : baseName;
            genericArity = CountTopLevelTypeArguments(structTypeName);
        }

        return compilation.SyntaxTrees
            .SelectMany(tree => tree.GetRoot()
                .DescendantNodes()
                .OfType<StructDeclarationSyntax>())
            .FirstOrDefault(structDecl =>
            {
                ISymbol? symbol = compilation
                    .GetSemanticModel(structDecl.SyntaxTree)
                    .GetDeclaredSymbol(structDecl);

                string symbolName = symbol?.ToDisplayString() ?? "";

                if (!symbolName.StartsWith("global::") && structTypeName.StartsWith("global::"))
                    symbolName = "global::" + symbolName;

                if (symbolName == structTypeName)
                    return true;

                string[] parts = symbolName.Split('.');

                if (parts.Length > 1)
                    symbolName = parts[^1];

                if (symbolName == structTypeName)
                    return true;

                // Generic base-name + arity fallback — only reached when the exact/simple-name checks
                // above miss, so it can only turn a previously-unresolved generic instantiation into a
                // match, never change an existing non-null result.
                return genericBaseName is not null &&
                       symbol is INamedTypeSymbol { Arity: > 0 } namedSymbol &&
                       namedSymbol.Arity == genericArity &&
                       namedSymbol.Name == genericBaseName;
            });
    }

    /// <summary>
    /// Resolves a fully-qualified type name to its <see cref="INamedTypeSymbol"/>, searching the
    /// compilation's own assembly AND every referenced assembly — including METADATA references.
    /// </summary>
    /// <remarks>
    /// The syntax-based <see cref="FindStructDeclaration"/> can only see types whose SOURCE is in
    /// this compilation (or in a <see cref="CompilationReference"/>). A real MSBuild build hands a
    /// <c>&lt;ProjectReference&gt;</c> to the compiler as a <see cref="PortableExecutableReference"/>
    /// — compiled metadata, no syntax trees — so every cross-package type resolved to null there.
    /// Symbol lookup has no such blind spot: metadata and source types resolve identically.
    /// </remarks>
    public static INamedTypeSymbol? FindTypeSymbol(this Compilation compilation, string typeName)
    {
        typeName = GetUnderlyingTypeName(typeName);

        if (typeName.StartsWith("global::", StringComparison.Ordinal))
            typeName = typeName["global::".Length..];

        // Split on the dots that separate NAME segments, ignoring any dot nested inside a type
        // argument list (`Ns.Outer<A.B>.Inner`), and reduce each segment to its metadata form
        // (`array<ulong>` → ``array`1``, verbatim `@internal` → `internal`).
        List<string> segments = [];
        int depth = 0, start = 0;

        for (int i = 0; i <= typeName.Length; i++)
        {
            if (i < typeName.Length)
            {
                switch (typeName[i])
                {
                    case '<':
                        depth++;
                        continue;
                    case '>':
                        depth--;
                        continue;
                    case '.' when depth == 0:
                        break;
                    default:
                        continue;
                }
            }
            else if (depth != 0)
            {
                // Unbalanced angle brackets — not a name this routine can parse.
                return null;
            }

            segments.Add(ToMetadataSegment(typeName[start..i]));
            start = i + 1;
        }

        if (segments.Count == 0)
            return null;

        // A namespace separator and a nested-type separator are both rendered `.` in a display
        // string but differ in metadata (`.` vs `+`), and the name alone cannot say which is which.
        // Try every split, deepest nesting last, so the common all-namespaces form is tried first.
        for (int nested = 0; nested < segments.Count; nested++)
        {
            string candidate = string.Join(".", segments.Take(segments.Count - nested)) +
                               (nested > 0 ? "+" + string.Join("+", segments.Skip(segments.Count - nested)) : "");

            INamedTypeSymbol? symbol = compilation.GetTypeByMetadataName(candidate);

            if (symbol is not null)
                return symbol;
        }

        return null;
    }

    // Reduces one display-name segment to its metadata spelling: drops the verbatim-identifier
    // escape C# display formatting adds to keyword-like names (`@internal`), and replaces a type
    // argument list with the arity suffix metadata uses (`array<ulong>` → ``array`1``).
    private static string ToMetadataSegment(string segment)
    {
        segment = segment.Replace("@", "");

        int ltIndex = segment.IndexOf('<');

        if (ltIndex < 0 || !segment.EndsWith(">", StringComparison.Ordinal))
            return segment;

        return $"{segment[..ltIndex]}`{CountTopLevelTypeArguments(segment)}";
    }

    // Counts the top-level type arguments of a generic type reference's OUTERMOST `<…>`
    // (`Foo<Bar<Baz>, Qux>` → 2), tracking nesting so an inner comma is not counted.
    private static int CountTopLevelTypeArguments(string typeName)
    {
        int lt = typeName.IndexOf('<');

        if (lt < 0 || !typeName.EndsWith(">"))
            return 0;

        string inner = typeName[(lt + 1)..^1];

        if (inner.Trim().Length == 0)
            return 0;

        int depth = 0;
        int count = 1;

        foreach (char c in inner)
        {
            switch (c)
            {
                case '<':
                    depth++;
                    break;
                case '>':
                    depth--;
                    break;
                case ',' when depth == 0:
                    count++;
                    break;
            }
        }

        return count;
    }

    public static StructDeclarationSyntax? GetLocalStructDeclaration(this GeneratorExecutionContext context, string structTypeName)
    {
        return context.Compilation.FindStructDeclaration(structTypeName);
    }

    public static (StructDeclarationSyntax?, Compilation?) GetStructDeclaration(this GeneratorExecutionContext context, string structTypeName)
    {
        // First check current project using existing method
        StructDeclarationSyntax? result = context.GetLocalStructDeclaration(structTypeName);
        
        if (result is not null)
            return (result, context.Compilation);

        structTypeName = GetUnderlyingTypeName(structTypeName);

        string[] parts = structTypeName.Split('.');

        if (parts.Length < 2)
            return (null, null);

        string structPackageName = parts[^2];
        structTypeName = parts[^1];

        // Get all referenced projects' compilations
        IEnumerable<Compilation> referencedAssemblies = context.Compilation.References
            .OfType<CompilationReference>()
            .Select(reference => reference.Compilation);

        foreach (Compilation? compilation in referencedAssemblies)
        {
            ClassDeclarationSyntax? packageClass = compilation.FindPackageClass();

            if (packageClass is null)
                continue;

            string packageName = packageClass.Identifier.Text;

            // Check if the package name matches the struct package name
            if (!string.Equals(packageName, structPackageName, StringComparison.Ordinal))
                continue;

            result = compilation.FindStructDeclaration(structTypeName);

            if (result is not null)
                return (result, compilation);
        }

        return (null, null);
    }
}
