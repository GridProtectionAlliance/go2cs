package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) convKeyValueExpr(keyValueExpr *ast.KeyValueExpr, context KeyValueContext) string {
	valueExpr := v.convExpr(keyValueExpr.Value, nil)

	if context.ident != nil {
		keySourceType := v.getIdentType(context.ident)

		if keySourceType != nil {
			if needsInterfaceCast, isEmpty := isInterface(keySourceType); needsInterfaceCast && !isEmpty {
				valueType := v.getExprType(keyValueExpr.Value)
				valueExpr = v.convertToInterfaceType(keySourceType, valueType, valueExpr)
			}
		}
	}

	if context.source == StructSource {
		// Struct field initializer
		return fmt.Sprintf("%s: %s", v.convExpr(keyValueExpr.Key, nil), valueExpr)
	} else if context.source == MapSource {
		// Map key/value initializer
		return fmt.Sprintf("[%s] = %s", v.convExpr(keyValueExpr.Key, nil), valueExpr)
	} else if context.source == ArraySource {
		// Sparse array initializer
		return fmt.Sprintf("%s[%s] = %s", context.ident, v.convExpr(keyValueExpr.Key, nil), valueExpr)
	} else {
		panic(fmt.Sprintf("Unexpected key/value source: %#v", context.source))
	}
}
