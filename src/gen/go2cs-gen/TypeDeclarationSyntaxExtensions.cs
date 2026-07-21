// TypeDeclarationSyntaxExtensions.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace go2cs;

public static class TypeDeclarationSyntaxExtensions
{
    public static bool AllGenericTypesHaveConstraint(this TypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel, string constraintName)
    {
        // Get the symbol for the type declaration
        if (semanticModel.GetDeclaredSymbol(typeDeclaration) is not INamedTypeSymbol { IsGenericType: true } typeSymbol)
            return false;

        // Get the constraint symbol from the compilation
        Compilation compilation = semanticModel.Compilation;
        INamedTypeSymbol? constraintSymbol = compilation.GetTypeByMetadataName(constraintName);

        if (constraintSymbol is null)
            return false;

        // Check each type parameter
        foreach (ITypeParameterSymbol? typeParameter in typeSymbol.TypeParameters)
        {
            // If there are no constraints on the type parameter, then it doesn't implement the interface
            if (typeParameter.ConstraintTypes.Length == 0)
                return false;

            // Check each constraint on the type parameter
            foreach (ITypeSymbol? constraintType in typeParameter.ConstraintTypes)
            {
                // Check if the constraint implements IEqualityOperators (directly or indirectly)
                if (!ImplementsInterface(constraintType, constraintSymbol))
                    return false;
            }
        }

        return true;
    }

    private static bool ImplementsInterface(ITypeSymbol typeSymbol, INamedTypeSymbol interfaceSymbol)
    {
        // Check if the type is the interface itself
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol && SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, interfaceSymbol))
            return true;

        // Check if the type implements the interface
        foreach (INamedTypeSymbol? implementedInterface in typeSymbol.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(implementedInterface.OriginalDefinition, interfaceSymbol))
                return true;
        }

        return false;
    }
}
