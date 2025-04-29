//******************************************************************************************************
//  AssemblyAttributeFinder.cs - Gbtc
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
//  01/16/2025 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace go2cs;

public sealed class AssemblyAttributeFinder(string attributeFullName) : ISyntaxContextReceiver
{
    public List<(AttributeSyntax attribute, GeneratorSyntaxContext context, CompilationUnitSyntax compilationUnit, FileScopedNamespaceDeclarationSyntax? namespaceSyntax)> TargetAttributes = [];

    public bool HasAttributes => TargetAttributes.Count > 0;

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        // Assembly attributes are contained in AttributeListSyntax nodes that have the "assembly" target specifier
        if (context.Node is not AttributeListSyntax attributeList || attributeList.Target?.Identifier.Text != "assembly")
            return;

        List<AttributeSyntax> attributes = GetSemanticTargetForGeneration(context, attributeList);

        if (attributes.Count == 0)
            return;

        // Get the compilation unit (file-level syntax node) - allows access to other code elements in same file
        CompilationUnitSyntax? compilationUnit = attributeList.Ancestors().OfType<CompilationUnitSyntax>().FirstOrDefault();

        if (compilationUnit is null)
            return;

        // Find the first namespace in the file (if any)
        FileScopedNamespaceDeclarationSyntax? namespaceSyntax = compilationUnit.Members
            .OfType<FileScopedNamespaceDeclarationSyntax>()
            .FirstOrDefault();

        // Add each attribute with its containing context
        foreach (AttributeSyntax attribute in attributes)
            TargetAttributes.Add((attribute, context, compilationUnit, namespaceSyntax));
    }

    private List<AttributeSyntax> GetSemanticTargetForGeneration(GeneratorSyntaxContext context, AttributeListSyntax attributeList)
    {
        List<AttributeSyntax> attributes = [];

        foreach (AttributeSyntax attributeSyntax in attributeList.Attributes)
        {
            if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                continue;

            INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;

            if (attributeContainingTypeSymbol.IsGenericType && string.Equals(attributeFullName, attributeContainingTypeSymbol.OriginalDefinition.ToDisplayString(), StringComparison.Ordinal))
                attributes.Add(attributeSyntax);
        }

        return attributes;
    }
}
