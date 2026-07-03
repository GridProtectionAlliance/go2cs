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
using static go2cs.Common;

#if DEBUG_GENERATOR
using System.Diagnostics;
#endif

namespace go2cs;

[Generator]
public class ImplementGenerator : ISourceGenerator
{
    private const string Namespace = "go";
    private const string AttributeName = "GoImplement";
    private const string FullAttributeName = $"{Namespace}.{AttributeName}Attribute<TStruct, TInterface>";

    public void Initialize(GeneratorInitializationContext context)
    {
    #if DEBUG_GENERATOR
        if (!Debugger.IsAttached)
            Debugger.Launch();
    #endif

        // Register to find "GoImplementAttribute" on assembly attribute declarations
        context.RegisterForSyntaxNotifications(() => new AssemblyAttributeFinder(FullAttributeName));
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not AssemblyAttributeFinder { HasAttributes: true } attributeFinder)
            return;

        // Roslyn hintNames are case-insensitive; Go type names differing only by case are legal
        // and common, so disambiguate like the other generators (a collision throws and suppresses
        // ALL interface implementations for the package).
        HashSet<string> emittedHintNames = new(StringComparer.OrdinalIgnoreCase);

        // A (struct, interface) pair can carry Pointer and Promoted on SEPARATE attribute
        // instances (the converter records the ж-form and the embed-promotion independently);
        // the pointer-adapter emission needs to know the pair is promoted, so pre-index.
        HashSet<string> promotedPairs = new(StringComparer.Ordinal);

        foreach ((AttributeSyntax attributeSyntax, GeneratorSyntaxContext syntaxContext, _, _) in attributeFinder.TargetAttributes)
        {
            (string name, string value)[] arguments = attributeSyntax.GetArgumentValues();

            if (!bool.Parse(arguments.FirstOrDefault(arg => arg.name.Equals("Promoted")).value?.Trim() ?? "false"))
                continue;

            (ITypeSymbol? structType, ITypeSymbol? interfaceType) = attributeSyntax.Get2GenericTypeArguments(syntaxContext);

            if (structType is not null && interfaceType is not null)
                promotedPairs.Add($"{structType.ToDisplayString()}|{interfaceType.ToDisplayString()}");
        }

        foreach ((AttributeSyntax attributeSyntax, GeneratorSyntaxContext syntaxContext, CompilationUnitSyntax compilationUnit, FileScopedNamespaceDeclarationSyntax? namespaceSyntax) in attributeFinder.TargetAttributes)
        {
            SyntaxTree syntaxTree = attributeSyntax.SyntaxTree;
            SemanticModel semanticModel = context.Compilation.GetSemanticModel(syntaxTree);

            string packageNamespace = GetNamespace(namespaceSyntax) ?? Namespace;
            string packageClassName = GetFirstClassName(compilationUnit) ?? throw new MissingMemberException($"No package class found in same file as [assembly: {AttributeName}]");
            string packageName = packageClassName.EndsWith("_package") ? packageClassName[..^8] : packageClassName;
            
            string[] usingStatements = GetFullyQualifiedUsingStatements(syntaxTree, semanticModel);

            // Extract generic type arguments from "GoImplementAttribute"
            (ITypeSymbol? structType, ITypeSymbol? interfaceType) = attributeSyntax.Get2GenericTypeArguments(syntaxContext);
            
            if (structType is null || interfaceType is null)
                throw new InvalidOperationException($"Invalid usage of [assembly: {AttributeName}] attribute, must specify two generic type arguments.");

            if (structType.TypeKind != TypeKind.Struct)
                throw new InvalidOperationException($"Invalid usage of [assembly: {AttributeName}] attribute, first generic type argument must be a struct.");

            if (interfaceType.TypeKind != TypeKind.Interface)
                throw new InvalidOperationException($"Invalid usage of [assembly: {AttributeName}] attribute, second generic type argument must be an interface.");

            string structName = structType.GetFullTypeName();
            string interfaceName = interfaceType.GetFullTypeName(true);

            // Get the attribute's Promoted / Pointer argument values, if defined
            (string name, string value)[] arguments = attributeSyntax.GetArgumentValues();
            bool promoted = bool.Parse(arguments.FirstOrDefault(arg => arg.name.Equals("Promoted")).value?.Trim() ?? "false");
            bool pointer = bool.Parse(arguments.FirstOrDefault(arg => arg.name.Equals("Pointer")).value?.Trim() ?? "false");

            // Get all extension methods for the struct, any directly defined receivers
            // take precedence over promoted interface methods that have the same name
            (StructDeclarationSyntax? structDecl, Compilation? compilation) = context.GetStructDeclaration(structType.ToDisplayString());
            IEnumerable<MethodInfo>? structMethods = structDecl is null ? [] : structDecl.GetExtensionMethods(compilation!);
            HashSet<string> overrides = new(structMethods?.Select(method => method.Name) ?? [], StringComparer.Ordinal);

            List<MethodInfo> methods = interfaceType.AllInterfaces
                .Concat([interfaceType]) // Include the original interface
                .SelectMany(iface => iface.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(method => method.MethodKind == MethodKind.Ordinary)
                    .Where(method => !method.IsStatic)
                    .Select(method => (name: iface.ToDisplayString(), method)))
                .Select(info => new MethodInfo
                {
                    Name = promoted && !pointer && !overrides.Contains(GetSimpleName(info.method.Name)) ? info.method.Name : $"{info.name}.{info.method.Name}",
                    ReturnType = info.method.ReturnType.ToDisplayString(),
                    Parameters = info.method.Parameters.Select(param => (type: param.Type.ToDisplayString(), name: param.Name)).ToArray(),
                    GenericTypes = string.Join(", ", info.method.TypeParameters.Select(type => type.ToDisplayString())),
                    TypeConstraints = info.method.TypeParameters.ToDictionary(type => type.Name, type => type.ConstraintTypes.Select(constraint => constraint.ToDisplayString()).ToArray())
                })
                .Distinct()
                .ToList();

            // An interface member with NO direct struct method may be satisfied by Go method
            // promotion through an embedded POINTER field (`type rtype struct { *abi.Type }`).
            // That promotion is syntax-resolved at Go call sites (the converter emits the hop
            // `t.Type.Value.M()`), so the explicit interface implementation must forward through
            // the same hop — `this.M()` has nothing to bind (CS1929). Gated to a SINGLE hop
            // (Go's promotion ambiguity rules make multi-embed satisfaction rare; extend when
            // the corpus surfaces one).
            List<string> embedHops = structDecl?.GetEmbeddedPointerHopNames() ?? [];
            string? embedHop = embedHops.Count == 1 ? embedHops[0] : null;

            if (pointer)
            {
                // A POINTER-sourced interface cast (`var s Iface = &t`): the interface value must
                // alias the receiver box (Go's interface holds the *T), so emit the IжAdapter
                // wrapper instead of the value-boxing partial struct — which copies, and cannot
                // bind direct-ж receiver methods from a struct's `this` at all (CS1929).
                Dictionary<string, string> forwardReceivers = new(StringComparer.Ordinal);

                foreach (MethodInfo structMethod in structMethods ?? [])
                {
                    // A [GoRecv] ref extension has a RecvGenerator ж-twin that binds on the box;
                    // a plain value-receiver method needs the deref'd value (Go copies at the call).
                    forwardReceivers[structMethod.Name] = structMethod.IsRefRecv ? "m_box" : "m_box.Value";
                }

                foreach (string boxReceiverName in structDecl?.GetBoxReceiverMethodNames(compilation!) ?? [])
                    forwardReceivers[boxReceiverName] = "m_box"; // direct-ж primary form binds the box itself

                // Interface members with NO struct method forward through a single embedded-pointer
                // hop, mirroring the value-form template (Go method promotion through `*abi.Type`).
                if (embedHops.Count == 1)
                {
                    foreach (MethodInfo method in methods)
                    {
                        string simpleName = GetSimpleName(method.Name);

                        if (!forwardReceivers.ContainsKey(simpleName))
                            forwardReceivers[simpleName] = $"m_box.Value.{embedHops[0]}.Value";
                    }
                }

                // PROMOTED members (an embedded INTERFACE field — sort's `type reverse struct
                // { Interface }`) forward through the interface field itself, mirroring the
                // value-form template's promoted arm (`m_box.Len()` has nothing to bind — CS1929).
                // The Promoted flag may live on the pair's SIBLING attribute instance.
                if (promoted || promotedPairs.Contains($"{structType.ToDisplayString()}|{interfaceType.ToDisplayString()}"))
                {
                    string interfaceFieldName = GetSimpleName(interfaceName);

                    foreach (MethodInfo method in methods)
                    {
                        string simpleName = GetSimpleName(method.Name);

                        if (!forwardReceivers.ContainsKey(simpleName))
                            forwardReceivers[simpleName] = $"m_box.Value.{interfaceFieldName}";
                    }
                }

                string adapterScope = GetScope(structName) == "public" && GetScope(GetSimpleName(interfaceName)) == "public" ? "public" : "internal";

                string adapterSource = new AdapterImplTemplate
                {
                    PackageNamespace = packageNamespace,
                    PackageName = packageName,
                    StructName = structName,
                    InterfaceName = interfaceName,
                    AdapterName = $"{structName}ᴵ{GetSimpleName(interfaceName)}",
                    AdapterScope = adapterScope,
                    Methods = methods,
                    ForwardReceivers = forwardReceivers,
                    UsingStatements = usingStatements
                }
                .Generate();

                context.AddSource(GetUniqueHintName(emittedHintNames, GetValidFileName($"{packageNamespace}.{packageClassName}.{structName}-{interfaceName}-ptr.g.cs")), adapterSource);
                continue;
            }

            string generatedSource = new InterfaceImplTemplate
            {
                PackageNamespace = packageNamespace,
                PackageName = packageName,
                StructName = structName,
                InterfaceName = interfaceName,
                Promoted = promoted,
                Overrides = overrides,
                Methods = methods,
                EmbedHop = embedHop,
                UsingStatements = usingStatements
            }
            .Generate();

            // Add the source code to the compilation
            context.AddSource(GetUniqueHintName(emittedHintNames, GetValidFileName($"{packageNamespace}.{packageClassName}.{structName}-{interfaceName}.g.cs")), generatedSource);
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
}
