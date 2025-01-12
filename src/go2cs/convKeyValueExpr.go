package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) convKeyValueExpr(keyValueExpr *ast.KeyValueExpr, context KeyValueContext) string {
	if context.source == StructSource {
		// Struct field initializer
		return fmt.Sprintf("%s: %s", v.convExpr(keyValueExpr.Key, nil), v.convExpr(keyValueExpr.Value, nil))
	} else if context.source == MapSource {
		// Map key/value initializer
		return fmt.Sprintf("[%s] = %s", v.convExpr(keyValueExpr.Key, nil), v.convExpr(keyValueExpr.Value, nil))
	} else if context.source == ArraySource {
		// Sparse array initializer
		return fmt.Sprintf("%s[%s] = %s", context.ident, v.convExpr(keyValueExpr.Key, nil), v.convExpr(keyValueExpr.Value, nil))
	} else {
		panic(fmt.Sprintf("Unexpected key/value source: %#v", context.source))
	}
}
