package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"strconv"
	"strings"
)

func (v *Visitor) convCompositeLit(compositeLit *ast.CompositeLit, context KeyValueContext) string {
	result := &strings.Builder{}

	if compositeLit.Type == nil {
		// An untyped (type-inferred) composite literal — e.g. the inner `{lockRankSysmon, …}` of a
		// `[][]lockRank{ key: {…} }`. The target-typed `new(…)` ctor form below is correct for a STRUCT
		// element type (the struct ctor takes the field values), but a SLICE/ARRAY element type has no
		// element-list ctor — `new slice<lockRank>(a, b, …)` is CS1729. Emit the element-array
		// projection (`new lockRank[]{…}.slice()` / `.array()`) for those, matching the typed path.
		if inferred := v.info.TypeOf(compositeLit); inferred != nil {
			switch u := inferred.Underlying().(type) {
			case *types.Slice:
				csElem := convertToCSTypeName(v.getTypeName(u.Elem(), false))
				return fmt.Sprintf("new %s[]{%s}.slice()", csElem, v.convExprList(compositeLit.Elts, compositeLit.Lbrace, nil))
			case *types.Array:
				csElem := convertToCSTypeName(v.getTypeName(u.Elem(), false))
				return fmt.Sprintf("new %s[]{%s}.array()", csElem, v.convExprList(compositeLit.Elts, compositeLit.Lbrace, nil))
			case *types.Pointer:
				// An untyped composite whose inferred type is `*Struct` — the `[]*T{ {…} }` shorthand
				// for `&T{…}` (e.g. runtime's `dbgvars = []*dbgVar{ {name, &debug.x}, … }`). Emit the
				// boxed struct constructor `Ꮡ(new T(field: val, …))`; a bare `new(…)` targets the box
				// `ж<T>`, whose constructor has no such field params (CS1739).
				if _, ok := u.Elem().Underlying().(*types.Struct); ok {
					structName := v.getCSTypeName(u.Elem())
					return fmt.Sprintf("%s(new %s(%s))", AddressPrefix, structName, v.convExprList(compositeLit.Elts, compositeLit.Lbrace, nil))
				}
			}
		}

		rparenSuffix := ""

		if len(compositeLit.Elts) > 0 && v.isLineFeedBetween(compositeLit.Elts[len(compositeLit.Elts)-1].End(), compositeLit.Rbrace) {
			rparenSuffix = fmt.Sprintf("%s%s", v.newline, v.indent(v.indentLevel))
		}

		result.WriteString(fmt.Sprintf("new(%s", v.convExprList(compositeLit.Elts, compositeLit.Lbrace, nil)))
		v.writeStandAloneCommentString(result, compositeLit.Rbrace, nil, " ")
		result.WriteString(fmt.Sprintf("%s)", rparenSuffix))

		return result.String()
	}

	var name string

	if context.ident != nil {
		if len(context.ident.Name) > 0 {
			name = context.ident.Name
		}
	}

	if len(name) == 0 {
		name = "type"
	}

	var indentOffset int

	if v.inFunction {
		indentOffset = 1
	}

	// Check if the composite type is a struct or pointer to a struct
	if structType, exprType := v.extractStructType(compositeLit.Type); structType != nil && !v.liftedTypeExists(structType) {
		v.indentLevel += indentOffset
		v.visitStructType(structType, exprType, name, nil, true, nil)
		v.indentLevel -= indentOffset
	}

	// Check if the composite type is an anonymous interface
	if interfaceType, exprType := v.extractInterfaceType(compositeLit.Type); interfaceType != nil && !v.liftedTypeExists(interfaceType) {
		v.indentLevel += indentOffset
		v.visitInterfaceType(interfaceType, exprType, name, nil, true, nil)
		v.indentLevel -= indentOffset
	}

	exprType := v.getExprType(compositeLit.Type)
	arrayTypeContext := DefaultArrayTypeContext()
	callContext := DefaultCallExprContext()
	callContext.keyValueIdent = context.ident
	compositeSuffix := ""

	// Get the element type for arrays/slices/maps to check for interfaces
	var elementType types.Type
	var needsInterfaceCast bool
	var isEmpty bool
	var definedLen int
	var namedArrayComposite bool

	// Check composite lit elements against struct fields
	checkStructFields := func(structType *types.Struct) {
		for i := range structType.NumFields() {
			field := structType.Field(i)

			if i < len(compositeLit.Elts) {
				// Check if field is an embedded interface
				if fieldType := field.Type(); fieldType != nil {
					if needsInterfaceCast, isEmpty := isInterface(fieldType); needsInterfaceCast && !isEmpty {
						// Record implementation
						var eltType types.Type

						if keyValueExpr, ok := compositeLit.Elts[0].(*ast.KeyValueExpr); ok {
							eltType = v.info.TypeOf(keyValueExpr.Value)
						} else {
							eltType = v.info.TypeOf(compositeLit.Elts[i])
						}

						if eltType != nil {
							if _, ok := eltType.Underlying().(*types.Struct); ok || field.Embedded() {
								v.convertToInterfaceType(fieldType, eltType, "")
							}
						}
					} else if ok := isPointer(fieldType); ok {
						callContext.argTypeIsPtr[i] = true
					}
				}
			}
		}
	}

	switch t := exprType.(type) {
	case *types.Array:
		elementType = t.Elem()
		callContext.keyValueSource = ArraySource
		arrayTypeContext.compositeInitializer = true
		compositeSuffix = ".array()"
		definedLen = int(t.Len())
	case *types.Slice:
		elementType = t.Elem()
		callContext.keyValueSource = ArraySource
		arrayTypeContext.compositeInitializer = true
		compositeSuffix = ".slice()"
	case *types.Map:
		elementType = t.Elem()
		callContext.keyValueSource = MapSource
	case *types.Named:
		if structType, ok := t.Underlying().(*types.Struct); ok {
			checkStructFields(structType)
		} else if arrayType, ok := t.Underlying().(*types.Array); ok {
			// A named array type (e.g. `type d [3]rune`) lowers to a struct wrapping
			// array<T>; its composite literal cannot use C# collection-initializer braces
			// (no Add). Emit the underlying array literal wrapped in the named ctor:
			// `new d(new rune[]{...}.array())`.
			elementType = arrayType.Elem()
			callContext.keyValueSource = ArraySource
			arrayTypeContext.compositeInitializer = true
			compositeSuffix = ".array()"
			definedLen = int(arrayType.Len())
			namedArrayComposite = true
		} else if sliceType, ok := t.Underlying().(*types.Slice); ok {
			// A named slice type (e.g. `type s []int`) lowers to a struct wrapping
			// slice<T>; same treatment, with the slice constructor form.
			elementType = sliceType.Elem()
			callContext.keyValueSource = ArraySource
			arrayTypeContext.compositeInitializer = true
			compositeSuffix = ".slice()"
			namedArrayComposite = true
		}
	case *types.Struct:
		checkStructFields(t)
	}

	// Check if element type is an interface
	if elementType != nil {
		needsInterfaceCast, isEmpty = isInterface(elementType)
		if needsInterfaceCast && !isEmpty {
			for i := range compositeLit.Elts {
				callContext.interfaceTypes[i] = elementType
			}
		}
	}

	var lbracePrefix, rbracePrefix string
	lbrace := "{"
	rbrace := "}"

	// A struct composite uses the constructor form `new T(field: v)`. Test the UNDERLYING type so
	// this also covers a type ALIAS to a struct (`type name = abi.Name`, a *types.Alias in Go 1.22+,
	// which is neither *types.Named nor *types.Struct) — otherwise it would wrongly keep the `{`
	// object-initializer braces and emit the un-compilable Go form `new name{field: v}`.
	if exprType != nil {
		if _, ok := exprType.Underlying().(*types.Struct); ok {
			lbrace = "("
			rbrace = ")"
		}
	}

	if len(compositeLit.Elts) > 0 && v.isLineFeedBetween(compositeLit.Elts[len(compositeLit.Elts)-1].Pos(), compositeLit.Rbrace) {
		rbracePrefix = fmt.Sprintf("%s%s", v.newline, v.indent(v.indentLevel))
	}

	// Check for sparse array initialization
	if callContext.keyValueSource == ArraySource && len(compositeLit.Elts) > 0 {
		if _, ok := compositeLit.Elts[0].(*ast.KeyValueExpr); ok {
			callContext.keyValueSource = MapSource
			// This is an indexed slice/array literal emitted as a SparseArray (int-indexed), NOT a
			// real map — record it so a defined-integer-type key is cast to the int index type.
			callContext.keyValueArrayBacked = true
			maxKeyValue := 0

			for _, elt := range compositeLit.Elts {
				if keyValue, ok := elt.(*ast.KeyValueExpr); ok {
					if basicLit, ok := keyValue.Key.(*ast.BasicLit); ok {
						// Check for rune literal
						if strings.HasPrefix(basicLit.Value, "'") && strings.HasSuffix(basicLit.Value, "'") {
							basicLit.Value = strconv.Itoa(int(basicLit.Value[1 : len(basicLit.Value)-1][0]))
						}

						if keyValue, err := strconv.ParseInt(basicLit.Value, 0, 64); err == nil {
							if int(keyValue) > maxKeyValue {
								maxKeyValue = int(keyValue)
							}
						} else {
							maxKeyValue = 0
							break
						}
					}
				}
			}

			if maxKeyValue > 0 {
				arrayTypeContext.compositeInitializer = false

				if definedLen > 0 {
					arrayTypeContext.maxLength = definedLen
				} else {
					arrayTypeContext.maxLength = maxKeyValue + 1
				}

				compositeSuffix = ""
			} else {
				arrayTypeContext.indexedInitializer = true
			}
		}
	}

	var newSpace string

	if callContext.keyValueSource == ArraySource || callContext.keyValueSource == MapSource || callContext.keyValueSource == StructSource || arrayTypeContext.compositeInitializer {
		newSpace = " "
	} else {
		newSpace = ""
	}

	identContext := DefaultIdentContext()
	identContext.isType = true

	if context.ident != nil {
		identContext.ident = context.ident
	}

	contexts := []ExprContext{arrayTypeContext, identContext}

	typeRender := v.convExpr(compositeLit.Type, contexts)

	if namedArrayComposite {
		// Wrap the underlying array/slice literal in the named type's constructor:
		// `new d(new rune[]{...}.array())`. The element literal and its `.array()`/
		// `.slice()` suffix render via the ArraySource path below; close the ctor here.
		csElementType := convertToCSTypeName(v.getTypeName(elementType, false))
		typeRender = fmt.Sprintf("%s(new %s[]", typeRender, csElementType)
		compositeSuffix += ")"
	}

	result.WriteString(fmt.Sprintf("new%s%s%s%s", newSpace, typeRender, lbracePrefix, lbrace))

	if len(compositeLit.Elts) > 0 {
		v.writeStandAloneCommentString(result, compositeLit.Elts[0].Pos(), nil, " ")
	} else {
		// If constructing a struct with no parameters, pass in a nill value
		if _, ok := exprType.(*types.Named); ok {
			result.WriteString("nil")
		}
	}

	// Convert elements with potential interface casting
	if needsInterfaceCast && !isEmpty {
		elements := &strings.Builder{}

		for i, elt := range compositeLit.Elts {
			if i > 0 {
				elements.WriteString(", ")
			}

			var expr string

			if kv, ok := elt.(*ast.KeyValueExpr); ok {
				// Handle key-value pairs (for maps or sparse arrays)
				keyStr := v.convExpr(kv.Key, nil)
				valStr := v.convExpr(kv.Value, nil)
				expr = fmt.Sprintf("%s, %s", keyStr, v.convertToInterfaceType(elementType, v.getType(kv.Value, false), valStr))
			} else {
				// Handle regular elements
				expr = v.convertToInterfaceType(elementType, v.getType(elt, false), v.convExpr(elt, nil))
			}

			elements.WriteString(expr)
		}
		result.WriteString(elements.String())
	} else {
		result.WriteString(v.convExprList(compositeLit.Elts, compositeLit.Lbrace, callContext))
	}

	result.WriteString(fmt.Sprintf("%s%s%s", rbracePrefix, rbrace, compositeSuffix))

	return result.String()
}
