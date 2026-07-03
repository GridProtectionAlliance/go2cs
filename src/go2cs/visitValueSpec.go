package main

import (
	"fmt"
	"go/ast"
	"go/constant"
	"go/token"
	"go/types"
	"strconv"
	"strings"
)

func (v *Visitor) visitValueSpec(valueSpec *ast.ValueSpec, doc *ast.CommentGroup, tok token.Token) {
	v.targetFile.WriteString(v.newline)
	v.writeDoc(doc, valueSpec.End())

	if tok == token.VAR {
		// A package-level `var a, b = f()` — ONE call initializer whose result tuple Go
		// deconstructs across the names. C# field initializers cannot deconstruct, so the
		// per-name loop below assigned the WHOLE ValueTuple to the first field (CS0029 —
		// edwards25519's `var identity, _ = new(Point).SetBytes(…)`) and left the rest
		// uninitialized. Route through the ValueTuple component-read emission instead.
		// In-function specs keep the existing path (`:=` tuples are visitAssignStmt's).
		if !v.inFunction && valueSpec.Type == nil && len(valueSpec.Names) > 1 && len(valueSpec.Values) == 1 {
			if _, isCall := valueSpec.Values[0].(*ast.CallExpr); isCall {
				if tuple, isTuple := v.info.TypeOf(valueSpec.Values[0]).(*types.Tuple); isTuple {
					v.visitPackageTupleVarSpec(valueSpec, tuple)
					return
				}
			}
		}

		for i, ident := range valueSpec.Names {
			var isAnyType bool
			var isInterfaceType bool
			var ifaceDeclType types.Type

			// Check if this is an interface type being assigned a value
			if len(valueSpec.Values) > i {
				// Get the type - either from explicit type or from value's type
				var declType types.Type

				if valueSpec.Type != nil {
					declType = v.info.TypeOf(valueSpec.Type)
				} else {
					declType = v.info.TypeOf(ident)
				}

				if declType != nil {
					// Check if it's an interface type
					if isInterface, isEmpty := isInterface(declType); isInterface {
						isInterfaceType = true

						if isEmpty {
							isAnyType = true
						} else {
							// Get the concrete type from the RHS
							rhsType := v.info.TypeOf(valueSpec.Values[i])

							// Record the implementation; the render sites below route the RHS
							// through the interface conversion (pointer-adapter wrapping).
							if rhsType != nil {
								v.convertToInterfaceType(declType, rhsType, "")
								ifaceDeclType = declType
							}
						}
					}
				}
			}

			goIDName := v.getIdentName(ident)
			csIDName := getSanitizedIdentifier(goIDName)

			if csIDName == "_" {
				if v.inFunction {
					csIDName = v.getTempVarName("_")
				} else {
					csIDName = getGlobalTempVarName("_") + CapturedVarMarker
				}
			}

			context := DefaultBasicLitContext()
			context.u8StringOK = !isInterfaceType

			if len(valueSpec.Values) <= i {
				def := v.info.Defs[ident]

				if def != nil {
					if i > 0 {
						v.targetFile.WriteString(v.newline)
					}

					// Check if value spec type is a struct or a pointer to a struct
					valueSpecType := valueSpec.Type

					if subStructType, exprType := v.extractStructType(valueSpecType); subStructType != nil && !v.liftedTypeExists(subStructType) {
						v.visitStructType(subStructType, exprType, csIDName, valueSpec.Comment, true, nil)
					}

					// Check if value spec type is an interface or a pointer to an interface
					if subInterfaceType, exprType := v.extractInterfaceType(valueSpecType); subInterfaceType != nil && !v.liftedTypeExists(subInterfaceType) {
						v.visitInterfaceType(subInterfaceType, exprType, csIDName, valueSpec.Comment, true, nil)
					}

					goTypeName := v.getTypeName(def.Type(), false)
					csTypeName := convertToCSTypeName(goTypeName)

					typeLenDeviation := token.Pos(len(csTypeName) - len(goTypeName) + len(goIDName) + (len(csIDName) - len(goIDName)))

					if v.inFunction {
						heapTypeDecl := v.convertToHeapTypeDecl(ident, true)

						if len(heapTypeDecl) > 0 {
							v.writeOutput(heapTypeDecl)
						} else {
							if arrayType, ok := valueSpecType.(*ast.ArrayType); ok && arrayType.Len != nil {
								// Handle array type
								var arrayLenValue string
								arrayLenExpr := v.convExpr(arrayType.Len, nil)

								// Check if length expression is in type information
								if tv, ok := v.info.Types[arrayType.Len]; ok {
									// Check if it's a constant
									if tv.Value != nil {
										length := tv.Value
										intLength, _ := constant.Int64Val(length)
										arrayLenValue = strconv.FormatInt(intLength, 10)
									}
								}

								if len(arrayLenValue) > 0 && arrayLenValue != arrayLenExpr {
									v.writeOutput("%s %s = new(%s); /* %s */", csTypeName, csIDName, arrayLenValue, arrayLenExpr)
								} else {
									v.writeOutput("%s %s = new(%s);", csTypeName, csIDName, arrayLenExpr)
								}
							} else {
								v.writeOutput("%s %s = default!;", csTypeName, csIDName)
							}
						}
					} else {
						access := getAccess(goIDName)
						typeLenDeviation += token.Pos(len(access) + 6)

						// A fixed-size array global must be allocated (`new(N)`); the default
						// `array<T>` value is empty, so indexing it NREs (e.g. runtime's stackpool).
						// Locals already get this in the in-function branch above.
						var arrayLenValue string

						if arrayType, ok := valueSpecType.(*ast.ArrayType); ok && arrayType.Len != nil {
							arrayLenValue = v.convExpr(arrayType.Len, nil)

							if tv, ok := v.info.Types[arrayType.Len]; ok && tv.Value != nil {
								intLength, _ := constant.Int64Val(tv.Value)
								arrayLenValue = strconv.FormatInt(intLength, 10)
							}
						}

						if v.isAddressedGlobal(ident) {
							// Box an N-sized array, not the empty default, so writes through the
							// pointer (and indexing) hit real storage.
							initExpr := ""

							if len(arrayLenValue) > 0 {
								initExpr = fmt.Sprintf("new %s(%s)", csTypeName, arrayLenValue)
							}

							v.writeAddressedGlobalDecl(access, csTypeName, csIDName, initExpr, isInherentlyHeapAllocatedType(v.getIdentType(ident)))
						} else if len(arrayLenValue) > 0 {
							v.writeOutput("%s static %s %s = new(%s);", access, csTypeName, csIDName, arrayLenValue)
						} else {
							v.writeOutput("%s static %s %s;", access, csTypeName, csIDName)
						}
					}

					v.writeComment(valueSpec.Comment, ident.End()+typeLenDeviation-token.Pos(len(csTypeName)))
				}
				continue
			}

			tv := v.info.Types[valueSpec.Values[i]]

			if tv.Value == nil {
				def := v.info.Defs[ident]

				if def != nil {
					if i > 0 {
						v.targetFile.WriteString(v.newline)
					}

					// A package-global var whose type is inferred from an anonymous-struct
					// composite literal: lift the struct with the var name up front so the
					// declaration type resolves to that lifted name (and the composite
					// literal reuses it) instead of emitting raw `struct{…}` text. Mirrors
					// the explicit-type path used for uninitialized vars.
					if !v.inFunction && valueSpec.Type == nil {
						if compositeLit, ok := valueSpec.Values[i].(*ast.CompositeLit); ok {
							if subStructType, exprType := v.extractStructType(compositeLit.Type); subStructType != nil && !v.liftedTypeExists(subStructType) {
								v.visitStructType(subStructType, exprType, csIDName, valueSpec.Comment, true, nil)
							}
						}
					}

					csTypeName := v.getCSTypeName(def.Type())
					typeLenDeviation := token.Pos(len(csTypeName) + (len(csIDName) - len(goIDName)))

					if v.inFunction {
						// A func-literal initializer (`var f T = func(){ …capture… }`) emits its
						// captured-variable snapshot decls inline; collect them in the hoist buffer
						// and write them on their own line(s) before this declaration.
						hoistBuf := &strings.Builder{}
						savedHoist := v.hoistedDecls
						v.hoistedDecls = hoistBuf
						valExpr := v.convInterfaceDeclValue(valueSpec.Values[i], ifaceDeclType, context)
						v.hoistedDecls = savedHoist

						// A narrow-integer arithmetic initializer (`var x uint8 = a + b`) needs the
						// same cast back to the declared type as the assignment forms — Go wraps the
						// arithmetic at the operand width, C# promotes it to int (CS0266 / lost wrap).
						if narrowCast := v.narrowArithmeticCastTypeFor(def.Type(), valueSpec.Values[i], valExpr); len(narrowCast) > 0 {
							valExpr = fmt.Sprintf("(%s)(%s)", narrowCast, valExpr)
						}

						if hoistBuf.Len() > 0 {
							// The decls carry their own leading newline + per-line indentation;
							// writeOutput below re-indents the declaration line that follows.
							v.targetFile.WriteString(strings.TrimRight(hoistBuf.String(), " \t"))
						}

						heapTypeDecl := v.convertToHeapTypeDecl(ident, true)

						if len(heapTypeDecl) > 0 {
							v.writeOutputLn(heapTypeDecl)
							v.targetFile.WriteString(v.newline)
							v.writeOutput("%s = %s;", csIDName, valExpr)
						} else {
							// Following declarations must use explicit type, do not use `v.options.preferVarDecl` for these:
							v.writeOutput("%s %s = %s;", csTypeName, csIDName, valExpr)
						}
					} else {
						access := getAccess(goIDName)
						typeLenDeviation -= token.Pos(len(access) + 9)

						if v.isAddressedGlobal(ident) {
							v.writeAddressedGlobalDecl(access, csTypeName, csIDName, v.convInterfaceDeclValue(valueSpec.Values[i], ifaceDeclType, context), isInherentlyHeapAllocatedType(v.getIdentType(ident)))
						} else {
							v.writeOutput("%s static %s %s = %s;", access, csTypeName, csIDName, v.convInterfaceDeclValue(valueSpec.Values[i], ifaceDeclType, context))
						}
					}

					v.writeComment(valueSpec.Comment, valueSpec.Values[i].End()-typeLenDeviation)
				}
				continue
			}

			if i > 0 {
				v.targetFile.WriteString(v.newline)
			}

			var csTypeName string

			if isAnyType {
				csTypeName = "any"
			} else {
				csTypeName = convertToCSTypeName(v.getTypeName(tv.Type, false))
			}

			goValue := tv.Value.ExactString()
			csValue := v.convExpr(valueSpec.Values[i], []ExprContext{context})
			typeLenDeviation := token.Pos(len(csTypeName) + len(csValue) + (len(csIDName) - len(goIDName)) + (len(csValue) - len(goValue)))

			if v.inFunction {
				headTypeDecl := v.convertToHeapTypeDecl(ident, true)

				if len(headTypeDecl) > 0 {
					v.writeOutput(headTypeDecl)

					if len(csValue) > 0 {
						v.targetFile.WriteString(v.newline)
						v.writeOutput("%s = %s;", csIDName, csValue)
					}
				} else {
					if len(csValue) > 0 {
						v.writeOutput("%s %s = %s;", csTypeName, csIDName, csValue)
					} else {
						v.writeOutput("%s %s;", csTypeName, csIDName)
					}
				}
			} else {
				access := getAccess(goIDName)
				typeLenDeviation += token.Pos(len(access) + 4)
				if v.isAddressedGlobal(ident) {
					// An addressed package var must be heap-boxed even when its initializer folds to
					// a constant (runtime's `var uint16Eface any = uint16InterfacePtr(0)`, addressed
					// via `efaceOf(&uint16Eface)`): convUnaryExpr emits the box form `Ꮡuint16Eface`,
					// so without boxing here that identifier would not exist (CS0103).
					v.writeAddressedGlobalDecl(access, csTypeName, csIDName, csValue, isInherentlyHeapAllocatedType(v.getIdentType(ident)))
				} else {
					v.writeOutput("%s static %s %s = %s;", access, csTypeName, csIDName, csValue)
				}
			}

			v.writeComment(valueSpec.Comment, ident.End()+typeLenDeviation)
		}
	} else if tok == token.CONST {
		for i, ident := range valueSpec.Names {
			goIDName := v.getIdentName(ident)
			csIDName := getSanitizedIdentifier(goIDName)

			if csIDName == "_" {
				if v.inFunction {
					csIDName = v.getTempVarName("_")
				} else {
					csIDName = getGlobalTempVarName("_") + CapturedVarMarker
				}
			}

			c := v.info.ObjectOf(ident).(*types.Const)
			goTypeName := v.getTypeName(c.Type(), false)
			csTypeName := convertToCSTypeName(goTypeName)
			access := getAccess(goIDName)
			typeLenDeviation := token.Pos(len(csTypeName) + len(access) + (len(csIDName) - len(goIDName)))

			// Check if the type is a named type (user-defined), not a basic type
			isNamedType := false

			if _, ok := c.Type().(*types.Named); ok {
				isNamedType = true
			} else if csTypeName == "UntypedInt" || csTypeName == "UntypedFloat" || csTypeName == "UntypedComplex" {
				isNamedType = true
			}

			var tokEnd token.Pos
			var srcVal string
			var constVal string

			if c.Val().Kind() == constant.String && len(valueSpec.Values) >= i+1 {
				if lit, ok := valueSpec.Values[i].(*ast.BasicLit); ok && lit.Kind == token.STRING {
					constVal = v.convBasicLit(lit, DefaultBasicLitContext())
				} else {
					constVal, _ = v.getStringLiteral(c.Val().ExactString())
				}
			} else if c.Val().Kind() == constant.Float {
				constVal = c.Val().String()
			} else {
				constVal = c.Val().ExactString()
			}

			if valueSpec.Type == nil && len(valueSpec.Values) >= i+1 {
				tokEnd = valueSpec.Values[i].End()

				if ident := getIdentifier(valueSpec.Values[i]); ident != nil {
					srcVal = ident.Name
				} else if lit, ok := valueSpec.Values[i].(*ast.BasicLit); ok {
					srcVal = lit.Value
				}

				typeLenDeviation += token.Pos(len(constVal) - len(srcVal) - 4)
			} else {
				tokEnd = ident.End()
			}

			constHandled := false

			writeUntypedConst := func() {
				if i > 0 {
					v.targetFile.WriteString(v.newline)
				}

				if v.inFunction {
					v.writeOutput("GoUntyped %s = /* ", csIDName)
				} else {
					v.writeOutput("%s static readonly GoUntyped %s = /* ", access, csIDName)
				}

				if len(valueSpec.Values) >= i+1 {
					v.targetFile.WriteString(v.getPrintedNode(valueSpec.Values[i]))
				}

				v.targetFile.WriteString(" */")
				v.writeComment(valueSpec.Comment, tokEnd+token.Pos(len(access)-5))
				v.targetFile.WriteString(v.newline)

				v.writeOutput("%sGoUntyped.Parse(\"%s\");", v.indent(v.indentLevel+1), constVal)
				constHandled = true
			}

			if c.Val().Kind() == constant.Int {
				// Use an untyped (BigInteger) const only when the value fits in
				// neither uint64 nor int64. ParseUint alone rejects negatives, which
				// would wrongly promote ordinary negative consts (e.g. -1) to GoUntyped.
				_, errUint := strconv.ParseUint(constVal, 0, 64)
				_, errInt := strconv.ParseInt(constVal, 0, 64)
				if errUint != nil && errInt != nil {
					writeUntypedConst()
				}
			}

			if c.Val().Kind() == constant.Float {
				// Check if const float value will exceed float64 limits
				if _, err := strconv.ParseFloat(constVal, 64); err != nil {
					constVal = c.Val().ExactString()
					writeUntypedConst()
				}
			}

			if c.Val().Kind() == constant.Complex {
				// Check if const complex value will exceed complex128 limits
				if _, err := strconv.ParseComplex(constVal, 128); err != nil {
					constVal = c.Val().ExactString()

					// TODO: Assignment of complex value to GoUntyped will need to be handled
					writeUntypedConst()
				}
			}

			if c.Val().Kind() == constant.String {
				if i > 0 {
					v.targetFile.WriteString(v.newline)
				}

				if v.inFunction {
					v.writeOutput("@string %s = %s;", csIDName, constVal)
				} else {
					v.writeOutput("%s static readonly @string %s = %s;", access, csIDName, constVal)
				}

				v.writeComment(valueSpec.Comment, tokEnd+typeLenDeviation-1)
				constHandled = true
			}

			// A native-sized integer constant (nint/nuint, incl. the uintptr alias) whose value
			// does not fit a C# constant of that type — e.g. `uintptr MaxUintptr = ^uintptr(0)`
			// = 0xFFFFFFFFFFFFFFFF, a ulong literal needing a non-constant nuint conversion
			// (CS0133/CS0266) — must be emitted as `static readonly` with an unchecked cast
			// rather than `const`. Small native-int consts (e.g. `const nint iota = 0`) are fine.
			nativeIntConst := false
			uintptrConst := false

			// A NAMED type over uintptr (`type Handle uintptr`) has the same gap as raw uintptr:
			// its generated struct bridges the numeric world only through nuint/UntypedInt
			// (UintptrBridgeOperators), so a beyond-int32 folded constant renders as a ulong
			// literal with no implicit path (CS0266 — syscall InvalidHandle = ^Handle(0)).
			namedUintptrType := false

			if isNamedType {
				if basic, ok := c.Type().Underlying().(*types.Basic); ok && basic.Kind() == types.Uintptr {
					namedUintptrType = true
				}
			}

			if c.Val().Kind() == constant.Int && (csTypeName == "nint" || csTypeName == "nuint" || csTypeName == "uintptr" || namedUintptrType) {
				// A C# constant of a native-int type only accepts an int-range literal; a value
				// beyond int32 (e.g. runtime/alg's `uintptr c0 = 33054211828000289`) has no
				// implicit/constant conversion to nint/nuint (CS0133/CS0266), so it must be emitted
				// as `static readonly` with an unchecked cast rather than `const`.
				if _, errInt := strconv.ParseInt(constVal, 0, 32); errInt != nil {
					nativeIntConst = true
				}

				// uintptr is a golib STRUCT (golib/uintptr.cs — distinct from uint), and C# forbids
				// `const` of a user struct entirely: every uintptr const is `static readonly`. An
				// int-range value still initializes via the constant-conversion chain (`= 1`), so
				// the unchecked cast stays reserved for the beyond-int32 case above.
				if csTypeName == "uintptr" {
					uintptrConst = true
				}
			}

			if !constHandled {
				if i > 0 {
					v.targetFile.WriteString(v.newline)
				}

				if srcVal == "iota" {
					constVal = "iota"
				}

				// A plain integer-literal initializer keeps its Go source formatting when it is
				// also a valid C# literal (hex/binary/`_` separators — preserveGoIntLiteral):
				// `const m5 = 0x1d8e4e27c47d124f` emits the hex directly, which also elides the
				// now-redundant `/* original */` comment below. Folded expressions/iota keep the
				// comment form; the GoUntyped path above keeps the decimal (BigInteger.Parse).
				if c.Val().Kind() == constant.Int && len(valueSpec.Values) >= i+1 {
					if lit, ok := valueSpec.Values[i].(*ast.BasicLit); ok && lit.Kind == token.INT {
						constVal = preserveGoIntLiteral(lit.Value, constVal)
					}
				}

				orgExpr := ""

				if len(valueSpec.Values) >= i+1 {
					orgExpr = strings.TrimSpace(v.getPrintedNode(valueSpec.Values[i]))
				}

				if constVal == orgExpr {
					orgExpr = ""
				} else {
					// Try parse both constVal and orgExpr as floating point numbers to see if they are same
					if constNum, err := strconv.ParseFloat(constVal, 64); err == nil {
						if orgNum, err := strconv.ParseFloat(orgExpr, 64); err == nil {
							if constNum == orgNum {
								orgExpr = ""
							}
						}
					}

					if len(orgExpr) > 0 {
						if strings.Contains(orgExpr, "unsafe.Sizeof") {
							v.showWarning("Go const converted to C# using 'unsafe.Sizeof' may not match run-time value - verify usage: const %s = %s", goIDName, orgExpr)
						}

						orgExpr = fmt.Sprintf(" /* %s */", orgExpr)
					}
				}

				var constExpr string
				constValExpr := constVal

				if isNamedType {
					constExpr = "static readonly"

					// Beyond-int32 named-uintptr const: same unchecked cast the native-int
					// consts use — `unchecked((ΔHandle)18446744073709551615)`.
					if nativeIntConst {
						constValExpr = fmt.Sprintf("unchecked((%s)%s)", csTypeName, constVal)
					}
				} else if nativeIntConst {
					constExpr = "static readonly"
					constValExpr = fmt.Sprintf("unchecked((%s)%s)", csTypeName, constVal)
				} else if uintptrConst {
					constExpr = "static readonly"
				} else {
					constExpr = "const"

					// A float32 const initialized from a (double) literal needs an `f` suffix —
					// `const float hashLoad = 6.5` is CS0664 without it. Applied to the emitted value
					// only (constVal is still used above for the doc-comment elision check).
					if c.Val().Kind() == constant.Float && csTypeName == "float32" {
						constValExpr += "f"
					}
				}

				if v.inFunction {
					// C# locals cannot be declared "static readonly"; for named
					// (custom) types "const" is also invalid, so emit a plain local
					// variable. Primitive/string consts can still use "const" locally.
					if isNamedType || nativeIntConst || uintptrConst {
						v.writeOutput("%s %s =%s %s;", csTypeName, csIDName, orgExpr, constValExpr)
					} else {
						v.writeOutput("%s %s %s =%s %s;", constExpr, csTypeName, csIDName, orgExpr, constValExpr)
					}
				} else {
					v.writeOutput("%s %s %s %s =%s %s;", access, constExpr, csTypeName, csIDName, orgExpr, constValExpr)
				}

				v.writeComment(valueSpec.Comment, tokEnd+typeLenDeviation+1)
			}
		}
	} else {
		println(fmt.Sprintf("Unexpected ValueSpec token type: %s", tok))
	}
}

// convInterfaceDeclValue renders a var-decl initializer. When the declared type is a non-empty
// interface (ifaceDeclType non-nil), the value routes through the interface conversion: a POINTER
// value renders as the box and wraps in the pointer-interface adapter (`var inc Incrementer = c`
// emits `Incrementer inc = new CounterᴵIncrementer(c)`) — Go's interface value holds the *T.
// A nil ifaceDeclType renders the plain expression.
func (v *Visitor) convInterfaceDeclValue(value ast.Expr, ifaceDeclType types.Type, context ExprContext) string {
	if ifaceDeclType == nil {
		return v.convExpr(value, []ExprContext{context})
	}

	rhsType := v.info.TypeOf(value)
	contexts := []ExprContext{context}

	if _, isPtr := rhsType.(*types.Pointer); isPtr {
		identContext := DefaultIdentContext()
		identContext.isPointer = true
		contexts = []ExprContext{identContext, context}
	}

	return v.convertToInterfaceType(ifaceDeclType, rhsType, v.convExpr(value, contexts))
}

// visitPackageTupleVarSpec emits a package-level `var a, b = f()` — a single multi-value call
// initializer with no explicit type. C# static field initializers cannot deconstruct a tuple, so
// each non-blank name reads its ValueTuple component. With exactly ONE non-blank name the call
// stays inline on that field and reads its component (`internal static ж<Point> identity =
// @new<Point>().SetBytes(…).Item1;` — blank names keep the plain path's uninitialized `_ᴛNʗ`
// field emission, and the call still runs exactly once). With two or more non-blank names the
// call is evaluated ONCE into a hidden tuple field and each name reads its component from it —
// C# static field initializers run in textual order within a class part, so the reads follow the
// temp. `.ItemN` binds both unnamed and named result tuples. (Comma-ok package vars —
// `var v, ok = m[k]` — are not calls and keep the existing path; no stdlib occurrence.)
func (v *Visitor) visitPackageTupleVarSpec(valueSpec *ast.ValueSpec, tuple *types.Tuple) {
	nonBlankCount := 0

	for _, ident := range valueSpec.Names {
		if ident.Name != "_" {
			nonBlankCount++
		}
	}

	context := DefaultBasicLitContext()
	context.u8StringOK = true

	callExpr := v.convExpr(valueSpec.Values[0], []ExprContext{context})
	componentSource := callExpr
	firstLine := true

	if nonBlankCount > 1 {
		// Hidden once-evaluated tuple holder; the named fields read components from it.
		tempName := getGlobalTempVarName("tuple") + CapturedVarMarker
		componentTypes := make([]string, tuple.Len())

		for i := range tuple.Len() {
			componentTypes[i] = v.getCSTypeName(tuple.At(i).Type())
		}

		v.writeOutput("internal static (%s) %s = %s;", strings.Join(componentTypes, ", "), tempName, callExpr)
		componentSource = tempName
		firstLine = false
	}

	for i, ident := range valueSpec.Names {
		goIDName := v.getIdentName(ident)
		csIDName := getSanitizedIdentifier(goIDName)
		isBlank := csIDName == "_"

		if isBlank {
			csIDName = getGlobalTempVarName("_") + CapturedVarMarker
		}

		if !firstLine {
			v.targetFile.WriteString(v.newline)
		}

		firstLine = false
		csTypeName := v.getCSTypeName(tuple.At(i).Type())
		access := getAccess(goIDName)

		// An ALL-BLANK spec (`var _, _ = f()`) must still evaluate the call once for its side
		// effect: carry it on the first blank's initializer. Otherwise blanks stay uninitialized —
		// the call already ran via the non-blank/temp field.
		if isBlank && !(nonBlankCount == 0 && i == 0) {
			v.writeOutput("%s static %s %s;", access, csTypeName, csIDName)
		} else if v.isAddressedGlobal(ident) {
			v.writeAddressedGlobalDecl(access, csTypeName, csIDName, fmt.Sprintf("%s.Item%d", componentSource, i+1), isInherentlyHeapAllocatedType(tuple.At(i).Type()))
		} else {
			v.writeOutput("%s static %s %s = %s.Item%d;", access, csTypeName, csIDName, componentSource, i+1)
		}
	}

	v.writeComment(valueSpec.Comment, valueSpec.End())
}
