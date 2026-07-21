// InterfaceDeclarationSyntaxExtensions.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace go2cs;

public static class InterfaceDeclarationSyntaxExtensions
{
    public static MethodInfo[] GetInterfaceMethods(
        this InterfaceDeclarationSyntax interfaceDeclaration,
        GeneratorExecutionContext context)
    {
        // Get the semantic model to access symbol information
        SemanticModel semanticModel = context.Compilation.GetSemanticModel(interfaceDeclaration.SyntaxTree);

        // Get the symbol for the interface
        if (semanticModel.GetDeclaredSymbol(interfaceDeclaration) is not INamedTypeSymbol interfaceSymbol)
            return [];

        // Get all methods, including inherited ones
        List<MethodInfo> allMethods = [];

        // First, collect methods from this interface
        IEnumerable<MethodInfo> directMethods = interfaceDeclaration
            .Members
            .OfType<MethodDeclarationSyntax>()
            .Select(method => method.GetMethodInfo(context.Compilation));

        allMethods.AddRange(directMethods);

        // Next, collect methods from base interfaces
        foreach (INamedTypeSymbol? baseInterface in interfaceSymbol.AllInterfaces)
        {
            foreach (IMethodSymbol? member in baseInterface.GetMembers().OfType<IMethodSymbol>())
            {
                // Skip methods that might be overridden in the derived interface
                if (!allMethods.Any(method => method.IsSameSignature(member)))
                    allMethods.Add(member.GetMethodInfo());
            }
        }

        return allMethods.ToArray();
    }
}
