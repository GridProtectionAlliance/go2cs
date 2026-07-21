// nilArgParamOperations.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"go/ast"
	"go/types"
)

// packageNilArgPtrParams maps a function to the set of its POINTER-parameter indices that receive
// the untyped `nil` at some call site in the package. Such a parameter can legally be nil at run
// time even when the body never nil-COMPARES it — the deref sits behind an ordinary value guard
// (text/scanner's `digits(ch0 rune, base int, invalid *rune)` derefs `*invalid` only under
// `ch >= max`, and is called `digits(ch, 10, nil)`). collectNilSafePtrParams' body scan sees no
// `invalid == nil`, so without this the eager entry deref alias `ref var invalid = ref Ꮡinvalid.Value`
// throws a nil-pointer panic at function entry where Go never dereferences. Recording the nil-passed
// parameter positions lets that alias use the nil-safe accessor (DerefOrNil) instead.
//
// Populated by a synchronous pre-pass over all files (so cross-file nil call sites are visible at the
// callee's declaration), then read-only during file visiting. Keyed by *types.Func, interned per
// function across files. SAME-PACKAGE call sites only — a parameter passed nil solely from another
// package is not covered (the converter processes one package at a time); that residual case keeps
// the strict `.Value` form, exactly as before.
var packageNilArgPtrParams map[*types.Func]HashSet[int]

// collectNilArgPtrParams scans every call site in the package and records, per callee function, the
// positional indices at which a pointer parameter is passed the untyped `nil`.
func collectNilArgPtrParams(files []FileEntry, info *types.Info) {
	for _, fileEntry := range files {
		ast.Inspect(fileEntry.file, func(n ast.Node) bool {
			call, ok := n.(*ast.CallExpr)

			if !ok {
				return true
			}

			fn := calleeFuncOf(call, info)

			if fn == nil {
				return true
			}

			sig, ok := fn.Type().(*types.Signature)

			if !ok {
				return true
			}

			params := sig.Params()

			for i, arg := range call.Args {
				// Only fixed (non-variadic-tail) positions map one-to-one to a declared parameter;
				// the variadic parameter is a slice, excluded by the pointer-type test below anyway.
				if i >= params.Len() {
					break
				}

				if !argIsUntypedNil(arg, info) {
					continue
				}

				if _, isPtr := params.At(i).Type().Underlying().(*types.Pointer); !isPtr {
					continue
				}

				indices := packageNilArgPtrParams[fn]

				if indices == nil {
					indices = HashSet[int]{}
					packageNilArgPtrParams[fn] = indices
				}

				indices.Add(i)
			}

			return true
		})
	}
}

// calleeFuncOf resolves a call's callee to its *types.Func — a plain function or package-qualified
// call (`f(…)`, `pkg.F(…)`) via info.Uses, or a method call (`x.m(…)`) via info.Selections. Returns
// nil for a call through a func value, a builtin, or a conversion (no single declared callee).
func calleeFuncOf(call *ast.CallExpr, info *types.Info) *types.Func {
	fun := call.Fun

	for {
		if paren, ok := fun.(*ast.ParenExpr); ok {
			fun = paren.X
			continue
		}

		break
	}

	switch expr := fun.(type) {
	case *ast.Ident:
		if fn, ok := info.Uses[expr].(*types.Func); ok {
			return fn
		}
	case *ast.SelectorExpr:
		if sel, ok := info.Selections[expr]; ok {
			if fn, ok := sel.Obj().(*types.Func); ok {
				return fn
			}
		} else if fn, ok := info.Uses[expr.Sel].(*types.Func); ok {
			// Package-qualified call (`pkg.F`) — no selection entry, the Sel resolves directly.
			return fn
		}
	}

	return nil
}

// argIsUntypedNil reports whether arg is the predeclared `nil` identifier (not a shadowing local,
// and not an `&x` / typed-nil expression).
func argIsUntypedNil(arg ast.Expr, info *types.Info) bool {
	ident, ok := arg.(*ast.Ident)

	if !ok || ident.Name != "nil" {
		return false
	}

	if obj := info.Uses[ident]; obj != nil {
		return obj == types.Universe.Lookup("nil")
	}

	if tv, ok := info.Types[arg]; ok {
		return tv.Type == types.Typ[types.UntypedNil]
	}

	return false
}
