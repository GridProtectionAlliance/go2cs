package main

import (
	"fmt"
	"go/ast"
	"log"
	"regexp"
	"strings"
)

// packageQualifiedNameRegex matches a dotted qualified identifier. Segments may contain Unicode
// letters/digits the converter uses in generated names (e.g. Δ, ꓸ, ᴛ), so the class is Unicode-aware.
var packageQualifiedNameRegex = regexp.MustCompile(`[\p{L}_][\p{L}\p{N}_]*(?:\.[\p{L}_][\p{L}\p{N}_]*)*`)

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
			return match
		}

		for i, seg := range strings.Split(match, ".") {
			if strings.HasSuffix(seg, PackageSuffix) {
				// Only a sub-namespace package class (not the leading segment) needs rooting.
				if i > 0 {
					return RootNamespace + "." + match
				}

				break
			}
		}

		return match
	})
}

func (v *Visitor) visitImportSpec(importSpec *ast.ImportSpec, doc *ast.CommentGroup) {
	v.currentImportPath = strings.Trim(importSpec.Path.Value, "\"")

	if !v.options.parseCgoTargets && v.currentImportPath == "C" {
		log.Fatalf("cgo target parsing is not supported: file \"%s\"", v.fset.Position(importSpec.Pos()).Filename)
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

			v.packageImports.WriteString(fmt.Sprintf("using %s = %s;", getSanitizedImport(alias), importPath))
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

		v.packageImports.WriteString(fmt.Sprintf("using %s = %s;", getSanitizedImport(strings.TrimSuffix(importName, PackageSuffix)), importPath))
	}

	v.writeCommentString(v.packageImports, importSpec.Comment, importSpec.End())
	v.packageImports.WriteString(v.newline)
}

// rootQualifyIfAmbiguous prefixes an imported namespace with the root namespace ("go.") when its
// leading segment also appears in the current package's namespace path. Without this, C# relative
// name resolution binds the leading segment to the closer (current-namespace) match instead of the
// intended root-level one — e.g. a package in `go.runtime.@internal` importing `internal/goarch`
// (namespace `@internal.goarch_package`) resolves `@internal` to `go.runtime.@internal`, not
// `go.@internal` → CS0234. Non-colliding imports (the common case) are returned unchanged, so this
// adds no churn for packages whose namespace does not nest under a colliding segment.
func rootQualifyIfAmbiguous(ns string) string {
	if ns == "" || strings.HasPrefix(ns, RootNamespace+".") {
		return ns
	}

	firstSeg := ns

	if dot := strings.Index(ns, "."); dot != -1 {
		firstSeg = ns[:dot]
	}

	for _, seg := range strings.Split(packageNamespace, ".") {
		if seg != RootNamespace && seg == firstSeg {
			return RootNamespace + "." + ns
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

func convertImportPathToNamespace(importPath string, packageSuffix string) string {
	// Split import path by "/"
	importPathParts := strings.Split(importPath, "/")

	// Update all import path parts to sanitized identifiers
	for i, part := range importPathParts {
		if i == len(importPathParts)-1 {
			part = part + packageSuffix
		}

		importPathParts[i] = getSanitizedImport(part)
	}

	return strings.Join(importPathParts, ".")
}
