package main

import (
	"fmt"
	"go/ast"
	"go/types"
)

func (v *Visitor) convKeyValueExpr(keyValueExpr *ast.KeyValueExpr, context KeyValueContext) string {
	// For a struct field initializer whose field is a pointer type, the value must be emitted as a
	// pointer (box) — e.g. a deref'd pointer parameter `val` used as `key: val` where `key` is `*V`
	// needs `Ꮡval` (like the same value passed as a pointer call argument). Resolve the field from
	// the key name so this works for keyed literals regardless of field order.
	var valueContexts []ExprContext

	if context.source == StructSource {
		if keyIdent, ok := keyValueExpr.Key.(*ast.Ident); ok {
			if fieldObj := v.info.Uses[keyIdent]; fieldObj != nil {
				if _, isPtr := fieldObj.Type().(*types.Pointer); isPtr {
					identContext := DefaultIdentContext()
					identContext.isPointer = true
					valueContexts = []ExprContext{identContext}
				}
			}
		}
	}

	valueExpr := v.convExpr(keyValueExpr.Value, valueContexts)

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
