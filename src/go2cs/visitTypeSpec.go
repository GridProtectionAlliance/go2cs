package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"regexp"
	"strings"
)

func (v *Visitor) visitTypeSpec(typeSpec *ast.TypeSpec, doc *ast.CommentGroup) {
	name := v.getIdentName(typeSpec.Name)
	identType := v.getIdentType(typeSpec.Name)

	// A defined type whose name COLLIDES with a method name (go/ast's `type Filter func(string)
	// bool` vs `(CommentMap).Filter`) is Δ-prefixed at every USE (convIdent →
	// getSanitizedIdentifier), but getIdentName returns the raw name — so the DECLARATION
	// emitted a bare `delegate … Filter` that both duplicated the method (CS0102) and was
	// unreachable from the ΔFilter uses (CS0246 ×14). Match the declaration to the uses. Manual
	// types already record the Δ form explicitly (below), so this covers the auto-emitted kinds.
	if nameCollisions[typeSpec.Name.Name] {
		name = getSanitizedIdentifier(typeSpec.Name.Name)
	}

	// A DEFINED type over an INTERFACE (`type Token any`, `type Reader io.Reader`) has EXACTLY the
	// interface's method set and can carry no methods of its own (Go forbids an interface receiver),
	// so it is emitted as a `global using` alias to that interface — the SAME form as a real Go type
	// alias below — never a `[GoType] partial struct` wrapper. A struct wrapper over `any` (= object)
	// admits no implicit conversion FROM a concrete value (C# bars user-defined conversions from
	// object), so every `StartElement → Token` assignment was CS0029 (encoding/xml's `type Token
	// any`, ×16). Restricted to a NAMED-type RHS (Ident/Selector); an inline interface DEFINITION
	// (`type X interface{…}`) is an *ast.InterfaceType and still emits a C# interface via the switch.
	definedOverInterface := false

	if !typeSpec.Assign.IsValid() {
		switch typeSpec.Type.(type) {
		case *ast.Ident, *ast.SelectorExpr:
			if _, isIface := identType.Underlying().(*types.Interface); isIface {
				definedOverInterface = true
			}
		}
	}

	// Handle type alias (or a defined type over an interface — see above)
	if typeSpec.Assign.IsValid() || definedOverInterface {
		// Get types.Type from typeSpec.Type expr
		typeSpecType := v.info.TypeOf(typeSpec.Type)

		if typeSpecType == nil {
			panic(fmt.Sprintf("@visitTypeSpec - Failed to get type for type alias %s", name))
		}

		var usePackagePrefix bool

		// Check if the aliased type is a struct or pointer to a struct
		if structType, exprType := v.extractStructType(typeSpec.Type); structType != nil && !v.liftedTypeExists(structType) {
			if v.inFunction {
				v.indentLevel++
			}

			v.visitStructType(structType, exprType, name, doc, true, nil)

			if v.inFunction {
				v.indentLevel--
			}

			usePackagePrefix = true
		}

		// Check if the aliased type is an anonymous interface
		if interfaceType, exprType := v.extractInterfaceType(typeSpec.Type); interfaceType != nil && !v.liftedTypeExists(interfaceType) {
			if v.inFunction {
				v.indentLevel++
			}

			v.visitInterfaceType(interfaceType, exprType, name, doc, true, nil)

			if v.inFunction {
				v.indentLevel--
			}

			usePackagePrefix = true
		}

		// A type alias to a SAME-PACKAGE named type (`type alias = Inner`) must qualify the target
		// with the package class — the `global using` sits at namespace scope, outside the class, so
		// a bare `go.Inner` does not resolve (Inner is `go.main_package.Inner`). Cross-package targets
		// already carry their own qualification, so only same-package named targets need this.
		if !usePackagePrefix {
			if named, ok := types.Unalias(typeSpecType).(*types.Named); ok {
				if obj := named.Obj(); obj != nil && obj.Pkg() == v.pkg {
					usePackagePrefix = true
				}
			}
		}

		var typeNamePrefix string

		if usePackagePrefix {
			// The same-package alias TARGET must be qualified with the package CLASS
			// (`<pkg>_package`), but for a package in a NESTED namespace (internal/fuzz →
			// `go.@internal`, io/fs → `go.io`) the class alone roots the target one segment too
			// shallow: a bare `fuzz_package/CorpusEntryᴛ1` renders `go.fuzz_package.CorpusEntryᴛ1`,
			// but the lifted type lives at `go.@internal.fuzz_package.CorpusEntryᴛ1` → CS0234 at the
			// `global using` line and every use (internal/fuzz's CorpusEntry, ×60). Prepend the
			// namespace segments between the root and the class (`@internal`, `io`), taken from the
			// SAME packageNamespace that emitted the `namespace …;` declaration so the two always
			// agree. A top-level package's namespace is exactly RootNamespace, leaving no prefix
			// segments, so the target stays `<pkg>_package/…` (byte-for-byte no-op).
			nsSegments := strings.TrimPrefix(strings.TrimPrefix(packageNamespace, RootNamespace), ".")
			classQualifier := getSanitizedImport(fmt.Sprintf("%s%s", packageName, PackageSuffix))

			if nsSegments == "" {
				typeNamePrefix = classQualifier + "/"
			} else {
				typeNamePrefix = nsSegments + "." + classQualifier + "/"
			}
		}

		typeName := convertToCSFullTypeName(typeNamePrefix + v.getFullTypeName(typeSpecType, false))

		// The empty interface target (`type X any` / `type X = any` / `type X interface{}`) renders
		// as `go.any`, which does not resolve in a using-alias RHS (any is a csproj-level alias, and
		// the safe-name rewrite below deliberately skips `.`-qualified names) — it IS `object`. Emit
		// object directly (encoding/xml's `type Token any`).
		if iface, ok := typeSpecType.Underlying().(*types.Interface); ok && iface.Empty() {
			typeName = "object"
		} else {
			// A `using` alias RHS is resolved WITHOUT reference to other using directives - the
			// csproj-level golib aliases (`uint64`, `float64`, `any`, ...) that resolve everywhere
			// else in the compilation are CS0246 here (fiat: `type p224UntypedFieldElement =
			// [4]uint64` must emit `global using ... = go.array<ulong>;`, not `...<uint64>`). Rewrite
			// those names to their using-safe C# keyword/BCL equivalents for this context only.
			typeName = getUsingAliasSafeTypeName(typeName)

			// The ROOTED namespace of a global-using RHS must use the CANONICAL package qualifier,
			// never the file-local collision-rename: os re-exports `type DirEntry = fs.DirEntry`, but
			// os aliases its `io` import to `Δio` (io is shadowed once io/fs is in the reference
			// closure), and getAliasedTypeName applies that rename even when rooting — emitting
			// `go.Δio.fs_package.DirEntry`, where `Δio` is a file-local `using`, not a namespace under
			// `go` → CS0234 (os's DirEntry/PathError/FileInfo/FileMode re-exports). Un-rename the
			// qualifier right after the root when it is a known import rename (a Δ-renamed TYPE segment
			// is left untouched — only an entry the import-rename map produced is reverted).
			rootPrefix := RootNamespace + "."
			if after, rooted := strings.CutPrefix(typeName, rootPrefix); rooted {
				if seg, rest, found := strings.Cut(after, "."); found {
					if canonical, wasRenamed := strings.CutPrefix(seg, ShadowVarMarker); wasRenamed && packageImportAliasRenames[canonical] == seg {
						typeName = rootPrefix + canonical + "." + rest
					}
				}
			}
		}

		v.typeAliasDeclarations.WriteString(fmt.Sprintf("global using %s = %s;%s", name, typeName, v.newline))

		// Add exported type aliases to package info
		if getAccess(name) == "public" {
			packageLock.Lock()
			exportedTypeAliases[name] = typeName
			packageLock.Unlock()
		}

		return
	}

	// A manually-converted type (see manualTypeOperations.go) emits only a marker comment; the
	// package's *_impl.cs declares the type. Both its plain and collision-renamed forms are
	// recorded (a type-vs-method collision Δ-prefixes the TYPE — guintptr → Δguintptr) so the
	// GoImplicitConv attribute emission can skip conversions referencing either rendering.
	if v.isManualType(typeSpec.Name.Name) {
		packageLock.Lock()
		packageManualTypeNames[name] = true
		packageManualTypeNames[getSanitizedIdentifier(typeSpec.Name.Name)] = true
		packageLock.Unlock()

		if !v.inFunction {
			v.targetFile.WriteString(v.newline)
		}

		v.writeOutput("// type %s is hand-converted with managed semantics — see the package's *_impl.cs ([module: GoManualConversion])", name)
		v.targetFile.WriteString(v.newline)
		return
	}

	// An unexported type used as an exported struct field must be emitted as public (CS0051/
	// CS0052). Set the access modifier for the type-kind emitter below to consume.
	if v.isPublicizedType(typeSpec.Name) {
		v.pendingTypeAccess = "public "
	}

	defer func() { v.pendingTypeAccess = "" }()

	switch typeSpecType := typeSpec.Type.(type) {
	case *ast.ArrayType:
		v.visitArrayType(typeSpecType, identType, name, typeSpec.Comment)
	case *ast.ChanType:
		v.visitChanType(typeSpecType, name)
	case *ast.FuncType:
		v.visitFuncType(typeSpecType, identType, name)
	case *ast.Ident:
		v.visitIdent(typeSpecType, identType, name, v.inFunction)
	case *ast.InterfaceType:
		v.visitInterfaceType(typeSpecType, v.info.Defs[typeSpec.Name].Type(), name, doc, v.inFunction, nil)
	case *ast.MapType:
		v.visitMapType(typeSpecType, name)
	case *ast.ParenExpr:
		v.targetFile.WriteString(v.convParenExpr(typeSpecType, DefaultLambdaContext()))
	case *ast.SelectorExpr:
		// A DEFINED type over a cross-package named type (`type stdFunction unsafe.Pointer`,
		// `type goroutineProfileStateHolder atomic.Uint32`). Emit an inherited `[GoType]` wrapper of
		// the NAMED type (go2cs-gen's InheritedTypeTemplate wraps a plain type-name definition);
		// writing the bare selector text alone is an orphan type reference (CS1585). Use the named
		// type, not its underlying (which may expose unexported cross-package fields → CS0246).
		if rhsType := v.info.TypeOf(typeSpecType); rhsType != nil {
			csName := convertToCSTypeName(v.getFullTypeName(rhsType, false))

			// The GoType attribute is consumed by the generated `<X>.g.cs`, which has no file-local
			// `using` aliases. unsafe.Pointer (a *types.Basic, a C# keyword) renders via the
			// `@unsafe` alias; rewrite it to the alias-free package class so it resolves there.
			// A Δ collision-renamed leading namespace segment must revert to the canonical
			// qualifier for the same reason (see canonicalizeQualifierRename).
			csName = strings.ReplaceAll(csName, "@unsafe.", "unsafe_package.")
			csName = canonicalizeQualifierRename(csName)

			access := v.pendingTypeAccess
			v.pendingTypeAccess = ""

			if !v.inFunction {
				v.targetFile.WriteString(v.newline)
			}

			v.writeOutput("[GoType(\"%s\")] %spartial struct %s;", csName, access, getSanitizedIdentifier(name))
			v.targetFile.WriteString(v.newline)
		} else {
			v.targetFile.WriteString(v.convSelectorExpr(typeSpecType, DefaultLambdaContext()))
		}
	case *ast.StarExpr:
		{
			// A defined POINTER type — `type dequeueNil *struct{}` (sync/poolqueue). The bare
			// converted star-type text (`ж<EmptyStruct>`) is not a declaration (CS1585 — sync's
			// wave-1 error); emit the `[GoType("ж<T>")] partial class` forward declaration whose
			// Pointer template go2cs-gen implements (the generator matches a CLASS declaration
			// for ж<-prefixed definitions — a named pointer is reference-like).
			pointerTypeName := v.convStarExpr(typeSpecType, DefaultStarExprContext())
			access := v.pendingTypeAccess
			v.pendingTypeAccess = ""
			v.targetFile.WriteString(v.newline)
			v.writeOutputLn("[GoType(\"%s\")] %spartial class %s;", pointerTypeName, access, getSanitizedIdentifier(name))
			usesUnsafeCode = true
		}
	case *ast.StructType:
		v.visitStructType(typeSpecType, v.info.Defs[typeSpec.Name].Type(), name, doc, v.inFunction, nil)
	default:
		panic(fmt.Sprintf("@visitTypeSpec - Unexpected TypeSpec type: %#v", v.getPrintedNode(typeSpecType)))
	}
}

// golibAliasSafeNames maps the golib csproj-level `<Using Alias="...">` names to equivalents
// that resolve inside a `using` alias directive's RHS. C# resolves a using directive's target
// without reference to other using directives - aliases are not visible to one another - so a
// rendered `uint64`, valid everywhere else, fails CS0246 inside `global using X = ...;`.
// C# keywords (byte, bool, nint, nuint) and `go.`-qualified golib types (go.@string,
// go.complex64) are already safe and are not mapped.
var golibAliasSafeNames = map[string]string{
	"uint8":      "byte",
	"uint16":     "ushort",
	"uint32":     "uint",
	"uint64":     "ulong",
	"int8":       "sbyte",
	"int16":      "short",
	"int32":      "int",
	"int64":      "long",
	"float32":    "float",
	"float64":    "double",
	"complex128": "System.Numerics.Complex",
	"rune":       "int",
	"any":        "object",
	"GoUntyped":  "System.Numerics.BigInteger",
}

// Matches a golib csproj-alias name standing alone as an identifier: at string start or after a
// type-syntax delimiter (`<`, `(`, `,`, space) - deliberately NOT after `.`, so a package-
// qualified user type that happens to share a builtin name is left untouched.
var golibAliasNameExpr = regexp.MustCompile(`(^|[<(, ])(uint8|uint16|uint32|uint64|int8|int16|int32|int64|float32|float64|complex128|rune|any|GoUntyped)\b`)

// getUsingAliasSafeTypeName rewrites golib csproj-alias type names inside a rendered C# type
// name into forms that resolve in a `using` alias RHS. Applied ONLY when emitting
// `global using <name> = <type>;` type-alias declarations - code-body renderings keep the
// Go-visual alias names.
func getUsingAliasSafeTypeName(typeName string) string {
	return golibAliasNameExpr.ReplaceAllStringFunc(typeName, func(match string) string {
		sub := golibAliasNameExpr.FindStringSubmatch(match)
		return sub[1] + golibAliasSafeNames[sub[2]]
	})
}
