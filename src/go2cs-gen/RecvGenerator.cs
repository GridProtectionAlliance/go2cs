//******************************************************************************************************
//  GoRecvGenerator.cs - Gbtc
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

//#define DEBUG_GENERATOR

using System.Collections.Generic;
using go2cs.Templates.ReceiverMethod;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;


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

        foreach ((MethodDeclarationSyntax targetSyntax, _) in attributeFinder.TargetAttributes)
        {
            string packageNamespace = targetSyntax.GetNamespaceName();
            string packageClassName = targetSyntax.GetParentClassName();
            string packageName = packageClassName.EndsWith("_package") ? packageClassName[..^8] : packageClassName;
            string identifier = targetSyntax.Identifier.Text;
            string scope = char.IsUpper(identifier[0]) ? "public" : "private";

            string[] usingStatements = targetSyntax.SyntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<UsingDirectiveSyntax>()
                .Select(directive => directive.GetText().ToString().Trim())
                .ToArray();

            (List<(string typeName, string paramName)> methodParams, bool refRecv) = targetSyntax.GetMethodParameters(context);

            // Only process methods with a reference receiver to create
            // a generated overload the handles a ptr<T> receiver
            if (methodParams.Count == 0 || !refRecv)
                continue;

            string generatedSource = new ReceiverMethodTemplate
                {
                    PackageNamespace = packageNamespace,
                    PackageName = packageName,
                    Scope = scope,
                    MethodName = identifier,
                    ReturnType = targetSyntax.GetReturnType(context),
                    MethodParameters = methodParams,
                    UsingStatements = usingStatements
                }
                .Generate();


            // Add the source code to the compilation
            context.AddSource($"{packageNamespace}.{packageClassName}.{identifier}.g.cs", generatedSource);
        }
    }
}
