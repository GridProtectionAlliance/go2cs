package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
)

// packageAddressedGlobals holds the package-level value vars whose address is taken
// somewhere in the package (via &g, &g.field, or &g[i]). Such a global must be backed
// by a heap box (ж<T>) so the pointer references the original storage rather than a
// copy — `Ꮡ(value)` heap-allocates a copy, which silently breaks `&global` mutation.
// Populated by a synchronous pre-pass over all files (so cross-file address-taking is
// visible at the global's declaration), then read-only during concurrent file visiting.
// Keyed by the var's types.Object, which is interned per variable across files.
var packageAddressedGlobals map[types.Object]bool

// collectAddressedGlobals scans every file for address-of expressions rooted at a
// package-level var and records those vars in packageAddressedGlobals.
func collectAddressedGlobals(files []FileEntry, pkg *types.Package, info *types.Info) {
	for _, fileEntry := range files {
		ast.Inspect(fileEntry.file, func(n ast.Node) bool {
			unaryExpr, ok := n.(*ast.UnaryExpr)

			if !ok || unaryExpr.Op != token.AND {
				return true
			}

			// Peel field selectors and index expressions down to the root operand,
			// e.g. &G.X or &G[i] both make G escape.
			root := unaryExpr.X

			for {
				switch expr := root.(type) {
				case *ast.SelectorExpr:
					root = expr.X
					continue
				case *ast.IndexExpr:
					root = expr.X
					continue
				case *ast.ParenExpr:
					root = expr.X
					continue
				}

				break
			}

			if ident, ok := root.(*ast.Ident); ok {
				if varObj, ok := info.Uses[ident].(*types.Var); ok && varObj.Parent() == pkg.Scope() {
					packageAddressedGlobals[varObj] = true
				}
			}

			return true
		})
	}
}

// writeAddressedGlobalDecl emits a package-level var that is backed by a heap box so
// `&global` (emitted as the "Ꮡname" identifier) references the original storage. The
// box holds the value; the var name becomes a ref-returning property over the box, so
// reads/writes of the global are unchanged. An empty initExpr defaults the value.
func (v *Visitor) writeAddressedGlobalDecl(access, csTypeName, csIDName, initExpr string) {
	box := AddressPrefix + csIDName

	if len(initExpr) == 0 {
		// Use an explicitly typed default so the ж(in T value) constructor is chosen
		// (a bare `default` would bind to the ж(NilType) ctor and yield a nil box).
		initExpr = fmt.Sprintf("default(%s)", csTypeName)
	}

	v.writeOutput("%s static %s<%s> %s = new(%s);", access, PointerPrefix, csTypeName, box, initExpr)
	v.targetFile.WriteString(v.newline)
	v.writeOutput("%s static ref %s %s => ref %s.val;", access, csTypeName, csIDName, box)
}

// isAddressedGlobal reports whether the identifier resolves to a package-level var
// whose address is taken in the package (and so is backed by a heap box).
func (v *Visitor) isAddressedGlobal(ident *ast.Ident) bool {
	if ident == nil || packageAddressedGlobals == nil {
		return false
	}

	obj := v.info.Uses[ident]

	if obj == nil {
		obj = v.info.Defs[ident]
	}

	return obj != nil && packageAddressedGlobals[obj]
}
