//******************************************************************************************************
//  StructDeclarationSyntaxExtensions.cs - Gbtc
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
//  09/16/2024 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static go2cs.Symbols;

namespace go2cs;

public static class StructDeclarationSyntaxExtensions
{
    public static List<(string typeName, string fieldName, bool isReferenceType)> GetStructFields(
        this StructDeclarationSyntax structDeclaration, 
        GeneratorExecutionContext context)
    {
        // Obtain the SemanticModel from the context
        SemanticModel semanticModel = context.Compilation.GetSemanticModel(structDeclaration.SyntaxTree);

        List<(string typeName, string fieldName, bool isReferenceType)> fields = [];

        foreach (FieldDeclarationSyntax? fieldDeclaration in structDeclaration.Members.OfType<FieldDeclarationSyntax>())
        {
            if (fieldDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
                continue;

            TypeInfo typeInfo = semanticModel.GetTypeInfo(fieldDeclaration.Declaration.Type);
            ITypeSymbol? typeSymbol = typeInfo.Type;
            string fullyQualifiedTypeName = typeSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "object";

            // Determine if the type is a reference type or an unconstrained generic type
            bool isReferenceType = IsReferenceTypeOrUnconstrainedGeneric(typeSymbol);

            foreach (VariableDeclaratorSyntax variable in fieldDeclaration.Declaration.Variables)
                fields.Add((fullyQualifiedTypeName, variable.Identifier.Text, isReferenceType));
        }

        return fields;
    }

    public static List<(string typeName, string propertyName, bool isReferenceType)> GetStructProperties(
        this StructDeclarationSyntax structDeclaration, 
        GeneratorExecutionContext context)
    {
        SemanticModel semanticModel = context.Compilation.GetSemanticModel(structDeclaration.SyntaxTree);
        List<(string typeName, string propertyName, bool isReferenceType)> properties = [];

        foreach (PropertyDeclarationSyntax? propertyDeclaration in structDeclaration.Members.OfType<PropertyDeclarationSyntax>())
        {
            if (propertyDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
                continue;

            TypeSyntax propertyType = propertyDeclaration.Type is RefTypeSyntax refType ? refType.Type : propertyDeclaration.Type;
            TypeInfo typeInfo = semanticModel.GetTypeInfo(propertyType);
            ITypeSymbol? typeSymbol = typeInfo.Type;
            string fullyQualifiedTypeName = typeSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "object";

            // Determine if the type is a reference type or an unconstrained generic type
            bool isReferenceType = IsReferenceTypeOrUnconstrainedGeneric(typeSymbol);

            properties.Add((fullyQualifiedTypeName, propertyDeclaration.Identifier.Text, isReferenceType));
        }

        return properties;
    }

    // Gets fields and properties of a struct, maintaining the order in which they are defined
    public static List<(string typeName, string memberName, bool isReferenceType, bool isProperty)> GetStructMembers(
        this StructDeclarationSyntax structDeclaration,
        Compilation compilation,
        bool filterToRefProperties = false)
    {
        SemanticModel semanticModel = compilation.GetSemanticModel(structDeclaration.SyntaxTree);
        List<(string typeName, string memberName, bool isReferenceType, bool isProperty)> members = [];

        foreach (MemberDeclarationSyntax member in structDeclaration.Members)
        {
            if (member.Modifiers.Any(SyntaxKind.StaticKeyword))
                continue;

            switch (member)
            {
                case PropertyDeclarationSyntax propertyDeclaration:
                    {
                        if (filterToRefProperties && propertyDeclaration.Type.Kind() != SyntaxKind.RefType)
                            continue;

                        TypeSyntax propertyType = propertyDeclaration.Type is RefTypeSyntax refType ? refType.Type : propertyDeclaration.Type;
                        TypeInfo typeInfo = semanticModel.GetTypeInfo(propertyType);
                        ITypeSymbol? typeSymbol = typeInfo.Type;
                        string fullyQualifiedTypeName = typeSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "object";

                        // Determine if the type is a reference type or an unconstrained generic type
                        bool isReferenceType = IsReferenceTypeOrUnconstrainedGeneric(typeSymbol);

                        members.Add((fullyQualifiedTypeName, propertyDeclaration.Identifier.Text, isReferenceType, true));

                        break;
                    }
                case FieldDeclarationSyntax fieldDeclaration:
                    {
                        TypeInfo typeInfo = semanticModel.GetTypeInfo(fieldDeclaration.Declaration.Type);
                        ITypeSymbol? typeSymbol = typeInfo.Type;
                        string fullyQualifiedTypeName = typeSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "object";

                        // Determine if the type is a reference type or an unconstrained generic type
                        bool isReferenceType = IsReferenceTypeOrUnconstrainedGeneric(typeSymbol);

                        foreach (VariableDeclaratorSyntax variable in fieldDeclaration.Declaration.Variables)
                            members.Add((fullyQualifiedTypeName, variable.Identifier.Text, isReferenceType, false));

                        break;
                    }
            }
        }

        return members;
    }

    // Determine if type is a reference type or unconstrained generic type parameter
    private static bool IsReferenceTypeOrUnconstrainedGeneric(ITypeSymbol? typeSymbol)
    {
        if (typeSymbol is null)
            return true; // Default to true for safety if type is unknown

        // If it's already a reference type, return true
        if (typeSymbol.IsReferenceType)
            return true;

        // Check if it's a type parameter (generic) and has no constraints or only has reference type constraint
        return typeSymbol is ITypeParameterSymbol { HasValueTypeConstraint: false };
    }

    public static IEnumerable<MethodInfo> GetExtensionMethods(
        this StructDeclarationSyntax structDeclaration,
        Compilation compilation)
    {
        string structName = structDeclaration.Identifier.Text;

        // A GENERIC struct's receiver renders WITH its type parameters (`this ref nistCurve<Point>
        // curve`), which the bare identifier `nistCurve` never equals — so a generic struct's methods
        // matched NONE and a generic embed promoted no methods (crypto/elliptic's p256Curve embedding
        // nistCurve<Point>). Append the type-parameter list (matching the converter's `<T, …>` render)
        // so the receiver comparison in IsExtensionMethodForStruct succeeds.
        if (structDeclaration.TypeParameterList is { Parameters.Count: > 0 } typeParameterList)
            structName += $"<{string.Join(", ", typeParameterList.Parameters.Select(parameter => parameter.Identifier.Text))}>";

        // Get all extension method declarations in the compilation
        IEnumerable<MethodDeclarationSyntax> extensions = compilation.SyntaxTrees
            .SelectMany(tree => tree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(method => 
                    method.Modifiers.Any(m =>  m.IsKind(SyntaxKind.StaticKeyword)) &&
                    method.ParameterList.Parameters.Count > 0))
            .Where(method => method.IsExtensionMethodForStruct(structName));

        return extensions.Select(method => method.GetMethodInfo(compilation));
    }

    /// <summary>
    /// Box-receiver counterpart to <see cref="GetExtensionMethods"/>: the struct's direct-ж primary
    /// methods (<c>static M(this ж&lt;T&gt; …)</c>), as full <see cref="MethodInfo"/>. Such a method
    /// promotes through a POINTER embed unchanged — the converter emits the embed hop
    /// <c>target.&lt;embed&gt;</c> as a <c>ж&lt;T&gt;</c>, so <c>target.&lt;embed&gt;.M()</c> binds the
    /// box receiver directly (no box hop). <see cref="IsExtensionMethodForStruct"/> matches only
    /// value-receiver forms, so these need a separate harvest — sha3's <c>cshakeState</c> embeds
    /// <c>*state</c>, whose <c>Write</c> is <c>this ж&lt;state&gt;</c>; without this it had no promoted
    /// forwarder (CS1929). Callers must gate to POINTER embeds: a VALUE embed's <c>target.&lt;embed&gt;</c>
    /// is a value that cannot bind a ж-receiver (that shape needs the box-hop form).
    /// </summary>
    public static IEnumerable<MethodInfo> GetBoxReceiverExtensionMethods(
        this StructDeclarationSyntax structDeclaration,
        Compilation compilation)
    {
        string boxType = $"{PointerPrefix}<{structDeclaration.Identifier.Text}>";

        return compilation.SyntaxTrees
            .SelectMany(tree => tree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(method =>
                    method.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)) &&
                    method.ParameterList.Parameters.Count > 0))
            .Where(method =>
            {
                ParameterSyntax? firstParam = method.ParameterList.Parameters.FirstOrDefault();

                return firstParam is not null &&
                       firstParam.Modifiers.Any(m => m.IsKind(SyntaxKind.ThisKeyword)) &&
                       (firstParam.Type?.ToString() ?? "") == boxType;
            })
            .Select(method => method.GetMethodInfo(compilation));
    }

    /// <summary>
    /// Gets the embedded-POINTER hop properties on the struct — the `public partial ref
    /// ж&lt;X&gt; F { get; }` members the converter emits for a Go embedded pointer field
    /// (`type rtype struct { *abi.Type }`) — as (property name, embedded type name) pairs.
    /// Method promotion through such an embed is syntax-resolved at Go call sites (the converter
    /// emits the hop `t.F.Value.M()`), so an interface member with no direct struct method must
    /// forward through the hop the same way. The embedded type name lets the caller split the
    /// hop receiver per method: direct-ж primaries bind the box field itself (`this.F.M()`).
    /// </summary>
    public static List<(string Name, string TypeName)> GetEmbeddedPointerHopNames(this StructDeclarationSyntax structDeclaration)
    {
        List<(string, string)> hops = [];

        foreach (PropertyDeclarationSyntax property in structDeclaration.Members.OfType<PropertyDeclarationSyntax>())
        {
            TypeSyntax type = property.Type is RefTypeSyntax refType ? refType.Type : property.Type;
            string typeText = type.ToString();

            if (typeText.StartsWith("ж<") && typeText.EndsWith(">") && property.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                hops.Add((property.Identifier.Text, typeText.Substring(2, typeText.Length - 3)));
        }

        return hops;
    }

    /// <summary>
    /// Gets the (field name, embedded type name) pairs for embedded VALUE struct fields — the
    /// converter emits an embed as a <c>partial ref</c> property whose name matches its type's
    /// simple name (<c>public partial ref CommonType CommonType {{ get; }}</c>). The TypeGenerator
    /// heap-boxes the field and emits a public static ref accessor (<c>Ꮡ{Embed}</c>), so a
    /// pointer-interface adapter can project the receiver box onto the embedded field's box.
    /// </summary>
    public static List<(string Name, string TypeName)> GetEmbeddedValueHopNames(this StructDeclarationSyntax structDeclaration)
    {
        List<(string, string)> hops = [];

        foreach (PropertyDeclarationSyntax property in structDeclaration.Members.OfType<PropertyDeclarationSyntax>())
        {
            TypeSyntax type = property.Type is RefTypeSyntax refType ? refType.Type : property.Type;
            string typeText = type.ToString();

            if (typeText.StartsWith("ж<") || typeText.Contains('<') || !property.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                continue;

            string simpleTypeName = typeText;
            int lastDot = simpleTypeName.LastIndexOf('.');

            if (lastDot >= 0)
                simpleTypeName = simpleTypeName.Substring(lastDot + 1);

            if (simpleTypeName == property.Identifier.Text)
                hops.Add((property.Identifier.Text, typeText));
        }

        return hops;
    }

    private static bool IsExtensionMethodForStruct(this MethodDeclarationSyntax method, string structName)
    {
        ParameterSyntax? firstParam = method.ParameterList.Parameters.FirstOrDefault();

        if (firstParam is null || !firstParam.Modifiers.Any(m => m.IsKind(SyntaxKind.ThisKeyword)))
            return false;

        string paramType = firstParam.Type?.ToString() ?? "";

        return paramType == structName ||
               paramType == $"ref {structName}" ||
               paramType == $"in {structName}" ||
               paramType == $"ref readonly {structName}";
    }

    /// <summary>
    /// Gets the names of extension methods whose receiver is the struct's box <c>ж&lt;T&gt;</c> —
    /// the direct-ж primary form (emitted by the converter when a method needs the real receiver
    /// box, e.g. it takes the address of a receiver field). These bind on the box itself, so a
    /// pointer-interface adapter forwards to <c>m_box.M(...)</c> directly.
    /// </summary>
    public static HashSet<string> GetBoxReceiverMethodNames(
        this StructDeclarationSyntax structDeclaration,
        Compilation compilation)
    {
        return GetBoxReceiverMethodNames(structDeclaration.Identifier.Text, compilation);
    }

    /// <summary>
    /// Type-name form of <see cref="GetBoxReceiverMethodNames(StructDeclarationSyntax, Compilation)"/>
    /// for types with no local declaration in hand — e.g. the TARGET of an embedded-pointer hop
    /// (os's `fileWithoutWriteTo` embeds `*File`; File's `Read` is a direct-ж primary, so the hop
    /// must bind `this.File.Read(p)`, not the deref'd value — CS1929). Only converter-emitted
    /// primaries are visible in this compilation's syntax trees (sibling-generator ж-twins are
    /// not), which is exactly the discrimination needed.
    /// </summary>
    public static HashSet<string> GetBoxReceiverMethodNames(string typeName, Compilation compilation)
    {
        string boxType = $"ж<{typeName}>";

        return new HashSet<string>(compilation.SyntaxTrees
            .SelectMany(tree => tree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(method =>
                    method.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)) &&
                    method.ParameterList.Parameters.Count > 0))
            .Where(method =>
            {
                ParameterSyntax? firstParam = method.ParameterList.Parameters.FirstOrDefault();

                return firstParam is not null &&
                       firstParam.Modifiers.Any(m => m.IsKind(SyntaxKind.ThisKeyword)) &&
                       (firstParam.Type?.ToString() ?? "") == boxType;
            })
            .Select(method => method.Identifier.Text), StringComparer.Ordinal);
    }

    /// <summary>
    /// METADATA counterpart to <see cref="GetBoxReceiverMethodNames(string, Compilation)"/>: gets the
    /// names of PUBLIC direct-ж extension methods (<c>static M(this ж&lt;T&gt;)</c>) declared on a
    /// FOREIGN type's containing package class, visible only through compiled metadata — a syntax-tree
    /// scan of the current compilation cannot see them. Needed to forward an interface member promoted
    /// through a VALUE-embedded foreign field: database/sql's <c>driverConn</c> value-embeds
    /// <c>sync.Mutex</c>, whose <c>Lock</c>/<c>Unlock</c> are <c>this ж&lt;Mutex&gt;</c> extensions in
    /// the compiled sync assembly, so the box hop must bind <c>m_box.of(driverConn.ᏑMutex).Lock()</c>
    /// exactly as a local direct-ж primary would. Mirrors the foreignStruct arm's boxBound scan:
    /// only a PUBLIC ж-extension binds cross-assembly (unexported RecvGenerator twins are internal).
    /// </summary>
    public static HashSet<string> GetForeignBoxReceiverMethodNames(INamedTypeSymbol embedType)
    {
        HashSet<string> boxMethods = new(StringComparer.Ordinal);

        if (embedType.ContainingType is not INamedTypeSymbol packageClass)
            return boxMethods;

        foreach (IMethodSymbol method in packageClass.GetMembers().OfType<IMethodSymbol>())
        {
            if (!method.IsStatic ||
                method.DeclaredAccessibility != Accessibility.Public ||
                method.Parameters.Length == 0)
                continue;

            if (method.Parameters[0].Type is INamedTypeSymbol recvType &&
                recvType.Name == "ж" &&
                recvType.TypeArguments.Length == 1 &&
                SymbolEqualityComparer.Default.Equals(recvType.TypeArguments[0], embedType))
            {
                boxMethods.Add(method.Name);
            }
        }

        return boxMethods;
    }

    /// <summary>
    /// Gets the (field name, embedded type) pairs for a FOREIGN struct's VALUE-embedded struct fields,
    /// read from METADATA (the struct has no syntax declaration here). The converter emits an embed as a
    /// <c>public partial ref {Embed} {Embed}</c> property, so the member name equals its type's simple
    /// name — the same Go-embed convention the syntax-based <see cref="GetEmbeddedValueHopNames"/> matches.
    /// Used to forward an interface member the foreign struct PROMOTES through the embed (parse's
    /// <c>RangeNode</c> embeds <c>BranchNode</c>, whose <c>String</c> is promoted, not declared).
    /// </summary>
    public static List<(string Name, INamedTypeSymbol Type)> GetForeignValueEmbeds(INamedTypeSymbol structType)
    {
        List<(string, INamedTypeSymbol)> embeds = [];

        foreach (ISymbol member in structType.GetMembers())
        {
            if (member.IsStatic)
                continue;

            INamedTypeSymbol? memberType = member switch
            {
                IPropertySymbol property => property.Type as INamedTypeSymbol,
                IFieldSymbol field => field.Type as INamedTypeSymbol,
                _ => null
            };

            // Embed convention: member NAME equals its type's simple name; a NON-generic struct (a
            // generic member type would be a container field, not an embed).
            if (memberType is not null && !memberType.IsGenericType && member.Name == memberType.Name)
                embeds.Add((member.Name, memberType));
        }

        return embeds;
    }

    /// <summary>
    /// Gets the (member name, element type) pairs for a struct's embedded-POINTER fields read from its
    /// type SYMBOL — a Go embedded pointer field (<c>*bufio.Writer</c>) emits a <c>ж&lt;X&gt;</c> member
    /// named after X's simple name (<c>partial ref ж&lt;Writer&gt; Writer</c>), so the embed convention is
    /// the pointer sibling of <see cref="GetForeignValueEmbeds"/>. Works for FOREIGN structs (metadata
    /// members) and for resolving a LOCAL struct's FOREIGN hop element, where the syntax-based
    /// <see cref="GetEmbeddedPointerHopNames"/> knows only the member's type TEXT. A Δ-collision-renamed
    /// element keeps its markerless member name, mirroring the embedded-interface-field detection.
    /// </summary>
    public static List<(string Name, INamedTypeSymbol Type)> GetPointerEmbeds(ITypeSymbol structType)
    {
        List<(string, INamedTypeSymbol)> embeds = [];

        foreach (ISymbol member in structType.GetMembers())
        {
            if (member.IsStatic)
                continue;

            INamedTypeSymbol? memberType = member switch
            {
                IPropertySymbol property => property.Type as INamedTypeSymbol,
                IFieldSymbol field => field.Type as INamedTypeSymbol,
                _ => null
            };

            if (memberType is { TypeArguments.Length: 1 } named && named.Name == PointerPrefix &&
                named.TypeArguments[0] is INamedTypeSymbol elementType &&
                (member.Name == elementType.Name || ShadowVarMarker + member.Name == elementType.Name))
            {
                embeds.Add((member.Name, elementType));
            }
        }

        return embeds;
    }

    /// <summary>
    /// METADATA scan of a FOREIGN type's containing package class for its PUBLIC VALUE/REF-receiver
    /// extension methods (<c>static M(this {ref|in} T)</c>) — the sibling of
    /// <see cref="GetForeignBoxReceiverMethodNames"/> for the non-box receiver forms. Maps each method
    /// name to its receiver <see cref="RefKind"/> so a promoted-embed forward can spell the static call's
    /// receiver argument (<c>ref</c>/<c>in</c>/value). Only PUBLIC extensions bind cross-assembly.
    /// </summary>
    public static Dictionary<string, RefKind> GetForeignValueReceiverMethods(INamedTypeSymbol type)
    {
        Dictionary<string, RefKind> methods = new(StringComparer.Ordinal);

        if (type.ContainingType is not INamedTypeSymbol packageClass)
            return methods;

        foreach (IMethodSymbol method in packageClass.GetMembers().OfType<IMethodSymbol>())
        {
            if (!method.IsStatic ||
                method.DeclaredAccessibility != Accessibility.Public ||
                method.Parameters.Length == 0)
                continue;

            // The receiver is the type ITSELF (value/ref/in) — NOT the ж<T> box form, which
            // GetForeignBoxReceiverMethodNames covers and which binds on a box hop, not m_box.Value.<embed>.
            if (SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, type) &&
                !methods.ContainsKey(method.Name))
            {
                methods[method.Name] = method.Parameters[0].RefKind;
            }
        }

        return methods;
    }
}
