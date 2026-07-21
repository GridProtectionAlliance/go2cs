//******************************************************************************************************
//  ImplicitConvGenerator.cs - Gbtc
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
//  01/16/2025 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

//#define DEBUG_GENERATOR

using System;
using System.Collections.Generic;
using System.Linq;
using go2cs.Templates.ImplicitConv;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static go2cs.Common;
using static go2cs.Symbols;

#if DEBUG_GENERATOR
using System.Diagnostics;
#endif

namespace go2cs;

[Generator]
public class ImplicitConvGenerator : ISourceGenerator
{
    private const string Namespace = "go";
    private const string AttributeName = "GoImplicitConv";
    private const string FullAttributeName = $"{Namespace}.{AttributeName}Attribute<TSource, TTarget>";

    // Fully-qualified, keyword-escaped, special-types display format used to reference a FOREIGN named
    // type unambiguously from a generated file (e.g. `global::go.@internal.abi_package.NameOff`) and to
    // render an underlying basic as its C# keyword (`int`, `ulong`).
    private static readonly SymbolDisplayFormat s_qualifiedFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

    public void Initialize(GeneratorInitializationContext context)
    {
#if DEBUG_GENERATOR
        if (!Debugger.IsAttached)
            Debugger.Launch();
#endif

        // Register to find "GoImplicitConv" on assembly attribute declarations
        context.RegisterForSyntaxNotifications(() => new AssemblyAttributeFinder(FullAttributeName));
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not AssemblyAttributeFinder { HasAttributes: true } attributeFinder)
            return;

        HashSet<string> emittedHintNames = new(StringComparer.OrdinalIgnoreCase);
        HashSet<string> emittedConversions = new(StringComparer.Ordinal);

        foreach ((AttributeSyntax attributeSyntax, GeneratorSyntaxContext syntaxContext, CompilationUnitSyntax compilationUnit, FileScopedNamespaceDeclarationSyntax? namespaceSyntax) in attributeFinder.TargetAttributes)
        {
            SyntaxTree syntaxTree = attributeSyntax.SyntaxTree;
            SemanticModel semanticModel = context.Compilation.GetSemanticModel(syntaxTree);

            string packageNamespace = GetNamespace(namespaceSyntax) ?? Namespace;
            string packageClassName = GetFirstClassName(compilationUnit) ?? throw new MissingMemberException($"No package class found in same file as [assembly: {AttributeName}]");
            string packageName = packageClassName.EndsWith(PackageSuffix) ? packageClassName[..^PackageSuffix.Length] : packageClassName;

            string[] usingStatements = GetFullyQualifiedUsingStatements(syntaxTree, semanticModel);

            // Extract generic type arguments from "GoImplicitConv"
            (ITypeSymbol? sourceType, ITypeSymbol? targetType) = attributeSyntax.Get2GenericTypeArguments(syntaxContext);
            
            if (sourceType is null || targetType is null)
                throw new InvalidOperationException($"Invalid usage of [assembly: {AttributeName}] attribute, must specify two generic type arguments.");

            if (sourceType.TypeKind != TypeKind.Struct)
                // Source is not a struct (e.g. a defined type over a non-struct underlying). This one
                // conversion can't be generated; skip it rather than aborting ALL of this generator's
                // output for the whole compilation.
                continue;

            // Keyword-escape the type names: a Go defined type named after a C# keyword (`type short
            // int16` in github.com/mattn/go-colorable) is declared `partial struct @short`, so the
            // operator's host, return, and `new` must use `@short` too — the raw symbol name `short`
            // yields `partial struct short`, which parses the operator into the enclosing static class
            // (CS0715/CS0057). EscapeCsKeyword is a no-op for every non-keyword (and generic) name.
            string sourceTypeName = EscapeCsKeyword(sourceType.GetFullTypeName());
            string targetTypeName = EscapeCsKeyword(targetType.GetFullTypeName());

            // Get the attribute's argument values, if defined
            (string name, string value)[] arguments = attributeSyntax.GetArgumentValues();
            bool inverted = bool.Parse(arguments.FirstOrDefault(arg => arg.name.Equals("Inverted")).value?.Trim() ?? "false");
            bool indirect = bool.Parse(arguments.FirstOrDefault(arg => arg.name.Equals("Indirect")).value?.Trim() ?? "false");
            string? valueType = arguments.FirstOrDefault(arg => arg.name.Equals("ValueType")).value?.Trim();

            List<(string typeName, string memberName)> structMembers;

            if (string.IsNullOrWhiteSpace(valueType))
            {
                StructDeclarationSyntax? structDeclaration = GetStructDeclaration(syntaxContext, targetTypeName);

                if (structDeclaration is null)
                    // The target type has no local struct declaration to enumerate members from — e.g. a
                    // golib generic box such as `ж<Type>` produced by an embedded cross-package pointer.
                    // Skip just this one conversion rather than throwing, which would suppress ALL of this
                    // generator's output for the whole compilation.
                    continue;

                structMembers = structDeclaration
                    .GetStructMembers(context.Compilation, true)
                    .Select(member => (member.typeName, member.memberName))
                    .ToList();
            }
            else
            {
                valueType = valueType![1..^1];
                structMembers = [];
                targetTypeName = EscapeCsKeyword(targetType.GetFullTypeName(true));
            }

            // Cross-package numeric conversion whose operator constructs a FOREIGN named-numeric type
            // (declared in another assembly). Two ways this arises and both are broken by default:
            //   • foreign SOURCE via a local alias (`GoImplicitConv<nameOff, Δhex>(Inverted = true)`,
            //     where runtime's `nameOff` aliases `internal/abi.NameOff`): the operator is hosted in
            //     `partial struct {sourceTypeName}` — a phantom LOCAL type, since a foreign type can't
            //     be extended here (CS1729) — and constructs the foreign source.
            //   • foreign TARGET via a qualified reference (`GoImplicitConv<Hx, pkg.Off>`): the host is
            //     local, but the body still `new pkg.Off(...)`s a foreign type.
            // In both, casting `src.Value` straight to the foreign named type has no cross-assembly route
            // (`ulong`→`NameOff` ⇒ CS0030). The constructed type is whichever side the operator builds
            // via `new` (the LH type: source when inverted, else target). When it is foreign, construct
            // it through its UNDERLYING basic — `new global::…NameOff((int)src.Value)` — mirroring the
            // converter's through-underlying inline cast; and if the default host (the source type) is
            // itself foreign, relocate the operator into the LOCAL type so it can be declared at all.
            string? hostTypeNameOverride = null, lhTypeNameOverride = null, rhTypeNameOverride = null, convExprOverride = null;

            if (!string.IsNullOrWhiteSpace(valueType))
            {
                ITypeSymbol constructedType = inverted ? sourceType : targetType; // the type built via `new`
                bool constructedIsForeign = !SymbolEqualityComparer.Default.Equals(constructedType.ContainingAssembly, context.Compilation.Assembly);

                if (constructedIsForeign)
                {
                    string? constructedUnderlying = GetUnderlyingBasicName(constructedType);

                    if (constructedUnderlying is not null)
                    {
                        string qualifiedConstructed = constructedType.ToDisplayString(s_qualifiedFormat);

                        lhTypeNameOverride = qualifiedConstructed;
                        convExprOverride = $"new {qualifiedConstructed}(({constructedUnderlying})src.Value)";

                        // The default host is the SOURCE type's partial struct. If that is itself
                        // foreign it can't be extended here (CS1729 phantom); relocate into the LOCAL
                        // target type and reference the param (RH) type fully-qualified.
                        bool sourceIsForeign = !SymbolEqualityComparer.Default.Equals(sourceType.ContainingAssembly, context.Compilation.Assembly);
                        bool targetIsLocal = SymbolEqualityComparer.Default.Equals(targetType.ContainingAssembly, context.Compilation.Assembly);

                        if (sourceIsForeign && targetIsLocal)
                        {
                            hostTypeNameOverride = targetType.Name;
                            rhTypeNameOverride = targetType.ToDisplayString(s_qualifiedFormat);
                        }
                    }
                }
            }

            // A MIXED-accessibility local pair (public ΔKind ↔ internal flag — reflect's
            // GoImplicitConv<ΔKind, flag>(Inverted)): C# operators are necessarily public, so
            // hosting in the MORE accessible type makes the less-accessible parameter fail
            // CS0057. Relocate into the LESS accessible side — a public operator inside an
            // internal struct is legal, and both operand types are visible there.
            // Accessibility derives from the GO export rule (GetScope on the Δ-stripped name):
            // at analysis time the [GoType] partials are modifier-less — the public/internal
            // modifier lives on the TypeGenerator's OWN output, which single-pass generators
            // cannot see (DeclaredAccessibility reads Private for both).
            if (hostTypeNameOverride is null &&
                GetScope(GetSimpleName(sourceType.Name, dropCollisionPrefix: true)) == "public" &&
                GetScope(GetSimpleName(targetType.Name, dropCollisionPrefix: true)) != "public" &&
                SymbolEqualityComparer.Default.Equals(targetType.ContainingAssembly, context.Compilation.Assembly))
            {
                hostTypeNameOverride = targetType.Name;
            }

            // The operator's body casts `src.Value` to the constructed type; a uintptr-BACKED
            // src wrapper's Value is the golib uintptr STRUCT, and struct→ΔKind chains two user
            // conversions (CS0030 — reflect's flag→ΔKind). Hop through nuint. The backing kind
            // comes from the src side's [GoType("num:uintptr")] tag — the generated Value
            // property is invisible to a single-pass sibling generator.
            if (convExprOverride is null && !string.IsNullOrWhiteSpace(valueType))
            {
                ITypeSymbol srcSide = inverted ? targetType : sourceType;
                StructDeclarationSyntax? srcDecl = GetStructDeclaration(syntaxContext, srcSide.Name);

                string? goTypeTag = srcDecl?.AttributeLists
                    .SelectMany(list => list.Attributes)
                    .Where(attr => attr.Name.ToString() is "GoType" or "GoTypeAttribute")
                    .Select(attr => attr.ArgumentList?.Arguments.FirstOrDefault()?.ToString().Trim('"'))
                    .FirstOrDefault();

                if (goTypeTag == "num:uintptr")
                    convExprOverride = $"new {valueType}(({valueType})(nuint)src.Value)";
            }

            // A LOCAL numeric pair whose SOURCE underlying does not implicitly convert to the
            // CONSTRUCTED type's underlying — internal/trace's Inverted `timestamp`(uint64) →
            // `Time`(int64): the default `(TargetWrapper)src.Value` cast routes `ulong`→(the
            // wrapper's `long`)→wrapper, but `ulong`→`long` is not an implicit C# conversion, so it
            // is CS0030. Construct through the constructed type's underlying basic with an explicit
            // cast instead: `new ΔTime((long)src.Value)`. Each side's underlying comes from its
            // [GoType("num:X")] tag (the generated Value property is invisible to a single-pass
            // sibling generator, so the implicit-conversion test runs over the corresponding
            // SpecialTypes). Because the default cast compiles IFF that same source→constructed basic
            // conversion is implicit, this fires ONLY on cases that do not currently compile — every
            // already-compiling conversion stays byte-identical. (uintptr-backed pairs are handled by
            // the block above and keep their nuint hop; `int`/`uint` native-width wrappers are left
            // out where the classification is version-sensitive.)
            if (convExprOverride is null && !string.IsNullOrWhiteSpace(valueType))
            {
                ITypeSymbol constructedType = inverted ? sourceType : targetType;
                ITypeSymbol srcSideType = inverted ? targetType : sourceType;

                string? constructedBasic = GetNumBasic(syntaxContext, constructedType.Name);
                string? srcBasic = GetNumBasic(syntaxContext, srcSideType.Name);

                if (constructedBasic is not null && srcBasic is not null)
                {
                    string? constructedKeyword = NumBasicToKeyword(constructedBasic);

                    if (constructedKeyword is not null && NumBasicToKeyword(srcBasic) is not null &&
                        !IsImplicitNumericConversion(srcBasic, constructedBasic))
                    {
                        string lhName = inverted ? sourceTypeName : targetTypeName;
                        convExprOverride = $"new {lhName}(({constructedKeyword})src.Value)";
                    }
                }
            }

            // The emitted user-defined conversion operator's signature is (sourceTypeName,
            // targetTypeName, inverted) — `direct` vs `indirect` only changes the body. The same
            // pair can be recorded as BOTH a direct and an indirect conversion (e.g. `g` ↔ `ж<g>`),
            // which would emit two identical operators (CS0557). Skip an exact-signature duplicate.
            if (!emittedConversions.Add($"{sourceTypeName}->{targetTypeName}|{inverted}"))
                continue;

            string generatedSource = new ImplicitConvTemplate
            {
                PackageNamespace = packageNamespace,
                PackageName = packageName,
                SourceTypeName = sourceTypeName,
                TargetTypeName = targetTypeName,
                Inverted = inverted,
                Indirect = indirect,
                ValueType = valueType,
                StructMembers = structMembers,
                UsingStatements = usingStatements,
                HostTypeNameOverride = hostTypeNameOverride,
                LHTypeNameOverride = lhTypeNameOverride,
                RHTypeNameOverride = rhTypeNameOverride,
                ConvExprOverride = convExprOverride
            }
            .Generate();

            // Add the source code to the compilation
            context.AddSource(GetUniqueHintName(emittedHintNames, GetValidFileName($"{packageNamespace}.{packageClassName}.{sourceTypeName}-{targetTypeName}{(inverted ? "-inv" : "")}.g.cs")), generatedSource);
        }
    }

    private static string? GetNamespace(FileScopedNamespaceDeclarationSyntax? namespaceSyntax)
    {
        return namespaceSyntax?.Name.ToString();
    }

    private static string? GetFirstClassName(CompilationUnitSyntax compilationUnit)
    {
        return compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault()?.Identifier.Text;
    }

    // A generated named-numeric struct exposes its underlying basic value through a public `Value`
    // property (see InheritedTypeTemplate). That basic is the through-underlying cast target needed to
    // construct a foreign named-numeric type cross-assembly. Returns null when no such property exists.
    private static string? GetUnderlyingBasicName(ITypeSymbol type)
    {
        IPropertySymbol? valProperty = type.GetMembers("Value").OfType<IPropertySymbol>().FirstOrDefault();
        return valProperty?.Type.ToDisplayString(s_qualifiedFormat);
    }

    // Reads a LOCAL named-numeric struct's [GoType("num:X")] underlying basic ("int64", "uint64", …)
    // from syntax — the generated Value property is invisible to a single-pass sibling generator, so
    // the tag is the only place the underlying is knowable here. Returns null when the type has no
    // local declaration or no numeric GoType tag.
    private static string? GetNumBasic(GeneratorSyntaxContext context, string typeName)
    {
        string? tag = GetStructDeclaration(context, typeName)?.AttributeLists
            .SelectMany(list => list.Attributes)
            .Where(attr => attr.Name.ToString() is "GoType" or "GoTypeAttribute")
            .Select(attr => attr.ArgumentList?.Arguments.FirstOrDefault()?.ToString().Trim('"'))
            .FirstOrDefault();

        return tag is not null && tag.StartsWith("num:") ? tag["num:".Length..] : null;
    }

    // The C# implicit numeric conversions among the fixed-width integer and float basics — the exact
    // set that decides whether the default `(Wrapper)src.Value` cast compiles. `int`/`uint`/`uintptr`
    // (native-width) are intentionally absent (NumBasicToKeyword returns null for them, so the caller
    // leaves them to the default / uintptr paths). Identity counts as convertible.
    private static bool IsImplicitNumericConversion(string src, string dst)
    {
        if (src == dst)
            return true;

        return src switch
        {
            "int8" => dst is "int16" or "int32" or "rune" or "int64" or "float32" or "float64",
            "int16" => dst is "int32" or "rune" or "int64" or "float32" or "float64",
            "int32" or "rune" => dst is "int64" or "float32" or "float64",
            "int64" => dst is "float32" or "float64",
            "uint8" or "byte" => dst is "int16" or "uint16" or "int32" or "rune" or "uint32" or "int64" or "uint64" or "float32" or "float64",
            "uint16" => dst is "int32" or "rune" or "uint32" or "int64" or "uint64" or "float32" or "float64",
            "uint32" => dst is "int64" or "uint64" or "float32" or "float64",
            "uint64" => dst is "float32" or "float64",
            "float32" => dst is "float64",
            _ => false
        };
    }

    // The C# keyword for a Go numeric basic, used as the explicit through-underlying cast target.
    private static string? NumBasicToKeyword(string basic) => basic switch
    {
        "int8" => "sbyte",
        "int16" => "short",
        "int32" or "rune" => "int",
        "int64" => "long",
        "uint8" or "byte" => "byte",
        "uint16" => "ushort",
        "uint32" => "uint",
        "uint64" => "ulong",
        "float32" => "float",
        "float64" => "double",
        _ => null
    };

    private static StructDeclarationSyntax? GetStructDeclaration(GeneratorSyntaxContext context, string structName)
    {
        if (PointerExpr.IsMatch(structName))
            structName = structName[(structName.IndexOf('<') + 1)..^1];

        // Match on ValueText (the identifier WITHOUT any `@` escape) against the `@`-stripped name, so a
        // keyword-named struct declared `partial struct @short` is found whether the caller passes the
        // symbol name `short` or the escaped `@short` — otherwise its [GoType("num:…")] tag is missed and
        // the numeric-conversion body falls back to a broken `(@short)src.Value` cast (CS0030).
        return context.SemanticModel.Compilation.SyntaxTrees
            .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<StructDeclarationSyntax>())
            .FirstOrDefault(structDeclaration => structDeclaration.Identifier.ValueText == structName.TrimStart('@'));
    }
}
