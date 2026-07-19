package main

import (
	"go/ast"
	"go/types"
	"strings"
)

// linknameHandles is the set of THIS package's symbol names that carry a definition-side
// one-argument `//go:linkname <name>` directive — Go 1.23's opt-in that AUTHORIZES other packages to
// linkname-PULL <name> (runtime/linkname.go lists overflowError, divideError, write, doInit, … each
// "used in" a named consumer). The authorization is puller-AGNOSTIC (it opens the symbol to linkname
// generally, it does not name a specific puller), so the faithful C# emission is `public` — a
// puller in a SEPARATE assembly can then reach the symbol through its forwarding property. Reset per
// package alongside projectImports; populated by collectLinknameHandles.
var linknameHandles HashSet[string]

// conversionGraph is the -stdlib convert-set dependency graph (nil for a single-package or -tests
// conversion, where no cross-package cycle can arise from the one package under conversion). Set once
// the graph is built; read by linknamePullWouldCycle. currentPackagePath is the import path of the
// package currently being converted (set per package in resetPackageState).
var conversionGraph *DependencyGraph
var currentPackagePath string

// linknamePullWouldCycle reports whether forwarding a var pull to targetPath would create a CYCLIC C#
// project reference — targetPath is (or transitively depends on) the current package. Go's linkname is
// link-time and cycle-free; a C# project reference is compile-time and cannot be circular. A downward
// pull (math/bits → runtime) is safe; the reverse (runtime → internal/syscall/windows.CanUseLongPaths)
// is not, and keeps its null-field form. Without the graph (single-package / -tests) a lone package
// cannot form a cross-package cycle, so only a self-reference is rejected.
func linknamePullWouldCycle(targetPath string) bool {
	if targetPath == currentPackagePath {
		return true
	}

	if conversionGraph == nil {
		return false
	}

	return conversionGraph.DependsOn(targetPath, currentPackagePath)
}

// collectLinknameHandles scans every file's comments for a definition-side one-argument
// `//go:linkname <name>` handle and records <name> in linknameHandles. A TWO-argument form
// (`//go:linkname local pkgpath.remote`) is a PULL, not a handle, and is skipped — the pull is
// emitted as a forwarding property on the LOCAL side (see varLinknamePull). Mirrors the
// collectPublicizedTypes analysis pass: a package-wide pre-pass whose result the per-file emission
// consults (a handle in runtime/linkname.go must widen a var defined in runtime/panic.go).
func collectLinknameHandles(files []*ast.File) {
	for _, file := range files {
		for _, group := range file.Comments {
			for _, comment := range group.List {
				fields := strings.Fields(comment.Text)

				// One-arg handle: exactly the directive + the authorized symbol name.
				if len(fields) == 2 && fields[0] == "//go:linkname" {
					linknameHandles.Add(fields[1])
				}
			}
		}
	}
}

// packageVarAccess returns the C# access modifier for a package-level var. It is normally the Go
// name's exported-ness (getAccess), but a var carrying a definition-side one-arg //go:linkname handle
// is emitted `public`: Go 1.23 has deliberately opened it to cross-package linkname pulls, so a
// puller in another assembly must be able to reach it — a lowercase-name `internal` would hide it.
//
// A handle var is publicized ONLY when its TYPE is itself publicly accessible: a public member cannot
// expose a less-accessible type (CS0052/CS0053), and runtime's handle list includes deep-internal
// state whose type is unexported (`sched` of `schedt`, `writeBarrier` of an anonymous struct,
// `lastmoduledatap` of `*moduledata`). Such a var could not be linkname-pulled across a C# assembly
// boundary anyway — a foreign package cannot name the internal type — so it keeps its `internal` form.
func packageVarAccess(goIDName string, varType types.Type) string {
	if linknameHandles.Contains(goIDName) && typeIsPubliclyAccessible(varType) {
		return "public"
	}

	return getAccess(goIDName)
}

// typeIsPubliclyAccessible reports whether a value of type t can be exposed by a `public` member —
// every NAMED type it references must be exported (or already publicized, or a universe type like
// `error`). Composites recurse to their element; an anonymous struct/signature and any other shape is
// conservatively NOT accessible (its emission may reference unexported members), so its handle var
// stays internal rather than risk CS0052/CS0053.
func typeIsPubliclyAccessible(t types.Type) bool {
	switch t := t.(type) {
	case *types.Basic:
		return true
	case *types.Alias:
		return typeIsPubliclyAccessible(types.Unalias(t))
	case *types.Named:
		obj := t.Obj()

		// A universe type (`error`, `comparable`) has no package and is always accessible.
		if obj == nil || obj.Pkg() == nil {
			return true
		}

		return obj.Exported() || packagePublicizedTypes[obj]
	case *types.Pointer:
		return typeIsPubliclyAccessible(t.Elem())
	case *types.Slice:
		return typeIsPubliclyAccessible(t.Elem())
	case *types.Array:
		return typeIsPubliclyAccessible(t.Elem())
	default:
		return false
	}
}

// varLinknamePull recognizes a package var carrying a TWO-argument `//go:linkname <name>
// <pkgpath>.<remote>` PULL directive (name matches the var). It returns the fully-qualified C#
// reference to the remote symbol (`go.runtime_package.overflowError`) and the remote's import path,
// so the var is emitted as a forwarding property to it and the path is queued for a project
// reference. The fully-qualified form resolves inside `namespace go;` without a file-local using.
// Go 1.23 requires the remote's package to authorize the pull with a matching one-arg handle, which
// the converter honors by emitting that remote public (see packageVarAccess) — so the forwarding
// property compiles across the assembly boundary. The comment may sit on the GenDecl or the spec.
func varLinknamePull(name string, docs ...*ast.CommentGroup) (ref string, pkgPath string, ok bool) {
	for _, doc := range docs {
		if doc == nil {
			continue
		}

		for _, comment := range doc.List {
			fields := strings.Fields(comment.Text)

			// //go:linkname <local> <pkgpath>.<remote>
			if len(fields) != 3 || fields[0] != "//go:linkname" || fields[1] != name {
				continue
			}

			target := fields[2]
			dot := strings.LastIndex(target, ".")

			if dot < 0 {
				continue
			}

			pkgPath = target[:dot]

			// A pull whose forwarding reference would form a project-ref cycle keeps its null-field
			// form (the pre-feature behavior) — runtime pulling internal/syscall/windows.CanUseLongPaths.
			if linknamePullWouldCycle(pkgPath) {
				return "", "", false
			}

			remote := getSanitizedIdentifier(target[dot+1:])
			class := RootNamespace + "." + convertImportPathToNamespace(pkgPath, PackageSuffix)

			return class + "." + remote, pkgPath, true
		}
	}

	return "", "", false
}
