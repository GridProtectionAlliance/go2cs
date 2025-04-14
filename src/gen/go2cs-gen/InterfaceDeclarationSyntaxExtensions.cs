//******************************************************************************************************
//  InterfaceDeclarationSyntaxExtensions.cs - Gbtc
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
//  04/12/2025 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

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
            .Select(method => method.GetMethodInfo(context));

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
