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

using go2cs.Templates.InheritedType;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using go2cs.Templates.StructType;

#if DEBUG_GENERATOR
using System.Diagnostics;
#endif

namespace go2cs;

[Generator]
public class TypeGenerator : ISourceGenerator
{
    private const string Namespace = "go";
    private const string AttributeName = $"{Namespace}.GoTypeAttribute";

    public void Initialize(GeneratorInitializationContext context)
    {
    #if DEBUG_GENERATOR
        if (!Debugger.IsAttached)
            Debugger.Launch();
    #endif

        // Register to find "GoTypeAttribute" on type declarations
        context.RegisterForSyntaxNotifications(() => new AttributeFinder<BaseTypeDeclarationSyntax>(AttributeName));
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not AttributeFinder<BaseTypeDeclarationSyntax> { HasAttributes: true } attributeFinder)
            return;
            
        foreach ((BaseTypeDeclarationSyntax targetSyntax, List<AttributeSyntax> attributes) in attributeFinder.TargetAttributes)
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

            foreach (AttributeSyntax attribute in attributes)
            {
                // Get the attribute's argument values
                string[] arguments = attribute.GetArgumentValues();

                if (arguments.Length < 1)
                    continue;

                // Get the attribute's first constructor argument value, type definition
                string typeDefinition = arguments[0][1..^1].Trim();

                string generatedSource = string.Empty;

                if (targetSyntax is StructDeclarationSyntax structDeclaration)
                {
                    if (typeDefinition.Equals("struct"))
                    {
                        generatedSource = new StructTypeTemplate
                        {
                            PackageNamespace = packageNamespace,
                            PackageName = packageName,
                            Scope = scope,
                            StructName = identifier,
                            StructDeclaration = structDeclaration,
                            StructFields = structDeclaration.GetStructFields(context),
                            //PromotedStructs = [], // TODO: Fix this
                            //PromotedFunctions = [], // TODO: Fix this
                            //PromotedFields = [], // TODO: Fix this
                            UsingStatements = usingStatements
                        }
                        .Generate();
                    }
                    else if (typeDefinition.StartsWith("map["))
                    {
                        //string[] mapTypes = typeDefinition[4..^1].Split(',');

                        //string keyTypeName = mapTypes[0].Trim();
                        //string valueTypeName = mapTypes[1].Trim();

                        //string fullKeyTypeName = ConvertToFullCSTypeName(keyTypeName);
                        //string fullValueTypeName = ConvertToFullCSTypeName(valueTypeName);

                        //generatedSource = new InheritedTypeTemplate
                        //{
                        //    NamespacePrefix = packageNamespace,
                        //    NamespaceHeader = namespaceHeader,
                        //    PackageName = packageName,
                        //    StructName = identifier,
                        //    Scope = scope,
                        //    //TypeInfo = new TypeInfo // TODO: Fix this
                        //    //{
                        //    //    Name = valueTypeName,
                        //    //    TypeName = $"map<{keyTypeName}, {valueTypeName}>",
                        //    //    FullTypeName = $"go.map<{fullKeyTypeName}, {fullValueTypeName}>",
                        //    //    TypeClass = TypeClass.Map
                        //    //}
                        //}
                        //.TransformText();
                    }
                    else if (typeDefinition.StartsWith("[]"))
                    {
                        string typeName = typeDefinition[2..];

                        generatedSource = new InheritedTypeTemplate
                        {
                            PackageNamespace = packageNamespace,
                            PackageName = packageName,
                            StructName = identifier,
                            Scope = scope,
                            TypeName = $"slice<{typeName}>",
                            TargetTypeName = typeName,
                            TypeClass = "Slice"
                        }
                        .Generate();
                    }
                }
                else if (targetSyntax is InterfaceDeclarationSyntax interfaceDeclaration)
                {
                    if (typeDefinition.Equals("interface"))
                    {
                        //generatedSource = new InterfaceTypeTemplate
                        //{
                        //    NamespacePrefix = packageNamespace,
                        //    NamespaceHeader = namespaceHeader,
                        //    PackageName = packageName,
                        //    InterfaceName = identifier,
                        //    Scope = scope,
                        //    Interface = default!, // TODO: Fix this
                        //    Functions = [], // TODO: Fix this
                        //    UsingStatements = usingStatements
                        //}
                        //.TransformText();
                    }
                }

                // Add the source code to the compilation
                context.AddSource($"{packageNamespace}.{packageClassName}.{identifier}.g.cs", generatedSource);
            }
        }
    }

}
