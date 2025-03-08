//******************************************************************************************************
//  StructDeclarationSyntaxExtensions.cs - Gbtc
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
//  09/16/2024 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace go2cs;

public static class StructDeclarationSyntaxExtensions
{
    public static List<(string typeName, string fieldName, bool isReferenceType)> GetStructFields(
        this StructDeclarationSyntax structDeclaration, 
        GeneratorExecutionContext context)
    {
        // Obtain the SemanticModel from the context
        SemanticModel semanticModel = context.Compilation.GetSemanticModel(structDeclaration.SyntaxTree);

        List<(string typeName, string fieldName, bool isReferenceType)> fields = [];

        foreach (FieldDeclarationSyntax? fieldDeclaration in structDeclaration.Members.OfType<FieldDeclarationSyntax>())
        {
            if (fieldDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
                continue;

            TypeInfo typeInfo = semanticModel.GetTypeInfo(fieldDeclaration.Declaration.Type);
            ITypeSymbol? typeSymbol = typeInfo.Type;
            string fullyQualifiedTypeName = typeSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "object";

            // Determine if the type is a reference type or an unconstrained generic type
            bool isReferenceType = IsReferenceTypeOrUnconstrainedGeneric(typeSymbol);

            foreach (VariableDeclaratorSyntax variable in fieldDeclaration.Declaration.Variables)
                fields.Add((fullyQualifiedTypeName, variable.Identifier.Text, isReferenceType));
        }

        return fields;
    }

    public static List<(string typeName, string propertyName, bool isReferenceType)> GetStructProperties(
        this StructDeclarationSyntax structDeclaration, 
        GeneratorExecutionContext context)
    {
        SemanticModel semanticModel = context.Compilation.GetSemanticModel(structDeclaration.SyntaxTree);
        List<(string typeName, string propertyName, bool isReferenceType)> properties = [];

        foreach (PropertyDeclarationSyntax? propertyDeclaration in structDeclaration.Members.OfType<PropertyDeclarationSyntax>())
        {
            if (propertyDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
                continue;

            TypeSyntax propertyType = propertyDeclaration.Type is RefTypeSyntax refType ? refType.Type : propertyDeclaration.Type;
            TypeInfo typeInfo = semanticModel.GetTypeInfo(propertyType);
            ITypeSymbol? typeSymbol = typeInfo.Type;
            string fullyQualifiedTypeName = typeSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "object";

            // Determine if the type is a reference type or an unconstrained generic type
            bool isReferenceType = IsReferenceTypeOrUnconstrainedGeneric(typeSymbol);

            properties.Add((fullyQualifiedTypeName, propertyDeclaration.Identifier.Text, isReferenceType));
        }

        return properties;
    }

    // Gets fields and properties of a struct, maintaining the order in which they are defined
    public static List<(string typeName, string memberName, bool isReferenceType, bool isProperty)> GetStructMembers(
        this StructDeclarationSyntax structDeclaration,
        GeneratorExecutionContext context,
        bool filterToRefProperties = false)
    {
        SemanticModel semanticModel = context.Compilation.GetSemanticModel(structDeclaration.SyntaxTree);
        List<(string typeName, string memberName, bool isReferenceType, bool isProperty)> members = [];

        foreach (MemberDeclarationSyntax? member in structDeclaration.Members)
        {
            if (member.Modifiers.Any(SyntaxKind.StaticKeyword))
                continue;

            switch (member)
            {
                case PropertyDeclarationSyntax propertyDeclaration:
                    {
                        if (filterToRefProperties && propertyDeclaration.Type.Kind() != SyntaxKind.RefType)
                            continue;

                        TypeSyntax propertyType = propertyDeclaration.Type is RefTypeSyntax refType ? refType.Type : propertyDeclaration.Type;
                        TypeInfo typeInfo = semanticModel.GetTypeInfo(propertyType);
                        ITypeSymbol? typeSymbol = typeInfo.Type;
                        string fullyQualifiedTypeName = typeSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "object";

                        // Determine if the type is a reference type or an unconstrained generic type
                        bool isReferenceType = IsReferenceTypeOrUnconstrainedGeneric(typeSymbol);

                        members.Add((fullyQualifiedTypeName, propertyDeclaration.Identifier.Text, isReferenceType, true));

                        break;
                    }
                case FieldDeclarationSyntax fieldDeclaration:
                    {
                        TypeInfo typeInfo = semanticModel.GetTypeInfo(fieldDeclaration.Declaration.Type);
                        ITypeSymbol? typeSymbol = typeInfo.Type;
                        string fullyQualifiedTypeName = typeSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "object";

                        // Determine if the type is a reference type or an unconstrained generic type
                        bool isReferenceType = IsReferenceTypeOrUnconstrainedGeneric(typeSymbol);

                        foreach (VariableDeclaratorSyntax variable in fieldDeclaration.Declaration.Variables)
                            members.Add((fullyQualifiedTypeName, variable.Identifier.Text, isReferenceType, false));

                        break;
                    }
            }
        }

        return members;
    }

    // Determine if type is a reference type or unconstrained generic type parameter
    private static bool IsReferenceTypeOrUnconstrainedGeneric(ITypeSymbol? typeSymbol)
    {
        if (typeSymbol is null)
            return true; // Default to true for safety if type is unknown

        // If it's already a reference type, return true
        if (typeSymbol.IsReferenceType)
            return true;

        // Check if it's a type parameter (generic) and has no constraints or only has reference type constraint
        return typeSymbol is ITypeParameterSymbol { HasValueTypeConstraint: false };
    }

    public static IEnumerable<MethodInfo> GetExtensionMethods(
        this StructDeclarationSyntax structDeclaration, 
        GeneratorExecutionContext context)
    {
        string structName = structDeclaration.Identifier.Text;
        Compilation compilation = context.Compilation;

        // Get all extension method declarations in the compilation
        IEnumerable<MethodDeclarationSyntax> extensions = compilation.SyntaxTrees
            .SelectMany(tree => tree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(method => 
                    method.Modifiers.Any(m =>  m.IsKind(SyntaxKind.StaticKeyword)) &&
                    method.ParameterList.Parameters.Count > 0))
            .Where(method => method.IsExtensionMethodForStruct(structName));

        return extensions.Select(method => method.GetMethodInfo(context));
    }

    private static bool IsExtensionMethodForStruct(this MethodDeclarationSyntax method, string structName)
    {
        ParameterSyntax? firstParam = method.ParameterList.Parameters.FirstOrDefault();
        
        if (firstParam is null || !firstParam.Modifiers.Any(m => m.IsKind(SyntaxKind.ThisKeyword)))
            return false;

        string paramType = firstParam.Type?.ToString() ?? "";
        
        return paramType == structName ||
               paramType == $"ref {structName}" ||
               paramType == $"in {structName}" ||
               paramType == $"ref readonly {structName}";
    }
}
