// initOrderOperations.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"os"
	"path/filepath"
	"sort"
	"strings"
)

// PackageInitFileName is the synthetic per-package file that carries the ordered static
// constructor for package-level vars whose initialization order cannot be reproduced by C#'s
// static-field-initializer execution order (see packageMovedInitVars). Named to not collide with
// any converted `<gofile>.cs` (no stdlib Go source file is named package_init.go).
const PackageInitFileName = "package_init.cs"

// packageMovedInitVars holds the package-level vars whose initializer must be RELOCATED into an
// ordered static constructor, mapped to their InitOrder ordinal. Go initializes package-level vars
// in dependency order (types.Info.InitOrder); C# runs static field initializers in textual order
// WITHIN a class part and in an unspecified order ACROSS parts (separate source files). So an
// initializer whose Go dependencies include a package var that is declared in a DIFFERENT file
// (cross-part order is undefined — syscall's `Stdin = getStdHandle(…)` read zsyscall_windows.go's
// `procGetStdHandle` while still nil), or LATER in the SAME file (a forward reference C# reads as
// the zero value), or that was itself relocated (its value is only assigned in the ctor, after
// every field initializer) cannot stay a C# field initializer. Those vars are emitted as BARE
// fields plus a tiny `initᴛ<name>()` method IN THEIR HOME FILE (so the rendered expression keeps
// the file's own using aliases), and a per-package `static <pkg>_package()` ctor (package_init.cs)
// calls the methods in InitOrder — which, because C# runs all static field initializers (every
// part) BEFORE any static-ctor body, is guaranteed to see every non-relocated dependency already
// initialized. Dependencies are resolved TRANSITIVELY through same-package function/method bodies
// and function literals, mirroring Go's own initialization-order analysis (spec: "references …
// directly or through function calls"). Keyed by the var's types.Object (interned per variable).
var packageMovedInitVars map[types.Object]int

// packageMovedInitMethods maps each relocated initializer's InitOrder ordinal to the name of the
// per-file init method that performs the assignment; the package_init.cs ctor calls them in
// ordinal order.
var packageMovedInitMethods map[int]string

// collectMovedInitVars flags the package-level var initializers that must be relocated into the
// ordered static constructor (see packageMovedInitVars). It walks types.Info.InitOrder — Go's
// dependency-sorted list of package-level initializers — resolving each initializer's same-package
// var dependencies transitively through called/referenced package functions (and func-literal
// bodies, which Go's own analysis also treats as references), then flags the initializer when a
// dependency is cross-file, a same-file forward reference, or itself already flagged.
func collectMovedInitVars(fset *token.FileSet, pkg *types.Package, info *types.Info, syntax []*ast.File) {
	scope := pkg.Scope()

	// Per-function direct references: package-level vars read and package functions/methods
	// referenced by each function body in the package (methods included — their *types.Func is
	// the map key either way). Built over the FULL syntax set (including files skipped for
	// manual conversion — dependency analysis is about Go semantics, not emission).
	funcVarRefs := map[*types.Func]map[*types.Var]bool{}
	funcFuncRefs := map[*types.Func]map[*types.Func]bool{}

	collectRefs := func(node ast.Node, vars map[*types.Var]bool, funcs map[*types.Func]bool) {
		ast.Inspect(node, func(n ast.Node) bool {
			ident, ok := n.(*ast.Ident)

			if !ok {
				return true
			}

			switch obj := info.Uses[ident].(type) {
			case *types.Var:
				if obj.Parent() == scope {
					vars[obj] = true
				}
			case *types.Func:
				funcs[obj] = true
			}

			return true
		})
	}

	for _, file := range syntax {
		for _, decl := range file.Decls {
			funcDecl, ok := decl.(*ast.FuncDecl)

			if !ok || funcDecl.Body == nil {
				continue
			}

			funcObj, ok := info.Defs[funcDecl.Name].(*types.Func)

			if !ok {
				continue
			}

			vars := map[*types.Var]bool{}
			funcs := map[*types.Func]bool{}
			collectRefs(funcDecl.Body, vars, funcs)
			funcVarRefs[funcObj] = vars
			funcFuncRefs[funcObj] = funcs
		}
	}

	// Transitive closure: all package vars reachable from a function (cycle-safe, memoized).
	closureCache := map[*types.Func]map[*types.Var]bool{}
	inProgress := map[*types.Func]bool{}

	var funcClosure func(fn *types.Func) map[*types.Var]bool

	funcClosure = func(fn *types.Func) map[*types.Var]bool {
		if cached, ok := closureCache[fn]; ok {
			return cached
		}

		if inProgress[fn] {
			return nil // recursion cycle — the outer walk already accumulates these refs
		}

		directVars, hasBody := funcVarRefs[fn]

		if !hasBody {
			return nil // imported/bodyless (asm) function — no same-package var deps
		}

		inProgress[fn] = true
		result := map[*types.Var]bool{}

		for varObj := range directVars {
			result[varObj] = true
		}

		for callee := range funcFuncRefs[fn] {
			for varObj := range funcClosure(callee) {
				result[varObj] = true
			}
		}

		delete(inProgress, fn)
		closureCache[fn] = result

		return result
	}

	fileOf := func(pos token.Pos) string {
		if pos == token.NoPos {
			return ""
		}

		return fset.Position(pos).Filename
	}

	movedVars := map[*types.Var]bool{}

	for ordinal, initializer := range info.InitOrder {
		// Direct references in the RHS — INCLUDING func-literal bodies: Go's analysis counts
		// references inside literals (and an IIFE literal actually executes at init time).
		directVars := map[*types.Var]bool{}
		directFuncs := map[*types.Func]bool{}
		collectRefs(initializer.Rhs, directVars, directFuncs)

		deps := map[*types.Var]bool{}

		for varObj := range directVars {
			deps[varObj] = true
		}

		for fn := range directFuncs {
			for varObj := range funcClosure(fn) {
				deps[varObj] = true
			}
		}

		if len(deps) == 0 {
			continue
		}

		mustMove := false

		for _, lhs := range initializer.Lhs {
			lhsFile := fileOf(lhs.Pos())

			for dep := range deps {
				if movedVars[dep] {
					mustMove = true // depends on a relocated var — only the ctor assigns it
				} else if fileOf(dep.Pos()) != lhsFile {
					mustMove = true // cross-file: C# cross-part initializer order is undefined
				} else if dep.Pos() > lhs.Pos() {
					mustMove = true // same-file forward reference: C# reads the zero value
				}
			}
		}

		if !mustMove {
			continue
		}

		for _, lhs := range initializer.Lhs {
			movedVars[lhs] = true

			// Blank (`_`) initializers exist only for their (discarded) side effect/witness;
			// their value is never read, so their init order is immaterial. Leaving them inline
			// keeps the emission simple and never mis-orders a real reader.
			if lhs.Name() != "_" {
				packageMovedInitVars[lhs] = ordinal
			}
		}
	}
}

// movedInitOrdinal reports whether obj is a relocated package-var initializer and, if so, its
// InitOrder ordinal (used to sequence the ctor's init-method calls).
func (v *Visitor) movedInitOrdinal(obj types.Object) (int, bool) {
	if packageMovedInitVars == nil || obj == nil {
		return 0, false
	}

	ordinal, ok := packageMovedInitVars[obj]
	return ordinal, ok
}

// packageInitMethodName composes the per-file init method name for a relocated var — the
// TempVarMarker keeps it collision-free (no Go identifier can contain it), and a keyword-escaped
// var name (`@base`) drops its escape (the composed name is not a keyword).
func packageInitMethodName(csIDName string) string {
	return "init" + TempVarMarker + strings.TrimPrefix(csIDName, "@")
}

// recordMovedInitMethod registers a relocated initializer's per-file method for the ordered ctor.
func recordMovedInitMethod(ordinal int, methodName string) {
	packageLock.Lock()
	packageMovedInitMethods[ordinal] = methodName
	packageLock.Unlock()
}

// writePackageInitFile emits the synthetic per-package init file whose static constructor calls
// each relocated initializer's per-file init method in Go's InitOrder. No-op when nothing was
// relocated. The methods live in their home files, so this file needs no using directives.
func writePackageInitFile(outputDir, packageNamespace, packageName string) error {
	if len(packageMovedInitMethods) == 0 {
		return nil
	}

	packageClassName := getSanitizedImport(fmt.Sprintf("%s%s", packageName, PackageSuffix))

	ordinals := make([]int, 0, len(packageMovedInitMethods))

	for ordinal := range packageMovedInitMethods {
		ordinals = append(ordinals, ordinal)
	}

	sort.Ints(ordinals)

	var sb strings.Builder

	sb.WriteString("// Code generated by go2cs. DO NOT EDIT.\r\n")
	sb.WriteString("// Package-level variable initialization ordered to match Go's dependency order\r\n")
	sb.WriteString("// (types.Info.InitOrder), where C#'s static field initializer order (undefined\r\n")
	sb.WriteString(fmt.Sprintf("// across partial class files) would differ. Each init%s method lives beside its\r\n", TempVarMarker))
	sb.WriteString("// variable's declaration; C# runs every static field initializer before this\r\n")
	sb.WriteString("// constructor body, so all non-relocated dependencies are already initialized.\r\n")
	sb.WriteString(fmt.Sprintf("namespace %s;\r\n\r\n", packageNamespace))
	sb.WriteString(fmt.Sprintf("partial class %s {\r\n", packageClassName))
	sb.WriteString(fmt.Sprintf("    static %s() {\r\n", packageClassName))

	for _, ordinal := range ordinals {
		sb.WriteString("        ")
		sb.WriteString(packageMovedInitMethods[ordinal])
		sb.WriteString("();\r\n")
	}

	sb.WriteString("    }\r\n")
	sb.WriteString(fmt.Sprintf("} // end %s\r\n", packageClassName))

	return os.WriteFile(filepath.Join(outputDir, PackageInitFileName), []byte(sb.String()), 0644)
}
