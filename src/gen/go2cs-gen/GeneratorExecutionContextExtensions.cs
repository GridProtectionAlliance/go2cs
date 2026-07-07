//******************************************************************************************************
//  GeneratorExecutionContextExtensions.cs - Gbtc
//
//  Copyright © 2025, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
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
