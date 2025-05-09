﻿//******************************************************************************************************
//  ImplicitConvGenerator.cs - Gbtc
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

//#define DEBUG_GENERATOR

using System;
using System.Collections.Generic;
using System.Linq;
using go2cs.Templates.ImplicitConv;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static go2cs.Common;

#if DEBUG_GENERATOR
using System.Diagnostics;
#endif

namespace go2cs;

[Generator]
public class ImplicitConvGenerator : ISourceGenerator
{
    private const string Namespace = "go";
    private const string AttributeName = "GoImplicitConv";
    private const string FullAttributeName = $"{Namespace}.{AttributeName}Attribute<TSource, TTarget>";

    public void Initialize(GeneratorInitializationContext context)
    {
#if DEBUG_GENERATOR
        if (!Debugger.IsAttached)
            Debugger.Launch();
#endif

        // Register to find "GoImplicitConv" on assembly attribute declarations
        context.RegisterForSyntaxNotifications(() => new AssemblyAttributeFinder(FullAttributeName));
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not AssemblyAttributeFinder { HasAttributes: true } attributeFinder)
            return;

        foreach ((AttributeSyntax attributeSyntax, GeneratorSyntaxContext syntaxContext, CompilationUnitSyntax compilationUnit, FileScopedNamespaceDeclarationSyntax? namespaceSyntax) in attributeFinder.TargetAttributes)
        {
            SyntaxTree syntaxTree = attributeSyntax.SyntaxTree;
            SemanticModel semanticModel = context.Compilation.GetSemanticModel(syntaxTree);

            string packageNamespace = GetNamespace(namespaceSyntax) ?? Namespace;
            string packageClassName = GetFirstClassName(compilationUnit) ?? throw new MissingMemberException($"No package class found in same file as [assembly: {AttributeName}]");
            string packageName = packageClassName.EndsWith("_package") ? packageClassName[..^8] : packageClassName;

            string[] usingStatements = GetFullyQualifiedUsingStatements(syntaxTree, semanticModel);

            // Extract generic type arguments from "GoImplicitConv"
            (ITypeSymbol? sourceType, ITypeSymbol? targetType) = attributeSyntax.Get2GenericTypeArguments(syntaxContext);
            
            if (sourceType is null || targetType is null)
                throw new InvalidOperationException($"Invalid usage of [assembly: {AttributeName}] attribute, must specify two generic type arguments.");

            if (sourceType.TypeKind != TypeKind.Struct)
                throw new InvalidOperationException($"Invalid usage of [assembly: {AttributeName}] attribute, first generic type argument must be a struct.");

            string sourceTypeName = sourceType.GetFullTypeName();
            string targetTypeName = targetType.GetFullTypeName();

            // Get the attribute's argument values, if defined
            (string name, string value)[] arguments = attributeSyntax.GetArgumentValues();
            bool inverted = bool.Parse(arguments.FirstOrDefault(arg => arg.name.Equals("Inverted")).value?.Trim() ?? "false");
            bool indirect = bool.Parse(arguments.FirstOrDefault(arg => arg.name.Equals("Indirect")).value?.Trim() ?? "false");
            string? valueType = arguments.FirstOrDefault(arg => arg.name.Equals("ValueType")).value?.Trim();

            List<(string typeName, string memberName)> structMembers;

            if (string.IsNullOrWhiteSpace(valueType))
            {
                StructDeclarationSyntax? structDeclaration = GetStructDeclaration(syntaxContext, targetTypeName);

                if (structDeclaration is null)
                    throw new InvalidOperationException($"Unable to find struct declaration named \"{targetTypeName}\"");

                structMembers = structDeclaration
                    .GetStructMembers(context.Compilation, true)
                    .Select(member => (member.typeName, member.memberName))
                    .ToList();
            }
            else
            {
                valueType = valueType![1..^1];
                structMembers = [];
                targetTypeName = targetType.GetFullTypeName(true);
            }

            string generatedSource = new ImplicitConvTemplate
            {
                PackageNamespace = packageNamespace,
                PackageName = packageName,
                SourceTypeName = sourceTypeName,
                TargetTypeName = targetTypeName,
                Inverted = inverted,
                Indirect = indirect,
                ValueType = valueType,
                StructMembers = structMembers,
                UsingStatements = usingStatements
            }
            .Generate();

            // Add the source code to the compilation
            context.AddSource(GetValidFileName($"{packageNamespace}.{packageClassName}.{sourceTypeName}-{targetTypeName}{(inverted ? "-inv" : "")}.g.cs"), generatedSource);
        }
    }

    private static string? GetNamespace(FileScopedNamespaceDeclarationSyntax? namespaceSyntax)
    {
        return namespaceSyntax?.Name.ToString();
    }

    private static string? GetFirstClassName(CompilationUnitSyntax compilationUnit)
    {
        return compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault()?.Identifier.Text;
    }

    private static StructDeclarationSyntax? GetStructDeclaration(GeneratorSyntaxContext context, string structName)
    {
        if (PointerExpr.IsMatch(structName))
            structName = structName[(structName.IndexOf('<') + 1)..^1];

        return context.SemanticModel.Compilation.SyntaxTrees
            .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<StructDeclarationSyntax>())
            .FirstOrDefault(structDeclaration => structDeclaration.Identifier.Text == structName);
    }
}
