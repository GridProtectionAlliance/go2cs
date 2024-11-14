package main

import (
	"fmt"
	"go/ast"
	"go/types"
)

func (v *Visitor) convCallExpr(callExpr *ast.CallExpr) string {
	if ok, typeName := v.isTypeConversion(callExpr); ok {
		return fmt.Sprintf("(%s)(%s)", typeName, v.convExpr(callExpr.Args[0], nil))
	}

	constructType := ""

	if v.isConstructorCall(callExpr) {
		constructType = "new "
	}

	// u8 readonly spans cannot be used as arguments to functions that take interface parameters
	context := DefaultCallExprContext()

	// Check if any parameters of callExpr.Fun are interface or pointer types
	if funType, ok := v.info.TypeOf(callExpr.Fun).(*types.Signature); ok {
		for i := 0; i < funType.Params().Len(); i++ {
			var paramType types.Type

			context.u8StringArgOK[i] = true

			if paramType, ok = getParameterType(funType, i); !ok {
				continue
			}

			if result, _ := isInterface(paramType); result {
				context.u8StringArgOK[i] = false
			} else if isPointer(paramType) {
				ident := getIdentifier(callExpr.Args[i])

				if !v.isPointer(ident) || v.identIsParameter(ident) {
					context.argTypeIsPtr[i] = true
				}
			}
		}
	}

	// Handle make call as a special case
	if ident, ok := callExpr.Fun.(*ast.Ident); ok && ident.Name == "make" {
		typeParam := v.info.TypeOf(callExpr.Args[0])
		typeName := v.convExpr(callExpr.Args[0], nil)
		remainingArgs := v.convExprList(callExpr.Args[1:], callExpr.Lparen, context)

		if typeParam != nil {
			if _, ok := typeParam.(*types.Slice); ok {
				return fmt.Sprintf("new %s(%s)", typeName, remainingArgs)
			} else if _, ok := typeParam.(*types.Map); ok {
				return fmt.Sprintf("new %s(%s)", typeName, remainingArgs)
			} else if _, ok := typeParam.(*types.Chan); ok {
				return fmt.Sprintf("new %s(%s)", typeName, remainingArgs)
			}
		}

		return fmt.Sprintf("make<%s>(%s)", typeName, remainingArgs)
	}

	return fmt.Sprintf("%s%s(%s)", constructType, getSanitizedIdentifier(v.convExpr(callExpr.Fun, nil)), v.convExprList(callExpr.Args, callExpr.Lparen, context))
}

func (v *Visitor) isTypeConversion(callExpr *ast.CallExpr) (bool, string) {
	// Get the object associated with the function being called
	var obj types.Object

	switch funExpr := callExpr.Fun.(type) {
	case *ast.Ident:
		obj = v.info.ObjectOf(funExpr)
	case *ast.SelectorExpr:
		obj = v.info.ObjectOf(funExpr.Sel)
	default:
		return false, ""
	}
	if obj == nil {
		return false, ""
	}

	// Check if the function being called is a type name
	typeName, ok := obj.(*types.TypeName)

	if !ok {
		return false, ""
	}

	// Get the target type
	targetType := typeName.Type()

	// Type conversions typically have exactly one argument
	if len(callExpr.Args) != 1 {
		return false, ""
	}

	// Get the type of the argument
	argType := v.info.TypeOf(callExpr.Args[0])

	// Check if the argument type is convertible to the target type
	return types.ConvertibleTo(argType, targetType), typeName.Name()
}

func (v *Visitor) isConstructorCall(callExpr *ast.CallExpr) bool {
	// Get the object associated with the function being called
	var obj types.Object

	switch funExpr := callExpr.Fun.(type) {
	case *ast.Ident:
		obj = v.info.ObjectOf(funExpr)
	case *ast.SelectorExpr:
		obj = v.info.ObjectOf(funExpr.Sel)
	default:
		return false
	}

	if obj == nil {
		return false
	}

	// Determine if the object is a type name
	switch obj.(type) {
	case *types.TypeName:
		// The function being called is a type (constructor call)
		return true
	case *types.Builtin:
		// Built-in functions like len, cap, etc.
		return false
	case *types.Func, *types.Var:
		// Regular functions or variables of function type
		return false
	default:
		return false
	}
}
