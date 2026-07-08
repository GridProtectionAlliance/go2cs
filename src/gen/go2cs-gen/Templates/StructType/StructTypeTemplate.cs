using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static go2cs.Common;
using static go2cs.Symbols;

namespace go2cs.Templates.StructType;

internal class StructTypeTemplate : TemplateBase
{
    // Template Parameters
    public required GeneratorExecutionContext Context;
    public required string StructName;
    public required string FullyQualifiedStructType;
    public required List<(string typeName, string memberName, bool isReferenceType, bool isPromotedStruct)> StructMembers;
    public required bool HasEqualityOperators;

    private string? m_nonGenericStructName;
    public string NonGenericStructName => m_nonGenericStructName ??= GetSimpleName(StructName, true);

    private List<(string typeName, string memberName, bool isReferenceType, bool isPromotedStruct)>? m_publicStructMembers;
    private List<(string typeName, string memberName, bool isReferenceType, bool isPromotedStruct)> PublicStructMembers =>
        m_publicStructMembers ??= StructMembers.Where(item =>
        {
            string simpleName = GetSimpleName(item.memberName);

            // Blank/underscore fields (e.g. an embedded unexported marker like `_ noCopy`)
            // are not exported and must not drive a public constructor — their type can be
            // less accessible than the struct, which makes a public ctor invalid (CS0051).
            return !simpleName.StartsWith("_") && GetScope(simpleName) == "public";
        }).ToList();

    public override string TemplateBody =>
        $$"""
            [{{GeneratedCodeAttribute}}]
            {{Scope}} partial struct {{StructName}}
            {
                // Promoted Struct References
                {{PromotedStructDeclarations}}
        
                // Field References
                {{FieldReferences}}
                
                // Constructors
                {{Constructors}}
                
                // Handle comparisons between struct '{{NonGenericStructName}}' instances
                public bool Equals({{StructName}} other) =>
                    {{CompareFields}};
                
                public override bool Equals(object? obj) => obj is {{StructName}} other && Equals(other);
                
                public override int GetHashCode() => {{HashCode}};
                
                public static bool operator ==({{StructName}} left, {{StructName}} right) => left.Equals(right);
                
                public static bool operator !=({{StructName}} left, {{StructName}} right) => !(left == right);
        
                // Handle comparisons between 'nil' and struct '{{NonGenericStructName}}'
                public static bool operator ==({{StructName}} value, NilType nil) => value.Equals(default({{StructName}}));

                public static bool operator !=({{StructName}} value, NilType nil) => !(value == nil);

                public static bool operator ==(NilType nil, {{StructName}} value) => value == nil;

                public static bool operator !=(NilType nil, {{StructName}} value) => value != nil;

                public static implicit operator {{StructName}}(NilType nil) => default({{StructName}});

                public override string ToString() => string.Concat("{", string.Join(" ",
                [
                    {{(StructMembers.Count > 0 ? string.Join(",\r\n            ", StructMembers.Select(GetToStringImplementation)) : "\"\"")}}
                ]), "}");
            }{{PromotedStructReceivers()}}
        """;

    private string PromotedStructDeclarations
    {
        get
        {
            (string typeName, string memberName, bool isReferenceType, bool isPromotedStruct)[] promotedStructs = StructMembers.Where(item => item.isPromotedStruct).ToArray();

            if (promotedStructs.Length == 0)
                return $"// -- {NonGenericStructName} has no promoted structs";

            StringBuilder result = new();

            // The composed box-field name must use the unescaped member name — a C#-keyword embed
            // (`type base struct{…}`) arrives escaped `@base`, but `Ꮡʗ@base` is invalid ('@' only
            // leads an identifier). The standalone member ACCESS sites keep `@base`.
            foreach ((string typeName, string memberName, _, _) in promotedStructs)
            {
                if (result.Length > 0)
                    result.Append($"\r\n{TypeElemIndent}");

                result.Append($"private readonly {PointerPrefix}<{typeName}> {AddressPrefix}{CapturedVarMarker}{GetUnsanitizedIdentifier(memberName)};");
            }

            result.Append($"\r\n\r\n{TypeElemIndent}// Promoted Struct Accessors");

            foreach ((string typeName, string memberName, _, _) in promotedStructs)
            {
                string typeScope = GetScope(GetSimpleName(typeName));
                result.Append($"\r\n{TypeElemIndent}{typeScope} partial ref {typeName} {memberName} => ref {AddressPrefix}{CapturedVarMarker}{GetUnsanitizedIdentifier(memberName)}.Value;");
            }

            result.Append($"\r\n\r\n{TypeElemIndent}// Promoted Struct Field Accessors");

            // Go's shadowing rule: an OWN (declared) field with the same name shadows the embedded
            // member, so promotion must skip it — reflect's makeFuncImpl declares `fn` while
            // embedding makeFuncCtxt, whose `fn` would otherwise emit a duplicate accessor
            // (CS0102) and a duplicate Ꮡfn reference (CS0111).
            HashSet<string> declaredMemberNames = new(StructMembers.Select(m => GetSimpleName(m.memberName)));

            // Go's AMBIGUITY rule is DEPTH-AWARE: a member name is promoted iff there is a
            // UNIQUE occurrence at the SHALLOWEST embedding depth. Same-depth duplicates are
            // ambiguous and drop (bufio's ReadWriter embeds *Reader AND *Writer, both carrying
            // err/buf — CS0102/CS0111), but a shallower occurrence WINS over deeper ones —
            // macho's FatArch embeds FatArchHeader (Cpu/SubCpu at depth 1) AND *File (whose
            // flattened surface carries FileHeader's Cpu/SubCpu at depth 2): the depth-blind
            // count dropped BOTH, orphaning the converter's bare `fa.Cpu` (CS1061 x4).
            Dictionary<string, int> promotedFieldMinDepth = new(StringComparer.Ordinal);
            Dictionary<string, int> promotedFieldMinDepthCount = new(StringComparer.Ordinal);

            foreach ((string promotedStructType, _, _, _) in promotedStructs)
            {
                foreach ((_, string memberName, int depth) in getStructMembers(promotedStructType))
                {
                    string simpleName = GetSimpleName(memberName);

                    if (!promotedFieldMinDepth.TryGetValue(simpleName, out int minDepth) || depth < minDepth)
                    {
                        promotedFieldMinDepth[simpleName] = depth;
                        promotedFieldMinDepthCount[simpleName] = 1;
                    }
                    else if (depth == minDepth)
                    {
                        promotedFieldMinDepthCount[simpleName]++;
                    }
                }
            }

            bool promotes(string simpleName, int depth) =>
                promotedFieldMinDepth[simpleName] == depth && promotedFieldMinDepthCount[simpleName] == 1;

            foreach ((string promotedStructType, _, _, _) in promotedStructs)
            {
                // Rewrite the embed's type PARAMETERS to this instantiation's type ARGUMENTS on the
                // promoted member TYPE (a generic embed's field carries `Point`, out of scope here).
                Dictionary<string, string> typeArgMap = GetEmbedTypeArgumentMap(promotedStructType);

                foreach ((string typeName, string memberName, int depth) in getStructMembers(promotedStructType))
                {
                    // A blank `_` field (padding / embedded unexported marker) is never promoted or
                    // selectable in Go; emitting an accessor for it collides with the enclosing
                    // struct's own `_` field (CS0102).
                    if (GetSimpleName(memberName) == "_")
                        continue;

                    if (declaredMemberNames.Contains(GetSimpleName(memberName)))
                        continue;

                    if (!promotes(GetSimpleName(memberName), depth))
                        continue;

                    // Scope derives from the MEMBER name (its exportedness), matching the box-field
                    // accessors: a lowercase field TYPE (uintptr Size_) made an EXPORTED promoted
                    // member internal - invisible cross-assembly (reflect via abi, CS1061 x22).
                    string typeScope = GetScope(GetSimpleName(memberName));

                    // A promoted-field accessor whose name equals the ENCLOSING type name is CS0542
                    // (a member cannot share its type's name) — debug/gosym's `type Func struct{ *Sym }`
                    // where `Sym` has a field `Func *Func`, so the promoted `Sym.Func` lands as a `Func`
                    // accessor inside `Func`. Δ-prefix the accessor NAME (the field ACCESS on the right
                    // keeps the original name), matching the ΔGoType/Δslice collision-rename precedent.
                    // The promoted field is read directly on the embedded struct (`sym.Func`), never via
                    // the outer value, so no converter reference needs coordinating; a package that DID
                    // read `outer.Func` would surface CS1061 in the gate.
                    string accessorName = GetSimpleName(memberName) == NonGenericStructName ? $"{ShadowVarMarker}{memberName}" : memberName;
                    result.Append($"\r\n{TypeElemIndent}{typeScope} ref {SubstituteTypeParameters(typeName, typeArgMap)} {accessorName} => ref {StripTypeArgs(GetSimpleName(promotedStructType, dropCollisionPrefix: true))}.{memberName};");
                }
            }

            result.Append($"\r\n\r\n{TypeElemIndent}// Promoted Struct Field Accessor References");

            foreach ((string promotedStructType, _, _, _) in promotedStructs)
            {
                Dictionary<string, string> typeArgMap = GetEmbedTypeArgumentMap(promotedStructType);

                foreach ((string typeName, string memberName, int depth) in getStructMembers(promotedStructType))
                {
                    // Blank `_` field — unaddressable in Go, and its `Ꮡ_` would collide with the
                    // enclosing struct's own `Ꮡ_` (CS0111).
                    if (GetSimpleName(memberName) == "_")
                        continue;

                    // Own-field shadowing — see the accessor loop above (CS0111 on Ꮡfn).
                    if (declaredMemberNames.Contains(GetSimpleName(memberName)))
                        continue;

                    // Cross-embed ambiguity — see the depth-aware counting pass above.
                    if (!promotes(GetSimpleName(memberName), depth))
                        continue;

                    // The Ꮡ-prefixed accessor NAME must use the unescaped member name — a C#-keyword
                    // field is escaped `@base`, but `Ꮡ@base` is invalid ('@' only leads). The member
                    // ACCESS on the right keeps `@base`.
                    // Scope derives from the MEMBER name (its exportedness), matching the box-field
                    // accessors: a lowercase field TYPE (uintptr Size_) made an EXPORTED promoted
                    // member internal - invisible cross-assembly (reflect via abi, CS1061 x22).
                    string typeScope = GetScope(GetSimpleName(memberName));
                    // StructName (with type parameters), not NonGenericStructName — a GENERIC
                    // struct's instance param must carry them (Δentry<K, V>, CS0305); the
                    // promoted-struct MEMBER access strips its type arguments (the property is
                    // `node`, not `node<K, V>` — internal/concurrent's entry[K,V]).
                    result.Append($"\r\n{TypeElemIndent}{typeScope} static ref {SubstituteTypeParameters(typeName, typeArgMap)} {AddressPrefix}{GetUnsanitizedIdentifier(memberName)}(ref {StructName} instance) => ref instance.{StripTypeArgs(GetSimpleName(promotedStructType, dropCollisionPrefix: true))}.{memberName};");
                }
            }

            return result.ToString();

            IEnumerable<(string typeName, string memberName, int depth)> getStructMembers(string structTypeName)
            {
                // Collect the embedded struct's members TRANSITIVELY: a member promoted into the
                // enclosing struct may itself come from a NESTED embedded struct (Go promotes through
                // every embedding level). e.g. stackWorkBuf embeds stackWorkBufHdr embeds workbufhdr,
                // so stackWorkBuf.nobj must promote workbufhdr's nobj — but reading stackWorkBufHdr's
                // DECLARED members alone misses it (nobj is a generated accessor on stackWorkBufHdr,
                // not a declared field). The emitted accessor stays single-hop (`stackWorkBuf.nobj =>
                // ref stackWorkBufHdr.nobj`), resolving through stackWorkBufHdr's own 1-level promotion.
                List<(string typeName, string memberName, int depth)> collected = [];
                HashSet<string> emittedNames = [];

                collect(structTypeName, [], 1);

                return collected;

                void collect(string typeName, HashSet<string> seenTypes, int depth)
                {
                    // Go forbids embedding cycles, but guard anyway so a malformed input can't loop.
                    if (!seenTypes.Add(typeName))
                        return;

                    (StructDeclarationSyntax? structDecl, Compilation? compilation) = Context.GetStructDeclaration(typeName);

                    if (structDecl is null)
                    {
                        // A CROSS-PACKAGE embedded type cannot be resolved by syntax in a real MSBuild
                        // build: project references arrive as METADATA references, never CompilationReference,
                        // so the referenced-compilations walk finds nothing — `type rtype struct { *abi.Type }`
                        // (runtime type.go) promoted NO fields and every `t.TFlag`/`t.Str`/`t.Kind_` was
                        // CS1061. Fall back to the semantic model: resolve the embedded type's symbol by
                        // metadata name and enumerate its public instance fields (the Go-visible members of
                        // a converted struct are plain public fields). Transitive promotion through a
                        // metadata type's own embeds is not chased (none of the runtime's cross-package
                        // embeds need it); the emitted single-hop accessor resolves through the embedded
                        // type's own generated promotion when it exists.
                        foreach ((string fieldTypeName, string fieldName) in getMetadataStructFields(typeName))
                        {
                            if (emittedNames.Add(fieldName))
                                collected.Add((fieldTypeName, fieldName, depth));
                        }

                        return;
                    }

                    foreach ((string memberType, string memberName, _, bool isEmbedded) in structDecl.GetStructMembers(compilation!, true))
                    {
                        // First (closest) declaration of a name wins, matching Go's promotion rules.
                        if (emittedNames.Add(memberName))
                            collected.Add((memberType, memberName, depth));

                        // An embedded struct field contributes its own members transitively. The
                        // converter emits every embed - value or POINTER - as a `partial ref`
                        // PROPERTY (the marker the top level keys isPromotedStruct on), so recurse
                        // on that flag; a NAMED field whose name merely equals its type's simple
                        // name (`RCode RCode` in dnsmessage.Header) is a plain FIELD and must not
                        // contribute nested promotions.
                        if (isEmbedded)
                            collect(memberType, seenTypes, depth + 1);
                    }
                }

                IEnumerable<(string typeName, string fieldName)> getMetadataStructFields(string typeName)
                {
                    // Normalize the source-form type reference to a CLR metadata name: strip the ж<>
                    // pointer wrapper, the `global::` prefix and `@` identifier escapes, root it in the
                    // `go` namespace, and mark the final containing package class as a NESTED type —
                    // `ж<@internal.abi_package.Type>` → `go.internal.abi_package+Type` (a converted
                    // package is a static class inside the go namespace; its types are nested members).
                    string metadataName = GeneratorExecutionContextExtensions.GetUnderlyingTypeName(typeName)
                        .Replace("global::", "").Replace("@", "");

                    if (!metadataName.StartsWith("go.", StringComparison.Ordinal))
                        metadataName = $"go.{metadataName}";

                    int lastDot = metadataName.LastIndexOf('.');

                    if (lastDot < 0)
                        yield break;

                    INamedTypeSymbol? typeSymbol =
                        Context.Compilation.GetTypeByMetadataName($"{metadataName[..lastDot]}+{metadataName[(lastDot + 1)..]}");

                    if (typeSymbol is null)
                        yield break;

                    foreach (IFieldSymbol field in typeSymbol.GetMembers().OfType<IFieldSymbol>())
                    {
                        if (field.DeclaredAccessibility != Accessibility.Public || field.IsStatic || field.IsImplicitlyDeclared)
                            continue;

                        if (GetSimpleName(field.Name) == "_")
                            continue;

                        yield return (field.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), field.Name);
                    }

                    // A REFERENCED assembly's generated wrapper exposes its embedded member and its
                    // transitively-promoted fields as public REF-RETURNING PROPERTIES (the embed
                    // accessor `public partial ref Type Type` and the promoted accessors) - the
                    // fields-only enumeration missed them, so a struct embedding a cross-package
                    // wrapper (reflect's structType over abi.StructType over abi.Type) promoted
                    // NOTHING from the deeper levels (CS1061 x32 + CS0117 x13). Yield them too;
                    // the standard single-hop `X => ref Embed.X` emission handles both shapes.
                    foreach (IPropertySymbol property in typeSymbol.GetMembers().OfType<IPropertySymbol>())
                    {
                        if (property.DeclaredAccessibility != Accessibility.Public || property.IsStatic || !property.ReturnsByRef)
                            continue;

                        if (property.IsIndexer || GetSimpleName(property.Name) == "_")
                            continue;

                        yield return (property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), property.Name);
                    }
                }
            }
        }
    }

    private string PromotedStructReceivers()
    {
        (string typeName, string memberName, bool isReferenceType, bool isPromotedStruct)[] promotedStructs = StructMembers.Where(item => item.isPromotedStruct).ToArray();

        if (promotedStructs.Length == 0)
            return "";

        StringBuilder result = new();

        result.Append("\r\n\r\n    // Promoted Struct Receivers");

        // Get all extension methods for the struct, any directly defined receivers
        // take precedence over promoted struct methods that have the same name
        (StructDeclarationSyntax? structDecl, Compilation? compilation) = Context.GetStructDeclaration(FullyQualifiedStructType);
        IEnumerable<MethodInfo>? structMethods = structDecl is null ? [] : structDecl.GetExtensionMethods(compilation!);
        HashSet<string> structMethodNames = new(structMethods?.Select(method => method.Name) ?? [], StringComparer.Ordinal);

        // A POINTER-receiver method emitted ONLY as its box primary `M(this ж<T> …)` — Go's
        // `func (s *structType) string()` becomes a direct-ж extension when it takes the address
        // of a receiver field — is invisible to IsExtensionMethodForStruct (value-receiver forms
        // only). Without folding those names in, a same-named method promoted from an embed
        // (encoding/gob's structType embeds CommonType, both declaring string()/safeString()) is
        // not suppressed and its promoted box overload duplicates the direct one (CS0111).
        if (structDecl is not null)
            structMethodNames.UnionWith(structDecl.GetBoxReceiverMethodNames(compilation!));

        // POINTER-embedded types (`*state` → the `ж<state>` hop property): their BOX-receiver primaries
        // (`state.Write(this ж<state>)`) promote too — the embed hop `target.<embed>` is already a ж<T>,
        // so `target.<embed>.M()` binds the box receiver directly. GetExtensionMethods harvests only
        // value-receiver forms, so those box primaries are collected separately below. Keyed by embedded
        // TYPE name (matches promotedStructType). Gated to POINTER embeds: a VALUE embed's `target.<embed>`
        // is a value that cannot bind a ж-receiver (sha3's cshakeState embeds *state — CS1929 without this).
        // Keyed by the embed's SIMPLE type name (dropping any collision prefix) — promotedStructType
        // arrives as the qualified box form `global::go.ж<…Inner>`, which the loops below reduce with
        // the same GetSimpleName call the forwarder emission uses, so both sides normalize to `Inner`.
        HashSet<string> pointerEmbedTypeNames = structDecl is null
            ? new(StringComparer.Ordinal)
            : new(structDecl.GetEmbeddedPointerHopNames().Select(hop => GetSimpleName(hop.TypeName, dropCollisionPrefix: true)), StringComparer.Ordinal);

        // promotedStructType arrives as the qualified BOX form, so its GetSimpleName carries a trailing
        // `.Value` (the pointer deref) the embed keys above do not — strip it before the membership test.
        static string embedKey(string typeName) =>
            GetSimpleName(typeName, dropCollisionPrefix: true) is var n && n.EndsWith(".Value", StringComparison.Ordinal)
                ? n[..^".Value".Length]
                : n;

        // Go's AMBIGUITY rule: a method name promoted from TWO OR MORE embeds at the same depth
        // is not promoted at all — bufio ReadWriter's Reader.Size vs Writer.Size (Go requires the
        // qualified rw.Reader.Size(); both generated wrappers were CS0111).
        Dictionary<string, int> promotedMethodCounts = new(StringComparer.Ordinal);

        foreach ((string promotedStructType, _, _, _) in promotedStructs)
        {
            HashSet<string> embedMethodNames = new(StringComparer.Ordinal);

            // A DIRECT (depth-1) UNEXPORTED, NON-GENERIC VALUE embed also promotes its box-receiver
            // (direct-ж) primaries through a public descent shim (see IsValueEmbedBoxRecv). Count them
            // for the same cross-embed ambiguity rule so two such embeds carrying the same method name
            // drop (as Go's ambiguity rule requires) rather than emitting a duplicate forwarder. A plain
            // value embed's type name never carries '<' — both the pointer-box embed form (`ж<…>`) and a
            // generic embed do — which is a more robust discriminator than the pointerEmbedTypeNames
            // membership test (whose `@`-keyword-escaped names, e.g. os.File's `ж<@file>`, mismatch).
            bool directEmbedIsUnexportedValue = !promotedStructType.Contains("<") &&
                GetScope(GetSimpleName(promotedStructType, dropCollisionPrefix: true)) != "public";

            countPromotedMethods(promotedStructType, []);

            foreach (string name in embedMethodNames)
                promotedMethodCounts[name] = promotedMethodCounts.TryGetValue(name, out int count) ? count + 1 : 1;

            void countPromotedMethods(string typeName, HashSet<string> seenTypes)
            {
                if (!seenTypes.Add(typeName))
                    return;

                (StructDeclarationSyntax? decl, Compilation? comp) = Context.GetStructDeclaration(typeName);

                if (decl is null)
                    return;

                foreach (MethodInfo m in decl.GetExtensionMethods(comp!) ?? [])
                    embedMethodNames.Add(m.Name);

                // A POINTER embed's box-receiver primaries promote too (see below) — count them for
                // the same cross-embed ambiguity rule. A direct unexported VALUE embed likewise promotes
                // its box-receiver primaries via the descent shim.
                if (pointerEmbedTypeNames.Contains(embedKey(typeName)) || (typeName == promotedStructType && directEmbedIsUnexportedValue))
                {
                    foreach (MethodInfo m in decl.GetBoxReceiverExtensionMethods(comp!))
                        embedMethodNames.Add(m.Name);
                }

                foreach ((string memberType, _, _, bool isEmbedded) in decl.GetStructMembers(comp!, true))
                {
                    if (isEmbedded)
                        countPromotedMethods(memberType, seenTypes);
                }
            }
        }

        foreach ((string promotedStructType, _, _, _) in promotedStructs)
        {
            // Rewrite the embed's type PARAMETERS to this instantiation's type ARGUMENTS on the
            // promoted method's return + parameter types — a generic embed's method signature may
            // carry `Point` (`pointFromAffine` returns `(Point, error)`), out of scope on the
            // non-generic enclosing struct.
            Dictionary<string, string> typeArgMap = GetEmbedTypeArgumentMap(promotedStructType);

            // Collect the embedded struct's methods TRANSITIVELY — its own plus those promoted into
            // it from a deeper embedding level (Go promotes methods through every level). A 2+-level
            // method (e.g. top.greet from inner, via top→mid→inner) is forwarded through the 1-level
            // accessor `target.<promotedStruct>.greet()`, which exists by the embedded struct's own
            // one-level promotion of that method. Closest declaration of a name wins (Go's rule).
            List<MethodInfo> promotedStructMethods = [];
            HashSet<string> promotedMethodNames = new(StringComparer.Ordinal);

            // See the identical computation in the ambiguity-counting pass above.
            bool directEmbedIsUnexportedValue = !promotedStructType.Contains("<") &&
                GetScope(GetSimpleName(promotedStructType, dropCollisionPrefix: true)) != "public";

            collectPromotedMethods(promotedStructType, []);

            void collectPromotedMethods(string typeName, HashSet<string> seenTypes)
            {
                // Go forbids embedding cycles, but guard anyway.
                if (!seenTypes.Add(typeName))
                    return;

                (StructDeclarationSyntax? decl, Compilation? comp) = Context.GetStructDeclaration(typeName);

                if (decl is null)
                    return;

                foreach (MethodInfo m in decl.GetExtensionMethods(comp!) ?? [])
                {
                    if (promotedMethodNames.Add(m.Name))
                        promotedStructMethods.Add(m);
                }

                // A POINTER embed's BOX-receiver primaries (`this ж<T>`) promote unchanged — the hop
                // `target.<embed>` is a ж<T>, so the value/pointer forwarders below (`target.<embed>.M(…)`)
                // bind the box receiver directly. GetExtensionMethods above harvests only value-receiver
                // forms, so collect the box primaries here (sha3's cshakeState←*state.Write, CS1929).
                // A direct UNEXPORTED VALUE embed also promotes its box-receiver primaries, but through
                // the box-field descent shim (IsValueEmbedBoxRecv) — its hop `target.<embed>` is a value,
                // not a ж<T>, so the plain forwarder body would be CS1929. Only the direct embed is taken
                // (a deeper value hop would need a multi-level descent this narrow fix does not emit).
                bool valueEmbedBoxRecv = typeName == promotedStructType && directEmbedIsUnexportedValue;

                if (pointerEmbedTypeNames.Contains(embedKey(typeName)) || valueEmbedBoxRecv)
                {
                    foreach (MethodInfo m in decl.GetBoxReceiverExtensionMethods(comp!))
                    {
                        if (promotedMethodNames.Add(m.Name))
                            promotedStructMethods.Add(m with { IsBoxRecv = true, IsValueEmbedBoxRecv = valueEmbedBoxRecv });
                    }
                }

                // Recurse into nested embedded struct fields. The converter emits every embed -
                // value or POINTER - as a `partial ref` PROPERTY, the marker the top level keys
                // isPromotedStruct on; GetStructMembers(..., true) surfaces it as the 4th tuple
                // element. A NAMED field whose name merely equals its type's simple name
                // (`RCode RCode` in dnsmessage.Header) is a plain FIELD, not an embed - recursing
                // into it falsely promotes the field type's methods (Message got RCode's String()
                // forwarded as `target.Header.String()`, but Header has no String - CS1929).
                foreach ((string memberType, _, _, bool isEmbedded) in decl.GetStructMembers(comp!, true))
                {
                    if (isEmbedded)
                        collectPromotedMethods(memberType, seenTypes);
                }
            }

            foreach (MethodInfo method in promotedStructMethods)
            {
                if (structMethodNames.Contains(method.Name))
                {
                    result.Append($"\r\n    // '{GetSimpleName(promotedStructType)}.{method.Name}' method mapped to overridden '{NonGenericStructName}' receiver method");
                    continue;
                }

                if (promotedMethodCounts.TryGetValue(method.Name, out int nameCount) && nameCount > 1)
                {
                    result.Append($"\r\n    // '{GetSimpleName(promotedStructType)}.{method.Name}' promotion is AMBIGUOUS across embeds (Go requires the qualified selector) - not promoted");
                    continue;
                }

                // Add ref extension method
                string methodScope = Scope ?? "public";

                // Downgrade a public forwarder whose return type is LESS accessible than public
                // (CS0051: a public method cannot expose an internal return type). The name-based
                // GetScope heuristic treats every Go-lowercase-named type as unexported — but golib
                // builtins (@string, error, bool, nint, …) are PUBLIC C# types, so a promoted method
                // returning one (testing.common.Name → @string) was wrongly made internal and thus
                // invisible cross-assembly. For a DIRECT UNEXPORTED VALUE embed — the only promotion
                // the converter reaches cross-package as a bare `Ꮡt.M()` (b8764925a dropped the
                // inaccessible `.of(T.Ꮡ<embed>)` descent there), where an internal forwarder lets a
                // same-named FOREIGN extension win (flag.Name → CS1929) — trust the return type's
                // ACTUAL accessibility so a public-but-lowercase return keeps the forwarder public.
                // Every other promotion keeps the conservative name heuristic (no golden/compile churn).
                if (method.ReturnType != "void")
                {
                    bool returnTypeIsPublic = GetScope(GetSimpleName(method.ReturnType)) == "public" ||
                        (directEmbedIsUnexportedValue && method.ReturnTypeIsPublic);

                    if (!returnTypeIsPublic)
                        methodScope = "internal";
                }
                // The shim mirrors the SOURCE receiver kind: a by-value method stays by-value
                // so an RVALUE receiver binds (reflect v.Elem().kind() - CS1510 on forced ref).
                string recvMod = method.IsRefRecv ? "ref " : "";

                // Substituted return + parameter types (identity map for a non-generic embed).
                string returnType = SubstituteTypeParameters(method.ReturnType, typeArgMap);
                string typedParams = string.Join(", ", method.Parameters.Skip(1).Select(param => $"{SubstituteTypeParameters(param.type, typeArgMap)} {param.name}"));

                // A GENERIC enclosing struct promotes its embed's methods as GENERIC extension methods
                // carrying the struct's OWN type parameters — `wrapped<T>` embedding `tag<T>` emits
                // `static T show<T>(this wrapped<T> target)`, else the `T` in the receiver/return is an
                // undefined type name (CS0246). A NON-generic enclosing struct (p256Curve, whose embed
                // is a CONCRETE instantiation with the type argument already substituted in) carries
                // none, keeping the plain forwarder.
                int structTypeParamStart = StructName.IndexOf('<');
                string methodTypeParams = structTypeParamStart >= 0 ? StructName[structTypeParamStart..] : "";

                // A box-receiver (direct-ж) method promoted through an UNEXPORTED VALUE embed cannot use
                // the value hop `target.<embed>` (a value, not a ж<T> — CS1929). Emit a single PUBLIC box
                // shim that descends through the embed's box-field accessor exactly as the in-package
                // caller renders it inline: `M(this ж<T> Ꮡtarget) => Ꮡtarget.of(T.Ꮡ<embed>).M(args)`. The
                // `Ꮡ<embed>` accessor is `internal` (matching the unexported embed) but reachable from this
                // shim's own assembly, so a FOREIGN caller reaches the exported promoted method by the
                // plain `t.M(…)` call the converter now emits cross-package. No `this ref T` overload — a
                // box receiver cannot bind on a value. Restricted to a NON-generic enclosing struct (the
                // `T.Ꮡ<embed>` FieldReferences accessor is not itself generic); a generic one falls through.
                // The shim scope is the shared `methodScope` (the STRUCT's exportedness, downgraded for a
                // non-public return type) — an unexported enclosing struct (context's `afterFuncCtx`,
                // reflect's `structTypeUncommon`) has an internal receiver `ж<T>`, so a `public` shim there
                // is CS0051. It is additionally gated to an EXPORTED promoted method: an unexported method
                // is never reachable across packages (Go forbids it), so it needs no shim and its
                // in-package callers keep the inline descent. So the shim is public only for an EXPORTED
                // method on an EXPORTED struct returning void/public — exactly the reachable-cross-package
                // case (testing.T.Errorf/Helper/Logf).
                if (method.IsValueEmbedBoxRecv)
                {
                    if (structTypeParamStart < 0 && GetScope(GetSimpleName(method.Name)) == "public")
                    {
                        string embedBox = GetUnsanitizedIdentifier(StripTypeArgs(GetSimpleName(promotedStructType, dropCollisionPrefix: true)));

                        result.Append($"\r\n    {methodScope} static {returnType} {method.Name}(this {PointerPrefix}<{StructName}> {AddressPrefix}target");

                        if (method.Parameters.Length > 1)
                        {
                            result.Append(", ");
                            result.Append(typedParams);
                        }

                        result.Append($") => {AddressPrefix}target.of({StructName}.{AddressPrefix}{embedBox}).{method.Name}(");
                        result.Append(string.Join(", ", method.Parameters.Skip(1).Select(param => param.name)));
                        result.Append(");");
                    }

                    continue;
                }

                // A value-receiver method binds on the embed's deref'd value (`target.<embed>.Value` —
                // GetSimpleName appends `.Value` for a pointer embed); a BOX-receiver primary (`this ж<T>`)
                // binds on the box hop itself (`target.<embed>`), so drop the `.Value` for it. StripTypeArgs
                // reduces a GENERIC embed's hop to the bare property name (`nistCurve<…>` → `nistCurve`) —
                // the emitted accessor is not itself generic (a no-op for a non-generic embed's hop).
                string embedAccess = StripTypeArgs(GetSimpleName(promotedStructType, dropCollisionPrefix: true));

                if (method.IsBoxRecv && embedAccess.EndsWith(".Value", StringComparison.Ordinal))
                    embedAccess = embedAccess[..^".Value".Length];

                result.Append($"\r\n    {methodScope} static {returnType} {method.Name}{methodTypeParams}(this {recvMod}{StructName} target");

                if (method.Parameters.Length > 1)
                {
                    result.Append(", ");
                    result.Append(typedParams);
                }

                result.Append($") => target.{embedAccess}.{method.Name}(");
                result.Append(string.Join(", ", method.Parameters.Skip(1).Select(param => param.name)));
                result.Append(");");

                // Add pointer extension method
                result.Append($"\r\n    {methodScope} static {returnType} {method.Name}{methodTypeParams}(this {PointerPrefix}<{StructName}> {AddressPrefix}target");

                if (method.Parameters.Length > 1)
                {
                    result.Append(", ");
                    result.Append(typedParams);
                }

                result.AppendLine(")");
                result.AppendLine("    {");
                result.AppendLine($"        ref var target = ref {AddressPrefix}target.Value;");
                result.Append($"        {(method.ReturnType == "void" ? "" : "return ")}target.{method.Name}(");
                result.Append(string.Join(", ", method.Parameters.Skip(1).Select(param => param.name)));
                result.AppendLine(");");
                result.Append("    }");
            }
        }

        return result.ToString();
    }

    // StripTypeArgs reduces a generic type reference to its bare name (`node<K, V>` → `node`)
    // for MEMBER access through an embed's promoted property.
    private static string StripTypeArgs(string name)
    {
        int index = name.IndexOf('<');
        return index == -1 ? name : name[..index];
    }

    private static readonly Dictionary<string, string> s_noTypeArgs = new(StringComparer.Ordinal);

    // Builds the type-parameter → type-argument substitution for a promoted embed that is a GENERIC
    // INSTANTIATION (`nistCurve<P256PointжnistPoint>`). The embed's field and method signatures are
    // harvested from the generic DECLARATION (`nistCurve<Point>`), so they reference the declaration's
    // type PARAMETER names (`Point`); those must be rewritten to the instantiation's type ARGUMENTS
    // (`P256PointжnistPoint`) before promotion onto the enclosing struct — otherwise a promoted
    // accessor/forwarder references an out-of-scope `Point` (CS0246). Empty for a non-generic embed.
    private Dictionary<string, string> GetEmbedTypeArgumentMap(string promotedStructType)
    {
        List<string> typeArguments = ParseTopLevelTypeArguments(promotedStructType);

        if (typeArguments.Count == 0)
            return s_noTypeArgs;

        (StructDeclarationSyntax? decl, _) = Context.GetStructDeclaration(promotedStructType);
        TypeParameterListSyntax? typeParameterList = decl?.TypeParameterList;

        if (typeParameterList is null || typeParameterList.Parameters.Count != typeArguments.Count)
            return s_noTypeArgs;

        Dictionary<string, string> map = new(StringComparer.Ordinal);

        for (int i = 0; i < typeArguments.Count; i++)
            map[typeParameterList.Parameters[i].Identifier.Text] = typeArguments[i];

        return map;
    }

    // Splits the OUTERMOST `<…>` of a generic type reference into its top-level type arguments,
    // tracking nesting so an inner comma stays with its argument (`Foo<Bar<A, B>, C>` → [`Bar<A, B>`, `C`]).
    private static List<string> ParseTopLevelTypeArguments(string typeName)
    {
        int lt = typeName.IndexOf('<');

        if (lt < 0 || !typeName.EndsWith(">"))
            return [];

        string inner = typeName[(lt + 1)..^1];
        List<string> arguments = [];
        int depth = 0;
        int start = 0;

        for (int i = 0; i < inner.Length; i++)
        {
            switch (inner[i])
            {
                case '<':
                    depth++;
                    break;
                case '>':
                    depth--;
                    break;
                case ',' when depth == 0:
                    arguments.Add(inner[start..i].Trim());
                    start = i + 1;
                    break;
            }
        }

        string last = inner[start..].Trim();

        if (last.Length > 0)
            arguments.Add(last);

        return arguments;
    }

    // Rewrites whole-identifier occurrences of a promoted generic embed's type PARAMETERS to the
    // instantiation's type ARGUMENTS in an emitted type string (`global::System.Func<Point>` →
    // `global::System.Func<P256PointжnistPoint>`). Scans identifier tokens so a substring match
    // never fires (a `Point` map entry leaves `P256PointжnistPoint` untouched); the ж glyph is a
    // Unicode letter, so a marker-bearing argument reads as a single identifier.
    private static string SubstituteTypeParameters(string typeName, Dictionary<string, string> typeArgumentMap)
    {
        if (typeArgumentMap.Count == 0 || string.IsNullOrEmpty(typeName))
            return typeName;

        StringBuilder result = new(typeName.Length);
        int i = 0;

        while (i < typeName.Length)
        {
            char c = typeName[i];

            if (char.IsLetter(c) || c == '_')
            {
                int start = i++;

                while (i < typeName.Length && (char.IsLetterOrDigit(typeName[i]) || typeName[i] == '_'))
                    i++;

                string identifier = typeName[start..i];
                result.Append(typeArgumentMap.TryGetValue(identifier, out string? replacement) ? replacement : identifier);
            }
            else
            {
                result.Append(c);
                i++;
            }
        }

        return result.ToString();
    }

    private string FieldReferences
    {
        get
        {
            if (StructMembers.Count == 0)
                return $"// -- {NonGenericStructName} has no defined fields";

            StringBuilder result = new();

            foreach ((string typeName, string memberName, _, _) in StructMembers)
            {
                // A blank `_` field is unaddressable in Go; a second `_` field (own + promoted, or
                // multiple padding fields) would also make duplicate `Ꮡ_` accessors (CS0111).
                if (GetSimpleName(memberName) == "_")
                    continue;

                if (result.Length > 0)
                    result.Append($"\r\n{TypeElemIndent}");

                // The box-field accessor's accessibility must match the FIELD's, not the field type's:
                // an EXPORTED field (`Fun`) is addressable cross-package, so `ᏑFun` must be `public`
                // even when its type's simple name is lowercase (`array<nuint>` → GetScope would give
                // `internal`, making `other.of(ITab.ᏑFun)` unreachable, CS0117). Derive the scope from
                // the member name (its exportedness), as the field declaration itself does.
                string fieldScope = GetScope(GetSimpleName(memberName));
                result.Append($"{fieldScope} static ref {typeName} {AddressPrefix}{GetUnsanitizedIdentifier(memberName)}(ref {StructName} instance) => ref instance.{memberName};");
            }

            return result.ToString();
        }
    }

    private string Constructors
    {
        get
        {
            StringBuilder result = new();

            // Construct from nil: field initializers already ran (C# executes them in every explicitly
            // declared constructor) and C# 11 auto-defaults any field the body leaves unassigned, so
            // plain members need no assignment here — re-assigning `default!` would NULL an array
            // field's `= new(N)` backing (a `[N]T` field of `S{}` NREd on first index). Only the
            // promoted-embed boxes need construction: they are readonly `ж<T>` fields with no
            // initializer, so a box exists only when a constructor allocates it.
            result.AppendLine($"public {NonGenericStructName}(NilType _)");
            result.AppendLine($"{TypeElemIndent}{{");
            AppendPromotedBoxInitializers(result);
            result.AppendLine($"{TypeElemIndent}}}");

            // Parameterless constructor so C# RUNS the struct's field initializers — most importantly an
            // array field's `= new(N)`, which gives a Go `[N]T` field its fixed length (its backing T[]).
            // Without an EXPLICITLY declared parameterless constructor, `new S()` uses the implicit struct
            // constructor, which zeroes every field and SKIPS field initializers — leaving an array field's
            // backing null, so indexing/`len` on it throws NullReferenceException. (C# 11 auto-defaults any
            // field lacking an initializer; a slice/map/etc. field — which has no `= new(N)` initializer —
            // stays its nil zero value, matching Go.) The promoted-embed boxes are allocated here too, so
            // `new S()` — the zero value golib's `@new<T>()`/`heap()` materialize — is fully usable.
            result.AppendLine();
            result.AppendLine($"{TypeElemIndent}public {NonGenericStructName}()");
            result.AppendLine($"{TypeElemIndent}{{");
            AppendPromotedBoxInitializers(result);
            result.AppendLine($"{TypeElemIndent}}}");

            // Generate exported constructor from public fields. When an ALL-fields internal
            // ctor follows (mixed-visibility struct), a same-assembly named-args call matching
            // the public subset is ambiguous between the two all-optional overloads (CS0121,
            // os fileStat) - deprioritize the subset so same-assembly calls bind the full ctor;
            // cross-assembly callers never see the internal one, so resolution there is unaffected.
            if (PublicStructMembers.Count != StructMembers.Count && PublicStructMembers.Count > 0)
                result.AppendLine($"{TypeElemIndent}[global::System.Runtime.CompilerServices.OverloadResolutionPriority(-1)]");

            GenerateConstructor("public", PublicStructMembers, result);

            // Generate internal constructor with all fields
            if (PublicStructMembers.Count != StructMembers.Count)
            {
                result.AppendLine();
                GenerateConstructor("internal", StructMembers, result);
            }

            return result.ToString();
        }
    }

    // Allocates the readonly `ж<T>` box of every promoted embed with a nil-constructed value —
    // shared by the NilType and parameterless constructors (the parameterized constructors box
    // their incoming member values instead).
    private void AppendPromotedBoxInitializers(StringBuilder result)
    {
        foreach ((string typeName, string memberName, _, bool isPromotedStruct) in StructMembers)
        {
            if (!isPromotedStruct)
                continue;

            result.Append($"{TypeElemIndent}    ");
            // A keyword-named embed composes the box field from the UNESCAPED member name
            // ('@' is only valid leading an identifier - 'Ꮡʗ@base' is CS1002).
            result.AppendLine($"{AddressPrefix}{CapturedVarMarker}{GetUnsanitizedIdentifier(memberName)} = new {PointerPrefix}<{typeName}>(new {typeName}(nil));");
        }
    }

    private void GenerateConstructor(string scope, List<(string typeName, string memberName, bool isReferenceType, bool isPromotedStruct)> structMembers, StringBuilder result)
    {
        if (structMembers.Count == 0)
            return;

        result.AppendLine();
        result.Append($"{TypeElemIndent}{scope} {NonGenericStructName}(");
        result.Append(string.Join(", ", structMembers.Select(item => $"{item.typeName} {item.memberName} = default!")));
        result.AppendLine(")");
        result.AppendLine($"{TypeElemIndent}{{");

        foreach ((string typeName, string memberName, _, bool isPromotedStruct) in structMembers)
        {
            result.Append($"{TypeElemIndent}    ");

            result.AppendLine(isPromotedStruct ?
                $"{AddressPrefix}{CapturedVarMarker}{GetUnsanitizedIdentifier(memberName)} = new {PointerPrefix}<{typeName}>({memberName});" :
                $"this.{memberName} = {memberName};");
        }

        result.Append($"{TypeElemIndent}}}");
    }

    private static string GetToStringImplementation((string typeName, string memberName, bool isReferenceType, bool isPromotedStruct) item)
    {
        return item.isReferenceType ? $"{item.memberName}?.ToString() ?? \"<nil>\"" : $"{item.memberName}.ToString()";
    }

    private string CompareFields => HasEqualityOperators && StructMembers.Count > 0 ? 
        string.Join(" &&\r\n            ", CompareList) :
        StructMembers.Count > 0 ? "false /* missing equality constraints */" : "true /* empty */";

    // Qualify the left operand with `this.` so a field whose name collides with the `Equals`
    // parameter (`other`) compares the field-to-field, not parameter-to-field. e.g. a struct with
    // a field literally named `other` would otherwise emit `other == other.other` — binding the
    // left `other` to the parameter (CS0019). `this.other == other.other` is unambiguous.
    private IEnumerable<string> CompareList => StructMembers.Select(member => $"this.{member.memberName} == other.{member.memberName}");

    public string HashCode => StructMembers.Count == 0 ? "base.GetHashCode()" :
        $"""
        global::go.golib.HashCode.Combine(
                    {ParamList})
        """;

    private string ParamList => string.Join(",\r\n            ", StructMembers.Select(member => member.memberName));
}
