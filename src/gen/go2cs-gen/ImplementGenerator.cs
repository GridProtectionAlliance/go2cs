//******************************************************************************************************
//  ImplementGenerator.cs - Gbtc
//
//  Copyright © 2025, Grid Protection Alliance.  All Rights Reserved.
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
//  01/16/2025 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

//#define DEBUG_GENERATOR

using System;
using System.Collections.Generic;
using System.Linq;
using go2cs.Templates.InterfaceImpl;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static go2cs.Common;
using static go2cs.Symbols;

#if DEBUG_GENERATOR
using System.Diagnostics;
#endif

namespace go2cs;

[Generator]
public class ImplementGenerator : ISourceGenerator
{
    private const string Namespace = "go";
    private const string AttributeName = "GoImplement";
    private const string FullAttributeName = $"{Namespace}.{AttributeName}Attribute<TStruct, TInterface>";

    public void Initialize(GeneratorInitializationContext context)
    {
    #if DEBUG_GENERATOR
        if (!Debugger.IsAttached)
            Debugger.Launch();
    #endif

        // Register to find "GoImplementAttribute" on assembly attribute declarations
        context.RegisterForSyntaxNotifications(() => new AssemblyAttributeFinder(FullAttributeName));
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not AssemblyAttributeFinder { HasAttributes: true } attributeFinder)
            return;

        // Roslyn hintNames are case-insensitive; Go type names differing only by case are legal
        // and common, so disambiguate like the other generators (a collision throws and suppresses
        // ALL interface implementations for the package).
        HashSet<string> emittedHintNames = new(StringComparer.OrdinalIgnoreCase);

        // A (struct, interface) pair can carry Pointer and Promoted on SEPARATE attribute
        // instances (the converter records the ж-form and the embed-promotion independently);
        // the pointer-adapter emission needs to know the pair is promoted, so pre-index.
        HashSet<string> promotedPairs = new(StringComparer.Ordinal);

        // Emit ONE value-form implementation per (struct, interface) pair: the converter can
        // record the same pair both plain AND Promoted (archive/tar's lifted anon struct got
        // a promotion record from its interface embed and a plain record from the conversion
        // site) — the second partial re-emitted the comparison operators (CS0111 ×8). The
        // promotedPairs pre-index already folds the Promoted flag into the first emission.
        HashSet<string> emittedValuePairs = new(StringComparer.Ordinal);

        foreach ((AttributeSyntax attributeSyntax, GeneratorSyntaxContext syntaxContext, _, _) in attributeFinder.TargetAttributes)
        {
            (string name, string value)[] arguments = attributeSyntax.GetArgumentValues();

            if (!bool.Parse(arguments.FirstOrDefault(arg => arg.name.Equals("Promoted")).value?.Trim() ?? "false"))
                continue;

            (ITypeSymbol? structType, ITypeSymbol? interfaceType) = attributeSyntax.Get2GenericTypeArguments(syntaxContext);

            if (structType is not null && interfaceType is not null)
                promotedPairs.Add($"{structType.ToDisplayString()}|{interfaceType.ToDisplayString()}");
        }

        foreach ((AttributeSyntax attributeSyntax, GeneratorSyntaxContext syntaxContext, CompilationUnitSyntax compilationUnit, FileScopedNamespaceDeclarationSyntax? namespaceSyntax) in attributeFinder.TargetAttributes)
        {
            SyntaxTree syntaxTree = attributeSyntax.SyntaxTree;
            SemanticModel semanticModel = context.Compilation.GetSemanticModel(syntaxTree);

            string packageNamespace = GetNamespace(namespaceSyntax) ?? Namespace;
            string packageClassName = GetFirstClassName(compilationUnit) ?? throw new MissingMemberException($"No package class found in same file as [assembly: {AttributeName}]");
            string packageName = packageClassName.EndsWith("_package") ? packageClassName[..^8] : packageClassName;
            
            string[] usingStatements = GetFullyQualifiedUsingStatements(syntaxTree, semanticModel);

            // Extract generic type arguments from "GoImplementAttribute"
            (ITypeSymbol? structType, ITypeSymbol? interfaceType) = attributeSyntax.Get2GenericTypeArguments(syntaxContext);
            
            if (structType is null || interfaceType is null)
                throw new InvalidOperationException($"Invalid usage of [assembly: {AttributeName}] attribute, must specify two generic type arguments.");

            // A NAMED FUNC type with methods (flag's `type funcValue func(string) error`
            // implementing Value) arrives as a DELEGATE — it cannot be a partial struct, so
            // it routes to the VALUE adapter below. Anything else non-struct is a converter
            // bug worth failing loudly on — but NOT by throwing, which kills the entire
            // generator run for the package (flag lost all 11 of its adapters, CS0246 ×19).
            if (structType.TypeKind != TypeKind.Struct && structType.TypeKind != TypeKind.Delegate)
                continue;

            if (interfaceType.TypeKind != TypeKind.Interface)
                throw new InvalidOperationException($"Invalid usage of [assembly: {AttributeName}] attribute, second generic type argument must be an interface.");

            string structName = structType.GetFullTypeName();
            string interfaceName = GlobalQualify(interfaceType.GetFullTypeName(true));

            // Get the attribute's Promoted / Pointer argument values, if defined
            (string name, string value)[] arguments = attributeSyntax.GetArgumentValues();
            bool promoted = bool.Parse(arguments.FirstOrDefault(arg => arg.name.Equals("Promoted")).value?.Trim() ?? "false");
            bool pointer = bool.Parse(arguments.FirstOrDefault(arg => arg.name.Equals("Pointer")).value?.Trim() ?? "false");

            // Get all extension methods for the struct, any directly defined receivers
            // take precedence over promoted interface methods that have the same name
            (StructDeclarationSyntax? structDecl, Compilation? compilation) = context.GetStructDeclaration(structType.ToDisplayString());
            IEnumerable<MethodInfo>? structMethods = structDecl is null ? [] : structDecl.GetExtensionMethods(compilation!);
            HashSet<string> overrides = new(structMethods?.Select(method => method.Name) ?? [], StringComparer.Ordinal);

            List<MethodInfo> methods = interfaceType.AllInterfaces
                .Concat([interfaceType]) // Include the original interface
                .SelectMany(iface => iface.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(method => method.MethodKind == MethodKind.Ordinary)
                    .Where(method => !method.IsStatic)
                    .Select(method => (name: iface.ToDisplayString(), method)))
                .Select(info => new MethodInfo
                {
                    Name = promoted && !pointer && !overrides.Contains(GetSimpleName(info.method.Name)) ? info.method.Name : $"{GlobalQualify(info.name)}.{info.method.Name}",
                    ReturnType = GlobalQualify(info.method.ReturnType.ToDisplayString()),
                    // Carry the parameter REF KIND: an interface member declared with an `in`
                    // param (the hand-finished io stub's Reader.Read(in slice<byte>)) is a
                    // distinct signature - an explicit impl without it is CS0539.
                    Parameters = info.method.Parameters.Select(param => (type: $"{RefKindPrefix(param.RefKind)}{GlobalQualify(param.Type.ToDisplayString())}", name: param.Name)).ToArray(),
                    GenericTypes = string.Join(", ", info.method.TypeParameters.Select(type => type.ToDisplayString())),
                    TypeConstraints = info.method.TypeParameters.ToDictionary(type => type.Name, type => type.ConstraintTypes.Select(constraint => constraint.ToDisplayString()).ToArray())
                })
                .Distinct()
                .ToList();

            // The interface may inherit System.IFormattable (the hand-finished io stub's
            // Reader does, for the dyn machinery): its ToString(format, provider) cannot
            // forward through the box or an uncast struct (CS1501/CS0030) — the templates
            // emit a canned explicit impl instead when flagged.
            bool implementsFormattable = methods.RemoveAll(m => m.Name.StartsWith("System.IFormattable.")) > 0;

            // An interface member with NO direct struct method may be satisfied by Go method
            // promotion through an embedded POINTER field (`type rtype struct { *abi.Type }`).
            // That promotion is syntax-resolved at Go call sites (the converter emits the hop
            // `t.Type.Value.M()`), so the explicit interface implementation must forward through
            // the same hop — `this.M()` has nothing to bind (CS1929). Gated to a SINGLE hop
            // (Go's promotion ambiguity rules make multi-embed satisfaction rare; extend when
            // the corpus surfaces one).
            List<(string Name, string TypeName)> embedHops = structDecl?.GetEmbeddedPointerHopNames() ?? [];
            string? embedHop = embedHops.Count == 1 ? embedHops[0].Name : null;

            // Hop-target methods that are direct-ж primaries bind on the box FIELD itself
            // (`this.File.Read(p)`) — deref'ing first strands the extension receiver (CS1929).
            HashSet<string> embedHopBoxMethods = embedHops.Count == 1
                ? StructDeclarationSyntaxExtensions.GetBoxReceiverMethodNames(embedHops[0].TypeName, syntaxContext.SemanticModel.Compilation)
                : [];

            // A hop-type method may be declared DEEPER - on a VALUE-embedded field of the hop
            // type with a POINTER receiver (net's tcpConnWithoutWriteTo embeds *TCPConn; TCPConn
            // embeds conn by VALUE; Read/Write live on zh<conn>). `this.TCPConn.Value.Read(p)`
            // strands the extension receiver (CS1929 x2); project the field's box through the
            // generated ref accessor instead: `this.TCPConn.of(TCPConn.Rconn).Read(p)`.
            Dictionary<string, string> embedHopDeepPaths = new(StringComparer.Ordinal);

            if (embedHops.Count == 1)
            {
                (StructDeclarationSyntax? hopDecl, Compilation? hopCompilation) = context.GetStructDeclaration(embedHops[0].TypeName);

                if (hopDecl is not null && hopCompilation is not null)
                {
                    string hopSimpleName = GetSimpleName(embedHops[0].TypeName);

                    foreach ((string fieldName, string fieldTypeName) in hopDecl.GetEmbeddedValueHopNames())
                    {
                        foreach (string deepMethod in StructDeclarationSyntaxExtensions.GetBoxReceiverMethodNames(fieldTypeName, hopCompilation))
                        {
                            if (!embedHopBoxMethods.Contains(deepMethod) && !embedHopDeepPaths.ContainsKey(deepMethod))
                                embedHopDeepPaths[deepMethod] = $".of({packageClassName}.{hopSimpleName}.Ꮡ{fieldName})";
                        }
                    }
                }
            }

            if (pointer)
            {
                // A POINTER-sourced interface cast (`var s Iface = &t`): the interface value must
                // alias the receiver box (Go's interface holds the *T), so emit the IжAdapter
                // wrapper instead of the value-boxing partial struct — which copies, and cannot
                // bind direct-ж receiver methods from a struct's `this` at all (CS1929).
                Dictionary<string, string> forwardReceivers = new(StringComparer.Ordinal);
                Dictionary<string, string> forwardStaticCalls = new(StringComparer.Ordinal);

                foreach (MethodInfo structMethod in structMethods ?? [])
                {
                    // A [GoRecv] ref extension has a RecvGenerator ж-twin that binds on the box;
                    // a plain value-receiver method needs the deref'd value (Go copies at the call).
                    forwardReceivers[structMethod.Name] = structMethod.IsRefRecv ? "m_box" : "m_box.Value";
                }

                foreach (string boxReceiverName in structDecl?.GetBoxReceiverMethodNames(compilation!) ?? [])
                    forwardReceivers[boxReceiverName] = "m_box"; // direct-ж primary form binds the box itself

                // A FOREIGN struct (no local declaration — the pair is recorded locally because
                // the defining package never converts it: os never casts *File to io.Reader, so
                // fmt's Fscan(os.Stdin, …) has no exported adapter to reference — CS1503). Bind
                // forwarding from METADATA: the compiled assembly exposes every converter and
                // sibling-generator form as real symbols, so an extension on ж<T> binds the box
                // and everything else binds the deref'd value (ref extensions bind through the
                // ref-returning .Value).
                bool foreignStruct = structDecl is null && !SymbolEqualityComparer.Default.Equals(structType.ContainingAssembly, syntaxContext.SemanticModel.Compilation.Assembly);

                if (foreignStruct && structType.ContainingType is INamedTypeSymbol packageClass)
                {
                    HashSet<string> boxBound = new(StringComparer.Ordinal);
                    HashSet<string> refBound = new(StringComparer.Ordinal);

                    foreach (IMethodSymbol method in packageClass.GetMembers().OfType<IMethodSymbol>())
                    {
                        if (!method.IsStatic || method.Parameters.Length == 0)
                            continue;

                        // Only a PUBLIC zh-extension binds cross-assembly — the RecvGenerator
                        // twins of unexported methods are internal, visible to this METADATA
                        // scan but not to the consuming compilation (elf's zstd_ReaderzhReader
                        // forwarded m_box.Read to zstd's internal twin, CS1929).
                        if (method.DeclaredAccessibility == Accessibility.Public &&
                            method.Parameters[0].Type is INamedTypeSymbol recvType &&
                            recvType.Name == "ж" &&
                            recvType.TypeArguments.Length == 1 &&
                            SymbolEqualityComparer.Default.Equals(recvType.TypeArguments[0], structType))
                        {
                            boxBound.Add(method.Name);
                        }

                        // A [GoRecv] ref extension called STATICALLY needs the ref keyword.
                        if (method.DeclaredAccessibility == Accessibility.Public &&
                            method.Parameters[0].RefKind == RefKind.Ref &&
                            SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, structType))
                        {
                            refBound.Add(method.Name);
                        }
                    }

                    // A foreign package class in ANOTHER namespace segment (zstd_package lives
                    // in go.@internal) is not imported by the generated file, so its extensions
                    // are invisible to extension-method lookup (CS1929, elf's zstd_ReaderzhReader)
                    // — forward via the package-class STATIC call instead.
                    string emittingNamespace = packageNamespace;
                    string foreignNamespace = packageClass.ContainingNamespace?.ToDisplayString() ?? "";
                    string? staticClass = null;

                    if (!string.Equals(foreignNamespace, emittingNamespace, StringComparison.Ordinal))
                        staticClass = packageClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                    foreach (MethodInfo method in methods)
                    {
                        string simpleName = GetSimpleName(method.Name);
                        bool viaBox = boxBound.Contains(simpleName);

                        if (staticClass is not null)
                        {
                            string recvArg = viaBox ? "m_box" : refBound.Contains(simpleName) ? "ref m_box.Value" : "m_box.Value";
                            forwardStaticCalls[simpleName] = $"{staticClass}.{simpleName}";
                            forwardReceivers[simpleName] = recvArg;
                        }
                        else
                        {
                            forwardReceivers[simpleName] = viaBox ? "m_box" : "m_box.Value";
                        }
                    }
                }

                // Interface members with NO struct method forward through a single embedded-pointer
                // hop, mirroring the value-form template (Go method promotion through `*abi.Type`).
                if (embedHops.Count == 1)
                {
                    foreach (MethodInfo method in methods)
                    {
                        string simpleName = GetSimpleName(method.Name);

                        if (!forwardReceivers.ContainsKey(simpleName))
                        {
                            if (embedHopDeepPaths.TryGetValue(simpleName, out string? deepPath))
                                forwardReceivers[simpleName] = $"m_box.Value.{embedHop}{deepPath}";
                            else
                                forwardReceivers[simpleName] = embedHopBoxMethods.Contains(simpleName) ? $"m_box.Value.{embedHop}" : $"m_box.Value.{embedHop}.Value";
                        }
                    }
                }

                // Interface members may also promote through an embedded INTERFACE field —
                // zip's `type nopCloser struct { io.Writer }`: Write lives on the FIELD's
                // interface value (Go promotes its method set), Close on the struct. Forward
                // still-unbound members that the field's interface declares through the field:
                // `m_box.Value.Writer.Write(…)`. Semantic detection (field name equals its
                // interface type's simple name) — the converter emits embeds as real fields,
                // so the symbol sees them. Gated to a SINGLE embedded interface field.
                IFieldSymbol[] embeddedIfaceFields = structType.GetMembers()
                    .OfType<IFieldSymbol>()
                    .Where(field => !field.IsStatic && field.Type.TypeKind == TypeKind.Interface && field.Name == field.Type.Name)
                    .ToArray();

                if (embeddedIfaceFields.Length == 1 && embeddedIfaceFields[0].Type is INamedTypeSymbol ifaceFieldType)
                {
                    string fieldName = embeddedIfaceFields[0].Name;

                    HashSet<string> fieldMembers = new(ifaceFieldType.AllInterfaces
                        .Concat([ifaceFieldType])
                        .SelectMany(iface => iface.GetMembers().OfType<IMethodSymbol>())
                        .Select(method => method.Name), StringComparer.Ordinal);

                    foreach (MethodInfo method in methods)
                    {
                        string simpleName = GetSimpleName(method.Name);

                        if (!forwardReceivers.ContainsKey(simpleName) && fieldMembers.Contains(simpleName))
                            forwardReceivers[simpleName] = $"m_box.Value.{fieldName}";
                    }
                }

                // Interface members may instead promote through embedded VALUE struct(s) whose
                // methods are pointer-receiver extensions — dwarf's `type VoidType struct
                // { CommonType }` with `func (c *CommonType) Common()`, including CHAINED embeds
                // (`UintType → BasicType → CommonType`) — CS1929 ×18. The TypeGenerator heap-boxes
                // each embedded field with a public static ref accessor, so the adapter projects
                // the receiver box hop by hop onto the field's box:
                // `m_box.of(UintType.ᏑBasicType).of(BasicType.ᏑCommonType).Common()`. Direct-ж
                // methods bind on the projected box; everything else binds through its deref'd
                // .Value (ref extensions bind on the ref-returning Value, matching the pointer-hop
                // dichotomy above). Each level follows a SINGLE value embed (Go's promotion
                // ambiguity rules make multi-embed satisfaction rare), bounded to 4 hops.
                foreach (MethodInfo method in methods)
                {
                    string methodName = GetSimpleName(method.Name);

                    if (forwardReceivers.ContainsKey(methodName))
                        continue;

                    string receiver = "m_box";
                    StructDeclarationSyntax? currentDecl = structDecl;
                    string currentTypeName = structName;

                    for (int depth = 0; depth < 4 && currentDecl is not null; depth++)
                    {
                        List<(string Name, string TypeName)> valueEmbedHops = currentDecl.GetEmbeddedValueHopNames();

                        if (valueEmbedHops.Count != 1)
                            break;

                        (string embedName, string embedTypeName) = valueEmbedHops[0];

                        receiver = $"{receiver}.of({currentTypeName}.{AddressPrefix}{embedName})";
                        (currentDecl, Compilation? embedCompilation) = context.GetStructDeclaration(embedTypeName);
                        currentTypeName = embedTypeName;

                        if (StructDeclarationSyntaxExtensions.GetBoxReceiverMethodNames(embedTypeName, syntaxContext.SemanticModel.Compilation).Contains(methodName))
                        {
                            forwardReceivers[methodName] = receiver;
                            break;
                        }

                        if (currentDecl is not null && embedCompilation is not null &&
                            currentDecl.GetExtensionMethods(embedCompilation).Any(m => GetSimpleName(m.Name) == methodName))
                        {
                            forwardReceivers[methodName] = $"{receiver}.Value";
                            break;
                        }
                    }
                }

                // PROMOTED members (an embedded INTERFACE field — sort's `type reverse struct
                // { Interface }`) forward through the interface field itself, mirroring the
                // value-form template's promoted arm (`m_box.Len()` has nothing to bind — CS1929).
                // The Promoted flag may live on the pair's SIBLING attribute instance.
                if (promoted || promotedPairs.Contains($"{structType.ToDisplayString()}|{interfaceType.ToDisplayString()}"))
                {
                    string interfaceFieldName = GetSimpleName(interfaceName);

                    foreach (MethodInfo method in methods)
                    {
                        string simpleName = GetSimpleName(method.Name);

                        if (!forwardReceivers.ContainsKey(simpleName))
                            forwardReceivers[simpleName] = $"m_box.Value.{interfaceFieldName}";
                    }
                }

                // STRUCT scope by name-exportedness OR declared syntax modifier (the TypeGenerator's
                // `public partial` is a SIBLING generator's output - invisible to this one's symbol,
                // the single-pass limitation); INTERFACE scope by symbol:
                // the golib `error` interface is lowercase yet PUBLIC (its symbol comes from
                // METADATA, so DeclaredAccessibility is reliable), and the name heuristic
                // made io/fs's PathErrorжerror internal - unreachable from os (CS0122 x40).
                // Interface side is symbol-OR-name for the same reason: a SAME-assembly interface
                // (CrossPkgLib.Reporter) gets its public modifier from a sibling generator too.
                string adapterScope = (GetScope(structName) == "public" || structType.DeclaredAccessibility == Accessibility.Public) && (interfaceType.DeclaredAccessibility == Accessibility.Public || GetScope(GetSimpleName(interfaceName)) == "public") ? "public" : "internal";

                string adapterSource = new AdapterImplTemplate
                {
                    PackageNamespace = packageNamespace,
                    PackageName = packageName,
                    // FULLY-qualified for a foreign struct (no local using guarantees resolution;
                    // mirrors the value adapter's same-named-type shadow rationale).
                    StructName = foreignStruct ? GlobalQualify(structType.GetFullTypeName(true)) : structName,
                    InterfaceName = interfaceName,
                    // Adapter class name composes with the shared pointer glyph (CatжAnimal) - always
                    // via Symbols.PointerPrefix so a future symbol change follows automatically.
                    // A FOREIGN struct's name is PACKAGE-QUALIFIED (os_FileжReader): two
                    // same-named foreign structs adapting to one interface otherwise compose
                    // a single colliding class (math/big records both bytes.Reader and
                    // strings.Reader against io.ByteScanner - CS0102/CS0111/CS8646). The
                    // package name comes from the containing package class (bytes_package).
                    AdapterName = $"{(foreignStruct ? $"{ForeignPackagePrefix(structType)}{GetSimpleName(structName)}" : structName)}{PointerPrefix}{GetSimpleName(interfaceName)}",
                    AdapterScope = adapterScope,
                    Methods = methods,
                    ForwardReceivers = forwardReceivers,
                    ForwardStaticCalls = forwardStaticCalls,
                    ImplementsFormattable = implementsFormattable,
                    UsingStatements = usingStatements
                }
                .Generate();

                context.AddSource(GetUniqueHintName(emittedHintNames, GetValidFileName($"{packageNamespace}.{packageClassName}.{structName}-{interfaceName}-ptr.g.cs")), adapterSource);
                continue;
            }

            // A FOREIGN struct (no local declaration to partial - it lives in another
            // assembly) with a value conversion takes the VALUE adapter: a class wrapping a
            // COPY (Go value semantics), forwarding through the foreign package's extension
            // methods via the file's usings (os's Signal interface over syscall.Signal -
            // neither assembly can partial the other, exec_posix CS1503). A local NAMED FUNC
            // type (a DELEGATE - flag's funcValue) takes the same route: a delegate cannot
            // be a partial struct, and its Go methods are package extension methods that
            // bind on the wrapped copy.
            if (structType.TypeKind == TypeKind.Delegate ||
                (structDecl is null && !SymbolEqualityComparer.Default.Equals(structType.ContainingAssembly, syntaxContext.SemanticModel.Compilation.Assembly)))
            {
                // Symbol-OR-name on BOTH sides (mirrors the pointer arm): a public adapter
                // whose ctor takes an INTERNAL wrapped type is CS0051 (flag's internal
                // funcValue delegate under the public Value interface).
                string valueAdapterScope = (interfaceType.DeclaredAccessibility == Accessibility.Public || GetScope(GetSimpleName(interfaceName)) == "public") &&
                                           (structType.DeclaredAccessibility == Accessibility.Public || GetScope(GetSimpleName(structName)) == "public") ? "public" : "internal";

                string valueAdapterSource = new ValueAdapterImplTemplate
                {
                    PackageNamespace = packageNamespace,
                    PackageName = packageName,
                    // FULLY-qualified: the bare name resolves to the LOCAL same-named type
                    // inside this package class (os's ΔSignal interface shadowed syscall's
                    // ΔSignal struct - the adapter field/ctor typed the wrong side).
                    StructName = GlobalQualify(structType.GetFullTypeName(true)),
                    InterfaceName = interfaceName,
                    // Composes with Symbols.ValueAdapterInfix - the value sibling of the
                    // PointerPrefix-composed pointer adapters. A FOREIGN struct's name is
                    // PACKAGE-QUALIFIED (syscall_ΔSignalᴠΔSignal), mirroring the pointer arm's
                    // same-simple-name collision guard; a LOCAL delegate stays bare.
                    AdapterName = $"{(structDecl is null && !SymbolEqualityComparer.Default.Equals(structType.ContainingAssembly, syntaxContext.SemanticModel.Compilation.Assembly) ? ForeignPackagePrefix(structType) : "")}{GetSimpleName(structName)}{ValueAdapterInfix}{GetSimpleName(interfaceName)}",
                    AdapterScope = valueAdapterScope,
                    ImplementsFormattable = implementsFormattable,
                    Methods = methods,
                    UsingStatements = usingStatements
                }
                .Generate();

                context.AddSource(GetUniqueHintName(emittedHintNames, GetValidFileName($"{packageNamespace}.{packageClassName}.{structName}-{interfaceName}-val.g.cs")), valueAdapterSource);
                continue;
            }

            // One value-form impl per pair — a plain + Promoted duplicate would re-emit the
            // comparison operators (CS0111); the promotedPairs pre-index folds the flag in.
            if (!emittedValuePairs.Add($"{structType.ToDisplayString()}|{interfaceType.ToDisplayString()}"))
                continue;

            string generatedSource = new InterfaceImplTemplate
            {
                PackageNamespace = packageNamespace,
                PackageName = packageName,
                StructName = structName,
                InterfaceName = interfaceName,
                Promoted = promoted || promotedPairs.Contains($"{structType.ToDisplayString()}|{interfaceType.ToDisplayString()}"),
                Overrides = overrides,
                Methods = methods,
                EmbedHop = embedHop,
                EmbedHopBoxMethods = embedHopBoxMethods,
                EmbedHopDeepPaths = embedHopDeepPaths,
                // An interface member with NO direct struct method and a SINGLE VALUE embed
                // must promote through the embedded field (Go's promotion is what type-checked
                // it) - net's `addrPortUDPAddr struct { netip.AddrPort }`: the bare
                // `this.String()` bound an unrelated same-package extension by NAME (CS1929);
                // `this.AddrPort.String()` binds the embed's value-receiver method.
                ValueEmbedHop = embedHop is null && (structDecl?.GetEmbeddedValueHopNames() is [var singleValueHop]) ? singleValueHop.Name : null,
                // A FOREIGN value embed's extensions live in another namespace segment
                // (netip_package sits in go.net; the source file only ALIASES it, which does
                // not import extensions) - call the package-class static directly:
                // `global::go.net.netip_package.String(this.AddrPort)`.
                ValueEmbedHopStaticClass = embedHop is null && (structDecl?.GetEmbeddedValueHopNames() is [var svh]) && svh.TypeName.Contains('.')
                    ? "global::go." + svh.TypeName.Substring(0, svh.TypeName.LastIndexOf('.'))
                    : null,
                UsingStatements = usingStatements
            }
            .Generate();

            // Add the source code to the compilation
            context.AddSource(GetUniqueHintName(emittedHintNames, GetValidFileName($"{packageNamespace}.{packageClassName}.{structName}-{interfaceName}.g.cs")), generatedSource);
        }
    }

    /// <summary>
    /// Gets the disambiguating package prefix ("bytes_") for a FOREIGN struct's local adapter
    /// class name, derived from its containing package class ("bytes_package") — matching the
    /// converter's <c>getSanitizedIdentifier(pkg.Name()) + "_"</c> composition at the cast site.
    /// </summary>
    private static string ForeignPackagePrefix(ITypeSymbol structType)
    {
        string? packageClassName = structType.ContainingType?.Name;

        if (packageClassName is null || !packageClassName.EndsWith("_package"))
            return string.Empty;

        return $"{packageClassName.Substring(0, packageClassName.Length - "_package".Length)}_";
    }

    private static string? GetNamespace(FileScopedNamespaceDeclarationSyntax? namespaceSyntax)
    {
        return namespaceSyntax?.Name.ToString();
    }

    private static string? GetFirstClassName(CompilationUnitSyntax compilationUnit)
    {
        return compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault()?.Identifier.Text;
    }
}
