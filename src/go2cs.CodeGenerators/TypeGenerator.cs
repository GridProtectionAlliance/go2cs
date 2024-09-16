//******************************************************************************************************
//  TypeGenerator.cs - Gbtc
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
//  09/15/2024 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

//#define DEBUG_GENERATOR

using System.Collections.Generic;
using System.Linq;
using go2cs.Metadata;
using go2cs.Templates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeInfo = go2cs.Metadata.TypeInfo;

#if DEBUG_GENERATOR
using System.Diagnostics;
#endif

namespace go2cs.CodeGenerators;

[Generator]
public class TypeGenerator : ISourceGenerator
{
    private const string Namespace = "go";
    private const string AttributeName = $"{Namespace}.typeAttribute";

    public void Initialize(GeneratorInitializationContext context)
    {
    #if DEBUG_GENERATOR
        if (!Debugger.IsAttached)
            Debugger.Launch();
    #endif

        // Find specified attribute type on class declarations
        context.RegisterForSyntaxNotifications(() => new AttributeFinder<StructDeclarationSyntax>(AttributeName));
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not AttributeFinder<StructDeclarationSyntax> { HasAttributes: true } attributeFinder)
            return;

        foreach ((StructDeclarationSyntax targetSyntax, List<AttributeSyntax> attributes) in attributeFinder.TargetAttributes)
        {
            string packageNamespace = targetSyntax.GetNamespaceName();
            string namespaceHeader = $"namespace {packageNamespace}\r\n{{\r\n";
            string namespaceFooter = $"\r\n}}\r\n";
            string packageClassName = targetSyntax.GetParentClassName();
            string packageName = packageClassName.EndsWith("_package") ? packageClassName[..^8] : packageClassName;
            string identifier = targetSyntax.Identifier.Text;
            string scope = char.IsUpper(identifier[0]) ? "public" : "private";

            CompilationUnitSyntax root = (CompilationUnitSyntax)targetSyntax.SyntaxTree.GetRoot();

            IEnumerable<string> usingStatements = targetSyntax.SyntaxTree
                .GetRoot() // Get the root node of the syntax tree
                .DescendantNodesAndSelf() // Include the root node itself in the traversal
                .OfType<CompilationUnitSyntax>() // Get the CompilationUnitSyntax (root of the file)
                .SelectMany(cu => cu.Usings) // Select all using directives at the root level
                .Select(u => u.GetText().ToString().Trim()); // Get the text of each using directive

            foreach (AttributeSyntax attribute in attributes)
            {
                // Get the attribute's argument values
                string[] arguments = attribute.GetArgumentValues();

                if (arguments.Length < 1)
                    continue;

                // Get the attribute's first constructor argument value, type definition
                string typeDefinition = arguments[0][1..^1];

                string generatedSource;

                if (typeDefinition.Equals("struct"))
                {
                    generatedSource = new StructTypeTemplate
                    {
                        NamespacePrefix = packageNamespace,
                        NamespaceHeader = namespaceHeader,
                        NamespaceFooter = namespaceFooter,
                        PackageName = packageName,
                        StructName = identifier,
                        Scope = scope,
                        StructFields = GetStructFields(targetSyntax, context),
                        PromotedStructs = [], // TODO: Fix this
                        PromotedFunctions = [], // TODO: Fix this
                        PromotedFields = [], // TODO: Fix this
                        UsingStatements = usingStatements
                    }.TransformText();

                }
                else if (typeDefinition.StartsWith("[]"))
                {
                    generatedSource = string.Empty;
                }
                else
                {
                    generatedSource = string.Empty;
                }

                // Add the source code to the compilation
                context.AddSource($"{packageNamespace}.{packageClassName}.{identifier}.g.cs", generatedSource);
            }
        }
    }

    // TODO: Change the way template operates / fix field infos
    private FieldInfo[] GetStructFields(StructDeclarationSyntax structDeclaration, GeneratorExecutionContext context)
    {
        List<(string typeName, string fieldName)> fields = structDeclaration.GetStructFields(context);
        List<FieldInfo> fieldInfos = [];

        foreach ((string typeName, string fieldName) in fields)
        {
            fieldInfos.Add(new FieldInfo
            {
                Name = fieldName,
                Type = new TypeInfo
                {
                    Name = typeName,
                    TypeName = typeName,
                    FullTypeName = typeName,
                    TypeClass = TypeClass.Simple
                },
                Description = string.Empty,
                Comments = string.Empty,
                IsPromoted = false
            });
        }

        return fieldInfos.ToArray();
    }
}
