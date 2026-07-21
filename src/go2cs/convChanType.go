// convChanType.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"go/ast"
)

func (v *Visitor) convChanType(chanType *ast.ChanType) string {
	return convertToCSTypeName(v.getExprTypeName(chanType, false))
}
