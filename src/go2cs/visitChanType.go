// visitChanType.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"go/ast"
	"go/types"
)

// Handles channel types in context of a TypeSpec
func (v *Visitor) visitChanType(chanType *ast.ChanType, identType types.Type, name string) {
	// A defined channel type — `type closeWaiter chan struct{}` (net/http's h2 bundle) — emits
	// the `[GoType("chan T")] partial struct` forward declaration whose Channel template
	// go2cs-gen implements (the wrapper holds a `channel<T>` and forwards its send/receive/
	// select surface). The old stub emitted only a comment, leaving the type undeclared
	// (CS0246). The descriptor is direction-agnostic: golib's `channel<T>` does not model
	// `<-chan`/`chan<-` restrictions, so a directional named type wraps the same surface.
	elemType := convertToCSTypeName(v.getTypeName(v.info.TypeOf(chanType.Value), false))
	access := v.pendingTypeAccess
	v.pendingTypeAccess = ""

	// A channel type declared inside a function body cannot be a method-body statement in C#; hoist
	// it to member level (see liftLocalTypeDecl). A package-level declaration is unaffected — target
	// is v.targetFile and finish() is a no-op.
	name, target, finish := v.liftLocalTypeDecl(name, identType)

	if !v.inFunction {
		target.WriteString(v.newline)
	}

	v.writeStringLn(target, "[GoType(\"chan %s\")] %spartial struct %s;", elemType, access, getSanitizedIdentifier(name))
	finish()
}

// namedChanElemTypeArg returns an explicit type-argument list ("<T>") for a golib channel-op
// free-function call (`ᐸꟷ(ch)`, `close(ch)`) whose operand's static type is a NAMED channel
// type: the `[GoType("chan T")]` wrapper struct reaches `channel<T>` only through a
// user-defined conversion, which C# generic inference never considers (CS0411) — naming the
// element type explicitly lets the wrapper's implicit conversion apply at the argument
// instead. A plain `chan T` operand returns "" (emission unchanged, byte-identical).
func (v *Visitor) namedChanElemTypeArg(expr ast.Expr) string {
	named, ok := types.Unalias(v.getExprType(expr)).(*types.Named)

	if !ok {
		return ""
	}

	chanType, ok := named.Underlying().(*types.Chan)

	if !ok {
		return ""
	}

	return fmt.Sprintf("<%s>", convertToCSTypeName(v.getTypeName(chanType.Elem(), false)))
}
