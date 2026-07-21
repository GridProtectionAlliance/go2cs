// convIndexListExpr.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) convIndexListExpr(indexListExpr *ast.IndexListExpr) string {
	callExprContext := DefaultCallExprContext()
	callExprContext.sourceIsTypeParams = true

	// The base (X) renders WITHOUT its own generic type arguments — this appends the explicit
	// `<Indices>` here, so convSelectorExpr must not also append the inferred instance args
	// (`concurrent.NewHashTrieMap<K, V><K, V>` — unique, CS1525 cascade). See convIndexExpr.
	xContext := DefaultLambdaContext()
	xContext.suppressGenericTypeArgs = true

	// An explicitly written instantiation of a pointer-core generic drops its ERASED positions
	// (`clone[*thing, thing]` → `clone<thing>`, see explicitTypeArgsAfterErasure); a list that
	// erases to empty leaves the base bare (C# infers from the remaining value arguments).
	indices := indexListExpr.Indices

	if kept, erased := v.explicitTypeArgsAfterErasure(indexListExpr.X, indices); erased {
		if len(kept) == 0 {
			return v.convExpr(indexListExpr.X, []ExprContext{xContext})
		}

		indices = kept
	}

	return fmt.Sprintf("%s<%s>", v.convExpr(indexListExpr.X, []ExprContext{xContext}), v.convExprList(indices, indexListExpr.Lbrack, callExprContext))
}
