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
using System.Text.RegularExpressions;
using go2cs.Templates.InheritedType;
using go2cs.Templates.InterfaceType;
using go2cs.Templates.StructType;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static go2cs.Common;
using static go2cs.Symbols;

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

        HashSet<string> emittedHintNames = new(StringComparer.OrdinalIgnoreCase);

        foreach ((BaseTypeDeclarationSyntax targetSyntax, List<AttributeSyntax> attributes) in attributeFinder.TargetAttributes)
        {
            SyntaxTree syntaxTree = targetSyntax.SyntaxTree;
            SemanticModel semanticModel = context.Compilation.GetSemanticModel(syntaxTree);

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
            // "internal" scope is used so types can be referenced instead of "private".
            // An explicit modifier on the converter's partial declaration wins (e.g. an
            // unexported type publicized because it is an exported field's type — CS0051/CS0052).
            string scope = GetExplicitAccessModifier(targetSyntax) ?? GetScope(identifier);

            string[] usingStatements = GetFullyQualifiedUsingStatements(syntaxTree, semanticModel);

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
                    case StructDeclarationSyntax structDeclaration when string.IsNullOrWhiteSpace(typeDefinition) || typeDefinition.Equals("dyn"):
                        generatedSource = new StructTypeTemplate
                        {
                            PackageNamespace = packageNamespace,
                            PackageName = packageName,
                            Scope = scope,
                            Context = context,
                            StructName = identifier,
                            FullyQualifiedStructType = fullyQualifiedIdentifier,
                            StructMembers = structDeclaration.GetStructMembers(context.Compilation, true),
                            HasEqualityOperators = hasEqualityOperators,
                            UsingStatements = usingStatements
                        }
                        .Generate();

                        break;
                    
                    case StructDeclarationSyntax when typeDefinition.StartsWith("[]"): // slice
                        typeName = QualifySourceAliasReferences(typeDefinition[2..].Trim(), syntaxTree, semanticModel);

                        // m_value stays MUTABLE for a named-slice wrapper: a Go pointer-reinterpret to
                        // the underlying slice — `(*[][]byte)(buf)` with `buf *Buffers`, net
                        // fd_windows.go — projects a ж<slice<T>> VIEW over the wrapper's own field
                        // (`Ꮡbuf.of(Buffers.Ꮡm_value)`), so header writes through the view (poll
                        // FD.Writev's consume reslicing) land on the original (a readonly field would
                        // force a defensive copy and lose them).
                        generatedSource = new InheritedTypeTemplate
                        {
                            PackageNamespace = packageNamespace,
                            PackageName = packageName,
                            ObjectName = identifier,
                            Scope = scope,
                            TypeName = $"slice<{typeName}>",
                            TargetTypeName = typeName,
                            TypeClass = "Slice",
                            ReadOnlyValue = false
                        }
                        .Generate();

                        break;

                    case StructDeclarationSyntax when typeDefinition.StartsWith("map["):
                        (string keyTypeName, string valueTypeName) = SplitMapTypes(typeDefinition);
                        keyTypeName = QualifySourceAliasReferences(keyTypeName, syntaxTree, semanticModel);
                        valueTypeName = QualifySourceAliasReferences(valueTypeName, syntaxTree, semanticModel);

                        generatedSource = new InheritedTypeTemplate
                        {
                            PackageNamespace = packageNamespace,
                            PackageName = packageName,
                            Scope = scope,
                            ObjectName = identifier,
                            TypeName = $"map<{keyTypeName}, {valueTypeName}>",
                            TargetTypeName = keyTypeName,
                            TargetValueTypeName = valueTypeName,
                            TypeClass = "Map",
                            UsingStatements = usingStatements

                        }
                        .Generate();

                        break;

                    case StructDeclarationSyntax when typeDefinition.StartsWith("chan "):
                        typeName = QualifySourceAliasReferences(typeDefinition[5..].Trim(), syntaxTree, semanticModel);

                        generatedSource = new InheritedTypeTemplate
                        {
                            PackageNamespace = packageNamespace,
                            PackageName = packageName,
                            ObjectName = identifier,
                            Scope = scope,
                            TypeName = $"channel<{typeName}>",
                            TargetTypeName = typeName,
                            TypeClass = "Channel",
                            UsingStatements = usingStatements
                        }
                        .Generate();

                        break;
                    
                    case StructDeclarationSyntax when typeDefinition.StartsWith("["): // array
                        int sizeStart = typeDefinition.IndexOf('[') + 1;
                        int sizeEnd = typeDefinition.IndexOf(']');
                        string arraySize = typeDefinition[sizeStart..sizeEnd].Trim();
                        typeName = QualifySourceAliasReferences(typeDefinition[(sizeEnd + 1)..].Trim(), syntaxTree, semanticModel);

                        generatedSource = new InheritedTypeTemplate
                        {
                            PackageNamespace = packageNamespace,
                            PackageName = packageName,
                            ObjectName = identifier,
                            ReadOnlyValue = false,
                            Scope = scope,
                            TypeName = $"array<{typeName}>",
                            TargetTypeName = typeName,
                            TargetTypeSize = arraySize,
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
                            ObjectName = identifier,
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

                        // A defined type whose underlying is a STRUCT (`type winlibcall libcall`) exposes
                        // the underlying struct's fields in Go (`w.fn`). Resolve the underlying struct
                        // (same-package or a source-referenced package) and forward its members as get/set
                        // properties over a MUTABLE m_value, so `box.Value.fn = x` (a write through a
                        // ж<T>.Value ref) persists. Non-struct underlyings (a named type over an interface or
                        // another named type) resolve to null and keep the plain wrapper (no churn).
                        List<(string typeName, string memberName, bool isReferenceType, bool isProperty)>? forwardedMembers = null;
                        string? underlyingArrayElem = null;
                        bool mutableValue = false;

                        (StructDeclarationSyntax? underlyingStruct, Compilation? underlyingCompilation) = context.GetStructDeclaration(typeDefinition);

                        if (underlyingStruct is not null && underlyingCompilation is not null)
                        {
                            List<(string typeName, string memberName, bool isReferenceType, bool isProperty)> members = underlyingStruct.GetStructMembers(underlyingCompilation, false);

                            // Only forward + go mutable when the underlying actually contributes fields.
                            // An empty result (e.g. a named type over an array-typed named struct whose
                            // members are generated, not declared) keeps the plain readonly wrapper — no ripple.
                            if (members.Count > 0)
                            {
                                forwardedMembers = members;
                                mutableValue = true;
                            }
                            else
                            {
                                // A defined type over an ARRAY-backed [GoType] wrapper — `type pallocBits
                                // pageBits` where `type pageBits [8]uint64` — is len()'d / indexed directly
                                // in Go, which needs IArray on THIS wrapper (golib `len(IArray)`, CS1503
                                // otherwise; runtime mpallocbits.go). Detect it from the underlying's own
                                // [GoType] definition (`[N]elem`, not a `[]` slice) and implement
                                // IArray<elem> as a view over m_value (IArrayViewTypeTemplate).
                                string? underlyingDefinition = GetGoTypeDefinition(underlyingStruct);

                                if (underlyingDefinition is not null && underlyingDefinition.StartsWith("[") && !underlyingDefinition.StartsWith("[]"))
                                {
                                    int closeBracket = underlyingDefinition.IndexOf(']');

                                    if (closeBracket > 0 && closeBracket < underlyingDefinition.Length - 1)
                                    {
                                        underlyingArrayElem = underlyingDefinition[(closeBracket + 1)..].Trim();

                                        // The view's ref accessor must ensure the underlying's lazily-
                                        // allocated backing lands on THIS wrapper's own m_value (a
                                        // readonly field would force a defensive copy and lose writes).
                                        mutableValue = true;
                                    }
                                }
                            }
                        }

                        generatedSource = new InheritedTypeTemplate
                        {
                            PackageNamespace = packageNamespace,
                            PackageName = packageName,
                            ObjectName = identifier,
                            Scope = scope,
                            ReadOnlyValue = !mutableValue,
                            TypeName = typeName,
                            TargetTypeName = typeName,
                            TypeClass = typeDefinition,
                            ForwardedStructMembers = forwardedMembers,
                            UnderlyingArrayElementType = underlyingArrayElem,
                            UsingStatements = usingStatements
                        }
                        .Generate();

                        break;

                    case StructDeclarationSyntax:
                        throw new NotSupportedException($"Unsupported [{AttributeName}] definition \"{typeDefinition}\" on struct \"{identifier}\".");

                    case InterfaceDeclarationSyntax interfaceDeclaration:
                        string[]? operatorConstraints = null;
                        bool dynamic = false;

                        if (!string.IsNullOrWhiteSpace(typeDefinition))
                        {
                            string[] keys = typeDefinition.Split([';'], StringSplitOptions.RemoveEmptyEntries);

                            foreach (string key in keys)
                            {
                                string[] parts = key.Split(["="], StringSplitOptions.RemoveEmptyEntries);

                                if (parts.Length > 1)
                                {
                                    if (parts[0].Trim().Equals("operators", StringComparison.OrdinalIgnoreCase))
                                        operatorConstraints = parts[1].Split([','], StringSplitOptions.RemoveEmptyEntries).Select(part => part.Trim()).ToArray();
                                }
                                else if (key.Trim().Equals("dyn", StringComparison.OrdinalIgnoreCase))
                                {
                                    dynamic = true;
                                }
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
                            Methods = dynamic ? interfaceDeclaration.GetInterfaceMethods(context) : [],
                            Dynamic = dynamic,
                            UsingStatements = usingStatements
                        }
                        .Generate();

                        break;

                    case ClassDeclarationSyntax when typeDefinition.StartsWith($"{PointerPrefix}<"): // pointer
                        typeName = typeDefinition[2..^1];
                        
                        generatedSource = new InheritedTypeTemplate
                        {
                            PackageNamespace = packageNamespace,
                            PackageName = packageName,
                            ObjectName = identifier,
                            ObjectKind = "class",
                            Scope = scope,
                            TypeName = $"{PointerPrefix}<{typeName}>",
                            TargetTypeName = typeName,
                            TypeClass = "Pointer",
                            UsingStatements = usingStatements
                        }
                        .Generate();

                        break;

                    default:
                        throw new NotSupportedException($"Unsupported [{AttributeName}] on {targetSyntax.GetType().Name} type \"{identifier}\".");
                }

                // Add the source code to the compilation
                context.AddSource(GetUniqueHintName(emittedHintNames, GetValidFileName($"{packageNamespace}.{packageClassName}.{identifier}.g.cs")), generatedSource);
            }
        }
    }

    private static (string keyTypeName, string valueTypeName) SplitMapTypes(string typeDefinition)
    {
        string mapTypes = typeDefinition[4..^1];
        int depth = 0;

        for (int i = 0; i < mapTypes.Length; i++)
        {
            char ch = mapTypes[i];

            switch (ch)
            {
                case '<':
                case '[':
                case '(':
                    depth++;
                    break;
                case '>':
                case ']':
                case ')':
                    if (depth > 0)
                        depth--;
                    break;
                case ',' when depth == 0:
                    return (mapTypes[..i].Trim(), mapTypes[(i + 1)..].Trim());
            }
        }

        return (mapTypes.Trim(), string.Empty);
    }

    private static string QualifySourceAliasReferences(string typeName, SyntaxTree syntaxTree, SemanticModel semanticModel)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return typeName;

        string result = typeName;

        foreach (UsingDirectiveSyntax directive in syntaxTree.GetRoot().DescendantNodes().OfType<UsingDirectiveSyntax>())
        {
            if (directive is not { Alias: not null, Name: not null } || !directive.GlobalKeyword.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.None))
                continue;

            string alias = directive.Alias.Name.Identifier.Text;

            if (result.IndexOf($"{alias}.", StringComparison.Ordinal) < 0)
                continue;

            ISymbol? symbol = semanticModel.GetSymbolInfo(directive.Name).Symbol;
            string target = symbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? directive.Name.ToString();
            result = Regex.Replace(result, $@"(^|[<,\s\(\[]){Regex.Escape(alias)}\.", $"$1{target}.");
        }

        return GlobalQualify(result);
    }
    // Reads a struct declaration's own [GoType("…")] definition string (first constructor argument,
    // quotes stripped) — used to inspect the UNDERLYING type of a defined-over-defined chain
    // (`type pallocBits pageBits`: pageBits' definition is "[8]uint64"). Null when the struct has no
    // GoType attribute or no argument.
    private static string? GetGoTypeDefinition(StructDeclarationSyntax structDeclaration)
    {
        foreach (AttributeListSyntax attributeList in structDeclaration.AttributeLists)
        {
            foreach (AttributeSyntax attribute in attributeList.Attributes)
            {
                string name = attribute.Name.ToString();

                if (name != AttributeName && name != $"{AttributeName}Attribute")
                    continue;

                (string _, string value)[] arguments = attribute.GetArgumentValues();

                if (arguments.Length > 0)
                {
                    string value = arguments[0].value;

                    if (!string.IsNullOrWhiteSpace(value) && value.Length > 2)
                        return value[1..^1].Trim();
                }
            }
        }

        return null;
    }
}
