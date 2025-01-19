//******************************************************************************************************
//  ImplementGenerator.cs - Gbtc
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
using go2cs.Templates.InterfaceImpl;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#if DEBUG_GENERATOR
using System.Diagnostics;
#endif

namespace go2cs;

[Generator]
public class ImplGenerator : ISourceGenerator
{
    private const string Namespace = "go";
    private const string AttributeName = "GoImpl";
    private const string FullAttributeName = $"{Namespace}.{AttributeName}Attribute<TStruct, TInterface>";

    public void Initialize(GeneratorInitializationContext context)
    {
    #if DEBUG_GENERATOR
        if (!Debugger.IsAttached)
            Debugger.Launch();
    #endif

        // Register to find "GoImplementAttribute" on type declarations
        context.RegisterForSyntaxNotifications(() => new AssemblyAttributeFinder(FullAttributeName));
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not AssemblyAttributeFinder { HasAttributes: true } attributeFinder)
            return;

        foreach ((AttributeSyntax attribute, GeneratorSyntaxContext syntaxContext, CompilationUnitSyntax compilationUnit, NamespaceDeclarationSyntax? namespaceSyntax) in attributeFinder.TargetAttributes)
        {
            string packageNamespace = GetNamespace(namespaceSyntax) ?? Namespace;
            string packageClassName = GetFirstClassName(compilationUnit) ?? throw new MissingMemberException($"No package class found in same file as [assembly: {AttributeName}]");
            string packageName = packageClassName.EndsWith("_package") ? packageClassName[..^8] : packageClassName;

            string[] usingStatements = compilationUnit.SyntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<UsingDirectiveSyntax>()
                .Select(directive => directive.GetText().ToString().Trim())
                .ToArray();

            // Extract generic type arguments from "GoImplementAttribute"
            (ITypeSymbol? structType, ITypeSymbol? interfaceType) = GetGenericTypeArguments(attribute, syntaxContext);
            
            if (structType is null || interfaceType is null)
                throw new InvalidOperationException($"Invalid usage of [assembly: {AttributeName}] attribute, must specify two generic type arguments.");

            if (structType.TypeKind != TypeKind.Struct)
                throw new InvalidOperationException($"Invalid usage of [assembly: {AttributeName}] attribute, first generic type argument must be a struct.");

            if (interfaceType.TypeKind != TypeKind.Interface)
                throw new InvalidOperationException($"Invalid usage of [assembly: {AttributeName}] attribute, second generic type argument must be an interface.");

            string structName = structType.Name;
            string interfaceName = interfaceType.Name;

            List<MethodInfo> methods = interfaceType.AllInterfaces
                .Concat([interfaceType]) // Include the original interface
                .SelectMany(iface => iface.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(method => method.MethodKind == MethodKind.Ordinary)
                    .Where(method => !method.IsStatic)
                    .Select(method => (name: iface.ToDisplayString(), method)))
                .Select(t => new MethodInfo
                {
                    Name = $"{t.name}.{t.method.Name}",
                    ReturnType = t.method.ReturnType.ToDisplayString(),
                    Parameters = t.method.Parameters.Select(param => (name: param.Name, type: param.ToDisplayString())).ToArray(),
                    GenericTypes = string.Join(", ", t.method.TypeParameters.Select(type => type.ToDisplayString()))
                })
                .Distinct()
                .ToList();

            string generatedSource = new InterfaceImplTemplate
            {
                PackageNamespace = packageNamespace,
                PackageName = packageName,
                StructName = structName,
                InterfaceName = interfaceName,
                Methods = methods,
                UsingStatements = usingStatements
            }
            .Generate();

            // Add the source code to the compilation
            context.AddSource($"{packageNamespace}.{packageClassName}.{structName}-{interfaceName}.g.cs", generatedSource);
        }
    }

    private static string? GetNamespace(NamespaceDeclarationSyntax? namespaceSyntax)
    {
        return namespaceSyntax?.Name.ToString();
    }

    private static string? GetFirstClassName(CompilationUnitSyntax compilationUnit)
    {
        return compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault()?.Identifier.Text;
    }

    private static (ITypeSymbol? structType, ITypeSymbol? interfaceType) GetGenericTypeArguments(AttributeSyntax attributeSyntax, GeneratorSyntaxContext context)
    {
        // Check if the attribute type is generic
        if (attributeSyntax.Name is not GenericNameSyntax genericName)
            return (null, null);

        // Get the type arguments
        SeparatedSyntaxList<TypeSyntax> typeArguments = genericName.TypeArgumentList.Arguments;

        if (typeArguments.Count != 2)
            return (null, null);

        // Get semantic information for each type argument
        ITypeSymbol? structType = context.SemanticModel.GetTypeInfo(typeArguments[0]).Type;
        ITypeSymbol? interfaceType = context.SemanticModel.GetTypeInfo(typeArguments[1]).Type;

        return (structType, interfaceType);
    }
}
