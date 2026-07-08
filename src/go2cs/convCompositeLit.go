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
				// A KEYED elided slice literal (`{5: "x"}` inside a `[]map[…][]T{…}`) renders as a
				// golib SparseArray with `[index] = value` elements; the plain `new T[]{…}` array
				// initializer below cannot take the Go `key: value` keyed syntax (CS1003 cascade).
				// Mirrors the typed slice sparse-array path (see keyValueSource==ArraySource below).
				if compositeLitIsKeyed(compositeLit.Elts) {
					return fmt.Sprintf("new golib.SparseArray<%s>{%s}.slice()", csElem, v.convExprList(compositeLit.Elts, compositeLit.Lbrace, sparseArrayCompositeContext(compositeLit.Elts)))
				}
				return fmt.Sprintf("new %s[]{%s}.slice()", csElem, v.convExprList(compositeLit.Elts, compositeLit.Lbrace, nil))
			case *types.Array:
				csElem := convertToCSTypeName(v.getTypeName(u.Elem(), false))
				// A KEYED elided ARRAY literal — the inner `{joiningL: stateBefore, …}` of a
				// `[][numJoinTypes]joinState{stateStart: {…}}` (x/net/idna joinStates, CS1003 ×62).
				// Same SparseArray treatment as the slice case; `.array()` materializes the dense
				// fixed-length backing, matching the typed array sparse path.
				if compositeLitIsKeyed(compositeLit.Elts) {
					return fmt.Sprintf("new golib.SparseArray<%s>{%s}.array()", csElem, v.convExprList(compositeLit.Elts, compositeLit.Lbrace, sparseArrayCompositeContext(compositeLit.Elts)))
				}
				return fmt.Sprintf("new %s[]{%s}.array()", csElem, v.convExprList(compositeLit.Elts, compositeLit.Lbrace, nil))
			case *types.Pointer:
				// An untyped composite whose inferred type is `*Struct` — the `[]*T{ {…} }` shorthand
				// for `&T{…}` (e.g. runtime's `dbgvars = []*dbgVar{ {name, &debug.x}, … }`). Emit the
				// boxed struct constructor `Ꮡ(new T(field: val, …))`; a bare `new(…)` targets the box
				// `ж<T>`, whose constructor has no such field params (CS1739).
				if _, ok := u.Elem().Underlying().(*types.Struct); ok {
					structName := v.getCSTypeName(u.Elem())

					// Thread the pointed-to struct type so a keyed field named like its OWN struct type
					// takes the CS0542 type-colliding rename in the generated ctor (net/mail's
					// `[]*Address{{Address: …}}` kept the unrenamed `Address:` key, CS1739). Set
					// u8StringArgOK per element to match the nil-context default (an empty map would
					// silently strip the u8 suffix from string-literal element values).
					ptrElidedContext := DefaultCallExprContext()
					ptrElidedContext.keyValueCompositeType = u.Elem()

					for i := range compositeLit.Elts {
						ptrElidedContext.u8StringArgOK[i] = true
					}

					return fmt.Sprintf("%s(new %s(%s))", AddressPrefix, structName, v.convExprList(compositeLit.Elts, compositeLit.Lbrace, ptrElidedContext))
				}
			case *types.Map:
				// A MAP element type — the inner `{"domain": 53}` of a
				// `map[string]map[string]int{…}` (net lookup.go) — takes the map
				// collection-initializer form with `[key] = value` elements; the `new(…)` ctor
				// fallback rendered STRUCT-style named args (`"domain"u8: 53`, a 30-error
				// syntax cascade).
				mapContext := DefaultCallExprContext()
				mapContext.keyValueSource = MapSource
				mapContext.keyValueCompositeType = inferred

				for i := range compositeLit.Elts {
					mapContext.u8StringArgOK[i] = true
				}

				keyCS := convertToCSTypeName(v.getTypeName(u.Key(), false))
				valCS := convertToCSTypeName(v.getTypeName(u.Elem(), false))

				return fmt.Sprintf("new map<%s, %s>{%s}", keyCS, valCS, v.convExprList(compositeLit.Elts, compositeLit.Lbrace, mapContext))
			}
		}

		rparenSuffix := ""

		if len(compositeLit.Elts) > 0 && v.isLineFeedBetween(compositeLit.Elts[len(compositeLit.Elts)-1].End(), compositeLit.Rbrace) {
			rparenSuffix = fmt.Sprintf("%s%s", v.newline, v.indent(v.indentLevel))
		}

		// Thread the ELIDED literal's resolved type to the keyed-field emission so a field
		// named like its own struct type detects the declaration's type-colliding rename
		// (runtime/metrics `{Description: …}` inside `[]Description{…}`, CS1739 ×56).
		elidedContext := DefaultCallExprContext()
		elidedContext.keyValueCompositeType = v.info.TypeOf(compositeLit)

		// The nil-context path this replaces defaulted u8StringOK true per element; an empty
		// map defaults it FALSE, silently stripping the u8 suffix from string-literal elements.
		for i := range compositeLit.Elts {
			elidedContext.u8StringArgOK[i] = true
		}

		result.WriteString(fmt.Sprintf("new(%s", v.convExprList(compositeLit.Elts, compositeLit.Lbrace, elidedContext)))
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
	// Thread the enclosing statement's hoist target through the composite so a
	// func-literal FIELD value's capture decls hoist (see KeyValueContext.deferredDecls).
	callContext.deferredDecls = context.deferredDecls
	callContext.keyValueIdent = context.ident

	// Thread the composite's RESOLVED type to the keyed-field emission so a field named like
	// its OWN struct type detects the declaration's type-colliding rename (runtime/metrics
	// `Description{Description: …}`, CS1739 ×57). Derived from the LITERAL, not its Type
	// syntax — an elided element literal (`{Name: …}` inside `[]Description{…}`) has a nil
	// Type node but still resolves contextually.
	if tv, ok := v.info.Types[compositeLit]; ok && tv.Type != nil {
		if _, ok := tv.Type.Underlying().(*types.Struct); ok {
			callContext.keyValueCompositeType = tv.Type
		}
	}

	compositeSuffix := ""

	// Get the element type for arrays/slices/maps to check for interfaces
	var elementType types.Type
	var needsInterfaceCast bool
	var isEmpty bool
	var definedLen int
	var namedArrayComposite bool
	var namedMapComposite bool
	var namedMapRender string
	var aliasArrayComposite bool
	var namedStructWrapRender string

	// Check composite lit elements against struct fields
	checkStructFields := func(structType *types.Struct) {
		for i := range structType.NumFields() {
			field := structType.Field(i)

			if i < len(compositeLit.Elts) {
				// Check if field is an embedded interface
				if fieldType := field.Type(); fieldType != nil {
					if needsInterfaceCast, isEmpty := isInterface(fieldType); needsInterfaceCast && !isEmpty {
						// Record implementation. A KEYED composite resolves the element by FIELD
						// NAME — blindly pairing Elts[0]'s value with EVERY interface field
						// recorded a BOGUS GoImplement (gif's `encoder{g: *g}`: field 0 is
						// `w writer`, the first value is a GIF → GoImplement<GIF, writer>,
						// CS1929 ×3 in the generated impl for methods GIF never had).
						var eltType types.Type
						eltIndex := i

						if _, ok := compositeLit.Elts[0].(*ast.KeyValueExpr); ok {
							eltIndex = -1

							for ei, elt := range compositeLit.Elts {
								if kv, ok := elt.(*ast.KeyValueExpr); ok {
									if keyIdent, ok := kv.Key.(*ast.Ident); ok && keyIdent.Name == field.Name() {
										eltType = v.info.TypeOf(kv.Value)
										eltIndex = ei
										break
									}
								}
							}
						} else {
							eltType = v.info.TypeOf(compositeLit.Elts[i])
						}

						if eltType != nil {
							_, eltIsStruct := eltType.Underlying().(*types.Struct)

							// A named NON-struct element that implements the interface field —
							// hpack's `DecodingError{InvalidIndexError(idx)}`, where
							// `type InvalidIndexError int` has an Error() method satisfying the
							// `error` field — must ALSO be recorded + routed, or it is passed bare
							// to the interface-typed constructor parameter (surfaces as NilType,
							// CS1503). The struct/embedded triggers are unchanged; this adds
							// method-set satisfaction for a named, non-struct, non-interface value
							// (the call-argument path already routes any arg into an interface param
							// without a struct-only restriction). An interface-typed element is
							// excluded — it is already the interface, so it needs no adapter.
							eltImplementsIface := false

							if !eltIsStruct && !field.Embedded() {
								// The concrete type carrying the satisfying method set: the element
								// itself, OR the pointee of a POINTER element whose pointer-receiver
								// methods satisfy the field — `&logLoggerLevel` (`*LevelVar`) feeding a
								// `Leveler` field in log/slog's `&handlerWriter{…, &logLoggerLevel, …}`,
								// where `*LevelVar` implements Leveler via a pointer-receiver `Level()`
								// (CS1503; no `GoImplement<LevelVar, Leveler>(Pointer = true)` was
								// recorded because this arm previously matched only a NAMED value, not a
								// pointer). types.Implements is tested on the ELEMENT type (the pointer
								// method set), while the non-interface guard tests the pointee.
								implSource := eltType

								if ptr, ok := eltType.(*types.Pointer); ok {
									implSource = ptr.Elem()
								}

								if named, ok := implSource.(*types.Named); ok {
									if _, eltIsIface := named.Underlying().(*types.Interface); !eltIsIface {
										if iface, ok := fieldType.Underlying().(*types.Interface); ok && types.Implements(eltType, iface) {
											eltImplementsIface = true
										}
									}
								}
							}

							if eltIsStruct || field.Embedded() || eltImplementsIface {
								v.convertToInterfaceType(fieldType, eltType, "")

								// Route the element through the interface conversion at RENDER
								// too — the record-only call above can miss paths that bypass
								// it, and the ctor's interface param cannot take the bare
								// struct without its partial (archive/tar's lifted
								// `struct{ io.Reader }{fr}`, CS1503 ×3).
								if eltIndex >= 0 {
									callContext.interfaceTypes[eltIndex] = fieldType
								}
							}
						}
					} else if ok := isPointer(fieldType); ok {
						callContext.argTypeIsPtr[i] = true
					}
				}
			}
		}

		// A NAMED FUNC-type field initialized with a value of a DIFFERENT delegate type must
		// wrap in the target delegate's constructor — C# has no implicit conversion between
		// distinct delegate types (internal/concurrent's `keyHash: mapType.Hasher` feeding a
		// hashFunc field from a Func<…,…> field, CS1503 ×3). Keyed-aware: elements resolve
		// their field by NAME, so a reordered/partial literal maps correctly. FuncLit values
		// (anonymous-method conversion applies) and nil stay bare.
		fieldByName := map[string]*types.Var{}

		for i := range structType.NumFields() {
			fieldByName[structType.Field(i).Name()] = structType.Field(i)
		}

		// A struct instantiated over a constraint proxy (nistCurve<P224PointжnistPoint>) renders its
		// Point-typed FUNC fields with the proxy (`Func<P224PointжnistPoint> newPoint`), so a
		// method-group / func-value initializer needs a lambda re-wrap below (see wrapArgWithLambda).
		compositeHasProxy := false

		if litNamed, ok := types.Unalias(exprType).(*types.Named); ok {
			compositeHasProxy = v.namedHasConstraintProxy(litNamed)
		}

		for i, elt := range compositeLit.Elts {
			var fieldVar *types.Var
			var valueExpr ast.Expr

			if keyValue, ok := elt.(*ast.KeyValueExpr); ok {
				if keyIdent, ok := keyValue.Key.(*ast.Ident); ok {
					fieldVar = fieldByName[keyIdent.Name]
				}

				valueExpr = keyValue.Value
			} else {
				if i < structType.NumFields() {
					fieldVar = structType.Field(i)
				}

				valueExpr = elt
			}

			if fieldVar == nil || valueExpr == nil {
				continue
			}

			// FUNC field of a constraint-proxy instantiation: re-wrap the initializer as a lambda so
			// the proxy delegate position applies the ж↔proxy conversion a method-group conversion
			// can't (nistCurve's `newPoint: nistec.NewP224Point` → `() => nistec.NewP224Point()`,
			// CS0407). A FuncLit initializer already targets the proxy return, and nil stays bare.
			if compositeHasProxy {
				if sig, isSig := fieldVar.Type().Underlying().(*types.Signature); isSig {
					if _, isLit := valueExpr.(*ast.FuncLit); !isLit {
						if tv, isNil := v.info.Types[valueExpr]; !isNil || !tv.IsNil() {
							if callContext.wrapArgWithLambda == nil {
								callContext.wrapArgWithLambda = make(map[int]string)
							}

							params := make([]string, sig.Params().Len())

							for p := range params {
								params[p] = fmt.Sprintf("%sp%d", ShadowVarMarker, p)
							}

							callContext.wrapArgWithLambda[i] = strings.Join(params, ", ")
							continue
						}
					}
				}
			}

			named, ok := types.Unalias(fieldVar.Type()).(*types.Named)

			if !ok {
				continue
			}

			if _, isSig := named.Underlying().(*types.Signature); !isSig {
				continue
			}

			if _, isLit := valueExpr.(*ast.FuncLit); isLit {
				continue
			}

			if tv, ok := v.info.Types[valueExpr]; ok && tv.IsNil() {
				continue
			}

			valueType := v.info.TypeOf(valueExpr)

			if valueType == nil || types.Identical(types.Unalias(valueType), types.Unalias(fieldVar.Type())) {
				continue
			}

			if callContext.wrapArgWithNew == nil {
				callContext.wrapArgWithNew = make(map[int]string)
			}

			callContext.wrapArgWithNew[i] = v.getCSTypeName(fieldVar.Type())
		}
	}

	// Dispatch on the UNALIASED type (*types.Alias, Go 1.22+): an alias to an unnamed array
	// (fiat's `type p224UntypedFieldElement = [4]uint64`) matched no arm, so the literal kept
	// C# collection-initializer braces on the alias name (`new words{…}` — CS1061, array<T>
	// has no Add). An alias renders through its name (an Ident, not an ast.ArrayType), so the
	// array/slice arms also flag the typeRender swap to the element-array projection form the
	// unnamed literal uses (`new uint64[]{…}.array()`). An alias to a NAMED type falls into
	// the *types.Named arm and keeps the alias name (same wrapper struct either way).
	_, isAliasType := exprType.(*types.Alias)

	switch t := types.Unalias(exprType).(type) {
	case *types.Array:
		elementType = t.Elem()
		callContext.keyValueSource = ArraySource
		arrayTypeContext.compositeInitializer = true
		compositeSuffix = ".array()"
		definedLen = int(t.Len())
		aliasArrayComposite = isAliasType
	case *types.Slice:
		elementType = t.Elem()
		callContext.keyValueSource = ArraySource
		arrayTypeContext.compositeInitializer = true
		compositeSuffix = ".slice()"
		aliasArrayComposite = isAliasType
	case *types.Map:
		elementType = t.Elem()
		callContext.keyValueSource = MapSource
		// Carry the map type so convKeyValueExpr can box a pointer-typed VALUE (a bare-ident pointer
		// into a `ж<T>` map slot is CS0029, like the struct-field pointer case).
		callContext.keyValueCompositeType = t
	case *types.Named:
		if structType, ok := t.Underlying().(*types.Struct); ok {
			checkStructFields(structType)

			// A DEFINED type over a NAMED STRUCT type (`type decoder coder`) is emitted as
			// a WRAPPER whose only ctor takes the underlying (`decoder(coder value)`) — its
			// composite literal constructs the UNDERLYING and wraps:
			// `new decoder(new coder(order: …))` (encoding/binary, CS1739 ×5). A type
			// written directly over a struct LITERAL keeps its own keyed ctor (unchanged).
			if rhs, okRHS := packageTypeSpecRHS[t.Obj()]; okRHS && rhs != nil {
				if rhsNamed, ok := types.Unalias(rhs).(*types.Named); ok {
					if _, isStruct := rhsNamed.Underlying().(*types.Struct); isStruct {
						namedStructWrapRender = convertToCSTypeName(v.getTypeName(rhsNamed, false))
					}
				}
			}
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
		} else if mapType, ok := t.Underlying().(*types.Map); ok {
			// A named map type's composite literal wraps the CONCRETE map literal in the
			// named ctor — `new Grades(new map<@string, nint>{["a"u8] = 1})` — mirroring
			// named arrays/slices. The wrapper struct's default instance has no backing
			// dictionary for a direct indexer-initializer, and without this arm the keys
			// emitted Go-style (`"a"u8: 1` inside C# braces — CS1513/CS1002).
			elementType = mapType.Elem()
			callContext.keyValueSource = MapSource
			namedMapComposite = true
			namedMapRender = fmt.Sprintf("map<%s, %s>", convertToCSTypeName(v.getTypeName(mapType.Key(), false)), convertToCSTypeName(v.getTypeName(mapType.Elem(), false)))
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

		// A POINTER element type renders a bare-ident element as the pointer VALUE — the box
		// (`Ꮡc`), not a deref'd receiver ref-local (`c`) — mirroring the struct-field pointer
		// routing below and the call-argument pointer arm (convCallExpr): `[]*CommentGroup{c}`
		// inside a method that deref-aliases its `c *CommentGroup` parameter otherwise emits
		// the VALUE alias into a `ж<CommentGroup>[]` array (go/ast addComment, CS0029). Gated
		// to bare idents of pointer type: keyed elements (maps) and address-of/composite
		// elements manage their own pointer rendering.
		if _, ok := elementType.Underlying().(*types.Pointer); ok {
			for i, elt := range compositeLit.Elts {
				if ident, ok := elt.(*ast.Ident); ok {
					if _, isPtr := v.getType(ident, false).(*types.Pointer); isPtr {
						callContext.argTypeIsPtr[i] = true
					}
				}
			}
		}
	}

	// A NARROW-INTEGER element type (int8/uint8/int16/uint16) receiving a binary/unary
	// arithmetic element: Go evaluates `b/100 + '0'` at the element's narrow width (with
	// overflow wrapping), but C# promotes sub-int integer arithmetic to `int`, so a
	// NON-CONSTANT element needs an explicit cast back to the element type — both to compile
	// (int→narrow is not implicit, CS0266) and to preserve Go's wrap semantics. This is the
	// composite-literal twin of the narrow-integer call-argument cast (see convCallExpr),
	// which is why the equivalent `append(buf, b/100+'0')` form already compiles. Gated on
	// the element's Go type already matching the element type (so Go accepts it without a
	// conversion) and on it being an arithmetic expression — a bare ident is already the
	// narrow type, and a constant literal element gets C#'s implicit constant-expression
	// conversion. Key-value elements (sparse arrays / maps) are not ast.BinaryExpr/UnaryExpr,
	// so they never match.
	if elementType != nil && callContext.keyValueSource == ArraySource {
		if elemBasic, ok := elementType.Underlying().(*types.Basic); ok && isNarrowIntegerKind(elemBasic.Kind()) {
			csElemType := convertToCSTypeName(v.getTypeName(elementType, false))

			for i, elt := range compositeLit.Elts {
				switch elt.(type) {
				case *ast.BinaryExpr, *ast.UnaryExpr:
					if eltType := v.getType(elt, false); eltType != nil && types.Identical(eltType, elementType) {
						if callContext.castArgToType == nil {
							callContext.castArgToType = make(map[int]string)
						}

						callContext.castArgToType[i] = csElemType
					}
				}
			}
		}
	}

	// STRUCT-composite twin of the narrow-integer element cast above: each element maps to a
	// FIELD whose type may be a narrow integer (image/png's `color.Gray{(b >> 7) * 0xff}`,
	// field Y uint8). Same rationale — C# promotes sub-int arithmetic to int, so a
	// non-constant arithmetic element needs an explicit cast back to the field type (CS1503 +
	// Go wrap semantics). Positional and keyed both resolve their field; a bare ident / literal
	// element is already the field's width so it never matches.
	if st, ok := exprType.Underlying().(*types.Struct); ok {
		fieldByName := map[string]*types.Var{}

		for i := range st.NumFields() {
			fieldByName[st.Field(i).Name()] = st.Field(i)
		}

		for i, elt := range compositeLit.Elts {
			var fieldVar *types.Var
			var valueExpr ast.Expr

			if keyValue, ok := elt.(*ast.KeyValueExpr); ok {
				if keyIdent, ok := keyValue.Key.(*ast.Ident); ok {
					fieldVar = fieldByName[keyIdent.Name]
				}

				valueExpr = keyValue.Value
			} else if i < st.NumFields() {
				fieldVar = st.Field(i)
				valueExpr = elt
			}

			if fieldVar == nil || valueExpr == nil {
				continue
			}

			elemBasic, ok := fieldVar.Type().Underlying().(*types.Basic)

			if !ok || !isNarrowIntegerKind(elemBasic.Kind()) {
				continue
			}

			switch valueExpr.(type) {
			case *ast.BinaryExpr, *ast.UnaryExpr:
				if vt := v.getType(valueExpr, false); vt != nil && types.Identical(vt, fieldVar.Type()) {
					if callContext.castArgToType == nil {
						callContext.castArgToType = make(map[int]string)
					}

					callContext.castArgToType[i] = convertToCSTypeName(v.getTypeName(fieldVar.Type(), false))
				}
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
						// Check for rune literal — DECODE it (escapes and multi-byte runes): byte
						// [0] of the unquoted text read the BACKSLASH of an escape sequence (92),
						// so every escaped key (`'\t': 1`, bytes asciiSpace) corrupted to '\' — a
						// CS1012 syntax cascade across bytes/strings/os/fmt.
						if strings.HasPrefix(basicLit.Value, "'") && strings.HasSuffix(basicLit.Value, "'") {
							if r, _, _, err := strconv.UnquoteChar(basicLit.Value[1:len(basicLit.Value)-1], '\''); err == nil {
								basicLit.Value = strconv.Itoa(int(r))
							}
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

	// A generic type instantiated with a self-referential constraint-proxy pointer argument
	// (`nistCurve[*P224Point]`) must render its type arguments through the PROXY
	// (`nistCurve<P224PointжnistPoint>`) — convExpr walked the AST literal `nistCurve[*P224Point]`
	// and rendered the box `ж<P224Point>`, which mismatches the pointer adapter that wraps
	// `ж<nistCurve<P224PointжnistPoint>>` (CS0311/type mismatch). Re-render from the RESOLVED type,
	// whose getTypeName substitutes the proxy. Gated to the proxy case, so no churn elsewhere.
	if named, ok := types.Unalias(exprType).(*types.Named); ok && v.namedHasConstraintProxy(named) {
		typeRender = convertToCSTypeName(v.getTypeName(named, false))
	}

	if aliasArrayComposite {
		// The alias renders as its Ident name, which cannot take the composite-initializer
		// bracket rewrite an ast.ArrayType gets in convArrayType — substitute the same forms
		// the unnamed literal produces: the element-array projection for plain elements
		// (`new uint64[]{…}` + `.array()`/`.slice()`), SparseArray for non-constant indexed
		// keys, and the alias's int-length ctor for constant-indexed sparse literals
		// (`new words(4){[2] = 30}` — the alias IS array<T>, which has that ctor).
		csElementType := convertToCSTypeName(v.getTypeName(elementType, false))

		if arrayTypeContext.indexedInitializer {
			typeRender = fmt.Sprintf("golib.SparseArray<%s>", csElementType)
		} else if arrayTypeContext.compositeInitializer {
			typeRender = fmt.Sprintf("%s[]", csElementType)
		} else if arrayTypeContext.maxLength > 0 {
			typeRender = fmt.Sprintf("%s(%d)", typeRender, arrayTypeContext.maxLength)
		}
	}

	if namedArrayComposite {
		csElementType := convertToCSTypeName(v.getTypeName(elementType, false))

		// An EMPTY composite of a named-over-array/slice (`tmpBuf{}` where `type tmpBuf [32]byte`
		// — runtime string.go's `*buf = tmpBuf{}` — or `pm{}` over a named slice) is the type's
		// ZERO VALUE. The generic named-composite `nil` filler below would land INSIDE the
		// element literal (`new tmpBuf(new byte[]{nil}.array())` — CS0029 NilType→byte). Emit
		// the zero value directly: a zeroed FIXED-LENGTH backing for an array wrapper (Go's
		// zero `[N]T` — an empty `{}` literal would produce a length-0 backing, not `[N]T`),
		// and an empty non-nil backing for a slice wrapper.
		if len(compositeLit.Elts) == 0 {
			if definedLen > 0 {
				return fmt.Sprintf("new %s(new %s[%d]%s)", typeRender, csElementType, definedLen, compositeSuffix)
			}

			return fmt.Sprintf("new %s(new %s[]{}%s)", typeRender, csElementType, compositeSuffix)
		}

		if arrayTypeContext.maxLength > 0 {
			// A KEYED (sparse, constant-index) array literal — `timedEventArgs{1: v}` over
			// `[N]uint64` (internal/trace/oldtrace) — renders its elements as the `[i] = v` indexed
			// initializer, which is invalid on a raw C# array (`new uint64[]{[1] = v}` — CS0131). Back
			// it with the indexer-capable golib `array<T>(length)` instead, mirroring the alias form
			// (`new words(4){[2] = 30}`); the named ctor takes an `array<T>` just as the positional
			// `.array()` path produces.
			typeRender = fmt.Sprintf("%s(new array<%s>(%d)", typeRender, csElementType, arrayTypeContext.maxLength)
			compositeSuffix += ")"
		} else {
			// Wrap the underlying array/slice literal in the named type's constructor:
			// `new d(new rune[]{...}.array())`. The element literal and its `.array()`/
			// `.slice()` suffix render via the ArraySource path below; close the ctor here.
			typeRender = fmt.Sprintf("%s(new %s[]", typeRender, csElementType)
			compositeSuffix += ")"
		}
	}

	if namedMapComposite {
		// Open the named ctor around the concrete map literal; the ')' closes after the
		// brace via compositeSuffix.
		typeRender = fmt.Sprintf("%s(new %s", typeRender, namedMapRender)
		compositeSuffix += ")"
	}

	if namedStructWrapRender != "" {
		// Open the wrapper ctor around the underlying named struct's keyed ctor; the
		// closing ')' follows the rbrace via compositeSuffix.
		typeRender = fmt.Sprintf("%s(new %s", typeRender, namedStructWrapRender)
		compositeSuffix += ")"
	}

	result.WriteString(fmt.Sprintf("new%s%s%s%s", newSpace, typeRender, lbracePrefix, lbrace))

	if len(compositeLit.Elts) > 0 {
		v.writeStandAloneCommentString(result, compositeLit.Elts[0].Pos(), nil, " ")
	} else {
		// If constructing a struct with no parameters, pass in a nill value (a named-map
		// composite's braces belong to the inner CONCRETE map literal — nothing to fill)
		if _, ok := exprType.(*types.Named); ok && !namedMapComposite {
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

// compositeLitIsKeyed reports whether a composite literal's elements are index/key keyed
// (KeyValueExpr). Go requires a composite literal to be all-keyed or all-positional, so the
// first element is representative — matching the typed sparse-array detection below.
func compositeLitIsKeyed(elts []ast.Expr) bool {
	if len(elts) == 0 {
		return false
	}

	_, keyed := elts[0].(*ast.KeyValueExpr)
	return keyed
}

// sparseArrayCompositeContext builds the element context for a KEYED (sparse) array/slice
// composite literal so its KeyValueExpr elements render as `[index] = value` against a golib
// SparseArray (MapSource + arrayBacked), rather than the invalid Go `key: value` form. Mirrors
// the elided-map handling and the typed keyValueSource==ArraySource→MapSource conversion.
func sparseArrayCompositeContext(elts []ast.Expr) *CallExprContext {
	context := DefaultCallExprContext()
	context.keyValueSource = MapSource
	context.keyValueArrayBacked = true

	for i := range elts {
		context.u8StringArgOK[i] = true
	}

	return context
}
