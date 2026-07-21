// SyntaxNodeExtensions.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace go2cs;

public static class SyntaxNodeExtensions
{
    public static string GetNamespaceName(this SyntaxNode syntaxNode)
    {
        BaseNamespaceDeclarationSyntax[] namespaceDeclarations = syntaxNode.Ancestors()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .ToArray();

        if (!namespaceDeclarations.Any())
            return string.Empty;

        // Build the full namespace by joining nested namespaces
        string namespaceName = string.Join(".", namespaceDeclarations
            .Select(ns => ns.Name.ToString())
            .Reverse());

        return namespaceName;
    }

    public static string GetParentClassName(this SyntaxNode syntaxNode)
    {
        ClassDeclarationSyntax? classDeclaration = syntaxNode.Ancestors()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault();

        return classDeclaration?.Identifier.Text ?? string.Empty;
    }
}
