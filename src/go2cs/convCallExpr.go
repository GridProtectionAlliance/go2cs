package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"path/filepath"
	"strings"
)

func (v *Visitor) convCallExpr(callExpr *ast.CallExpr, context LambdaContext) string {
	if ok, targetTypeName := v.isTypeConversion(callExpr); ok {
		arg := callExpr.Args[0]
		expr := v.convExpr(arg, nil)

		targetTypeName = convertToCSTypeName(targetTypeName)
		targetType := v.getType(callExpr.Fun, false)
		argType := v.getType(arg, false)

		var targetTypeIsPointer bool

		// Check if funType is a struct or a pointer to a struct
		if ptrType, ok := targetType.(*types.Pointer); ok {
			targetType = ptrType.Elem().Underlying()
			targetTypeIsPointer = true
		}

		if _, ok := targetType.(*types.Struct); ok {
			// Check if argType is a struct or a pointer to a struct
			if ptrType, ok := argType.(*types.Pointer); ok {
				argType = ptrType.Elem().Underlying()
			}

			if _, ok := argType.(*types.Struct); ok {
				if targetTypeIsPointer {
					// Dereference target type when casting to pointer types,
					// in C# implicit casting operator requires the target type
					// to be a direct type, not a pointer type
					expr = fmt.Sprintf("(%s?.val ?? default!)", expr)
				}

				// If both funcType and argType are structs, track implicit conversions
				packageLock.Lock()

				var targetConversionsMap map[string]HashSet[string]

				if targetTypeIsPointer {
					targetConversionsMap = indirectImplicitConversions
				} else {
					targetConversionsMap = implicitConversions
				}

				argTypeName := v.getCSTypeName(argType)
				var conversions HashSet[string]
				var exists bool

				if conversions, exists = targetConversionsMap[argTypeName]; exists {
					conversions.Add(targetTypeName)
				} else {
					conversions = NewHashSet([]string{targetTypeName})
					targetConversionsMap[argTypeName] = conversions
				}

				v.addImplicitSubStructConversions(argType, targetTypeName, targetTypeIsPointer)

				packageLock.Unlock()
			}
		}

		// Determine if we need parentheses around the expression
		if v.needsParentheses(arg) {
			return fmt.Sprintf("((%s)(%s))", targetTypeName, expr)
		}

		return fmt.Sprintf("((%s)%s)", targetTypeName, expr)
	}

	constructType := ""

	if v.isConstructorCall(callExpr) {
		constructType = "new "
	}

	// u8 readonly spans cannot be used as arguments to functions that take interface parameters
	callExprContext := DefaultCallExprContext()
	callExprContext.callArgs = context.callArgs

	// Check if the call is using the spread operator "..."
	if callExpr.Ellipsis.IsValid() {
		callExprContext.hasSpreadOperator = true
	}

	var replacementArgs []string

	// Check if any parameters of callExpr.Fun are interface or pointer types
	if funType, ok := v.info.TypeOf(callExpr.Fun).(*types.Signature); ok {
		params := funType.Params()

		for i := range params.Len() {
			var paramType types.Type
			paramHasArg := callExpr.Args != nil && i < len(callExpr.Args)

			if paramHasArg {
				// Check if the parameter type is an anonymous struct
				if structType, exprType := v.extractStructType(callExpr.Args[i]); structType != nil {
					v.indentLevel++
					v.visitStructType(structType, exprType, params.At(i).Name(), nil, true)
					v.indentLevel--
				}

				// Check if the parameter type is an anonymous interface
				if interfaceType, exprType := v.extractInterfaceType(callExpr.Args[i]); interfaceType != nil {
					v.indentLevel++
					v.visitInterfaceType(interfaceType, exprType, params.At(i).Name(), nil, true)
					v.indentLevel--
				}
			}

			callExprContext.u8StringArgOK[i] = true
			funcName := v.convExpr(callExpr.Fun, nil)

			// Handle builtin functions that take `...Type` parameters, treat as `interface{}`
			if funcName == "print" || funcName == "println" {
				paramType = types.NewInterfaceType(nil, nil)
			} else if paramType, ok = getParameterType(funType, i); !ok {
				continue
			}

			if paramHasArg {
				argType := v.getType(callExpr.Args[i], false)
				targetType := paramType
				replacementArg := v.checkForDynamicStructs(argType, targetType)

				// If a replacement argument is found, add it to the replacementArgs slice,
				// creating the slice if it doesn't exist yet
				if len(replacementArg) > 0 {
					if replacementArgs == nil {
						replacementArgs = make([]string, params.Len())
					}

					replacementArgs[i] = replacementArg
				}
			}

			if needsInterfaceCast, isEmpty := isInterface(paramType); needsInterfaceCast {
				callExprContext.u8StringArgOK[i] = false

				if !isEmpty {
					callExprContext.interfaceTypes[i] = paramType
				}
			} else if isPointer(paramType) {
				ident := getIdentifier(callExpr.Args[i])

				if !v.isPointer(ident) || v.identIsParameter(ident) {
					callExprContext.argTypeIsPtr[i] = true
				}
			}
		}
	}

	callExprContext.replacementArgs = replacementArgs

	if ident, ok := callExpr.Fun.(*ast.Ident); ok {
		// Handle make call as a special case
		if ident.Name == "make" {
			typeExpr := callExpr.Args[0]
			typeParam := v.info.TypeOf(typeExpr)
			typeName := convertToCSTypeName(v.getExprTypeName(typeExpr, false))
			remainingArgs := v.convExprList(callExpr.Args[1:], callExpr.Lparen, callExprContext)
			isTypeParam := false

			if typeConstraint, ok := typeParam.(*types.TypeParam); ok {
				typeParam = v.getConstraintType(typeConstraint)
				isTypeParam = typeParam != nil
			}

			if typeParam != nil {
				if _, ok := typeParam.(*types.Chan); ok && !isTypeParam {
					if len(remainingArgs) == 0 {
						remainingArgs = "1"
					}
				}

				if isTypeParam {
					return fmt.Sprintf("make<%s>(%s)", typeName, remainingArgs)
				}

				if v.options.preferVarDecl {
					return fmt.Sprintf("new %s(%s)", typeName, remainingArgs)
				}

				return fmt.Sprintf("new(%s)", remainingArgs)
			}

			println(fmt.Sprintf("WARNING: @convCallExpr - unexpected call to `make` method for type '%s'", typeName))
			return fmt.Sprintf("make\u01C3<%s>(%s)", typeName, remainingArgs)
		}

		// Handle new call as a special case
		if ident.Name == "new" {
			typeExpr := callExpr.Args[0]

			typeName := convertToCSTypeName(v.getExprTypeName(typeExpr, false))
			return fmt.Sprintf("@new<%s>()", typeName)
		}
	}

	lambdaContext := DefaultLambdaContext()
	lambdaContext.isCallExpr = true
	lambdaContext.isPointerCast = context.isPointerCast
	lambdaContext.deferredDecls = context.deferredDecls

	funcType := v.getType(callExpr.Fun, false)
	var typeParamExpr string

	resultType := v.info.TypeOf(callExpr)

	if resultType != nil {
		if named, ok := resultType.(*types.Named); ok {
			if named.TypeArgs().Len() > 0 {
				var typeParams []string

				for i := 0; i < named.TypeArgs().Len(); i++ {
					typeParams = append(typeParams, v.getCSTypeName(named.TypeArgs().At(i)))
				}

				typeParamExpr = fmt.Sprintf("<%s>", strings.Join(typeParams, ", "))
			}
		}

		// In a pointer cast, we need to intermediately cast the target expression to an uintptr.
		// This is required since unsafe.Pointer is in its own library and no implicit cast can
		// be added for it on the pointer class (ж<T>) in the core library without creating a
		// circular dependency.
		if resultType.String() == "unsafe.Pointer" {
			if len(constructType) == 0 {
				constructType = "(uintptr)"
			} else if len(callExpr.Args) == 1 {
				// Check if current function is a receiver function
				if v.currentFuncSignature.Recv() != nil {
					// Get the receiver type
					recvType := v.currentFuncSignature.Recv().Type()

					// Check if receiver is a pointer type
					isRecvPointer := false

					if ptrType, ok := recvType.(*types.Pointer); ok {
						recvType = ptrType.Elem()
						isRecvPointer = true
					}

					// Get the unsafe.Pointer call argument type
					argType := v.info.TypeOf(callExpr.Args[0])
					isArgPointer := false

					// Check if the argument is a pointer
					if ptrType, ok := argType.(*types.Pointer); ok {
						argType = ptrType.Elem()
						isArgPointer = true
					}

					// Check if the receiver type is pointer and call argument matches
					if isRecvPointer && isArgPointer && types.Identical(recvType, argType) {
						// Since pointer-based receiver functions are converted to C# as ref-based
						// extension functions, we need to convert the pointer from a reference type
						return fmt.Sprintf("(uintptr)@unsafe.Pointer.FromRef(ref %s)", v.convExpr(callExpr.Args[0], nil))
					}
				}
			}
		}
	}

	funcTypeName := v.getTypeName(funcType, true)
	callExprContext.sourceIsRuneArray = funcTypeName == "[]rune"

	funcName := v.convExpr(callExpr.Fun, []ExprContext{lambdaContext})

	// Handle unsafe.Offsetof and unsafe.AlignOf as a special cases
	if len(constructType) == 0 && len(callExpr.Args) == 1 {
		argExpr := v.convExpr(callExpr.Args[0], nil)
		argParts := strings.Split(argExpr, ".")

		if funcName == "@unsafe.Offsetof" {
			println(fmt.Sprintf("WARNING: Go code converted to C# using 'unsafe.Offsetof' may not produce same value as Go - verify usage: %s in \"%s\"", v.getPrintedNode(callExpr), getShortFileName(v.file)))

			if len(argParts) == 2 {
				// `unsafe.Offsetof(structValue.field)` to
				// `@unsafe.Offsetof(structValue.GetType(), "field")`
				return fmt.Sprintf("%s(%s.GetType(), \"%s\")", funcName, argParts[0], argParts[1])
			} else {
				println(fmt.Sprintf("WARNING: Unexpected 'unsafe.Offsetof' argument format: %s", argExpr))
			}
		} else if funcName == "@unsafe.Alignof" {
			println(fmt.Sprintf("WARNING: Go code converted to C# using 'unsafe.Alignof' may not produce same value as Go - verify usage: %s in \"%s\"", v.getPrintedNode(callExpr), getShortFileName(v.file)))

			if len(argParts) == 1 {
				// `unsafe.Alignof(x)` to
				// `@unsafe.Alignof(x.GetType())`
				return fmt.Sprintf("%s(%s.GetType())", funcName, argParts[0])
			} else if len(argParts) == 2 {
				// `unsafe.Alignof(s.f)` to
				// `@unsafe.Alignof(s.GetType(), "f")`
				return fmt.Sprintf("%s(%s.GetType(), \"%s\")", funcName, argParts[0], argParts[1])
			} else {
				println(fmt.Sprintf("WARNING: Unexpected 'unsafe.Alignof' argument format: %s", argExpr))
			}
		} else if funcName == "@unsafe.Sizeof" {
			println(fmt.Sprintf("WARNING: Go code converted to C# using 'unsafe.Sizeof' may not produce same value as Go - verify usage: %s in \"%s\"", v.getPrintedNode(callExpr), getShortFileName(v.file)))
		}
	}

	if len(typeParamExpr) > 0 && !strings.HasSuffix(funcName, typeParamExpr) {
		funcName += typeParamExpr
	}

	if !context.renderParams && context.callArgs != nil {
		// Capture arguments for function literal in a defer context, but do not render
		v.convExprList(callExpr.Args, callExpr.Lparen, callExprContext)
		return fmt.Sprintf("%s%s", constructType, funcName)
	}

	return fmt.Sprintf("%s%s(%s)", constructType, funcName, v.convExprList(callExpr.Args, callExpr.Lparen, callExprContext))
}

func (v *Visitor) isTypeConversion(callExpr *ast.CallExpr) (bool, string) {
	// Get the object associated with the function being called
	var obj types.Object
	var isPointer bool

	targetExpr := callExpr.Fun

	for targetExpr != nil {
		switch funExpr := targetExpr.(type) {
		case *ast.ParenExpr:
			targetExpr = funExpr.X
			continue
		case *ast.IndexExpr:
			targetExpr = funExpr.X
			continue
		case *ast.StarExpr:
			if ident, ok := funExpr.X.(*ast.Ident); ok {
				obj = v.info.ObjectOf(ident)
				isPointer = true
			}
			targetExpr = nil
		case *ast.Ident:
			obj = v.info.ObjectOf(funExpr)
			targetExpr = nil
		case *ast.SelectorExpr:
			obj = v.info.ObjectOf(funExpr.Sel)
			targetExpr = nil
		default:
			return false, ""
		}
	}

	if obj == nil {
		return false, ""
	}

	// Check if the function being called is a type name
	resolvedTypeName, ok := obj.(*types.TypeName)

	if !ok {
		return false, ""
	}

	// Get the target type
	targetType := resolvedTypeName.Type()

	// Type conversions typically have exactly one argument
	if len(callExpr.Args) != 1 {
		return false, ""
	}

	// Get the type of the argument
	argType := v.info.TypeOf(callExpr.Args[0])

	// Check if the argument is a pointer
	if pointer, ok := argType.(*types.Pointer); ok {
		argType = pointer.Elem()
	}

	typeName := v.getTypeName(targetType, false)

	if isPointer {
		typeName = "*" + typeName
	}

	// Check if the argument type is convertible to the target type
	return types.ConvertibleTo(argType, targetType), typeName
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

func (v *Visitor) addImplicitSubStructConversions(sourceType types.Type, targetTypeName string, indirect bool) {
	if subStructTypes, exists := v.subStructTypes[sourceType]; exists {
		for _, subStructType := range subStructTypes {
			// Check if subStructType is a pointer
			if ptrType, ok := subStructType.(*types.Pointer); ok {
				subStructType = ptrType.Elem()
			}

			subStructTypeName := v.getCSTypeName(subStructType)
			sourceTypeName := getRootSubStructName(subStructTypeName)

			if strings.HasSuffix(targetTypeName, ">") {
				targetTypeName = fmt.Sprintf("%s_%s>", targetTypeName[:len(targetTypeName)-1], sourceTypeName)
			} else {
				targetTypeName = fmt.Sprintf("%s_%s", targetTypeName, sourceTypeName)
			}

			// Recursively add implicit conversions for sub-structs
			v.addImplicitSubStructConversions(subStructType, targetTypeName, indirect)

			var targetConversionsMap map[string]HashSet[string]

			if indirect {
				targetConversionsMap = indirectImplicitConversions
			} else {
				targetConversionsMap = implicitConversions
			}

			var conversions HashSet[string]
			var exists bool

			if conversions, exists = targetConversionsMap[subStructTypeName]; exists {
				conversions.Add(targetTypeName)
			} else {
				conversions = NewHashSet([]string{targetTypeName})
				targetConversionsMap[subStructTypeName] = conversions
			}
		}
	}
}

func getRootSubStructName(subStructName string) string {
	// Get text beyond last underscore
	lastUnderscoreIndex := strings.LastIndex(subStructName, "_")

	if lastUnderscoreIndex == -1 {
		return subStructName
	}

	return subStructName[lastUnderscoreIndex+1:]
}

func getShortFileName(fileToken *token.File) string {
	if fileToken == nil {
		return ""
	}

	return filepath.Base(fileToken.Name())
}
