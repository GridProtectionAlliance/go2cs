// RecvGenerator.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

//#define DEBUG_GENERATOR

using System;
using System.Collections.Generic;
using go2cs.Templates.ReceiverMethod;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static go2cs.Common;
using static go2cs.Symbols;

#if DEBUG_GENERATOR
using System.Diagnostics;
#endif

namespace go2cs;

[Generator]
public class RecvGenerator : ISourceGenerator
{
    private const string Namespace = "go";
    private const string AttributeName = "GoRecv";
    private const string FullAttributeName = $"{Namespace}.{AttributeName}Attribute";

    public void Initialize(GeneratorInitializationContext context)
    {
    #if DEBUG_GENERATOR
        if (!Debugger.IsAttached)
            Debugger.Launch();
    #endif

        // Register to find "GoRecvAttribute" on method declarations
        context.RegisterForSyntaxNotifications(() => new AttributeFinder<MethodDeclarationSyntax>(FullAttributeName));
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not AttributeFinder<MethodDeclarationSyntax> { HasAttributes: true } attributeFinder)
            return;

        // Roslyn hintNames are compared case-INSENSITIVELY, and Go routinely pairs an exported
        // method with an unexported case-twin on the same receiver (math/rand's Int31n/int31n on
        // *Rand) — a raw name-based hintName then throws, suppressing ALL ж-overloads for the
        // package (every box.Method() call fails CS1929).
        HashSet<string> emittedHintNames = new(StringComparer.OrdinalIgnoreCase);

        foreach ((MethodDeclarationSyntax methodSyntax, List<AttributeSyntax> attributes) in attributeFinder.TargetAttributes)
        {
            SyntaxTree syntaxTree = methodSyntax.SyntaxTree;
            SemanticModel semanticModel = context.Compilation.GetSemanticModel(syntaxTree);

            string packageNamespace = methodSyntax.GetNamespaceName();
            string packageClassName = methodSyntax.GetParentClassName();
            string packageName = packageClassName.EndsWith(PackageSuffix) ? packageClassName[..^PackageSuffix.Length] : packageClassName;
            string identifier = methodSyntax.Identifier.Text;
            string scope = GetScope(identifier);

            string[] usingStatements = GetFullyQualifiedUsingStatements(syntaxTree, semanticModel);

            foreach (AttributeSyntax attribute in attributes)
            {
                MethodInfo method = methodSyntax.GetMethodInfo(context.Compilation);

                // Only process methods with a reference receiver to create
                // a generated overload the handles a ptr<T> receiver
                if (method.Parameters.Length == 0 || !method.IsRefRecv)
                    continue;

                string generatedSource = new ReceiverMethodTemplate
                {
                    PackageNamespace = packageNamespace,
                    PackageName = packageName,
                    Scope = scope,
                    Method = method,
                    UsingStatements = usingStatements
                }
                .Generate();

                // Add the source code to the compilation
                context.AddSource(GetUniqueHintName(emittedHintNames, GetValidFileName($"{packageNamespace}.{packageClassName}.{identifier}.{method.Parameters[0].type}.g.cs")), generatedSource);
            }
        }
    }
}
