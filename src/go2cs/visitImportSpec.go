package main

import (
	"fmt"
	"go/ast"
	"go/build"
	"log"
	"os"
	"path/filepath"
	"regexp"
	"strings"
)

// packageQualifiedNameRegex matches a dotted qualified identifier. Segments may contain Unicode
// letters/digits the converter uses in generated names (e.g. Δ, ꓸ, ᴛ) and may carry the C#
// keyword escape (`@internal`) — the `@` must be INSIDE the match, or the root-qualifier below
// splices the prefix between the escape and its segment (`@internal.bisect_package.Writer`
// became `@go.internal.…` — a parse error in internal/godebug's GoImplement attribute).
var packageQualifiedNameRegex = regexp.MustCompile(`@?[\p{L}_][\p{L}\p{N}_]*(?:\.@?[\p{L}_][\p{L}\p{N}_]*)*`)

// systemCollidingTypeNames are top-level types of C# namespace `System` whose names a Go package can
// legitimately reuse for one of its own exported types (e.g. `internal/profile.ValueType`,
// `go/ast.Object`, `bytes.Buffer`). Assembly-level GoImplement/GoImplicitConv attributes are emitted
// at file scope where BOTH `using System;` and `using static go.<pkg>_package;` are in scope, so a
// BARE reference to such a local type is ambiguous between the System type and the package type
// (CS0104). qualifySystemCollidingLocalTypeRefs roots those bare references at the package class so
// they resolve unambiguously to the local type. Only names in this curated set are touched, so
// attributes whose type names never collide with System (every behavioral-test case) emit
// byte-identically (no golden churn).
var systemCollidingTypeNames = NewHashSet([]string{
	"Action", "Activator", "Array", "Attribute", "Boolean", "Buffer", "Byte", "Char", "Comparison",
	"Console", "Convert", "DateTime", "Decimal", "Delegate", "Double", "Enum", "Environment", "Exception",
	"Func", "Guid", "Half", "Index", "Int128", "Lazy", "Math", "Memory", "Nullable", "Object", "Predicate",
	"Progress", "Random", "Range", "SByte", "Single", "Span", "String", "TimeSpan", "TimeZone", "TimeZoneInfo",
	"Tuple", "Type", "UInt128", "Uri", "ValueType", "Version",
})

// rootQualifySubNamespaceTypeRefs prefixes the root namespace ("go.") onto package-qualified type
// references that live in a sub-namespace (e.g. image.color_package.ΔRGBA -> go.image.color_package.ΔRGBA).
// Assembly-level GoImplement attributes are emitted before the file's namespace with only `using go;`
// in scope; that directive imports the TYPES of namespace `go` (so a top-level `io_package.Writer`
// resolves unqualified) but NOT its nested namespaces, so a multi-segment package class such as
// `image.color_package` cannot be found and yields CS0246. References whose `_package` class is the
// first segment (single-segment packages such as io/fmt/sort) and references already rooted at "go."
// are returned unchanged, so single-segment GoImplements — every behavioral-test case — emit
// byte-identically (no golden churn).
func rootQualifySubNamespaceTypeRefs(name string) string {
	return packageQualifiedNameRegex.ReplaceAllStringFunc(name, func(match string) string {
		if strings.HasPrefix(match, RootNamespace+".") {
			// A go/*-package ref whose root the strip removed (`go.ast_package` for go/ast, whose
			// real namespace is `go.go`) is re-rooted to `go.go.ast_package`; the GoImplement/
			// GoImplicitConv attributes emit at assembly scope, so a bare root prefix resolves (no
			// global:: needed, unlike the in-namespace using aliases). A genuinely-rooted ref is
			// left unchanged. See isStrippedGoPathPackageRef.
			if isStrippedGoPathPackageRef(match) {
				return RootNamespace + "." + match
			}

			return match
		}

		for i, seg := range strings.Split(match, ".") {
			if strings.HasSuffix(seg, PackageSuffix) {
				// Only a sub-namespace package class (not the leading segment) needs rooting.
				if i > 0 {
					// A Δ-renamed import alias (io -> Δio, which collides with the `io` CHILD
					// namespace) is a FILE-LOCAL device; the rooted `go.` path needs the REAL
					// namespace segment — `go.io.fs_package.DirEntry`, not `go.Δio.fs_package…`
					// (CS0234, embed's GoImplement<@file, io/fs.DirEntry> lines). Strip the
					// collision marker from the leading segment.
					if raw, wasShadow := strings.CutPrefix(match, ShadowVarMarker); wasShadow {
						return RootNamespace + "." + raw
					}

					return RootNamespace + "." + match
				}

				break
			}
		}

		return match
	})
}

// qualifySystemCollidingLocalTypeRefs roots any BARE (single-segment) type reference whose name is a
// System type (systemCollidingTypeNames) at packagePrefix (e.g. `go.@internal.profile_package`), so it
// resolves to the LOCAL package type rather than being ambiguous with the `using System;`-imported type
// (CS0104) at the file scope where GoImplement/GoImplicitConv attributes are emitted. Dotted references
// (foreign `pkg_package.Type`, already-rooted `go.…`) are left untouched: a bare System-colliding name
// in these attributes can only be a local package type (foreign types are always package-qualified).
func qualifySystemCollidingLocalTypeRefs(name string, packagePrefix string) string {
	return packageQualifiedNameRegex.ReplaceAllStringFunc(name, func(match string) string {
		// Only bare (dotless) identifiers can be a local type name; qualified references are foreign.
		if strings.Contains(match, ".") {
			return match
		}

		if systemCollidingTypeNames.Contains(match) {
			return packagePrefix + "." + match
		}

		return match
	})
}

func (v *Visitor) visitImportSpec(importSpec *ast.ImportSpec, doc *ast.CommentGroup) {
	v.currentImportPath = strings.Trim(importSpec.Path.Value, "\"")

	if !v.options.parseCgoTargets && v.currentImportPath == "C" {
		log.Fatalf("cgo target parsing is not supported: file \"%s\"", v.fset.Position(importSpec.Pos()).Filename)
	}

	// Resolve a GOROOT-vendored import to its on-disk path (see resolveGorootVendoredPath) so
	// the import queue and the imported-alias loader look at the real output location. Gated on
	// the importing FILE living under GOROOT so a user module's own golang.org/x dependency is
	// untouched.
	if goroot := filepath.Clean(build.Default.GOROOT); strings.HasPrefix(filepath.Clean(v.fset.Position(importSpec.Pos()).Filename), goroot+string(filepath.Separator)) {
		v.currentImportPath = resolveGorootVendoredPath(v.currentImportPath)
	}

	v.importQueue.Add(v.currentImportPath)
	v.loadImportedTypeAliases(v.currentImportPath)

	importPath := rootQualifyIfAmbiguous(convertImportPathToNamespace(v.currentImportPath, PackageSuffix))

	// The canonical C# alias for this package — what an unaliased import emits and what getTypeName's
	// short-form type references (`pkg.Type`) resolve through. Record the import path when THIS import
	// actually emits that canonical alias (an unaliased import, or one explicitly aliased to the same
	// name), so visitFile does not re-emit (and duplicate) it; a blank/dot/renamed import does not emit
	// it, so a foreign type reference from this file still gets the alias supplied (see collectTypePackages).
	canonicalAlias, _ := packageUsingAlias(v.currentImportPath)

	v.writeDocString(v.packageImports, doc, importSpec.Pos())

	if importSpec.Name != nil {
		alias := importSpec.Name.Name

		if alias == "." {
			v.packageImports.WriteString(fmt.Sprintf("using static %s;", importPath))
		} else if alias == "_" {
			// A BLANK import (`import _ "unsafe"`) is side-effects-only: Go forbids referencing
			// the package through it, so the alias is never legitimately used — but emitting
			// `using _ = <ns>;` HIJACKS C#'s `_` DISCARD for the whole file: any deconstruction
			// discard (`(w, _) = w.ensure(…)`, runtime tracetime.go) then binds the namespace
			// alias instead (CS0118 + CS0029). Record the import as a comment only; the package's
			// exported aliases still load (loadImportedTypeAliases above) and a genuine type
			// reference gets its canonical `using` from visitFile's collectTypePackages machinery.
			v.packageImports.WriteString(fmt.Sprintf("// blank import: %s (side effects only; no using emitted — a `using _` alias hijacks C# discards)", importPath))
		} else {
			if getSanitizedImport(alias) == canonicalAlias {
				v.canonicalAliasImported.Add(v.currentImportPath)
			}

			emittedAlias := getSanitizedImport(importQualifier(alias))
			v.importAliasesEmitted.Add(emittedAlias)
			v.importPathAliases[v.currentImportPath] = emittedAlias
			v.packageImports.WriteString(fmt.Sprintf("using %s = %s;", emittedAlias, importPath))
		}
	} else {
		v.canonicalAliasImported.Add(v.currentImportPath)

		// Get package name from the import path, last name after last "."
		importName := importPath
		lastDotIndex := strings.LastIndex(importPath, ".")

		if lastDotIndex != -1 {
			importName = importPath[lastDotIndex+1:]

			namespace := importPath[:lastDotIndex]

			if len(namespace) > 0 && packageNamespace != fmt.Sprintf("%s.%s", RootNamespace, namespace) {
				v.requiredUsings.Add(namespace)
			}
		}

		emittedAlias := getSanitizedImport(importQualifier(strings.TrimSuffix(importName, PackageSuffix)))
		v.importAliasesEmitted.Add(emittedAlias)
		v.packageImports.WriteString(fmt.Sprintf("using %s = %s;", emittedAlias, importPath))
	}

	v.writeCommentString(v.packageImports, importSpec.Comment, importSpec.End())
	v.packageImports.WriteString(v.newline)
}

// rootQualified prefixes ns with the root namespace, using `global::go.` instead of a bare `go.`
// when the CURRENT package's own namespace shadows the root — a `go/*` stdlib package (go/token,
// go/ast, go/doc, go/build, …) lands in `namespace go.go.<pkg>`, so a bare `go.X` reference binds the
// leading `go` to the enclosing `go.go` namespace and resolves e.g. `go.sync` to the nonexistent
// `go.go.sync` (CS0234). `global::` forces resolution from the global namespace. Every other package
// (whose namespace's second segment is not `go`) keeps the bare `go.` prefix — `global::` there would
// be needless golden churn.
func rootQualified(ns string) string {
	segs := strings.Split(packageNamespace, ".")

	if len(segs) >= 2 && segs[0] == RootNamespace && segs[1] == RootNamespace {
		return "global::" + RootNamespace + "." + ns
	}

	return RootNamespace + "." + ns
}

// rootQualifyIfAmbiguous prefixes an imported namespace with the root namespace ("go.") when its
// leading segment also appears in the current package's namespace path. Without this, C# relative
// name resolution binds the leading segment to the closer (current-namespace) match instead of the
// intended root-level one — e.g. a package in `go.runtime.@internal` importing `internal/goarch`
// (namespace `@internal.goarch_package`) resolves `@internal` to `go.runtime.@internal`, not
// `go.@internal` → CS0234. Non-colliding imports (the common case) are returned unchanged, so this
// adds no churn for packages whose namespace does not nest under a colliding segment.
// isStrippedGoPathPackageRef reports whether a "go."-prefixed rendered name is actually a
// go/*-PACKAGE reference whose root was stripped — the leading "go" is the package import path's
// own first segment (go/ast, go/token, go/types → namespace go.go.ast), not the root namespace.
// Such a ref has a "_package" CLASS as the segment RIGHT AFTER "go.". A genuinely root-qualified
// ref (go.go.ast_package, go.io.fs_package) has a non-"_package" segment there, and a LOCAL package
// never renders its own class as "go.<class>_package" (after the root strip it is the bare
// "<class>_package"), so only go/*-package refs match. Used to re-root the go/* refs that
// convertToCSTypeName's redundant-root strip mangled to `go.ast_package` (CS0234/CS0426).
func isStrippedGoPathPackageRef(goPrefixed string) bool {
	rest := goPrefixed[len(RootNamespace)+1:]
	firstSeg := rest

	if dot := strings.IndexByte(rest, '.'); dot != -1 {
		firstSeg = rest[:dot]
	}

	return strings.HasSuffix(firstSeg, PackageSuffix)
}

func rootQualifyIfAmbiguous(ns string) string {
	if ns == "" {
		return ns
	}

	if strings.HasPrefix(ns, RootNamespace+".") {
		// A go/*-package namespace whose root the strip removed (`go.token_package` for go/token,
		// whose real namespace is `go.go`) is re-rooted to `go.go.token_package`, and ALWAYS with
		// `global::`: a bare `go.go.<pkg>_package` re-binds its leading `go` to the nearest enclosing
		// `go`, which mis-resolves from a go/*-package's own `go.go.*` namespace AND from any other
		// package under the root `go` (internal/pkgbits' `go.internal.pkgbits` resolved
		// `go.go.constant_package`'s second `go` inside `go.go`, CS0234). rootQualified only forces
		// `global::` when the IMPORTER itself nests `go.go`, so a non-go/* importer of a go/* package
		// was left bare — force it here. A genuinely-rooted ref is returned unchanged.
		if isStrippedGoPathPackageRef(ns) {
			return "global::" + RootNamespace + "." + ns
		}

		return ns
	}

	firstSeg := ns

	if dot := strings.Index(ns, "."); dot != -1 {
		firstSeg = ns[:dot]
	}

	// A relative target ALSO mis-binds when its leading segment is bound as a using-alias by a
	// same-package import — a sub-package import (`io/fs` → `io.fs_package`) whose parent (`io`) is
	// also imported (`using io = io_package;`) would otherwise bind `io` to that TYPE alias and
	// resolve `io.fs_package` to the nonexistent nested type `io_package.fs_package` (CS0426);
	// `go.io.fs_package` makes `io` resolve as the child namespace it was meant to be. A single-segment
	// namespace (`io_package`) has no leading qualifier to shadow, so this applies only to multi-segment
	// (sub-package) targets.
	if firstSeg != ns && packageImportLeadingSegments[firstSeg] {
		return rootQualified(ns)
	}

	for _, seg := range strings.Split(packageNamespace, ".") {
		if seg != RootNamespace && seg == firstSeg {
			return rootQualified(ns)
		}
	}

	// A relative alias target ALSO mis-binds when its first segment names a CHILD namespace of
	// the current namespace — contributed by the transitive reference closure, not the current
	// namespace's own path: runtime/metrics (namespace go.runtime) importing internal/godebugs
	// emitted `using godebugs = @internal.godebugs_package;`, but runtime.csproj's own
	// runtime/internal/* references put go.runtime.@internal in the compilation, so C#'s
	// inner-to-outer lookup binds `@internal` there (CS0234). packageChildNamespaces (the
	// CS0576 Δ-alias machinery) already mirrors that closure; walk every enclosing-namespace
	// prefix above the root, since any level can shadow the intended go.<firstSeg>.
	prefix := packageNamespace

	for prefix != "" && prefix != RootNamespace {
		if packageChildNamespaces[prefix+"."+firstSeg] {
			return rootQualified(ns)
		}

		if dot := strings.LastIndex(prefix, "."); dot != -1 {
			prefix = prefix[:dot]
		} else {
			break
		}
	}

	return ns
}

// packageUsingAlias returns the canonical C# using alias and target namespace for a Go import path,
// matching visitImportSpec's unaliased-import emission (`using <alias> = <namespace>;`). Used both to
// decide whether an import already emitted the canonical alias and to synthesize it in visitFile for a
// foreign type referenced without a canonical import.
func packageUsingAlias(importPath string) (alias string, namespace string) {
	namespace = rootQualifyIfAmbiguous(convertImportPathToNamespace(importPath, PackageSuffix))

	name := namespace

	if lastDot := strings.LastIndex(namespace, "."); lastDot != -1 {
		name = namespace[lastDot+1:]
	}

	alias = getSanitizedImport(strings.TrimSuffix(name, PackageSuffix))

	return alias, namespace
}

// resolveGorootVendoredPath maps a GOROOT-vendored import path to its ON-DISK form: a stdlib
// package imports `golang.org/x/text/transform` (the type info also carries the unprefixed
// path), but the converted package - its namespace, csproj, and output directory - lives at
// `vendor/golang.org/x/text/transform`, the key the stdlib dependency graph uses
// (stdLibConverter). Every namespace-text derivation must agree, or consumers emit
// `go.golang.org...` refs that exist nowhere (bidirule's 25 CS0234). The dotted-domain first
// segment is the cheap pre-filter (no plain stdlib path contains a dot). CAVEAT: a USER module
// that depends on the same golang.org/x package would false-positive here (its copy is not
// GOROOT-vendored); revisit with module-conversion support - the behavioral corpus has no such
// dependency today.
func resolveGorootVendoredPath(importPath string) string {
	firstSegment := importPath

	if idx := strings.Index(firstSegment, "/"); idx >= 0 {
		firstSegment = firstSegment[:idx]
	}

	if !strings.Contains(firstSegment, ".") || strings.HasPrefix(importPath, "vendor/") {
		return importPath
	}

	if _, err := os.Stat(filepath.Join(build.Default.GOROOT, "src", "vendor", filepath.FromSlash(importPath))); err == nil {
		return "vendor/" + importPath
	}

	return importPath
}

// majorVersionSegmentRegex matches a Go module major-version path segment (v2, v3, …).
var majorVersionSegmentRegex = regexp.MustCompile(`^v[0-9]+$`)

func convertImportPathToNamespace(importPath string, packageSuffix string) string {
	importPath = resolveGorootVendoredPath(importPath)

	// Split import path by "/"
	importPathParts := strings.Split(importPath, "/")

	// A MAJOR-VERSION directory (`math/rand/v2`): the Go package is named for the PARENT
	// segment (`rand`), and the emitted class follows the package NAME — namespace
	// go.math.rand + class rand_package. The path-derived v2_package exists nowhere
	// (CS0234, internal/concurrent importing math/rand/v2). Convention-based: a /vN dir
	// hosts the parent-named package (true stdlib-wide; a package literally named vN
	// would need the type-graph name instead).
	if len(importPathParts) > 1 {
		if last := importPathParts[len(importPathParts)-1]; majorVersionSegmentRegex.MatchString(last) {
			importPathParts[len(importPathParts)-1] = importPathParts[len(importPathParts)-2]
		}
	}

	// Update all import path parts to sanitized identifiers
	for i, part := range importPathParts {
		if i == len(importPathParts)-1 {
			part = part + packageSuffix
		}

		importPathParts[i] = getSanitizedImport(part)
	}

	return strings.Join(importPathParts, ".")
}
