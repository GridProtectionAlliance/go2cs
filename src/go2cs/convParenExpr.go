package main

import (
	"fmt"
	"go/ast"
)

func (v *Visitor) convParenExpr(parenExpr *ast.ParenExpr, context LambdaContext) string {
	starContext := DefaultStarExprContext()
	starContext.inParenExpr = true

	// In a pointer cast, we need to intermediately cast the target expression to an uintptr.
	// This is required since unsafe.Pointer is in its own library and no implicit cast can
	// be added for it on the pointer class (ж<T>) in the core library without creating a
	// circular dependency. Although C# allows circular dependencies, NuGet does not. If the
	// target happens to not be an unsafe pointer, the cast is still safe since all pointer
	// types support this cast operation.
	if context.isPointerCast {
		return fmt.Sprintf("(%s)(uintptr)", v.convExpr(parenExpr.X, []ExprContext{starContext}))
	}

	return fmt.Sprintf("(%s)", v.convExpr(parenExpr.X, []ExprContext{starContext}))
}
