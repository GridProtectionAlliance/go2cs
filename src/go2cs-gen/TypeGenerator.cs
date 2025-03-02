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

using System;
using System.Collections.Generic;
using System.Linq;
using go2cs.Templates.InheritedType;
using go2cs.Templates.InterfaceType;
using go2cs.Templates.StructType;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static go2cs.Common;

#if DEBUG_GENERATOR
using System.Diagnostics;
#endif

namespace go2cs;

[Generator]
public class TypeGenerator : ISourceGenerator
{
    private const string Namespace = "go";
    private const string AttributeName = "GoType";
    private const string FullAttributeName = $"{Namespace}.{AttributeName}Attribute";

    public void Initialize(GeneratorInitializationContext context)
    {
    #if DEBUG_GENERATOR
        if (!Debugger.IsAttached)
            Debugger.Launch();
    #endif

        // Register to find "GoTypeAttribute" on type declarations
        context.RegisterForSyntaxNotifications(() => new AttributeFinder<BaseTypeDeclarationSyntax>(FullAttributeName));
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not AttributeFinder<BaseTypeDeclarationSyntax> { HasAttributes: true } attributeFinder)
            return;
            
        foreach ((BaseTypeDeclarationSyntax targetSyntax, List<AttributeSyntax> attributes) in attributeFinder.TargetAttributes)
        {
            SemanticModel semanticModel = context.Compilation.GetSemanticModel(targetSyntax.SyntaxTree);

            string packageNamespace = targetSyntax.GetNamespaceName();
            string packageClassName = targetSyntax.GetParentClassName();
            string packageName = packageClassName.EndsWith("_package") ? packageClassName[..^8] : packageClassName;
            string identifier = targetSyntax.Identifier.Text;
            bool hasEqualityOperators = true;

            // Add generic type parameters to the identifier
            if (targetSyntax is TypeDeclarationSyntax { TypeParameterList.Parameters.Count: > 0 } typeDecl)
            {
                IEnumerable<string> typeParamNames = typeDecl.TypeParameterList.Parameters.Select(p => p.Identifier.Text);
                identifier += $"<{string.Join(", ", typeParamNames)}>";
                hasEqualityOperators = typeDecl.AllGenericTypesHaveConstraint(semanticModel, "System.Numerics.IEqualityOperators`3");
            }

            string fullyQualifiedIdentifier = semanticModel.GetDeclaredSymbol(targetSyntax)?.ToDisplayString() ?? $"{packageNamespace}.{packageClassName}.{identifier}";
            
            // Since many types are referenced by assembly attributes outside namespace,
            // "internal" scope is used so types can be referenced instead of "private"
            string scope = char.IsUpper(identifier[0]) ? "public" : "internal";

            string[] usingStatements = targetSyntax.SyntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<UsingDirectiveSyntax>()
                .Select(directive => directive.GetText().ToString().Trim())
                .ToArray();

            foreach (AttributeSyntax attribute in attributes)
            {
                // Get the attribute's argument values
                (string _, string value)[] arguments = attribute.GetArgumentValues();

                // Get the attribute's first constructor argument value, the type definition
                string typeDefinition = string.Empty;

                if (arguments.Length > 0)
                {
                    string value = arguments[0].value;
                    
                    if (!string.IsNullOrWhiteSpace(value) && value.Length > 2)
                        typeDefinition = value[1..^1].Trim();
                }

                string generatedSource, typeName;

                switch (targetSyntax)
                {
                    case StructDeclarationSyntax structDeclaration when string.IsNullOrWhiteSpace(typeDefinition):
                        generatedSource = new StructTypeTemplate
                        {
                            PackageNamespace = packageNamespace,
                            PackageName = packageName,
                            Scope = scope,
                            Context = context,
                            StructName = identifier,
                            FullyQualifiedStructType = fullyQualifiedIdentifier,
                            StructMembers = structDeclaration.GetStructMembers(context, true),
                            HasEqualityOperators = hasEqualityOperators,
                            UsingStatements = usingStatements
                        }
                        .Generate();

                        break;
                    
                    case StructDeclarationSyntax when typeDefinition.StartsWith("[]"): // slice
                        typeName = typeDefinition[2..].Trim();

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

                        break;

                    case StructDeclarationSyntax when typeDefinition.StartsWith("map["):
                        string[] mapTypes = typeDefinition[4..^1].Split(',');
                        string keyTypeName = mapTypes[0].Trim();
                        string valueTypeName = mapTypes[1].Trim();

                        generatedSource = new InheritedTypeTemplate
                        {
                            PackageNamespace = packageNamespace,
                            PackageName = packageName,
                            Scope = scope,
                            StructName = identifier,
                            TypeName = $"map<{keyTypeName}, {valueTypeName}>",
                            TargetTypeName = keyTypeName,
                            TargetValueTypeName = valueTypeName,
                            TypeClass = "Map",
                            UsingStatements = usingStatements

                        }
                        .Generate();

                        break;

                    case StructDeclarationSyntax when typeDefinition.StartsWith("chan "):
                        typeName = typeDefinition[5..].Trim();

                        generatedSource = new InheritedTypeTemplate
                        {
                            PackageNamespace = packageNamespace,
                            PackageName = packageName,
                            StructName = identifier,
                            Scope = scope,
                            TypeName = $"channel<{typeName}>",
                            TargetTypeName = typeName,
                            TypeClass = "Channel",
                            UsingStatements = usingStatements
                        }
                        .Generate();

                        break;
                    
                    case StructDeclarationSyntax when typeDefinition.StartsWith("["): // array
                        int bracketIndex = typeDefinition.IndexOf(']') + 1;
                        typeName = typeDefinition[bracketIndex..].Trim();

                        generatedSource = new InheritedTypeTemplate
                        {
                            PackageNamespace = packageNamespace,
                            PackageName = packageName,
                            StructName = identifier,
                            Scope = scope,
                            TypeName = $"array<{typeName}>",
                            TargetTypeName = typeName,
                            TypeClass = "Array",
                            UsingStatements = usingStatements
                        }
                        .Generate();

                        break;

                    case StructDeclarationSyntax when typeDefinition.StartsWith("num:"): // numeric
                        typeName = typeDefinition[4..].Trim();

                        generatedSource = new InheritedTypeTemplate
                        {
                            PackageNamespace = packageNamespace,
                            PackageName = packageName,
                            StructName = identifier,
                            Scope = $"{scope} readonly",
                            TypeName = typeName,
                            TargetTypeName = identifier,
                            TypeClass = "Numeric",
                            UsingStatements = usingStatements
                        }
                        .Generate();

                        break;

                    case StructDeclarationSyntax when !string.IsNullOrWhiteSpace(typeDefinition):
                        typeName = typeDefinition;

                        generatedSource = new InheritedTypeTemplate
                        {
                            PackageNamespace = packageNamespace,
                            PackageName = packageName,
                            StructName = identifier,
                            Scope = scope,
                            TypeName = typeName,
                            TargetTypeName = typeName,
                            TypeClass = typeDefinition,
                            UsingStatements = usingStatements
                        }
                        .Generate();

                        break;

                    case StructDeclarationSyntax:
                        throw new NotSupportedException($"Unsupported [{AttributeName}] definition \"{typeDefinition}\" on struct \"{identifier}\".");

                    case InterfaceDeclarationSyntax:
                        string[]? operatorConstraints = null;

                        if (!string.IsNullOrWhiteSpace(typeDefinition))
                        {
                            string[] parts = typeDefinition.Split(["="], StringSplitOptions.RemoveEmptyEntries);

                            if (parts.Length > 1)
                            {
                                if (parts[0].Trim().Equals("Operators", StringComparison.OrdinalIgnoreCase))
                                    operatorConstraints = parts[1].Split([','], StringSplitOptions.RemoveEmptyEntries).Select(part => part.Trim()).ToArray();
                            }
                        }

                        usingStatements = usingStatements.Append("using System.Numerics;").ToArray();

                        generatedSource = new InterfaceTypeTemplate
                        {
                            PackageNamespace = packageNamespace,
                            PackageName = packageName,
                            Scope = scope,
                            InterfaceName = identifier,
                            OperatorConstraints = operatorConstraints ?? [],
                            UsingStatements = usingStatements
                        }
                        .Generate();

                        break;

                    default:
                        throw new NotSupportedException($"Unsupported [{AttributeName}] on {targetSyntax.GetType().Name} type \"{identifier}\".");
                }

                // Add the source code to the compilation
                context.AddSource(GetValidFileName($"{packageNamespace}.{packageClassName}.{identifier}.g.cs"), generatedSource);
            }
        }
    }
}
