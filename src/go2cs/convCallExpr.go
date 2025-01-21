package main

import (
	"fmt"
	"go/ast"
	"go/types"
)

func (v *Visitor) convCallExpr(callExpr *ast.CallExpr, context LambdaContext) string {
	if ok, typeName := v.isTypeConversion(callExpr); ok {
		arg := callExpr.Args[0]
		expr := v.convExpr(arg, nil)
		typeName = convertToCSTypeName(typeName)

		// Determine if we need parentheses around the expression
		if v.needsParentheses(arg) {
			return fmt.Sprintf("((%s)(%s))", getSanitizedIdentifier(typeName), expr)
		}

		return fmt.Sprintf("((%s)%s)", getSanitizedIdentifier(typeName), expr)
	}

	constructType := ""

	if v.isConstructorCall(callExpr) {
		constructType = "new "
	}

	// u8 readonly spans cannot be used as arguments to functions that take interface parameters
	callExprContext := DefaultCallExprContext()

	// Check if the call is using the spread operator "..."
	if callExpr.Ellipsis.IsValid() {
		callExprContext.hasSpreadOperator = true
	}

	// Check if any parameters of callExpr.Fun are interface or pointer types
	if funType, ok := v.info.TypeOf(callExpr.Fun).(*types.Signature); ok {
		for i := 0; i < funType.Params().Len(); i++ {
			var paramType types.Type

			callExprContext.u8StringArgOK[i] = true

			var funcName string

			funcIdent := getIdentifier(callExpr.Fun)

			if funcIdent != nil {
				funcName = funcIdent.Name
			}

			// Handle builtin functions that take `...Type` parameters, treat as `interface{}`
			if funcName == "print" || funcName == "println" {
				paramType = types.NewInterfaceType(nil, nil)
			} else if paramType, ok = getParameterType(funType, i); !ok {
				continue
			}

			if needsInterfaceCast, isEmpty := isInterface(paramType); needsInterfaceCast {
				callExprContext.u8StringArgOK[i] = false

				if !isEmpty {
					callExprContext.interfaceType = paramType
				}
			} else if isPointer(paramType) {
				ident := getIdentifier(callExpr.Args[i])

				if !v.isPointer(ident) || v.identIsParameter(ident) {
					callExprContext.argTypeIsPtr[i] = true
				}
			}
		}
	}

	if ident, ok := callExpr.Fun.(*ast.Ident); ok {
		// Handle make call as a special case
		if ident.Name == "make" {
			typeExpr := callExpr.Args[0]
			typeParam := v.info.TypeOf(typeExpr)

			var typeName string

			if ident := getIdentifier(typeExpr); ident != nil {
				typeName = convertToCSTypeName(ident.Name)
			} else {
				typeName = v.getPrintedNode(typeExpr)
				println(fmt.Sprintf("WARNING: @convCallExpr - Failed to resolve `make` type argument %s", typeName))
				typeName = fmt.Sprintf("/* %s */", typeName)
			}

			remainingArgs := v.convExprList(callExpr.Args[1:], callExpr.Lparen, callExprContext)

			if typeParam != nil {
				if _, ok := typeParam.(*types.Slice); ok {
					if v.options.preferVarDecl {
						return fmt.Sprintf("new slice<%s>(%s)", typeName, remainingArgs)
					}

					return fmt.Sprintf("new(%s)", remainingArgs)
				} else if _, ok := typeParam.(*types.Map); ok {
					if v.options.preferVarDecl {
						if mapExpr, ok := typeExpr.(*ast.MapType); ok {
							ident = getIdentifier(mapExpr.Value)
							valueTypeName := convertToCSTypeName(ident.Name)
							return fmt.Sprintf("new map<%s, %s>(%s)", typeName, valueTypeName, remainingArgs)
						} else {
							println(fmt.Sprintf("WARNING: @convCallExpr - Failed to resolve `make` map value type argument %s", typeName))
						}
					}

					return fmt.Sprintf("new(%s)", remainingArgs)
				} else if _, ok := typeParam.(*types.Chan); ok {
					if len(remainingArgs) == 0 {
						remainingArgs = "1"
					}

					if v.options.preferVarDecl {
						return fmt.Sprintf("new channel<%s>(%s)", typeName, remainingArgs)
					}

					return fmt.Sprintf("new(%s)", remainingArgs)
				}
			}

			println(fmt.Sprintf("INFO: @convCallExpr - call to generic `make` method is sub-optimal, consider adding custom make case for %s", typeName))
			return fmt.Sprintf("make<%s>(%s)", typeName, remainingArgs)
		}

		// Handle new call as a special case
		if ident.Name == "new" {
			typeExpr := callExpr.Args[0]

			if ident := getIdentifier(typeExpr); ident != nil {
				return fmt.Sprintf("@new<%s>()", ident)
			}

			typeName := v.getPrintedNode(typeExpr)
			println(fmt.Sprintf("WARNING: @convCallExpr - Failed to resolve `new` type argument %s", typeName))
			typeName = fmt.Sprintf("/* %s */", typeName)
			return fmt.Sprintf("@new<%s>()", typeName)
		}
	}

	lambdaContext := DefaultLambdaContext()
	lambdaContext.isCallExpr = true
	lambdaContext.deferredDecls = context.deferredDecls

	funType := v.getTypeName(callExpr.Fun, true)
	callExprContext.sourceIsRuneArray = funType == "[]rune"

	return fmt.Sprintf("%s%s(%s)", constructType, v.convExpr(callExpr.Fun, []ExprContext{lambdaContext}), v.convExprList(callExpr.Args, callExpr.Lparen, callExprContext))
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

func (v *Visitor) needsParentheses(expr ast.Expr) bool {
	switch expr.(type) {
	case *ast.Ident:
		return false
	case *ast.CallExpr:
		return false
	case *ast.SelectorExpr:
		return false
	case *ast.BasicLit:
		return false
	case *ast.CompositeLit:
		return false
	case *ast.ParenExpr:
		return false
	case *ast.UnaryExpr:
		// Unary expressions like -x or !x need parentheses
		return true
	case *ast.BinaryExpr:
		// Binary expressions like x + y need parentheses
		return true
	case *ast.IndexExpr:
		// Array/slice indexing like arr[i] doesn't need parentheses
		return false
	default:
		// For any other expression types, err on the side of caution
		return true
	}
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
