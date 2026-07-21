// visitValueSpec.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"go/ast"
	"go/constant"
	"go/token"
	"go/types"
	"strconv"
	"strings"
	"unicode/utf8"
)

func (v *Visitor) visitValueSpec(valueSpec *ast.ValueSpec, doc *ast.CommentGroup, tok token.Token) {
	v.targetFile.WriteString(v.newline)
	v.writeDoc(doc, valueSpec.End())

	if tok == token.VAR {
		// A PACKAGE-LEVEL var whose initializer contains a func LITERAL (`var Support =
		// sync.OnceValue(func() bool { … })` — internal/syscall/windows) needs the same
		// per-function variable analysis a declared function gets — shadow renames,
		// reassignment tracking, capture state — for the literal's body. Wrap the initializers
		// in a synthetic function so the analysis walks them; without it the body's locals
		// emitted against stale/empty analysis state (`nint n += i` — CS1002 cascade).
		if !v.inFunction && len(valueSpec.Values) > 0 {
			hasFuncLit := false

			for _, value := range valueSpec.Values {
				ast.Inspect(value, func(n ast.Node) bool {
					if _, ok := n.(*ast.FuncLit); ok {
						hasFuncLit = true
						return false
					}

					return !hasFuncLit
				})
			}

			if hasFuncLit {
				stmts := make([]ast.Stmt, 0, len(valueSpec.Values))

				for _, value := range valueSpec.Values {
					stmts = append(stmts, &ast.ExprStmt{X: value})
				}

				syntheticDecl := &ast.FuncDecl{
					Name: ast.NewIdent(""),
					Type: &ast.FuncType{Params: &ast.FieldList{}},
					Body: &ast.BlockStmt{List: stmts},
				}

				// performVariableAnalysis writes v.varNames, which visitFuncDecl allocates
				// per REAL function — a global func-literal var declared BEFORE any function
				// in the file hit a nil map (recovered panic that silently DROPPED the rest
				// of the file, csproj references included — CrossPkgUser's CheckFunc), and
				// one declared after inherited the PREVIOUS function's stale rename table.
				// Give the synthetic decl its own fresh table.
				v.varNames = make(map[*types.Var]string)

				v.performVariableAnalysis(syntheticDecl, types.NewSignatureType(nil, nil, nil, nil, nil, false))
				v.performUntypedConstAnalysis(syntheticDecl)
			}
		}

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

		// A FUNCTION-LOCAL `var name, offset, abs = t.locabs()` (a grouped var spec is not a
		// `:=`, so visitAssignStmt never sees it) deconstructs the same way — the per-name
		// loop below assigned the WHOLE tuple to the first name and silently DEFAULTED the
		// rest (CS0029; time appendFormat read a zero abs). Emit the C# tuple deconstruction
		// (`var (name, offset, abs) = t.locabs();`), matching the `:=` form. Gated to specs
		// with no heap-escaping name (an escaping name needs its `ref heap<T>` box decl —
		// none in the corpus takes this shape yet).
		if v.inFunction && valueSpec.Type == nil && len(valueSpec.Names) > 1 && len(valueSpec.Values) == 1 {
			if _, isCall := valueSpec.Values[0].(*ast.CallExpr); isCall {
				if _, isTuple := v.info.TypeOf(valueSpec.Values[0]).(*types.Tuple); isTuple {
					allPlain := true

					for _, ident := range valueSpec.Names {
						if obj := v.info.Defs[ident]; obj != nil && v.identEscapesHeap[obj] {
							allPlain = false
							break
						}
					}

					if allPlain {
						names := make([]string, len(valueSpec.Names))

						for i, ident := range valueSpec.Names {
							names[i] = getSanitizedIdentifier(v.getIdentName(ident))
						}

						v.targetFile.WriteString(v.newline)
						v.writeOutput("var (%s) = %s;", strings.Join(names, ", "), v.convExpr(valueSpec.Values[0], nil))
						return
					}
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
					// A package var carrying a two-arg //go:linkname PULL (`//go:linkname overflowError
					// runtime.overflowError`, math/bits) is emitted as a forwarding PROPERTY to the remote
					// symbol — Go's linkname aliases their storage, so reads and writes both hit the remote —
					// not the null field this bodyless var would otherwise become. The remote is public via
					// its definition-side one-arg handle (packageVarAccess), and its package is queued for a
					// project reference; the fully-qualified `go.<pkg>_package.<remote>` resolves inside
					// `namespace go;` without a using. See linknameOperations.
					// An ADDRESS-TAKEN pull keeps its heap-box form below: a forwarding property has no
					// address, so `&zeroVal[0]` (reflect, pulling runtime.zeroVal) would reference a
					// nonexistent `ᏑzeroVal` box (CS0103). Such a pull falls through to the addressed-
					// global emission — the pre-feature behavior, which compiles.
					if ref, pkgPath, ok := varLinknamePull(goIDName, doc, valueSpec.Doc); ok && !v.inFunction && !v.isAddressedGlobal(ident) {
						if i > 0 {
							v.targetFile.WriteString(v.newline)
						}

						v.importQueue.Add(pkgPath)
						csTypeName := convertToCSTypeName(v.getTypeName(def.Type(), false))
						v.writeOutput("%s static %s %s { get => %s; set => %s = value; }", getAccess(goIDName), csTypeName, csIDName, ref, ref)
						continue
					}

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

					// An ANONYMOUS func-typed var — or a methodless NAMED func type, which the
					// converter collapses to (and renders everywhere as) its base delegate
					// (methodlessNamedFuncSignature, its own declaration skipped) — renders through
					// the signature-aware path (Func<…>/Action<…> via iifeDelegateType). The raw
					// getTypeName text mangles under convertToCSTypeName: an anonymous
					// `func(string, string) ([]byte, error)` collapses to `(<>byte, error)` (time
					// zoneinfo_read's loadTzinfoFromTzdata, CS1003), and a methodless named func type
					// whose signature carries a slash-bearing cross-package element — go/parser's
					// `var f parseSpecFunction` (`func(*go/ast.CommentGroup, go/token.Token, int)
					// ast.Spec`) — mangles those elements to the nonexistent `go.go.ast.CommentGroup`/
					// `go.go.token.Token` (CS0234), while its matching parameter and assigned-lambda
					// sites render structurally through getCSTypeName. getCSTypeName routes both forms
					// through iifeDelegateType, whose aliasedElementTypeName keeps each element's
					// `pkg.Type` alias. This precedence matches getCSTypeName's own (a func render wins
					// over the foreign-alias route below, whose alias would point at the SKIPPED
					// methodless-func declaration). A NAMED func type WITH methods keeps its delegate name.
					_, isSig := types.Unalias(def.Type()).(*types.Signature)
					_, isMethodlessFunc := methodlessNamedFuncSignature(def.Type())

					if isSig || isMethodlessFunc {
						csTypeName = v.getCSTypeName(def.Type())
					} else if aliased, ok := v.foreignAliasedTypeName(def.Type()); ok {
						// A local declared as a foreign RENAMED type routes through the recorded
						// alias (`syscallꓸSockaddr sa`, not the nonexistent `Δsyscall.Sockaddr` —
						// CS0426, internal/poll sockaddrToRaw).
						csTypeName = aliased
					}

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
									v.writeOutput("%s %s = new(%s); /* %s */", csTypeName, csIDName, v.arrayZeroValueArgs(arrayLenValue, def.Type()), arrayLenExpr)
								} else {
									v.writeOutput("%s %s = new(%s);", csTypeName, csIDName, v.arrayZeroValueArgs(arrayLenExpr, def.Type()))
								}
							} else if arrayType, ok := types.Unalias(def.Type()).(*types.Array); ok {
								// A type ALIAS to a fixed-size array (`type words = [4]uint64`,
								// *types.Alias in Go 1.22+): the spec's type syntax is an Ident, so
								// the ast.ArrayType check above misses — resolve the length through
								// types.Unalias or the local gets `default!` with a null backing
								// array (NRE on first element write).
								v.writeOutput("%s %s = new(%s);", csTypeName, csIDName, v.arrayZeroValueArgs(strconv.FormatInt(arrayType.Len(), 10), arrayType))
							} else if v.structHasPromotedEmbeds(def.Type()) {
								// A struct with a promoted embed stores it in a readonly `ж<T>`
								// box only the constructors initialize — `default!` leaves the
								// box null and the first promoted-member access NREs, so the
								// zero value must construct through the NilType ctor.
								v.writeOutput("%s %s = new(nil);", csTypeName, csIDName)
							} else if v.structZeroValueNeedsConstruction(def.Type()) {
								// A struct whose default(T) is broken by a fixed-size array
								// field (its `= new(N)` backing, at any nesting depth) must run
								// the generated parameterless constructor so those field
								// initializers (and AppendZeroValueInitializers) execute —
								// `default!` skips them, leaving a null backing (len 0 / NRE on
								// index). Mirrors go2cs-gen's NeedsConstruction.
								v.writeOutput("%s %s = new();", csTypeName, csIDName)
							} else {
								v.writeOutput("%s %s = default!;", csTypeName, csIDName)
							}
						}
					} else {
						access := packageVarAccess(goIDName, v.getIdentType(ident))
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
						} else if arrayType, ok := types.Unalias(def.Type()).(*types.Array); ok {
							// Alias-typed global (`var gw words` where `type words = [4]uint64`):
							// same types.Unalias resolution as the local path above.
							arrayLenValue = strconv.FormatInt(arrayType.Len(), 10)
						}

						// A nested/needy element type must be constructed per element — the bare
						// length would leave every element `default(T)` (see arrayZeroValueArgs).
						if len(arrayLenValue) > 0 {
							arrayLenValue = v.arrayZeroValueArgs(arrayLenValue, def.Type())
						}

						// A promoted-embed struct global has the same null-box hazard as the
						// local branch above: the readonly `ж<T>` embed boxes only exist when
						// a constructor runs, so the zero value must be `new(nil)`, not the
						// field's implicit default.
						hasPromotedEmbeds := v.structHasPromotedEmbeds(def.Type())

						// A struct global whose default(T) is broken by a fixed-size array field
						// (at any nesting depth) must likewise construct — `new()` runs the
						// generated parameterless constructor's field initializers. Mirrors the
						// local branch above (structZeroValueNeedsConstruction / go2cs-gen); the
						// promoted-embed case above already covers its own superset, so exclude it.
						needsConstruction := !hasPromotedEmbeds && v.structZeroValueNeedsConstruction(def.Type())

						if v.isAddressedGlobal(ident) {
							// Box an N-sized array, not the empty default, so writes through the
							// pointer (and indexing) hit real storage.
							initExpr := ""

							if len(arrayLenValue) > 0 {
								initExpr = fmt.Sprintf("new %s(%s)", csTypeName, arrayLenValue)
							} else if hasPromotedEmbeds {
								initExpr = fmt.Sprintf("new %s(nil)", csTypeName)
							} else if needsConstruction {
								initExpr = fmt.Sprintf("new %s()", csTypeName)
							}

							v.writeAddressedGlobalDecl(access, csTypeName, csIDName, initExpr, isInherentlyHeapAllocatedType(v.getIdentType(ident)))
						} else if len(arrayLenValue) > 0 {
							v.writeOutput("%s static %s %s = new(%s);", access, csTypeName, csIDName, arrayLenValue)
						} else if hasPromotedEmbeds {
							v.writeOutput("%s static %s %s = new(nil);", access, csTypeName, csIDName)
						} else if needsConstruction {
							v.writeOutput("%s static %s %s = new();", access, csTypeName, csIDName)
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
							} else if subStructType, exprType := v.extractMapValueStructType(compositeLit.Type); subStructType != nil && !v.liftedTypeExists(subStructType) {
								// A map literal whose VALUE type is an anonymous struct (crypto/internal/hpke's
								// `var SupportedKEMs = map[uint16]struct{…}{…}`): lift the value struct up front so
								// both the declaration type (getCSTypeName → getTypeName's Map arm) and the literal
								// type (convMapType → getExprTypeName) resolve it through liftedTypeMap to the lifted
								// name (`map<uint16, SupportedKEMsᴛ1>`), instead of emitting the value's raw Go
								// `struct{…}` syntax into the C# map signature — which does not parse (a
								// CS1519/CS1003 cascade). Mirrors the slice/array element-struct lift above; the
								// keyed element literals stay the target-typed `new(…)` ctor form.
								v.visitStructType(subStructType, exprType, csIDName, valueSpec.Comment, true, nil)
							}
						} else if callExpr, ok := valueSpec.Values[i].(*ast.CallExpr); ok && len(callExpr.Args) == 1 {
							// A builtin `new` over an anonymous struct — `var reserved =
							// new(struct{ types.Type })` (go/internal/gccgoimporter): lift the struct
							// with the var name up front so the declaration type (`ж<reservedᴛ1>`) and
							// the `@new<…>()` type argument both resolve through liftedTypeMap, instead
							// of the declaration falling to the raw `struct{…}` t.String() mangle and
							// the lift arriving from the call-argument path under builtin new's UNNAMED
							// parameter (an empty lift name — a whole-package syntax cascade). Mirrors
							// the composite-literal lift above.
							if funIdent, isIdent := callExpr.Fun.(*ast.Ident); isIdent && funIdent.Name == "new" {
								if _, isBuiltin := v.info.ObjectOf(funIdent).(*types.Builtin); isBuiltin {
									if subStructType, exprType := v.extractStructType(callExpr.Args[0]); subStructType != nil && !v.liftedTypeExists(subStructType) {
										v.visitStructType(subStructType, exprType, csIDName, valueSpec.Comment, true, nil)
									}
								}
							}
						}
					}

					var csTypeName string
					var typeLenDeviation token.Pos

					if v.inFunction {
						// A func-literal initializer (`var f T = func(){ …capture… }`) emits its
						// captured-variable snapshot decls inline; collect them in the hoist buffer
						// and write them on their own line(s) before this declaration.
						hoistBuf := &strings.Builder{}
						savedHoist := v.hoistedDecls
						v.hoistedDecls = hoistBuf
						valExpr := v.convInterfaceDeclValue(valueSpec.Values[i], ifaceDeclType, context)
						v.hoistedDecls = savedHoist

						// Render the declared type only AFTER converting the initializer: a
						// composite literal over an anonymous struct (`var sects = []struct{…}{…}`)
						// lifts the struct type during value conversion, so the declaration
						// resolves to the lifted name instead of raw `struct{…}` Go syntax.
						csTypeName = v.getCSTypeName(def.Type())
						typeLenDeviation = token.Pos(len(csTypeName) + (len(csIDName) - len(goIDName)))

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
						csTypeName = v.getCSTypeName(def.Type())

						access := packageVarAccess(goIDName, v.getIdentType(ident))
						typeLenDeviation = token.Pos(len(csTypeName)+(len(csIDName)-len(goIDName))) - token.Pos(len(access)+9)

						// A multi-value inner call spread in the initializer (`var debug =
						// template.Must(template.New(…).Parse(…))`, net/rpc debug.go) spills
						// into a hidden static tuple field via v.globalDeclHoist; flush it
						// BEFORE this var's field so the once-evaluated holder precedes its
						// readers (C# static field initializers run in textual order).
						globalHoist := &strings.Builder{}
						savedGlobalHoist := v.globalDeclHoist
						v.globalDeclHoist = globalHoist
						valExpr := v.convInterfaceDeclValue(valueSpec.Values[i], ifaceDeclType, context)
						v.globalDeclHoist = savedGlobalHoist

						// A package var whose initializer's Go init-order dependencies C#'s
						// static-field-initializer order cannot reproduce (cross-file / same-file
						// forward reference / dependency on another relocated var — see
						// collectMovedInitVars) is emitted BARE here, with the initializer relocated
						// into an adjacent per-file init method that the ordered static ctor
						// (package_init.cs) calls in InitOrder. The method lives in this file so the
						// rendered expression keeps the file's own using aliases. An addressed global
						// relocates too (its box is declared with the default value; the ctor
						// assignment writes through the ref property into the same box), else a moved
						// dependency of an addressed global would still read zero. The multi-value
						// hoist form falls back inline with a warning (no stdlib occurrence).
						ordinal, moved := v.movedInitOrdinal(def)

						if moved && globalHoist.Len() > 0 {
							v.showWarning("package var '%s' needs init-order relocation but has a multi-value hoisted initializer - left inline (init order NOT guaranteed)", goIDName)
							moved = false
						}

						if moved {
							if v.isAddressedGlobal(ident) {
								v.writeAddressedGlobalDecl(access, csTypeName, csIDName, "", isInherentlyHeapAllocatedType(v.getIdentType(ident)))
							} else {
								v.writeOutput("%s static %s %s;", access, csTypeName, csIDName)
							}

							methodName := packageInitMethodName(csIDName)
							v.targetFile.WriteString(v.newline)
							v.writeOutput("internal static void %s() { %s = %s; }", methodName, csIDName, valExpr)
							recordMovedInitMethod(ordinal, methodName)
						} else {
							if globalHoist.Len() > 0 {
								v.targetFile.WriteString(globalHoist.String())
							}

							if v.isAddressedGlobal(ident) {
								v.writeAddressedGlobalDecl(access, csTypeName, csIDName, valExpr, isInherentlyHeapAllocatedType(v.getIdentType(ident)))
							} else {
								v.writeOutput("%s static %s %s = %s;", access, csTypeName, csIDName, valExpr)
							}
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
			} else if valueSpec.Type != nil {
				// An EXPLICITLY typed spec keeps its DECLARED type — this constant-initializer
				// arm otherwise retypes the var from the VALUE (os's `var Kill Signal =
				// syscall.SIGKILL` emitted syscall's ΔSignal where the os.Signal INTERFACE was
				// declared — CS1503 at every Signal-typed use).
				csTypeName = convertToCSTypeName(v.getTypeName(v.info.TypeOf(valueSpec.Type), false))
			} else {
				csTypeName = convertToCSTypeName(v.getTypeName(tv.Type, false))
			}

			goValue := tv.Value.ExactString()
			csValue := v.convExpr(valueSpec.Values[i], []ExprContext{context})

			// A declared INTERFACE type over a constant initializer wraps the value in the
			// interface conversion (the constant's named type implements the interface —
			// SIGKILL's syscall.Signal implementing os.Signal).
			if valueSpec.Type != nil {
				if declType := v.info.TypeOf(valueSpec.Type); declType != nil {
					if needsCast, isEmpty := isInterface(declType); needsCast && !isEmpty {
						// A constant initializer folds its own named conversion away
						// (`Errno(errnoERROR_IO_PENDING)` renders as the bare reference), so
						// the value loses the type that implements the interface - re-impose
						// it before the interface conversion (syscall zsyscall_windows,
						// UntypedInt -> error CS0029).
						if named, ok := types.Unalias(tv.Type).(*types.Named); ok {
							if _, isBasic := named.Underlying().(*types.Basic); isBasic {
								namedCS := v.getCSTypeName(named)

								// Skip when the render already leads with its own cast
								// (`((errorString)(@string)"..."u8)` needs no second wrap).
								if !strings.HasPrefix(csValue, "(("+namedCS+")") {
									csValue = fmt.Sprintf("((%s)%s)", namedCS, csValue)
								}
							}
						}

						csValue = v.convertToInterfaceType(declType, tv.Type, csValue)
					}

					// An EMPTY-interface declared type (`var x any = 1`) boxes an untyped `int`
					// constant through nint — the numeric twin of the non-empty wrap above and the
					// @string boxing family — so a later `x.(int)` matches Go's boxed `int`. A no-op
					// for a non-empty/non-interface declared type and any non-int-constant value.
					csValue = v.boxUntypedIntAsNint(declType, valueSpec.Values[i], csValue)
				}
			}
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
				access := packageVarAccess(goIDName, v.getIdentType(ident))
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

			// A function-local untyped const whose every use resolves to ONE concrete basic
			// type is DECLARED at that type (see performUntypedConstAnalysis): the wrapper
			// indirection and the per-use casts disappear, C#'s `const` keyword applies where
			// legal for the type, and the existing typed-const machinery below (native-int
			// demotion, uintptr `static readonly`, the float32 `f` suffix) applies unchanged.
			declType := types.Type(c.Type())

			tightenedType, isTightened := v.tightenedConsts[c]

			if isTightened {
				declType = tightenedType
			}

			goTypeName := v.getTypeName(declType, false)
			csTypeName := convertToCSTypeName(goTypeName)
			access := getAccess(goIDName)
			typeLenDeviation := token.Pos(len(csTypeName) + len(access) + (len(csIDName) - len(goIDName)))

			// Check if the type is a named type (user-defined), not a basic type. Unalias first: a
			// const typed through a type ALIAS to a named type (Go 1.23 renders `type Errno =
			// syscall.Errno` as *types.Alias, not *types.Named) still needs `static readonly` — the
			// aliased type is a [GoType] struct C# cannot declare `const` (golang.org/x/sys/windows's
			// `ERROR_… Errno = …`, CS0283/CS0133). Unalias to a *types.Basic (an alias to a primitive)
			// stays non-named, so those remain plain `const`.
			isNamedType := false

			if _, ok := types.Unalias(c.Type()).(*types.Named); ok {
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
				} else if s := constant.StringVal(c.Val()); !utf8.ValidString(s) {
					// A CONCATENATED string const folds to one value here; unlike a single *ast.BasicLit
					// (handled by convBasicLit's byte-array machinery above) it bypassed that path, so a
					// raw-byte table like math/bits' `rev8tab` ("\x00\x80…", built by "" + … concatenation)
					// rendered a UTF-16 string literal whose @string byte view UTF-8-re-encodes each >=0x80
					// byte (`rev8tab[1]` == 0xC2, not 0x80 → Reverse8 wrong). A value that is not valid
					// UTF-8 cannot round-trip through a C# string/u8 literal, so emit its exact bytes; a
					// valid-UTF-8 value keeps the readable getStringLiteral form.
					constVal = byteArrayStringLiteral(s)
				} else {
					constVal, _ = v.getStringLiteral(c.Val().ExactString())
				}
			} else if c.Val().Kind() == constant.Float {
				if basic, ok := declType.(*types.Basic); ok && basic.Info()&types.IsInteger != 0 {
					// A float-KIND value under an INTEGER declared type — an integral untyped
					// float like `const infinity = 1e6` TIGHTENED to its int use type
					// (go/printer nodeSize) — must emit the integer form: C# has no implicit
					// double→int constant conversion (a `1e6` literal is CS0266 against nint),
					// and the tightening pass guaranteed integral representability.
					constVal = constant.ToInt(c.Val()).ExactString()
				} else {
					// The COMPILED value must be exact — Value.String() shortens to ~6 significant
					// digits, truncating the emitted literal (the exact value survived only in the
					// `/* … */` comment; math cbrt's C/D/E/F/G). Emit the source literal verbatim
					// when it is valid C#, else the shortest round-trip form (see exactFloatConstString).
					var srcExpr ast.Expr

					if len(valueSpec.Values) >= i+1 {
						srcExpr = valueSpec.Values[i]
					}

					constVal = exactFloatConstString(c.Val(), srcExpr, csTypeName == "float32")
				}
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

				// A typed const of a NAMED string type keeps that type: materializing it as
				// @string makes every comparison against a value of the named type ambiguous —
				// the [GoType("@string")] wrapper and @string convert implicitly BOTH ways
				// (CS0034 ×20, net/http pattern.go's `const equivalent relationship =
				// "equivalent"`). The u8-literal initializer binds through the wrapper's
				// ReadOnlySpan<byte> implicit operator (StringSurfaceMembers).
				strTypeName := "@string"

				if isNamedType {
					strTypeName = csTypeName
				}

				if v.inFunction {
					v.writeOutput("%s %s = %s;", strTypeName, csIDName, constVal)
				} else {
					v.writeOutput("%s static readonly %s %s = %s;", access, strTypeName, csIDName, constVal)
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
				if basic, ok := c.Type().Underlying().(*types.Basic); ok {
					// A named type over a WIDE UNSIGNED integer (uintptr / uint / uint64) whose folded
					// constant value exceeds int32 needs the same unchecked cast as a native-int const:
					// `^Class(0)` (bidi) / `^big.Word(0)` (go/constant) fold to the underlying's all-ones
					// value (a `ulong` literal in C#), which has no implicit conversion to the named
					// `[GoType]` wrapper (CS0266). Narrow-unsigned underlyings (byte/uint16) fold to
					// small values that the wrapper's int operator still accepts, so they are excluded.
					switch basic.Kind() {
					case types.Uintptr, types.Uint, types.Uint64:
						namedUintptrType = true
					}
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

				// golib's builtin declares `const nint iota = 0`, so a const initialized by
				// exactly the builtin `iota` references that constant directly when it can
				// express the SAME value at an ACCEPTING emitted type: an UntypedInt wrapper
				// takes the nint implicitly, and a declaration EMITTED at nint (explicit Go
				// `int`, or tightened to it) matches golib's constant type exactly —
				// `const nint stateInit = iota;` (compress/flate's stepState group). The `0`
				// gate keeps LATER group positions folded (`x = iota` at position 1 folds to
				// 1, which golib's constant cannot express), and any other emitted type —
				// named wrappers, non-int widths — keeps the folded `/* iota */ N` form
				// rather than casting golib's nint.
				if constVal == "0" && (csTypeName == "nint" || csTypeName == "UntypedInt") && len(valueSpec.Values) >= i+1 {
					if iotaIdent, ok := valueSpec.Values[i].(*ast.Ident); ok && iotaIdent.Name == "iota" {
						if obj := v.info.Uses[iotaIdent]; obj != nil && obj.Parent() == types.Universe {
							constVal = "iota"
						}
					}
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
						// A named type whose WRITTEN base is a CROSS-PACKAGE named type (registry's
						// `type Key syscall.Handle` — [GoType("syscall_package.ΔHandle")]) has NO
						// numeric bridge of its own; the literal hops through the base, one user
						// conversion per cast: `unchecked((Key)(syscall_package.ΔHandle)2147483648)`.
						baseHop := ""

						if named, ok := types.Unalias(c.Type()).(*types.Named); ok {
							if rhs, okRHS := packageTypeSpecRHS[named.Obj()]; okRHS && rhs != nil {
								if rhsNamed, ok := types.Unalias(rhs).(*types.Named); ok && rhsNamed.Obj().Pkg() != named.Obj().Pkg() {
									baseHop = fmt.Sprintf("(%s)", v.getCSTypeName(rhsNamed))
								}
							}
						}

						constValExpr = fmt.Sprintf("unchecked((%s)%s%s)", csTypeName, baseHop, constVal)
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
		// A `var` declaration initialized from an existing array value takes golib's
		// `.Clone()` for independent backing storage (see cloneArrayValueCopy).
		return v.cloneArrayValueCopy(nil, value, v.convExpr(value, []ExprContext{context}))
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

		// A tuple-deconstructing package var flagged for init-order relocation is not yet
		// supported (no stdlib occurrence) — surface it loudly instead of silently misordering.
		if def := v.info.Defs[ident]; def != nil {
			if _, moved := v.movedInitOrdinal(def); moved {
				v.showWarning("package tuple var '%s' needs init-order relocation (unsupported for tuple specs) - left inline (init order NOT guaranteed)", ident.Name)
			}
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
