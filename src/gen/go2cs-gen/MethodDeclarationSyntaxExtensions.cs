//******************************************************************************************************
//  MethodDeclarationSyntaxExtensions.cs - Gbtc
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
//  01/15/2025 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static go2cs.Templates.TemplateBase;
using static go2cs.Symbols;

namespace go2cs;

public record MethodInfo
{
    public required string Name { get; init; }

    public required string ReturnType { get; init; }

    public required (string type, string name)[] Parameters { get; init; }

    public required string GenericTypes { get; init; }

    public required Dictionary<string, string[]> TypeConstraints { get; init; }

    public bool IsRefRecv { get; init; }

    public bool IsGeneric => GenericTypes.Length > 0;

    public string CallParameters => GetCallParameters(true);
    
    public string GetCallParameters(bool allowDiscarded)
    {
        return string.Join(", ", Parameters.Select((param, index) =>
        {
            if (param.name == "_")
                return allowDiscarded ? "_" : $"p{TempVarMarker}{index}";

            return param.name;
        }));
    }

    public string TypedParameters => GetTypedParameters(true);

    public string GetTypedParameters(bool allowDiscarded)
    {
        return string.Join(", ", Parameters.Select((param, index) =>
        {
            if (param.name == "_")
                return allowDiscarded ? $"{param.type} _" : $"{param.type} p{TempVarMarker}{index}";
            
            return $"{param.type} {param.name}";
        }));
    }

    public string GetSignature(bool allowDiscarded = true)
    {
        return $"{Name}{GetGenericSignature()}({GetTypedParameters(allowDiscarded)}){GetWhereConstraints()}";
    }

    public string GetGenericSignature()
    {
        return IsGeneric ? $"<{GenericTypes}>" : "";
    }

    public string GetWhereConstraints()
    {
        if (!IsGeneric || TypeConstraints.Count == 0)
            return string.Empty;

        List<string> constraints = [];

        foreach (KeyValuePair<string, string[]> kvp in TypeConstraints)
        {
            string typeParam = kvp.Key;
            string[] typeConstraints = kvp.Value;

            if (typeConstraints.Length > 0)
                constraints.Add($"where {typeParam} : {string.Join(", ", typeConstraints)}");
        }

        return $"\r\n{TypeElemIndent}{string.Join("\r\n        ", constraints)}";
    }

    public bool IsSameSignature(IMethodSymbol methodSymbol)
    {
        // Compare method names
        if (Name != methodSymbol.Name)
            return false;

        // Compare return types - convert ITypeSymbol to string representation
        string returnTypeString = methodSymbol.ReturnType.ToDisplayString();

        if (ReturnType != returnTypeString)
            return false;

        // Compare parameter counts
        if (Parameters.Length != methodSymbol.Parameters.Length)
            return false;

        // Compare parameter types
        for (int i = 0; i < Parameters.Length; i++)
        {
            string paramType = methodSymbol.Parameters[i].Type.ToDisplayString();

            if (Parameters[i].type != paramType)
                return false;
        }

        // Compare generic type parameters count
        int genericTypesCount = methodSymbol.TypeParameters.Length;

        string[] genericTypes = string.IsNullOrEmpty(GenericTypes) ?
            [] : GenericTypes.Split(',').Select(type => type.Trim()).ToArray();

        return genericTypes.Length == genericTypesCount;
    }
}

public static class MethodSyntaxExtensions
{
    public static MethodInfo GetMethodInfo(this MethodDeclarationSyntax methodDeclaration, GeneratorExecutionContext context)
    {
        SemanticModel semanticModel = context.Compilation.GetSemanticModel(methodDeclaration.SyntaxTree);

        string[] typeParameters = methodDeclaration.TypeParameterList?.Parameters
            .Select(param => param.Identifier.Text)
            .ToArray() ?? [];

        Dictionary<string, string[]> typeConstraints = [];

        // Initialize dictionary with empty constraint arrays for each type parameter
        foreach (string typeParam in typeParameters)
            typeConstraints[typeParam] = [];

        // Process constraints if they exist
        if (methodDeclaration.ConstraintClauses.Any())
        {
            foreach (TypeParameterConstraintClauseSyntax constraintClause in methodDeclaration.ConstraintClauses)
            {
                string typeParamName = constraintClause.Name.Identifier.Text;

                if (!typeConstraints.ContainsKey(typeParamName))
                    continue;

                string[] constraints = constraintClause.Constraints
                    .Select(constraint => GetConstraintText(constraint, semanticModel))
                    .Where(text => !string.IsNullOrEmpty(text))
                    .ToArray();

                typeConstraints[typeParamName] = constraints;
            }
        }

        return new MethodInfo()
        {
            Name = methodDeclaration.Identifier.Text,
            ReturnType = methodDeclaration.GetReturnType(semanticModel),
            GenericTypes = string.Join(", ", typeParameters),
            TypeConstraints = typeConstraints,
            
            Parameters = methodDeclaration.ParameterList.Parameters.Select(param =>
            {
                if (param.Type is null)
                    return (type: "object", name: param.Identifier.Text);

                TypeInfo typeInfo = semanticModel.GetTypeInfo(param.Type);
                ITypeSymbol? typeSymbol = typeInfo.Type;
                string fullyQualifiedTypeName = typeSymbol?.ToDisplayString() ?? "object";

                return (type: fullyQualifiedTypeName, name: param.Identifier.Text);
            }).ToArray(),

            IsRefRecv = methodDeclaration.ParameterList.Parameters.Any(param =>
                param.Modifiers.Any(SyntaxKind.ThisKeyword) &&
                param.Modifiers.Any(SyntaxKind.RefKeyword))
        };
    }

    private static string GetConstraintText(TypeParameterConstraintSyntax constraint, SemanticModel semanticModel)
    {
        return constraint switch
        {
            ClassOrStructConstraintSyntax classOrStruct => classOrStruct.ClassOrStructKeyword.IsKind(SyntaxKind.ClassKeyword) ? "class" : "struct",
            ConstructorConstraintSyntax => "new()",
            DefaultConstraintSyntax => "default",
            TypeConstraintSyntax typeConstraint =>
                semanticModel.GetTypeInfo(typeConstraint.Type).Type?.ToDisplayString() ?? typeConstraint.Type.ToString(),
            _ => string.Empty
        };
    }

    private static string GetReturnType(this MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel)
    {
        TypeInfo typeInfo = semanticModel.GetTypeInfo(methodDeclaration.ReturnType);
        ITypeSymbol? typeSymbol = typeInfo.Type;

        return typeSymbol?.ToDisplayString() ?? "object";
    }

    public static MethodInfo GetMethodInfo(this IMethodSymbol methodSymbol)
    {
        // Convert parameters to the required tuple format
        (string type, string name)[] parameters = methodSymbol.Parameters
            .Select(parameter => (type: parameter.Type.ToDisplayString(), name: parameter.Name))
            .ToArray();

        // Extract generic type parameters
        string genericTypes = string.Join(", ", methodSymbol.TypeParameters.Select(typeParameter => typeParameter.Name));

        // Extract type constraints for generic parameters
        Dictionary<string, string[]> typeConstraints = new();

        foreach (ITypeParameterSymbol? typeParam in methodSymbol.TypeParameters)
        {
            List<string> constraints = [];

            // Add class/struct constraint
            if (typeParam.HasReferenceTypeConstraint)
                constraints.Add("class");
            else if (typeParam.HasValueTypeConstraint)
                constraints.Add("struct");

            // Add notnull constraint
            if (typeParam.HasNotNullConstraint)
                constraints.Add("notnull");

            // Add interface and type constraints
            constraints.AddRange(typeParam.ConstraintTypes.Select(constraintType => constraintType.ToDisplayString()));

            // Add unmanaged constraint
            if (typeParam.HasUnmanagedTypeConstraint)
                constraints.Add("unmanaged");

            // Add constructor constraint
            if (typeParam.HasConstructorConstraint)
                constraints.Add("new()");

            typeConstraints[typeParam.Name] = constraints.ToArray();
        }

        return new MethodInfo
        {
            Name = methodSymbol.Name,
            ReturnType = methodSymbol.ReturnType.ToDisplayString(),
            Parameters = parameters,
            GenericTypes = genericTypes,
            TypeConstraints = typeConstraints,
            IsRefRecv = methodSymbol.ReturnsByRef
        };
    }
}
