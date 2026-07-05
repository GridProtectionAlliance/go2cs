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
// package-level var and records those vars in packageAddressedGlobals. It also records a
// package-level value var on which a capture-mode method is called (`var locked atomic.Int32;
// locked.CompareAndSwap(...)`) — such a method needs the receiver box (`Ꮡlocked`), which the
// heap-box backing supplies and convSelectorExpr routes the call through, exactly as for an
// explicitly address-taken global. Runs after collectCaptureModeMethods, so the capture-mode
// set is populated.
func collectAddressedGlobals(files []FileEntry, pkg *types.Package, info *types.Info) {
	for _, fileEntry := range files {
		ast.Inspect(fileEntry.file, func(n ast.Node) bool {
			switch node := n.(type) {
			case *ast.UnaryExpr:
				if node.Op != token.AND {
					return true
				}

				// Peel field selectors and index expressions down to the root operand,
				// e.g. &G.X or &G[i] both make G escape.
				root := node.X

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

			case *ast.CallExpr:
				// A capture-mode / direct-ж method called on a package-level value global — directly
				// (`var locked atomic.Int32; locked.CompareAndSwap(…)`) or on a value FIELD of one
				// (`prof.signalLock.Store(…)`, `Δscavenge.gcPercentGoal.Store(…)`) — needs that global
				// heap-boxed so the receiver box (`Ꮡprof` → `Ꮡprof.of(T.Ꮡfield)`) exists and the call
				// routes through it. Such a method is emitted with only a `ж<T>` (box) receiver, so a
				// plain value/ref of the field cannot bind it (CS1929).
				selectorExpr, ok := node.Fun.(*ast.SelectorExpr)

				if !ok {
					return true
				}

				// Peel value field selectors to the receiver root. Bail at a pointer hop: beyond a
				// pointer the field address is already real (no global boxing needed), and that path
				// is intentionally NOT handled here to avoid disturbing pointer-receiver/param fields.
				recv := selectorExpr.X

				for {
					if t := info.TypeOf(recv); t != nil {
						if _, isPtr := t.Underlying().(*types.Pointer); isPtr {
							return true
						}
					}

					switch r := recv.(type) {
					case *ast.SelectorExpr:
						recv = r.X
						continue
					case *ast.IndexExpr:
						// A package-level value ARRAY whose element has a pointer-receiver method called on
						// it (`matchPool[i].Get()`, regexp's [N]sync.Pool) needs the array boxed so the
						// element address `Ꮡmatchpool.at<T>(i)` resolves (CS0103).
						recv = r.X
						continue
					}

					break
				}

				ident, ok := recv.(*ast.Ident)

				if !ok {
					return true
				}

				varObj, ok := info.Uses[ident].(*types.Var)

				if !ok || varObj.Parent() != pkg.Scope() {
					return true
				}

				// A pointer global already carries its box; only value globals need boxing.
				if _, isPtr := varObj.Type().(*types.Pointer); isPtr {
					return true
				}

				funcObj, ok := info.ObjectOf(selectorExpr.Sel).(*types.Func)

				if !ok || funcObj == nil {
					return true
				}

				// Box for a capture-mode method (known same-package) OR any pointer-receiver method —
				// the latter covers cross-package atomic methods (`func (x *Uint32) Store`), whose
				// capture-mode status is not in this package's set but which are likewise ж-only.
				shouldBox := packageCaptureModeMethods != nil && packageCaptureModeMethods[funcObj.Origin()]

				if !shouldBox {
					if sig, ok := funcObj.Type().(*types.Signature); ok && sig.Recv() != nil {
						_, shouldBox = sig.Recv().Type().(*types.Pointer)
					}
				}

				if shouldBox {
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
func (v *Visitor) writeAddressedGlobalDecl(access, csTypeName, csIDName, initExpr string, valueIsRefLike bool) {
	box := AddressPrefix + csIDName

	if len(initExpr) == 0 {
		// Use an explicitly typed default so the ж(in T value) constructor is chosen
		// (a bare `default` would bind to the ж(NilType) ctor and yield a nil box).
		initExpr = fmt.Sprintf("default(%s)", csTypeName)
	}

	// A REFERENCE-LIKE valued global (`var head *node`) reads the HELD value through the box,
	// which may legitimately be nil (Go reads a nil pointer global freely; only DEREFERENCING
	// it panics). The strict `val` nil-checks the slot, so the property reads `ValueSlot`
	// (the identical real slot, no check); a plain value global keeps the strict `val`.
	accessor := "Value"

	if valueIsRefLike {
		accessor = "ValueSlot"
	}

	v.writeOutput("%s static %s<%s> %s = new(%s);", access, PointerPrefix, csTypeName, box, initExpr)
	v.targetFile.WriteString(v.newline)
	v.writeOutput("%s static ref %s %s => ref %s.%s;", access, csTypeName, csIDName, box, accessor)
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
