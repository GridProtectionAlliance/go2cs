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
	funcType := v.getType(callExpr.Fun, false)

	// Check if the call is a type conversion
	if ok, targetTypeName := v.isTypeConversion(callExpr); ok {
		arg := callExpr.Args[0]
		targetTypeName = convertToCSTypeName(targetTypeName)
		expr := v.checkForImplicitConversion(funcType, arg, targetTypeName)

		// In a pointer cast, we need to intermediately cast the target expression to an uintptr.
		// This is required since unsafe.Pointer is in its own library and no implicit cast can
		// be added for it on the pointer class (ж<T>) in the core library without creating a
		// circular dependency. Although C# allows circular dependencies, NuGet does not. If the
		// target happens to not be an unsafe pointer, the cast is still safe since all pointer
		// types support this cast operation.
		if context.isPointerCast {
			return fmt.Sprintf("(%s)(uintptr)(%s)", targetTypeName, expr)
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
	funcSignature := v.getFunctionSignature(callExpr)

	if funcSignature != nil {
		// Check if any parameters of callExpr.Fun are interface or pointer types
		params := funcSignature.Params()

		for i := range params.Len() {
			var paramType types.Type
			paramHasArg := callExpr.Args != nil && i < len(callExpr.Args)

			if paramHasArg {
				// Check if the parameter type is an anonymous struct
				if structType, exprType := v.extractStructType(callExpr.Args[i]); structType != nil && !v.liftedTypeExists(structType) {
					v.indentLevel++
					v.visitStructType(structType, exprType, params.At(i).Name(), nil, true, nil)
					v.indentLevel--
				}

				// Check if the parameter type is an anonymous interface
				if interfaceType, exprType := v.extractInterfaceType(callExpr.Args[i]); interfaceType != nil && !v.liftedTypeExists(interfaceType) {
					v.indentLevel++
					v.visitInterfaceType(interfaceType, exprType, params.At(i).Name(), nil, true, nil)
					v.indentLevel--
				}
			}

			callExprContext.u8StringArgOK[i] = true
			funcName := v.convExpr(callExpr.Fun, nil)

			// Handle builtin functions that take `...Type` parameters, treat as `interface{}`
			var ok bool

			if funcName == "print" || funcName == "println" {
				paramType = types.NewInterfaceType(nil, nil)
			} else if paramType, ok = getParameterType(funcSignature, i); !ok {
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

			v.showWarning("@convCallExpr - unexpected call to 'make' method for type '%s'", typeName)
			return fmt.Sprintf("make\u01C3<%s>(%s)", typeName, remainingArgs)
		}

		// Handle new call as a special case
		if ident.Name == "new" {
			typeExpr := callExpr.Args[0]
			typeName := convertToCSTypeName(v.getExprTypeName(typeExpr, false))
			return fmt.Sprintf("@new<%s>()", typeName)
		}

		// Handle panic call as a special case
		if ident.Name == "panic" {
			context := DefaultBasicLitContext()
			context.u8StringOK = false
			return fmt.Sprintf("throw panic(%s)", v.convExpr(callExpr.Args[0], []ExprContext{context}))
		}
	}

	lambdaContext := DefaultLambdaContext()
	lambdaContext.isCallExpr = true
	lambdaContext.isPointerCast = context.isPointerCast
	lambdaContext.deferredDecls = context.deferredDecls

	var typeParamExpr string
	resultType := v.info.TypeOf(callExpr)

	if resultType != nil {
		if named, ok := resultType.(*types.Named); ok {
			if named.TypeArgs().Len() > 0 {
				var typeParams []string

				for i := range named.TypeArgs().Len() {
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

	if len(callExpr.Args) == 1 {
		argTypeName := v.getExprTypeName(callExpr.Args[0], true)

		if argTypeName == "unsafe.Pointer" {
			lambdaContext.isPointerCast = true
		}
	}

	funcName := v.convExpr(callExpr.Fun, []ExprContext{lambdaContext})

	// Handle unsafe.Offsetof and unsafe.AlignOf as a special cases
	if len(constructType) == 0 && len(callExpr.Args) == 1 {
		argExpr := v.convExpr(callExpr.Args[0], nil)
		argParts := strings.Split(argExpr, ".")

		if funcName == "@unsafe.Offsetof" {
			v.showWarning("Go code converted to C# using 'unsafe.Offsetof' may not produce same value as Go - verify usage: %s", v.getPrintedNode(callExpr))

			if len(argParts) == 2 {
				// `unsafe.Offsetof(structValue.field)` to
				// `@unsafe.Offsetof(structValue.GetType(), "field")`
				return fmt.Sprintf("%s(%s.GetType(), \"%s\")", funcName, argParts[0], argParts[1])
			} else {
				v.showWarning("Unexpected 'unsafe.Offsetof' argument format: %s", argExpr)
			}
		} else if funcName == "@unsafe.Alignof" {
			v.showWarning("Go code converted to C# using 'unsafe.Alignof' may not produce same value as Go - verify usage: %s", v.getPrintedNode(callExpr))

			if len(argParts) == 1 {
				// `unsafe.Alignof(x)` to
				// `@unsafe.Alignof(x.GetType())`
				return fmt.Sprintf("%s(%s.GetType())", funcName, argParts[0])
			} else if len(argParts) == 2 {
				// `unsafe.Alignof(s.f)` to
				// `@unsafe.Alignof(s.GetType(), "f")`
				return fmt.Sprintf("%s(%s.GetType(), \"%s\")", funcName, argParts[0], argParts[1])
			} else {
				v.showWarning("Unexpected 'unsafe.Alignof' argument format: %s", argExpr)
			}
		} else if funcName == "@unsafe.Sizeof" {
			v.showWarning("Go code converted to C# using 'unsafe.Sizeof' may not produce same value as Go - verify usage: %s", v.getPrintedNode(callExpr))
		}
	}

	if len(typeParamExpr) > 0 && !strings.HasSuffix(funcName, typeParamExpr) {
		funcName += typeParamExpr
	}

	var result string

	if !context.renderParams && context.callArgs != nil {
		// Capture arguments for function literal in a defer context, but do not render
		v.convExprList(callExpr.Args, callExpr.Lparen, callExprContext)
		result = fmt.Sprintf("%s%s", constructType, funcName)
	} else {
		expr := v.convExprList(callExpr.Args, callExpr.Lparen, callExprContext)

		if strings.HasSuffix(funcName, "(uintptr)") && strings.HasPrefix(expr, "(uintptr)") {
			// Remove redundant cast to uintptr
			expr = expr[9:]
		}

		result = fmt.Sprintf("%s%s(%s)", constructType, funcName, expr)
	}

	// Check each argument for implicit conversions
	for _, arg := range callExpr.Args {
		argType := v.getType(arg, false)
		argTypeName := convertToCSTypeName(v.getTypeName(argType, false))

		v.checkForImplicitConversion(funcType, arg, argTypeName)
	}

	return result
}

func (v *Visitor) checkForImplicitConversion(funcType types.Type, arg ast.Expr, targetTypeName string) string {
	expr := v.convExpr(arg, nil)
	argType := v.getType(arg, false)

	var targetTypeIsPointer bool

	// Check if function type is a signature, i.e., an anonymous struct
	if sigType, ok := funcType.(*types.Signature); ok && sigType.Params().Len() > 0 {
		funcType = sigType.Params().At(0).Type()
	}

	// Check if function type is a struct or a pointer to a struct
	if ptrType, ok := funcType.(*types.Pointer); ok {
		funcType = ptrType.Elem()
		targetTypeIsPointer = true
	}

	if _, ok := funcType.Underlying().(*types.Struct); ok {
		// Check if argType is a struct or a pointer to a struct
		if ptrType, ok := argType.(*types.Pointer); ok {
			argType = ptrType.Elem()
		}

		if !types.Identical(funcType, argType) {
			if _, ok := argType.Underlying().(*types.Struct); ok {
				if targetTypeIsPointer {
					// Dereference target type when casting to pointer types,
					// in C# implicit casting operator requires the target type
					// to be a direct type, not a pointer type
					expr = fmt.Sprintf("(%s?.val ?? default!)", expr)
				}

				argTypeName := v.getCSTypeName(argType)

				if targetTypeName != argTypeName {
					// If both funcType and argType are distinct structs, track implicit conversions
					packageLock.Lock()

					var targetConversionsMap map[string]HashSet[string]

					if targetTypeIsPointer {
						targetConversionsMap = indirectImplicitConversions
					} else {
						targetConversionsMap = implicitConversions
					}

					var conversions HashSet[string]
					var exists bool

					if conversions, exists = targetConversionsMap[argTypeName]; exists {
						conversions.Add(targetTypeName)
					} else {
						conversions = NewHashSet([]string{targetTypeName})
						targetConversionsMap[argTypeName] = conversions
					}

					packageLock.Unlock()

					v.addImplicitSubStructConversions(argType, targetTypeName, targetTypeIsPointer)
				}
			}
		}
	}

	// Check if the function type is an aliased numeric type
	if ok, _ := isAliasedNumericType(funcType); ok {
		// Check if argType is a pointer type
		if ptrType, ok := argType.(*types.Pointer); ok {
			argType = ptrType.Elem()
		}

		if !types.Identical(funcType, argType) {
			// Check if the arg type is an aliased numeric type
			if ok, valueTypeName := isAliasedNumericType(argType); ok {
				if targetTypeIsPointer {
					// Dereference target type when casting to pointer types,
					// in C# implicit casting operator requires the target type
					// to be a direct type, not a pointer type
					expr = fmt.Sprintf("(%s?.val ?? default!)", expr)
				}

				argTypeName := v.getCSTypeName(argType)

				if targetTypeName != argTypeName {
					if strings.Contains(argTypeName, ".") || strings.Contains(argTypeName, TypeAliasDot) {
						valueTypeName = fmt.Sprintf("imported:%s", valueTypeName)
						targetTypeName, argTypeName = argTypeName, targetTypeName
					}

					// If both funcType and argType are both aliased numeric types, track value conversions
					packageLock.Lock()

					var targetConversionsMap map[string]map[string]string

					if targetTypeIsPointer {
						targetConversionsMap = indirectNumericConversions
					} else {
						targetConversionsMap = numericConversions
					}

					var conversions map[string]string
					var exists bool

					if conversions, exists = targetConversionsMap[argTypeName]; exists {
						conversions[targetTypeName] = valueTypeName
					} else {
						conversions = make(map[string]string)
						conversions[targetTypeName] = valueTypeName
						targetConversionsMap[argTypeName] = conversions
					}

					packageLock.Unlock()
				}
			}
		}
	}

	return expr
}

func isAliasedNumericType(targetType types.Type) (bool, string) {
	if aliasedType, ok := targetType.(*types.Alias); ok {
		underlyingType := aliasedType.Underlying()
		return isNumericType(underlyingType), convertToCSTypeName(underlyingType.String())
	} else if namedType, ok := targetType.(*types.Named); ok {
		underlyingType := namedType.Underlying()
		return isNumericType(underlyingType), convertToCSTypeName(underlyingType.String())
	}

	return false, ""
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

			packageLock.Lock()

			if conversions, exists = targetConversionsMap[subStructTypeName]; exists {
				conversions.Add(targetTypeName)
			} else {
				conversions = NewHashSet([]string{targetTypeName})
				targetConversionsMap[subStructTypeName] = conversions
			}

			packageLock.Unlock()
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

func (v *Visitor) getFunctionSignature(callExpr *ast.CallExpr) *types.Signature {
	switch fun := callExpr.Fun.(type) {
	case *ast.Ident:
		// Simple identifiers
		obj := v.info.Uses[fun]
		if obj == nil {
			return nil
		}

		if fn, ok := obj.(*types.Func); ok {
			return fn.Type().(*types.Signature)
		}

		if vr, ok := obj.(*types.Var); ok {
			if sig, ok := vr.Type().(*types.Signature); ok {
				return sig
			}
		}

	case *ast.SelectorExpr:
		// Qualified identifiers and method calls
		sel, ok := v.info.Selections[fun]
		if ok {
			// Method call
			if fn, ok := sel.Obj().(*types.Func); ok {
				return fn.Type().(*types.Signature)
			}
			return nil
		}

		// Package-qualified function
		if _, ok := fun.X.(*ast.Ident); ok {
			obj := v.info.Uses[fun.Sel]
			if obj == nil {
				return nil
			}

			if fn, ok := obj.(*types.Func); ok {
				return fn.Type().(*types.Signature)
			}

			if vr, ok := obj.(*types.Var); ok {
				if sig, ok := vr.Type().(*types.Signature); ok {
					return sig
				}
			}
		}

	case *ast.ParenExpr:
		// Handle parenthesized expressions like (pkg.Func)()
		return v.getFunctionSignature(&ast.CallExpr{Fun: fun.X})

	case *ast.CallExpr:
		// Functions returned by other functions
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}

	case *ast.FuncLit:
		// Anonymous functions
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}

	case *ast.IndexExpr:
		// Generic function instantiations
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}

	case *ast.IndexListExpr:
		// Multiple type parameter instantiations
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}

	case *ast.TypeAssertExpr:
		// Type assertions: (x.(T))()
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}

	case *ast.StarExpr:
		// Dereferencing a function pointer: (*fnPtr)()
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}

	case *ast.UnaryExpr:
		// Unary expressions like (*ptr)()
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}

	case *ast.BinaryExpr:
		// Binary expressions that somehow evaluate to functions
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}

	case *ast.CompositeLit:
		// Composite literals that are callable
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}

	case *ast.ArrayType, *ast.ChanType, *ast.FuncType, *ast.InterfaceType,
		*ast.MapType, *ast.StructType:
		// Type expressions
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}
	}

	// Handle type conversion cases that return callable functions
	// This covers cases like (*byte)(unsafe.Pointer(...))
	if callExpr, ok := callExpr.Fun.(*ast.CallExpr); ok {
		if fun, ok := callExpr.Fun.(*ast.ParenExpr); ok {
			if t := v.info.TypeOf(fun.X); t != nil {
				if sig, ok := t.(*types.Signature); ok {
					return sig
				}
			}
		}

		// Handle types directly
		if t := v.info.TypeOf(callExpr.Fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}
	}

	// Final general fallback - this should catch most remaining cases
	if t := v.info.TypeOf(callExpr.Fun); t != nil {
		if sig, ok := t.(*types.Signature); ok {
			return sig
		}

		if sig, ok := t.Underlying().(*types.Signature); ok {
			return sig
		}
	}

	resultType := v.info.TypeOf(callExpr)

	if resultType != nil && strings.Contains(resultType.String(), "unsafe.Pointer") {
		pkg := types.NewPackage("unsafe", "unsafe")

		// Only concerned with making the parameter a "pointer like" type
		uintptrType := types.Typ[types.Uintptr]
		params := types.NewTuple(types.NewParam(token.NoPos, pkg, "", types.NewPointer(uintptrType)))

		return types.NewSignatureType(nil, nil, nil, params, nil, false)
	}

	return nil
}
