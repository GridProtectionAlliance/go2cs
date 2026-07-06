package main

import (
	"go/ast"
	"go/build"
	"go/types"
	"path/filepath"
	"strings"
)

// packageImportAliasRenames maps a package-local import qualifier (the identifier Go code
// qualifies with — the package's own name for an unaliased import, or the explicit alias) to
// its collision-renamed C# using alias. A C# using alias declared inside a namespace CONFLICTS
// with a same-named CHILD namespace visible from any (transitively) referenced assembly
// (CS0576 at every use): `using runtime = runtime_package;` inside `namespace go` collides
// with `go.runtime` the moment anything in the reference closure contains a runtime/*
// subpackage (runtime.csproj itself references runtime/internal/math|sys) — surfaced by
// iter/internal/weak in the first full-solution wave. Such an alias is Δ-renamed
// (`using Δruntime = runtime_package;` / `Δruntime.Goexit()`), the same marker every other
// collision rename uses. Populated by a synchronous pre-pass; read-only during concurrent
// file visiting.
var packageImportAliasRenames map[string]string

// packageChildNamespaces holds every namespace CHAIN contributed by the package's transitive
// import closure (package path a/b/c contributes namespaces go.a and go.a.b — the class c_package
// is not a namespace). Mirrors MSBuild's transitive ProjectReference visibility.
var packageChildNamespaces map[string]bool

// packageImportLeadingSegments holds the C# using-alias identifier bound by every DIRECT import in
// the current package (an unaliased import's canonical name, or an explicit alias). A sub-package
// import path (`io/fs`) emits a RELATIVE namespace target (`io.fs_package`); if the leading segment
// (`io`) is also imported (`using io = io_package;`), C# binds that segment to the TYPE alias, so
// `io.fs_package` resolves to the nonexistent nested type `io_package.fs_package` (CS0426). Recording
// the bound leading identifiers lets rootQualifyIfAmbiguous prefix "go." onto such a target
// (`go.io.fs_package`) so the segment resolves as the child NAMESPACE it was meant to be. Populated by
// computeImportAliasRenames' synchronous pre-pass; read-only during concurrent file visiting.
var packageImportLeadingSegments map[string]bool

// computeImportAliasRenames populates the two maps above for the package being converted.
// packageNS is the emission namespace of the current package (e.g. "go", "go.@internal").
func computeImportAliasRenames(files []FileEntry, pkg *types.Package, packageNS string) {
	closure := make(map[string]bool)

	var walk func(p *types.Package)

	walk = func(p *types.Package) {
		for _, imp := range p.Imports() {
			if !closure[imp.Path()] {
				closure[imp.Path()] = true
				walk(imp)
			}
		}
	}

	walk(pkg)

	// A GOROOT package's `golang.org/x/…` imports are GOROOT-VENDORED — visitImportSpec resolves them
	// to their `vendor/…` on-disk path (and namespace) when the importing file lives under GOROOT. The
	// child-namespace map must use the SAME resolved form or a vendored sub-namespace (e.g.
	// `go.vendor.golang.org.x.text.unicode`) is absent, so rootQualifyIfAmbiguous cannot see that a
	// stdlib alias's leading segment (`unicode` of `unicode/utf8`) collides with it — bidirule emitted
	// `using utf8 = unicode.utf8_package;` binding `unicode` to the vendored namespace (CS0234). Gated
	// on the package living under GOROOT so a user module's own golang.org/x dependency is untouched.
	isGorootPackage := false

	if len(files) > 0 {
		goroot := filepath.Clean(build.Default.GOROOT)
		isGorootPackage = strings.HasPrefix(filepath.Clean(files[0].filePath), goroot+string(filepath.Separator))
	}

	for path := range closure {
		if isGorootPackage {
			path = resolveGorootVendoredPath(path)
		}

		parts := strings.Split(path, "/")
		ns := RootNamespace

		for _, part := range parts[:len(parts)-1] {
			ns += "." + getSanitizedImport(part)
			packageChildNamespaces[ns] = true
		}
	}

	collides := func(qualifier string) bool {
		return packageChildNamespaces[packageNS+"."+getSanitizedImport(qualifier)]
	}

	// Canonical (unaliased) import names across the package.
	for _, imp := range pkg.Imports() {
		name := imp.Name()

		// Every canonically-imported package binds its name as a using-alias — record the SANITIZED
		// form (matching how namespace segments are rendered) so a sub-package's relative namespace
		// whose leading segment equals it gets root-qualified.
		packageImportLeadingSegments[getSanitizedImport(name)] = true

		if collides(name) {
			packageImportAliasRenames[name] = ShadowVarMarker + name
		}
	}

	// Explicitly aliased imports (`import foo "runtime"`) qualify by the alias instead.
	for _, fileEntry := range files {
		for _, importSpec := range fileEntry.file.Imports {
			if importSpec.Name == nil {
				continue
			}

			alias := importSpec.Name.Name

			if alias == "." || alias == "_" {
				continue
			}

			// An explicit alias binds THAT (sanitized) identifier as the file-local using-alias.
			packageImportLeadingSegments[getSanitizedImport(alias)] = true

			if collides(alias) {
				packageImportAliasRenames[alias] = ShadowVarMarker + alias
			}
		}
	}
}

// importQualifier returns the (possibly collision-renamed) C# alias for a package qualifier.
func importQualifier(name string) string {
	if packageImportAliasRenames != nil {
		if renamed, ok := packageImportAliasRenames[name]; ok {
			return renamed
		}
	}

	return name
}

// identIsRenamedImport reports whether ident is a package qualifier with a renamed alias.
func (v *Visitor) identifierIsPackageName(ident *ast.Ident) (string, bool) {
	if obj := v.info.ObjectOf(ident); obj != nil {
		if pkgName, ok := obj.(*types.PkgName); ok {
			return pkgName.Name(), true
		}
	}

	return "", false
}
