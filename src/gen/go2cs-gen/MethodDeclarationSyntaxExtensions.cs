// MethodDeclarationSyntaxExtensions.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static go2cs.Common;
using static go2cs.Templates.TemplateBase;
using static go2cs.Symbols;

namespace go2cs;

public record MethodInfo
{
    public required string Name { get; init; }

    public required string ReturnType { get; init; }

    public required (string type, string name)[] Parameters { get; init; }

    public required string GenericTypes { get; init; }

    public required Dictionary<string, string[]> TypeConstraints { get; init; }

    public bool IsRefRecv { get; init; }

    // True when the return type is a PUBLIC C# type transitively (see IsEffectivelyPublicType). The
    // name-based GetScope heuristic reads every Go-lowercase-named type as unexported, but golib
    // builtins (@string, error, bool, nint, slice<T>, …) are PUBLIC C# types despite their lowercase
    // names. A promoted forwarder returning such a type was wrongly downgraded to `internal` and so
    // was invisible cross-assembly (testing.T.Name → @string → CS1929 in x/net/nettest). This lets the
    // promotion machinery keep such a forwarder public where the return type is genuinely accessible.
    // Defaults false so any MethodInfo built without semantic type info falls back to the heuristic.
    public bool ReturnTypeIsPublic { get; init; }

    // Set when this is a direct-ж (box-receiver, `this ж<T>`) primary being promoted through a POINTER
    // embed: the promoted forwarder must call it on the embed's BOX hop (`target.<embed>`), not the
    // deref'd value (`target.<embed>.Value`) a value-receiver method uses — the box receiver needs ж<T>.
    public bool IsBoxRecv { get; init; }

    // Set when this is a direct-ж (box-receiver) primary promoted through an UNEXPORTED VALUE embed
    // (`testing.T.common`'s `Errorf`). Unlike a POINTER embed (whose hop `target.<embed>` is already a
    // ж<T>), a VALUE embed hop is a value that cannot bind a ж-receiver, so the converter renders the
    // in-package call as the box-field descent `Ꮡt.of(T.Ꮡ<embed>).M(…)`. That descent uses the embed's
    // `Ꮡ<embed>` box accessor, which is `internal` (matching the unexported embed) and thus invisible
    // cross-assembly (CS0117 — crypto/internal/cryptotest reaching testing.T.common). This flag makes
    // the promoted forwarder emit that descent in a PUBLIC shim, so a foreign package can reach the
    // exported promoted method by the plain `t.M(…)` call.
    public bool IsValueEmbedBoxRecv { get; init; }

    // Set for a cross-assembly UNEXPORTED interface method — Go's package-sealing markers such as
    // ast.Expr's `exprNode()`, ast.Stmt's `stmtNode()`, ast.Decl's `declNode()`, or
    // text/template/parse.Node's `tree()`/`writeTo()`. Its C# implementation is an INTERNAL extension
    // method in the INTERFACE's own assembly (ast's `internal static void exprNode(this ref IndexExpr)`),
    // so an adapter generated in a DIFFERENT assembly (go/internal/typeparams casting go/ast's *IndexExpr
    // to ast.Expr) cannot see it — forwarding `m_box.Value.exprNode()` is CS1061. Go never lets such a
    // method be called from outside its defining package, so the adapter satisfies the still-required
    // (public) interface member with a STUB body instead of forwarding to the inaccessible impl.
    public bool IsInaccessibleMarker { get; init; }

    public bool IsGeneric => GenericTypes.Length > 0;

    public string CallParameters => GetCallParameters(true);
    
    public string GetCallParameters(bool allowDiscarded)
    {
        return string.Join(", ", Parameters.Select((param, index) =>
        {
            if (param.name == "_")
                return allowDiscarded ? "_" : $"p{TempVarMarker}{index}";

            return param.name;
        }));
    }

    public string TypedParameters => GetTypedParameters(true);

    public string GetTypedParameters(bool allowDiscarded)
    {
        return string.Join(", ", Parameters.Select((param, index) =>
        {
            if (param.name == "_")
                return allowDiscarded ? $"{param.type} _" : $"{param.type} p{TempVarMarker}{index}";
            
            return $"{param.type} {param.name}";
        }));
    }

    public string GetSignature(bool allowDiscarded = true)
    {
        // The method name is emitted here as its own declaration-identifier token, so a Go
        // "sealing" method whose name is a C# reserved keyword — testing.TB.private(), the
        // ast.Node markers, encoding/gob's string() — must be `@`-escaped. Names read from
        // Roslyn (IMethodSymbol.Name) arrive UNescaped, so an inherited `private()` pulled in
        // through AllInterfaces would otherwise emit `void private()` (CS1520), corrupting the
        // enclosing class body. EscapeCsKeyword is a no-op for non-keywords and for names that
        // are already escaped or qualified, so this is safe for every caller.
        return $"{EscapeCsKeyword(Name)}{GetGenericSignature()}({GetTypedParameters(allowDiscarded)}){GetWhereConstraints()}";
    }

    public string GetGenericSignature()
    {
        return IsGeneric ? $"<{GenericTypes}>" : "";
    }

    public string GetWhereConstraints()
    {
        if (!IsGeneric || TypeConstraints.Count == 0)
            return string.Empty;

        List<string> constraints = [];

        foreach (KeyValuePair<string, string[]> kvp in TypeConstraints)
        {
            string typeParam = kvp.Key;
            string[] typeConstraints = kvp.Value;

            if (typeConstraints.Length > 0)
                constraints.Add($"where {typeParam} : {string.Join(", ", typeConstraints)}");
        }

        return $"\r\n{TypeElemIndent}{string.Join("\r\n        ", constraints)}";
    }

    public bool IsSameSignature(IMethodSymbol methodSymbol)
    {
        // Compare method names
        if (Name != methodSymbol.Name)
            return false;

        // Compare return types - convert ITypeSymbol to string representation
        string returnTypeString = GlobalQualify(methodSymbol.ReturnType.ToDisplayString());

        if (ReturnType != returnTypeString)
            return false;

        // Compare parameter counts
        if (Parameters.Length != methodSymbol.Parameters.Length)
            return false;

        // Compare parameter types
        for (int i = 0; i < Parameters.Length; i++)
        {
            string paramType = GlobalQualify(methodSymbol.Parameters[i].Type.ToDisplayString());

            if (Parameters[i].type != paramType)
                return false;
        }

        // Compare generic type parameters count
        int genericTypesCount = methodSymbol.TypeParameters.Length;

        string[] genericTypes = string.IsNullOrEmpty(GenericTypes) ?
            [] : GenericTypes.Split(',').Select(type => type.Trim()).ToArray();

        return genericTypes.Length == genericTypesCount;
    }
}

public static class MethodSyntaxExtensions
{
    // True when a type is a PUBLIC C# type transitively — the type itself and every type argument /
    // tuple element / array-or-pointer element is public (or a use-site-bound type parameter, or a
    // builtin special type). golib builtins (@string, error, bool, nint, slice<T>, …) are PUBLIC
    // despite their Go-lowercase names, which the name-based GetScope heuristic misreads as
    // unexported. Used to keep a promoted forwarder returning such a type PUBLIC (visible
    // cross-assembly) rather than wrongly downgrading it to internal (testing.T.Name → CS1929).
    internal static bool IsEffectivelyPublicType(ITypeSymbol? type)
    {
        switch (type)
        {
            case null:
                return false;
            case ITypeParameterSymbol:
                return true;                            // accessibility is bound at the use site
            case IArrayTypeSymbol array:
                return IsEffectivelyPublicType(array.ElementType);
            case IPointerTypeSymbol pointer:
                return IsEffectivelyPublicType(pointer.PointedAtType);
        }

        // A builtin special type (int, string primitive, void, …) is always public.
        if (type.SpecialType != SpecialType.None)
            return true;

        // A public type must not be nested inside a less-accessible one.
        for (ITypeSymbol? enclosing = type; enclosing is not null; enclosing = enclosing.ContainingType)
        {
            if (enclosing.DeclaredAccessibility is not (Accessibility.Public or Accessibility.NotApplicable))
                return false;
        }

        if (type is INamedTypeSymbol named)
        {
            // A ValueTuple's own accessibility is public even when an element is internal, so a Go
            // multi-return forwarder must check each element (CS0051 fires on the least-accessible one).
            if (named.IsTupleType)
                return named.TupleElements.All(element => IsEffectivelyPublicType(element.Type));

            return named.TypeArguments.All(IsEffectivelyPublicType);
        }

        return true;
    }

    public static MethodInfo GetMethodInfo(this MethodDeclarationSyntax methodDeclaration, Compilation compilation)
    {
        SemanticModel semanticModel = compilation.GetSemanticModel(methodDeclaration.SyntaxTree);

        string[] typeParameters = methodDeclaration.TypeParameterList?.Parameters
            .Select(param => param.Identifier.Text)
            .ToArray() ?? [];

        Dictionary<string, string[]> typeConstraints = [];

        // Initialize dictionary with empty constraint arrays for each type parameter
        foreach (string typeParam in typeParameters)
            typeConstraints[typeParam] = [];

        // Process constraints if they exist
        if (methodDeclaration.ConstraintClauses.Any())
        {
            foreach (TypeParameterConstraintClauseSyntax constraintClause in methodDeclaration.ConstraintClauses)
            {
                string typeParamName = constraintClause.Name.Identifier.Text;

                if (!typeConstraints.ContainsKey(typeParamName))
                    continue;

                string[] constraints = constraintClause.Constraints
                    .Select(constraint => GetConstraintText(constraint, semanticModel))
                    .Where(text => !string.IsNullOrEmpty(text))
                    .ToArray();

                typeConstraints[typeParamName] = constraints;
            }
        }

        return new MethodInfo()
        {
            Name = methodDeclaration.Identifier.Text,
            ReturnType = methodDeclaration.GetReturnType(semanticModel),
            ReturnTypeIsPublic = IsEffectivelyPublicType(semanticModel.GetTypeInfo(methodDeclaration.ReturnType).Type),
            GenericTypes = string.Join(", ", typeParameters),
            TypeConstraints = typeConstraints,

            Parameters = methodDeclaration.ParameterList.Parameters.Select(param =>
            {
                if (param.Type is null)
                    return (type: "object", name: param.Identifier.Text);

                TypeInfo typeInfo = semanticModel.GetTypeInfo(param.Type);
                ITypeSymbol? typeSymbol = typeInfo.Type;
                string fullyQualifiedTypeName = GlobalQualify(typeSymbol?.ToDisplayString() ?? "object");

                // Preserve a `params` (variadic) modifier: the converter emits a Go variadic method
                // as `add(this ref Builder b, params ꓸꓸꓸbyte bytesʗp)`, but the resolved type is the
                // bare `Span<byte>` — dropping `params` makes the generated ж<Builder> overload reject
                // a call passing individual elements (`c.add(0xff)` → CS1929, falling back to the
                // ref-receiver value method). The Go variadic is always the LAST, non-receiver
                // parameter, so `params` never lands on the `this ж<T>` receiver.
                if (param.Modifiers.Any(SyntaxKind.ParamsKeyword))
                    fullyQualifiedTypeName = $"params {fullyQualifiedTypeName}";

                return (type: fullyQualifiedTypeName, name: param.Identifier.Text);
            }).ToArray(),

            IsRefRecv = methodDeclaration.ParameterList.Parameters.Any(param =>
                param.Modifiers.Any(SyntaxKind.ThisKeyword) &&
                param.Modifiers.Any(SyntaxKind.RefKeyword))
        };
    }

    private static string GetConstraintText(TypeParameterConstraintSyntax constraint, SemanticModel semanticModel)
    {
        return constraint switch
        {
            ClassOrStructConstraintSyntax classOrStruct => classOrStruct.ClassOrStructKeyword.IsKind(SyntaxKind.ClassKeyword) ? "class" : "struct",
            ConstructorConstraintSyntax => "new()",
            DefaultConstraintSyntax => "default",
            TypeConstraintSyntax typeConstraint =>
                semanticModel.GetTypeInfo(typeConstraint.Type).Type?.ToDisplayString() ?? typeConstraint.Type.ToString(),
            _ => string.Empty
        };
    }

    private static string GetReturnType(this MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel)
    {
        TypeInfo typeInfo = semanticModel.GetTypeInfo(methodDeclaration.ReturnType);
        ITypeSymbol? typeSymbol = typeInfo.Type;

        return GlobalQualify(typeSymbol?.ToDisplayString() ?? "object");
    }

    public static MethodInfo GetMethodInfo(this IMethodSymbol methodSymbol)
    {
        // Convert parameters to the required tuple format
        (string type, string name)[] parameters = methodSymbol.Parameters
            .Select(parameter => (type: GlobalQualify(parameter.Type.ToDisplayString()), name: parameter.Name))
            .ToArray();

        // Extract generic type parameters
        string genericTypes = string.Join(", ", methodSymbol.TypeParameters.Select(typeParameter => typeParameter.Name));

        // Extract type constraints for generic parameters
        Dictionary<string, string[]> typeConstraints = new();

        foreach (ITypeParameterSymbol? typeParam in methodSymbol.TypeParameters)
        {
            List<string> constraints = [];

            // Add class/struct constraint
            if (typeParam.HasReferenceTypeConstraint)
                constraints.Add("class");
            else if (typeParam.HasValueTypeConstraint)
                constraints.Add("struct");

            // Add notnull constraint
            if (typeParam.HasNotNullConstraint)
                constraints.Add("notnull");

            // Add interface and type constraints
            constraints.AddRange(typeParam.ConstraintTypes.Select(constraintType => constraintType.ToDisplayString()));

            // Add unmanaged constraint
            if (typeParam.HasUnmanagedTypeConstraint)
                constraints.Add("unmanaged");

            // Add constructor constraint
            if (typeParam.HasConstructorConstraint)
                constraints.Add("new()");

            typeConstraints[typeParam.Name] = constraints.ToArray();
        }

        return new MethodInfo
        {
            Name = methodSymbol.Name,
            ReturnType = GlobalQualify(methodSymbol.ReturnType.ToDisplayString()),
            ReturnTypeIsPublic = IsEffectivelyPublicType(methodSymbol.ReturnType),
            Parameters = parameters,
            GenericTypes = genericTypes,
            TypeConstraints = typeConstraints,
            IsRefRecv = methodSymbol.ReturnsByRef
        };
    }
}
