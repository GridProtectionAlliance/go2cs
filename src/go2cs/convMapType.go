// convMapType.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) convMapType(mapType *ast.MapType) string {
	if v.options.preferVarDecl {
		var mapKeyTypeName, mapValueTypeName string

		mapKeyTypeName = convertToCSTypeName(v.getExprTypeName(mapType.Key, false))
		mapValueTypeName = convertToCSTypeName(v.getExprTypeName(mapType.Value, false))

		return fmt.Sprintf("map<%s, %s>", mapKeyTypeName, mapValueTypeName)
	}

	return "()"
}
