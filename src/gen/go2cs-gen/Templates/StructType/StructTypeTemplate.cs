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

            foreach ((string typeName, string memberName, _, _) in promotedStructs)
            {
                if (result.Length > 0)
                    result.Append($"\r\n{TypeElemIndent}");

                result.Append($"private readonly {PointerPrefix}<{typeName}> {AddressPrefix}{CapturedVarMarker}{memberName};");
            }

            result.Append($"\r\n\r\n{TypeElemIndent}// Promoted Struct Accessors");

            foreach ((string typeName, string memberName, _, _) in promotedStructs)
            {
                string typeScope = GetScope(GetSimpleName(typeName));
                result.Append($"\r\n{TypeElemIndent}{typeScope} partial ref {typeName} {memberName} => ref {AddressPrefix}{CapturedVarMarker}{memberName}.Value;");
            }

            result.Append($"\r\n\r\n{TypeElemIndent}// Promoted Struct Field Accessors");

            foreach ((string promotedStructType, _, _, _) in promotedStructs)
            {
                foreach ((string typeName, string memberName) in getStructMembers(promotedStructType))
                {
                    // A blank `_` field (padding / embedded unexported marker) is never promoted or
                    // selectable in Go; emitting an accessor for it collides with the enclosing
                    // struct's own `_` field (CS0102).
                    if (GetSimpleName(memberName) == "_")
                        continue;

                    string typeScope = GetScope(GetSimpleName(typeName));
                    result.Append($"\r\n{TypeElemIndent}{typeScope} ref {typeName} {memberName} => ref {GetSimpleName(promotedStructType, dropCollisionPrefix: true)}.{memberName};");
                }
            }

            result.Append($"\r\n\r\n{TypeElemIndent}// Promoted Struct Field Accessor References");

            foreach ((string promotedStructType, _, _, _) in promotedStructs)
            {
                foreach ((string typeName, string memberName) in getStructMembers(promotedStructType))
                {
                    // Blank `_` field — unaddressable in Go, and its `Ꮡ_` would collide with the
                    // enclosing struct's own `Ꮡ_` (CS0111).
                    if (GetSimpleName(memberName) == "_")
                        continue;

                    // The Ꮡ-prefixed accessor NAME must use the unescaped member name — a C#-keyword
                    // field is escaped `@base`, but `Ꮡ@base` is invalid ('@' only leads). The member
                    // ACCESS on the right keeps `@base`.
                    string typeScope = GetScope(GetSimpleName(typeName));
                    result.Append($"\r\n{TypeElemIndent}{typeScope} static ref {typeName} {AddressPrefix}{GetUnsanitizedIdentifier(memberName)}(ref {NonGenericStructName} instance) => ref instance.{GetSimpleName(promotedStructType, dropCollisionPrefix: true)}.{memberName};");
                }
            }

            return result.ToString();

            IEnumerable<(string typeName, string memberName)> getStructMembers(string structTypeName)
            {
                // Collect the embedded struct's members TRANSITIVELY: a member promoted into the
                // enclosing struct may itself come from a NESTED embedded struct (Go promotes through
                // every embedding level). e.g. stackWorkBuf embeds stackWorkBufHdr embeds workbufhdr,
                // so stackWorkBuf.nobj must promote workbufhdr's nobj — but reading stackWorkBufHdr's
                // DECLARED members alone misses it (nobj is a generated accessor on stackWorkBufHdr,
                // not a declared field). The emitted accessor stays single-hop (`stackWorkBuf.nobj =>
                // ref stackWorkBufHdr.nobj`), resolving through stackWorkBufHdr's own 1-level promotion.
                List<(string typeName, string memberName)> collected = [];
                HashSet<string> emittedNames = [];

                collect(structTypeName, []);

                return collected;

                void collect(string typeName, HashSet<string> seenTypes)
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
                                collected.Add((fieldTypeName, fieldName));
                        }

                        return;
                    }

                    foreach ((string memberType, string memberName, _, _) in structDecl.GetStructMembers(compilation!, true))
                    {
                        // First (closest) declaration of a name wins, matching Go's promotion rules.
                        if (emittedNames.Add(memberName))
                            collected.Add((memberType, memberName));

                        // An embedded struct field (Go embedding: the field name equals its type's
                        // simple name) contributes its own members transitively. A POINTER embed
                        // (`*traceBuf` → field type `ж<traceBuf>`) is an embed too, so compare against
                        // the dereferenced type name — `GetSimpleName` appends `.Value` for a pointer,
                        // which would never equal the bare field name.
                        if (GetSimpleName(GeneratorExecutionContextExtensions.GetUnderlyingTypeName(memberType)) == memberName)
                            collect(memberType, seenTypes);
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

        foreach ((string promotedStructType, _, _, _) in promotedStructs)
        {
            // Collect the embedded struct's methods TRANSITIVELY — its own plus those promoted into
            // it from a deeper embedding level (Go promotes methods through every level). A 2+-level
            // method (e.g. top.greet from inner, via top→mid→inner) is forwarded through the 1-level
            // accessor `target.<promotedStruct>.greet()`, which exists by the embedded struct's own
            // one-level promotion of that method. Closest declaration of a name wins (Go's rule).
            List<MethodInfo> promotedStructMethods = [];
            HashSet<string> promotedMethodNames = new(StringComparer.Ordinal);

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

                // Recurse into nested embedded struct fields (field name == type simple name). A
                // POINTER embed (`*traceBuf` → `ж<traceBuf>`) is an embed too — compare against the
                // dereferenced type name (`GetSimpleName` appends `.Value` for a pointer).
                foreach ((string memberType, string memberName, _, _) in decl.GetStructMembers(comp!, true))
                {
                    if (GetSimpleName(GeneratorExecutionContextExtensions.GetUnderlyingTypeName(memberType)) == memberName)
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

                // Add ref extension method
                string methodScope = Scope ?? "public";
                methodScope = method.ReturnType == "void" ? methodScope : GetScope(GetSimpleName(method.ReturnType)) == "public" ? methodScope : "internal";
                result.Append($"\r\n    {methodScope} static {method.ReturnType} {method.Name}(this ref {StructName} target");

                if (method.Parameters.Length > 1)
                {
                    result.Append(", ");
                    result.Append(string.Join(", ", method.Parameters.Skip(1).Select(param => $"{param.type} {param.name}")));
                }
                
                result.Append($") => target.{GetSimpleName(promotedStructType, dropCollisionPrefix: true)}.{method.Name}(");
                result.Append(string.Join(", ", method.Parameters.Skip(1).Select(param => param.name)));
                result.Append(");");

                // Add pointer extension method
                result.Append($"\r\n    {methodScope} static {method.ReturnType} {method.Name}(this {PointerPrefix}<{StructName}> {AddressPrefix}target");

                if (method.Parameters.Length > 1)
                {
                    result.Append(", ");
                    result.Append(string.Join(", ", method.Parameters.Skip(1).Select(param => $"{param.type} {param.name}")));
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

            result.AppendLine($"public {NonGenericStructName}(NilType _)");
            result.AppendLine($"{TypeElemIndent}{{");

            // Construct from nil
            foreach ((string typeName, string memberName, _, bool isPromotedStruct) in StructMembers)
            {
                result.Append($"{TypeElemIndent}    ");

                result.AppendLine(isPromotedStruct ? 
                    $"{AddressPrefix}{CapturedVarMarker}{memberName} = new {PointerPrefix}<{typeName}>(new {typeName}(nil));" : 
                    $"this.{memberName} = default!;");
            }

            result.AppendLine($"{TypeElemIndent}}}");

            // Parameterless constructor so C# RUNS the struct's field initializers — most importantly an
            // array field's `= new(N)`, which gives a Go `[N]T` field its fixed length (its backing T[]).
            // Without an EXPLICITLY declared parameterless constructor, `new S()` uses the implicit struct
            // constructor, which zeroes every field and SKIPS field initializers — leaving an array field's
            // backing null, so indexing/`len` on it throws NullReferenceException. (C# 11 auto-defaults any
            // field lacking an initializer, so the empty body is sufficient and correct; a slice/map/etc.
            // field — which has no `= new(N)` initializer — stays its nil zero value, matching Go.)
            result.AppendLine();
            result.AppendLine($"{TypeElemIndent}public {NonGenericStructName}()");
            result.AppendLine($"{TypeElemIndent}{{");
            result.AppendLine($"{TypeElemIndent}}}");

            // Generate exported constructor from public fields
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
                $"{AddressPrefix}{CapturedVarMarker}{memberName} = new {PointerPrefix}<{typeName}>({memberName});" :
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
