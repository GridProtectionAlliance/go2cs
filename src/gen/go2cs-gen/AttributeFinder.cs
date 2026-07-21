// AttributeFinder.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace go2cs;

public sealed class AttributeFinder<TDeclarationSyntax>(string attributeFullName) : ISyntaxContextReceiver where TDeclarationSyntax : MemberDeclarationSyntax
{
    public List<(TDeclarationSyntax targetSyntax, List<AttributeSyntax> attributes)> TargetAttributes = [];

    public bool HasAttributes => TargetAttributes.Count > 0;

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is not TDeclarationSyntax { AttributeLists.Count: > 0 })
            return;

        (TDeclarationSyntax, List<AttributeSyntax> attributes) semanticTarget = GetSemanticTargetForGeneration(context);

        if (semanticTarget.attributes.Count > 0)
            TargetAttributes.Add(semanticTarget);
    }

    private (TDeclarationSyntax, List<AttributeSyntax>) GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        List<AttributeSyntax> attributes = [];
        TDeclarationSyntax targetSyntax = (TDeclarationSyntax)context.Node;

        foreach (AttributeSyntax attributeSyntax in targetSyntax.AttributeLists.SelectMany(attributeListSyntax => attributeListSyntax.Attributes))
        {
            if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                continue;

            INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;

            if (string.Equals(attributeFullName, attributeContainingTypeSymbol.OriginalDefinition.ToDisplayString(), StringComparison.Ordinal))
                attributes.Add(attributeSyntax);
        }

        return (targetSyntax, attributes);
    }
}
