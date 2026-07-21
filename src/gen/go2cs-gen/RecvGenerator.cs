//******************************************************************************************************
//  GoRecvGenerator.cs - Gbtc
//
//  Copyright © 2026, J. Ritchie Carroll.  All Rights Reserved.
//
//  Licensed under the MIT License (MIT), the "License"; you may not use this file except in compliance
//  with the License. You may obtain a copy of the License at:
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
