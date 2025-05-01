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
        // If type is a pointer, i.e., ж<T>, then get the underlying type
        int startIndex = typeName.IndexOf(PointerPrefix, StringComparison.Ordinal);

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

                return symbolName == structTypeName;
            });
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
