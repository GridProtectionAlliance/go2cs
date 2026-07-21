//******************************************************************************************************
//  PartialStubGenerator.cs - Gbtc
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
//  06/26/2026 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static go2cs.Common;

namespace go2cs;

// Finds bodyless `partial` method declarations — go2cs emits these for Go functions with
// no body (assembly/cgo implemented). Their implementation is provided either by a
// hand-written companion (e.g. sync/atomic's doc_impl.cs) or, when none exists, by the
// PartialStubGenerator below.
public sealed class BodylessPartialMethodFinder : ISyntaxReceiver
{
    public List<MethodDeclarationSyntax> Candidates { get; } = [];

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is MethodDeclarationSyntax { Body: null, ExpressionBody: null } method &&
            method.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword)))
        {
            Candidates.Add(method);
        }
    }
}

// Emits a throwing implementation for every bodyless `partial` method that has no other
// implementing part in the compilation. This lets the converter emit asm/cgo functions as
// partial declarations: packages that ship a hand-written implementation companion use
// those bodies, while companion-less packages get a default stub so the code compiles
// (instead of CS8795 "partial method must have an implementation").
[Generator]
public class PartialStubGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new BodylessPartialMethodFinder());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not BodylessPartialMethodFinder finder)
            return;

        int index = 0;

        foreach (MethodDeclarationSyntax methodSyntax in finder.Candidates)
        {
            SemanticModel semanticModel = context.Compilation.GetSemanticModel(methodSyntax.SyntaxTree);

            if (semanticModel.GetDeclaredSymbol(methodSyntax) is not IMethodSymbol symbol)
                continue;

            // Only a partial DEFINITION that has no implementing part needs a stub. A
            // hand-written companion (e.g. sync/atomic doc_impl.cs) supplies the real body
            // for these asm functions and is detected here as PartialImplementationPart.
            if (!symbol.IsPartialDefinition || symbol.PartialImplementationPart is not null)
                continue;

            string packageNamespace = methodSyntax.GetNamespaceName();
            string packageClassName = methodSyntax.GetParentClassName();
            string identifier = methodSyntax.Identifier.Text;

            if (packageNamespace.Length == 0 || packageClassName.Length == 0)
                continue;

            // Reuse the declaration's exact signature (modifiers, return type, type params,
            // parameters, constraints) so the implementing part matches the definition, then
            // give it a throwing expression body.
            MethodDeclarationSyntax stub = methodSyntax
                .WithAttributeLists(default)
                .WithBody(null)
                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(
                    SyntaxFactory.ParseExpression(
                        $"throw new global::System.NotImplementedException(\"{identifier}: external (assembly or cgo) function is not implemented\")")))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                .WithLeadingTrivia()
                .WithTrailingTrivia()
                .NormalizeWhitespace();

            string[] usingStatements = GetFullyQualifiedUsingStatements(methodSyntax.SyntaxTree, semanticModel);

            // The package's using aliases go AFTER the file-scoped namespace declaration —
            // namespace-scoped, matching the converter's own file layout. A FILE-level alias
            // loses simple-name lookup to an enclosing namespace SEGMENT: in
            // `namespace go.@internal.syscall`, a file-level `using syscall = …` is shadowed by
            // the namespace's own `syscall` segment, so the stub's `ж<syscall.WSABuf>` resolved
            // as `go.@internal.syscall.WSABuf` (CS0234 + CS0759/CS8795 signature mismatch —
            // internal/syscall/windows WSASendtoInet4/6).
            string generatedSource =
                $$"""
                // <auto-generated/>
                #nullable enable
                using System.CodeDom.Compiler;

                namespace {{packageNamespace}};

                {{string.Join("\r\n", usingStatements)}}

                partial class {{packageClassName}}
                {
                    [{{GeneratedCodeAttribute}}]
                    {{stub}}
                }
                """;

            context.AddSource(GetValidFileName($"{packageNamespace}.{packageClassName}.{identifier}.{index++}.stub.g.cs"), generatedSource);
        }
    }
}
