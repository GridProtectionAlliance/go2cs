//******************************************************************************************************
//  AttributeFinder.cs - Gbtc
//
//  Copyright © 2024, Grid Protection Alliance.  All Rights Reserved.
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
//  07/08/2024 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace go2cs.CodeGenerators;

public sealed class AttributeFinder<TDeclarationSyntax>(string attributeFullName) : ISyntaxContextReceiver where TDeclarationSyntax : BaseTypeDeclarationSyntax
{
    public List<(TDeclarationSyntax targetSyntax, List<AttributeSyntax> attributes)> TargetAttributes = [];

    public bool HasAttributes => TargetAttributes.Count > 0;

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (DeclarationHasAttributes(context.Node))
            TargetAttributes.Add(GetSemanticTargetForGeneration(context));
    }

    private static bool DeclarationHasAttributes(SyntaxNode node)
    {
        return node is TDeclarationSyntax { AttributeLists.Count: > 0 };
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

            if (string.Equals(attributeFullName, attributeContainingTypeSymbol.ToDisplayString(), StringComparison.Ordinal))
                attributes.Add(attributeSyntax);
        }

        return (targetSyntax, attributes);
    }
}

public static class AttributeSyntaxExtensions
{
    public static string[] GetArgumentValues(this AttributeSyntax attribute)
    {
        SeparatedSyntaxList<AttributeArgumentSyntax> arguments = attribute.ArgumentList?.Arguments ?? default;
        return arguments.Select(argument => argument.Expression.NormalizeWhitespace().ToFullString()).ToArray();
    }
}

public static class StructDeclarationSyntaxExtensions
{
    public static List<(string typeName, string fieldName)> GetStructFields(this StructDeclarationSyntax structDeclaration, GeneratorExecutionContext context)
    {
        // Obtain the SemanticModel from the context
        SemanticModel semanticModel = context.Compilation.GetSemanticModel(structDeclaration.SyntaxTree);

        List<(string typeName, string fieldName)> fields = [];

        foreach (FieldDeclarationSyntax? fieldDeclaration in structDeclaration.Members.OfType<FieldDeclarationSyntax>())
        {
            TypeSyntax variableTypeSyntax = fieldDeclaration.Declaration.Type;

            TypeInfo typeInfo = semanticModel.GetTypeInfo(variableTypeSyntax);
            ITypeSymbol? typeSymbol = typeInfo.Type;

            if (typeSymbol == null)
                continue; // Type couldn't be resolved

            string fullyQualifiedTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            foreach (VariableDeclaratorSyntax variableDeclarator in fieldDeclaration.Declaration.Variables)
            {
                string fieldName = variableDeclarator.Identifier.Text;
                fields.Add((fullyQualifiedTypeName, fieldName));
            }
        }

        return fields;
    }
}

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
