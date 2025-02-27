//******************************************************************************************************
//  TypeDeclarationSyntaxExtensions.cs - Gbtc
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
//  02/26/2025 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

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
