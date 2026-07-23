// visitFuncDecl.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"strings"
	"unicode"
	"unicode/utf8"
)

const FunctionPrefixMarker = ">>MARKER:FUNC_%s_PREFIX<<"
const FunctionAccessMarker = ">>MARKER:FUNC_%s_ACCESS<<"
const FunctionUnsafeMarker = ">>MARKER:FUNC_%s_UNSAFE<<"
const FunctionPartialMarker = ">>MARKER:FUNC_%s_PARTIAL<<"
const FunctionAttributeMarker = ">>MARKER:FUNC_%s_RECEIVER<<"
const FunctionParametersMarker = ">>MARKER:FUNC_%s_PARAMETERS<<"
const FunctionExecContextMarker = ">>MARKER:FUNC_%s_EXEC_CONTEXT<<"
const FunctionBlockPrefixMarker = ">>MARKER:FUNC_%s_BLOCK_PREFIX<<"

// hasDuplicateBlankParams reports whether a parameter list has two or more blank (`_`) or unnamed
// parameters. Go permits repeated blank params, but C# forbids duplicate parameter names (CS0100), so
// such a list needs synthetic placeholder names. A LONE blank/unnamed param stays `_` (valid C# and
// visually closer to the Go source).
func hasDuplicateBlankParams(parameters *types.Tuple) bool {
	if parameters == nil {
		return false
	}

	count := 0

	for i := 0; i < parameters.Len(); i++ {
		if name := parameters.At(i).Name(); name == "" || name == "_" {
			count++

			if count >= 2 {
				return true
			}
		}
	}

	return false
}

// bodyUsesBlankDiscard reports whether the function's body contains a `_ = …` discard
// (single or tuple position). A LONE blank param normally keeps the literal `_` name
// (visually Go-like), but a parameter named `_` HIJACKS the body's C# discards —
// encoding/binary's bounds-check hints (`_ = b[7]`) assigned a byte to the blank
// littleEndian receiver (CS0029 ×12), so such functions synthesize blank names instead.
func bodyUsesBlankDiscard(funcDecl *ast.FuncDecl) bool {
	if funcDecl == nil || funcDecl.Body == nil {
		return false
	}

	found := false

	ast.Inspect(funcDecl.Body, func(node ast.Node) bool {
		if assign, ok := node.(*ast.AssignStmt); ok {
			for _, lhs := range assign.Lhs {
				if ident, ok := lhs.(*ast.Ident); ok && ident.Name == "_" {
					found = true
					return false
				}
			}
		}

		return !found
	})

	return found
}

// variadicElementType returns the C# element type name for a variadic parameter, the ellipsis alias
// identifier that stands for `Span<element>` in a signature, and whether the parameter must instead be
// emitted inline as `Span<T>`. Both the alias and the inline form sit at namespace scope, so a
// SAME-PACKAGE named element type must be qualified with the package class — a bare nested name like
// `statDep` does not resolve there (CS0246). Only the alias NAME is constrained to a plain identifier
// though; its REFERENT may be qualified freely, so the readable form survives qualification by keying
// the identifier off the element's GO-facing name (`ꓸꓸꓸShape` = `Span<main_package.Shape>`,
// `ꓸꓸꓸunsafeꓸPointer` = `Span<unsafe_package.Pointer>`). A POINTER transliterates through go2cs's own
// `ж` notation (`ꓸꓸꓸжbox`); only a type parameter or a constructed element with no such rendering
// (`map<@string, any>`, `slice<byte>`, `Action<…>`) has nothing to key on and stays inline.
func (v *Visitor) variadicElementType(elem types.Type) (typeName string, aliasName string, inline bool) {
	typeName, identBody, inline := v.variadicElementParts(elem)

	if inline {
		return typeName, "", true
	}

	return typeName, EllipsisOperator + identBody, false
}

// variadicElementParts is variadicElementType's recursive core, returning the alias identifier BODY
// (no ellipsis prefix) so a pointer element can compose its pointee's.
func (v *Visitor) variadicElementParts(elem types.Type) (typeName string, identBody string, inline bool) {
	typeName = v.getCSTypeName(elem)

	// A type parameter is not in scope at namespace scope, so it can be neither an alias referent nor
	// an alias name — `First<T>(params Span<T> valsʗp)` has no readable form.
	if _, isTypeParam := elem.(*types.TypeParam); isTypeParam {
		return typeName, "", true
	}

	// A POINTER is the one CONSTRUCTED form with an identifier-safe rendering: go2cs already writes
	// `*T` as `ж<T>`, so the identifier transliterates to `жT` and still reads like the Go source
	// (`bs ...*box` → `params ꓸꓸꓸжbox bsʗp`). The pointee resolves through this same routine because
	// it needs the identical namespace-scope qualification — the alias referent must say
	// `ж<main_package.box>` where the INLINE form can say bare `ж<box>`, since only the inline form
	// sits inside the package class. A pointee with no alias form of its own (a type parameter, a
	// constructed pointee such as `*[]byte`) takes the whole element inline with it.
	if pointer, ok := elem.(*types.Pointer); ok {
		if typeName != fmt.Sprintf("%s<%s>", PointerPrefix, v.getCSTypeName(pointer.Elem())) {
			// getCSTypeName rendered this pointer some other way (an erased pointer-core type
			// parameter, a lifted type) — do not guess at its structure.
			return typeName, "", true
		}

		pointeeType, pointeeIdent, pointeeInline := v.variadicElementParts(pointer.Elem())

		if pointeeInline {
			return typeName, "", true
		}

		return fmt.Sprintf("%s<%s>", PointerPrefix, pointeeType), PointerPrefix + pointeeIdent, false
	}

	// Every other constructed type carries '<'/'>', which a using-alias identifier cannot contain and
	// for which there is no established transliteration (`Action<ж<options>>`, `map<@string, any>`,
	// `slice<byte>`).
	if strings.ContainsAny(typeName, "<>") {
		return typeName, "", true
	}

	// Derived BEFORE the referent is qualified/rewritten below — the identifier tracks the Go source,
	// the referent tracks what C# can resolve, and the two deliberately diverge.
	aliasIdent := v.variadicAliasIdent(elem, typeName)
	selfQualified := false

	if named, ok := elem.(*types.Named); ok {
		// A methodless named func type has already been rendered AS its base delegate
		// (`Action<…>`/`Func<…>`) by getCSTypeName — it is not a package-class member, so the
		// `<pkg>_package.` qualifier below would mangle it (`main_package.Action`, CS0426).
		if _, isCollapsed := methodlessNamedFuncSignature(elem); !isCollapsed {
			if obj := named.Obj(); obj != nil && obj.Pkg() == v.pkg && !strings.Contains(typeName, ".") {
				typeName = fmt.Sprintf("%s%s.%s", packageName, PackageSuffix, typeName)
				selfQualified = true
			}
		}
	}

	// C# resolves a using-alias REFERENT with the compilation unit's own using directives NOT in
	// effect, so the referent may only name what resolves without them: a bare name (a golib type in
	// `namespace go`, or a GLOBAL-using alias from package_info.cs — `osꓸSignal`/`CorpusEntry` DO
	// carry over, which is why those two already ship as aliases), or a namespace-qualified class.
	// A cross-package SHORT form (`@unsafe.Pointer`, `ast.Expr`) leads with a FILE-LOCAL alias, so it
	// must be rewritten to that alias's own target — which is using-independent by construction,
	// being what the `using <alias> = <target>;` line itself resolves. Left as-is it fails CS0246
	// here, and go2cs-gen (which copies the using into its generated file) cannot resolve the symbol
	// either and falls back to unescaped text, `Span<unsafe.Pointer>`, whose bare keyword cascades
	// to CS8956.
	if !selfQualified {
		if head, member, qualified := strings.Cut(typeName, "."); qualified {
			switch {
			case v.importAliasTargets[head] != "":
				typeName = v.importAliasTargets[head] + "." + member
			case strings.HasSuffix(head, PackageSuffix):
				// Already a package CLASS (`io_package.Writer`), not an alias — a member of the
				// root `go` namespace, so it resolves from any converted file's namespace.
			default:
				// An alias this file has not bound yet — visitFile synthesizes canonical aliases for
				// inference-only foreign references AFTER the declarations are visited, so the target
				// is genuinely unknown here. Degrade to the inline form, which never needs one.
				return typeName, "", true
			}
		}
	}

	return typeName, aliasIdent, false
}

// variadicAliasIdent renders the body of a variadic element's ellipsis alias identifier so the
// signature reads like the Go source it came from: a qualifier is KEPT but joined with TypeAliasDot
// (`...unsafe.Pointer` → `params ꓸꓸꓸunsafeꓸPointer`), the same `pkgꓸType` convention package_info.cs
// already uses for its global usings — which is why os/signal's long-standing `ꓸꓸꓸosꓸSignal` and
// text/template's `ꓸꓸꓸreflectꓸValue` come out byte-identical through this path.
//
// The GO names are preferred over the emitted C# ones: they need no '@' keyword-escape stripping ('@'
// is legal only at identifier START, so `ꓸꓸꓸ@string` is a lex error, CS1002/CS0116), they carry no
// `_package` class suffix, and they undo a Δ collision-rename so go/types' `...Type` reads `ꓸꓸꓸType`
// rather than `ꓸꓸꓸΔType`. Any Go identifier is a legal C# one, and the ellipsis
// prefix means even a C# keyword (`...event`) cannot collide. Types with no such name — a basic type
// (`unsafe.Pointer`), a universe type (`error`), a lifted anonymous struct (internal/fuzz's
// `CorpusEntry`) — fall back to transliterating the emitted C# name.
func (v *Visitor) variadicAliasIdent(elem types.Type, typeName string) string {
	if named, ok := elem.(*types.Named); ok {
		if obj := named.Obj(); obj != nil && obj.Pkg() != nil {
			if obj.Pkg() == v.pkg {
				return obj.Name()
			}

			return obj.Pkg().Name() + TypeAliasDot + obj.Name()
		}
	}

	head, member, qualified := strings.Cut(typeName, ".")

	if !qualified {
		return strings.TrimPrefix(typeName, "@")
	}

	return strings.TrimSuffix(strings.TrimPrefix(head, "@"), PackageSuffix) + TypeAliasDot + member
}

// variadicParamType renders a variadic parameter's C# type, registering the file-local
// `using ꓸꓸꓸT = Span<…>;` alias whenever one applies. The alias is what keeps a variadic signature
// reading like its Go original (`shapes ...Shape` → `params ꓸꓸꓸShape shapesʗp`); it already backs
// os/signal's `Notify(…, params ꓸꓸꓸosꓸSignal sigʗp)` and internal/fuzz's `ꓸꓸꓸCorpusEntry`.
func (v *Visitor) variadicParamType(elem types.Type) string {
	typeName, aliasName, inline := v.variadicElementType(elem)
	spanType := fmt.Sprintf("Span<%s>", typeName)

	if inline {
		return spanType
	}

	// Two element types transliterating to ONE identifier in a single file would bind it twice
	// (CS1537). Keeping the qualifier makes that rare — a same-package `Shape` and an imported
	// `pkg.Shape` land on distinct `ꓸꓸꓸShape`/`ꓸꓸꓸpkgꓸShape` — but same-NAMED packages still collide
	// (`math/rand` and `crypto/rand` both yield `randꓸRand`). The first claim wins; the loser falls
	// back to the inline form, which is always correct if less readable.
	requiredUsing := fmt.Sprintf("%s = %s", aliasName, spanType)
	aliasPrefix := aliasName + " = "

	for _, existing := range v.requiredUsings.Keys() {
		if existing != requiredUsing && strings.HasPrefix(existing, aliasPrefix) {
			return spanType
		}
	}

	v.addRequiredUsing(requiredUsing)

	return aliasName
}

func (v *Visitor) visitFuncDecl(funcDecl *ast.FuncDecl) {
	// A declaration owned by a manual conversion (see manualTypeOperations.go) emits only a
	// marker comment — the package's *_impl.cs supplies the implementation.
	if v.isManualFuncDecl(funcDecl) {
		v.targetFile.WriteString(v.newline)
		v.writeOutput("// go2cs generated this placeholder — func %s is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])", funcDecl.Name.Name)
		v.targetFile.WriteString(v.newline)
		return
	}

	v.inFunction = true
	v.capturedVarCount = nil
	v.tempVarCount = nil
	v.useUnsafeFunc = false
	v.loopCopyBackStack = nil

	// Plan which repeated `string(x)` sstring conversions to lift to a single function-scope temp
	// (loop-invariant / repeated-conversion hoisting — see planSStringHoists). Runs after tempVarCount
	// is reset so the temp names are function-scoped, and before the body is emitted so visitBlockStmt
	// and convCallExpr can consult the plan.
	v.planSStringHoists(funcDecl)

	goFunctionName := funcDecl.Name.Name
	csFunctionName := getSanitizedFunctionName(goFunctionName)

	// A Go function named `_` (blank) is a compile-time-only construct — it is never callable, and
	// a package may declare several. Emitting it literally as a method `_` makes a `_ = expr`
	// discard in its body bind to the method group (CS1656). Give it a unique generated name so
	// `_` remains a discard inside (and multiple blank funcs don't collide).
	if goFunctionName == "_" && funcDecl.Recv == nil {
		csFunctionName = getGlobalTempVarName("_")
	}

	// A `-tests` variant registered THIS test-file method declarator for a Δ-rename — its name
	// collides with a pinned production element or a dot-imported function (B2/B9, see
	// performNameCollisionAnalysis); reference sites follow via convIdent's isMethod arm.
	if testMethodRenames[v.info.ObjectOf(funcDecl.Name)] {
		csFunctionName = ShadowVarMarker + csFunctionName
	}

	v.currentFuncDecl = funcDecl
	v.currentFuncName = csFunctionName
	v.currentFuncPrefix = &strings.Builder{}

	v.varNames = make(map[*types.Var]string)

	currentFuncType := v.info.ObjectOf(funcDecl.Name).(*types.Func)

	if currentFuncType == nil {
		panic("@visitFuncDecl - Failed to find function \"" + goFunctionName + "\" in the type info")
	}

	signature := currentFuncType.Signature()
	v.currentFuncSignature = signature
	v.currentReturnSignature = signature

	// A pointer-core type parameter (`[P *T]`) of a plain function is ERASED — dropped from the
	// emitted generic parameter list, rendered as `ж<T>`, and classified as a pointer everywhere.
	// Every renderer/classifier consults this identity set (see collectErasedTypeParams), so the
	// analyses below (nil-safe params, deref aliases) already see the erased pointers.
	v.erasedTypeParams = collectErasedTypeParams(signature)

	// A generic capture-mode method (e.g. atomic.Pointer[T]) is emitted with its heap box
	// AS the receiver (`this ж<T> Ꮡx`) so the receiver's type parameter stays in scope for
	// the field-ref form `Ꮡx.of(Type.ᏑField)`. See packageDirectBoxReceiverMethods.
	directBoxReceiver := packageDirectBoxReceiverMethods != nil && packageDirectBoxReceiverMethods[currentFuncType]

	// Analyze function variables for reassignments and redeclarations (variable shadows).
	v.performVariableAnalysis(funcDecl, signature)

	// Record the function-local untyped consts whose declaration tightens to a single
	// concrete basic type (the declaration and every wrapper-keyed cast site consult it).
	v.performUntypedConstAnalysis(funcDecl)

	// Scope defer/recover to THIS function's own body: a `defer`/`recover` inside a nested
	// function literal (an IIFE or closure) belongs to that literal, not to this function, so it
	// must not force a func() execution context here. (performVariableAnalysis sets hasDefer/
	// hasRecover by walking everything, including nested literals.)
	v.hasDefer, v.hasRecover = v.funcBodyDeferRecover(funcDecl.Body)

	// A function with named return values that also uses defer/recover needs the named
	// returns declared outside the func() wrapper and returned after it (see the field doc on
	// namedReturnDeferMode). Determine that here, before the body is emitted, so visitReturnStmt
	// can route returns through the named result params.
	v.namedReturnDeferMode = false
	v.namedReturnNames = nil

	if funcDecl.Body != nil {
		v.namedReturnDeferMode, v.namedReturnNames = v.detectNamedReturnDefer(signature, v.hasDefer, v.hasRecover)
	}

	// Collect parameter names from the function declaration
	if v.paramNames == nil {
		v.paramNames = HashSet[string]{}
	} else {
		v.paramNames.Clear()
	}

	// Collect the parameter OBJECTS too, so identIsParameter can distinguish a real parameter from
	// a local that merely SHADOWS a parameter's name (`func f(t *T){ { var t *T; … } }`).
	v.paramObjects = map[types.Object]bool{}

	for _, param := range funcDecl.Type.Params.List {
		for _, name := range param.Names {
			v.paramNames.Add(name.Name)

			if obj := v.info.Defs[name]; obj != nil {
				v.paramObjects[obj] = true
			}
		}
	}

	// Identify pointer parameters that are compared with `==`/`!=` in the body (a nil-terminated
	// walk: `for p != nil { …; p = p.next }`). Such a param's deref alias and pointer-reassignment
	// re-alias must use the nil-safe accessor so re-aliasing to a nil box yields a ref to default(T)
	// (never read while p is nil) instead of throwing a nil-pointer dereference (see comment on
	// nilSafePtrParamNames). Run AFTER paramObjects is populated (identIsParameter relies on it).
	v.collectNilSafePtrParams(funcDecl)

	// Loop through function results to check if any are structs
	if funcDecl.Type.Results != nil {
		for index, field := range funcDecl.Type.Results.List {
			var fieldName string

			if field.Names == nil {
				fieldName = fmt.Sprintf("R%d", index)
			} else {
				fieldName = field.Names[0].Name
			}

			// Check if the return type is a struct or pointer to a struct
			if structType, exprType := v.extractStructType(field.Type); structType != nil && !v.liftedTypeExists(structType) {
				v.indentLevel++
				v.visitStructType(structType, exprType, fieldName, field.Comment, true, nil)
				v.indentLevel--
			}

			// Check if the return type is an anonymous interface
			if interfaceType, exprType := v.extractInterfaceType(field.Type); interfaceType != nil && !v.liftedTypeExists(interfaceType) {
				v.indentLevel++
				v.visitInterfaceType(interfaceType, exprType, fieldName, field.Comment, true, nil)
				v.indentLevel--
			}
		}
	}

	// Loop through function parameters to check if any are structs
	if funcDecl.Type.Params != nil {
		for _, field := range funcDecl.Type.Params.List {
			for _, name := range field.Names {
				// Check if the parameter type is a struct or pointer to a struct
				if structType, exprType := v.extractStructType(field.Type); structType != nil && !v.liftedTypeExists(structType) {
					v.indentLevel++
					v.visitStructType(structType, exprType, name.Name, field.Comment, true, nil)
					v.indentLevel--
				}

				// Check if the parameter type is an anonymous interface
				if interfaceType, exprType := v.extractInterfaceType(field.Type); interfaceType != nil && !v.liftedTypeExists(interfaceType) {
					v.indentLevel++
					v.visitInterfaceType(interfaceType, exprType, name.Name, field.Comment, true, nil)
					v.indentLevel--
				}
			}
		}
	}

	functionPrefixMarker := fmt.Sprintf(FunctionPrefixMarker, goFunctionName)
	functionAccessMarker := fmt.Sprintf(FunctionAccessMarker, goFunctionName)
	functionUnsafeMarker := fmt.Sprintf(FunctionUnsafeMarker, goFunctionName)
	functionPartialMarker := fmt.Sprintf(FunctionPartialMarker, goFunctionName)
	functionAttributeMarker := fmt.Sprintf(FunctionAttributeMarker, goFunctionName)
	functionParametersMarker := fmt.Sprintf(FunctionParametersMarker, goFunctionName)
	functionExecContextMarker := fmt.Sprintf(FunctionExecContextMarker, goFunctionName)
	functionBlockPrefixMarker := fmt.Sprintf(FunctionBlockPrefixMarker, goFunctionName)

	v.targetFile.WriteString(v.newline)
	v.targetFile.WriteString(functionPrefixMarker)
	v.writeDoc(funcDecl.Doc, funcDecl.Pos())

	functionAccess := getAccess(goFunctionName)
	isModuleInitializer := false

	if funcDecl.Recv == nil {
		// Handle Go "main" function as a special case, in C# this should be capitalized "Main"
		if csFunctionName == "main" {
			csFunctionName = "Main"
		} else if csFunctionName == "init" {
			isModuleInitializer = true

			// C# module initializer functions should have internal scope
			functionAccess = "internal"

			packageLock.Lock()

			if initFuncCounter > 0 {
				csFunctionName = fmt.Sprintf("init%s%d", ShadowVarMarker, initFuncCounter)
			}

			initFuncCounter++

			packageLock.Unlock()
		}
	}

	blockContext := DefaultBlockStmtContext()
	blockContext.innerPrefix = functionBlockPrefixMarker
	typeParams, constraints := v.getGenericDefinition(currentFuncType.Type())

	v.writeOutput("%s%s static%s%s %s %s%s(%s)%s%s", functionAttributeMarker, functionAccessMarker, functionUnsafeMarker, functionPartialMarker, v.generateResultSignature(signature), csFunctionName, typeParams, functionParametersMarker, constraints, functionExecContextMarker)

	// The CONVERTED body text (for detecting whether a pointer parameter's deref VALUE alias is
	// actually referenced — a param used only through its box gets no alias; see below). Captured
	// from targetFile across the body visit; the signature written above is excluded.
	bodyText := ""

	if funcDecl.Body != nil {
		blockContext.format.useNewLine = len(constraints) > 0

		// In namedReturnDeferMode the func() wrapper is nested inside an extra block body, so
		// the lambda body sits one level deeper. Bump the indent across the body visit so the
		// statements (and the closing `}` that the `);` attaches to) align under `func(…`.
		bodyInBlockForm := v.namedReturnDeferMode

		if bodyInBlockForm {
			v.indentLevel++
		}

		bodyStart := v.targetFile.Len()
		v.visitBlockStmt(funcDecl.Body, blockContext)
		bodyText = v.targetFile.String()[bodyStart:]

		if bodyInBlockForm {
			v.indentLevel--
		}
	}

	signatureOnly := funcDecl.Body == nil
	useFuncExecutionContext := v.hasDefer || v.hasRecover
	// Pass a variadic params Span explicitly through the one-ref execution wrapper. The wrapper
	// lambda uses its own ref Span parameter instead of capturing the outer ref-like parameter
	// (CS9108), so the slice prologue remains inside and eligible uses retain an aliasing sslice.
	variadicExecRefMode := useFuncExecutionContext && signature.Variadic()
	variadicExecParamType := ""
	variadicExecParamName := ""

	if variadicExecRefMode {
		param := signature.Params().At(signature.Params().Len() - 1)
		variadicExecParamType = v.variadicParamType(param.Type().(*types.Slice).Elem())
		variadicExecParamName = getVariadicParamName(param)
	}

	parameterSignature, receiverAccess := v.generateParametersSignature(signature, true)
	blockPrefix := ""
	// In namedReturnDeferMode this holds the named-return declarations, emitted outside the
	// func() wrapper (see the exec-context assembly below).
	namedReturnDeclsStr := ""

	// If receiver access is not public, update function access to match
	if len(receiverAccess) > 0 && receiverAccess != "public" {
		functionAccess = receiverAccess
	}

	if !signatureOnly {
		resultParameters := &strings.Builder{}
		arrayClones := &strings.Builder{}
		implicitPointers := &strings.Builder{}
		paramHeapBoxes := &strings.Builder{}

		// In namedReturnDeferMode the named-return declarations are emitted OUTSIDE the func()
		// wrapper (so defers/recover mutate them by closure); collect them separately. Otherwise
		// they go into the block prefix inside the wrapper as before.
		namedReturnDecls := &strings.Builder{}

		// In-wrapper `ref var name = ref Ꮡname.Value;` re-aliases for heap-box-backed named
		// results in namedReturnDeferMode (see below) — joined into the block prefix with the
		// parameter deref aliases.
		namedResultAliases := &strings.Builder{}

		if funcDecl.Type.Results != nil && len(funcDecl.Type.Results.List) > 0 {
			resultParams := signature.Results()
			paramIndex := 0

			resultDeclTarget := resultParameters

			if v.namedReturnDeferMode {
				resultDeclTarget = namedReturnDecls
			}

			for _, field := range funcDecl.Type.Results.List {
				names := field.Names

				if len(names) == 0 {
					// Anonymous parameter (no name)
					paramIndex++
				} else {
					for _, ident := range names {
						name := ident.Name

						if isDiscardedVar(name) {
							paramIndex++
							continue
						}

						param := resultParams.At(paramIndex)
						paramName := getSanitizedIdentifier(v.getIdentName(ident))

						resultDeclTarget.WriteString(v.newline)

						// A heap-box-backed named result (routed to shared storage by the
						// capture/escape analyses — see identHasHeapBox) must declare the box
						// its render sites reference (`Ꮡerr`); the plain form leaves it
						// undeclared (CS0103 — internal/poll SendFile's deferred
						// TestHookDidSendFile reading `Ꮡerr.ValueSlot`).
						if v.identHasHeapBox(param, param.Type()) {
							if v.namedReturnDeferMode {
								// The decls sit OUTSIDE the func() wrapper, whose lambda cannot
								// capture a ref local (CS8175): create only the box here; the
								// wrapper re-aliases the value name inside (namedResultAliases),
								// and the final post-defer return reads through the box (see the
								// namedReturnDeferMode close-out below).
								v.writeString(resultDeclTarget, "%sheap<%s>(out var %s%s);", v.indent(v.indentLevel+1), v.getCSTypeName(param.Type()), AddressPrefix, v.boxBaseName(ident))
								v.writeString(namedResultAliases, "%s%sref var %s = ref %s%s%s;", v.newline, v.indent(v.indentLevel+1), paramName, AddressPrefix, v.boxBaseName(ident), namedResultBoxAccessor(param.Type()))
							} else {
								v.writeString(resultDeclTarget, "%s%s", v.indent(v.indentLevel+1), v.convertToHeapTypeDecl(ident, true))
							}

							paramIndex++
							continue
						}

						// A promoted-embed struct result must construct through the NilType
						// ctor — `default!` leaves the readonly embed boxes null (see
						// structHasPromotedEmbeds).
						zeroValue := "default!"

						if v.structHasPromotedEmbeds(param.Type()) {
							zeroValue = "new(nil)"
						}

						v.writeString(resultDeclTarget, "%s%s %s = %s;", v.indent(v.indentLevel+1), v.getCSTypeName(param.Type()), paramName, zeroValue)

						paramIndex++
					}
				}
			}
		}

		parameters := getParameters(signature, true)

		// A pointer param whose deref VALUE alias is skipped (dead — see below) still needs the
		// signature rebuilt so it emits as the box `Ꮡ<name>` the body references — but with no alias
		// there is nothing in implicitPointers to trigger that rebuild. This flag forces it.
		skippedDeadPointerAlias := false

		for i := 0; i < parameters.Len(); i++ {
			param := parameters.At(i)

			// For any array parameters, Go copies the array by value
			// The BODY-side name of a parameter may be shadow-renamed by the analysis (a param
			// sharing an imported package name the function uses — math/big's `rand *rand.Rand`
			// → randΔ1): every alias/clone declared here must use the ANALYZED name or the
			// renamed body uses reference a name that was never declared (CS0103 ×3). The box
			// (`Ꮡrand`) keeps the RAW name, as everywhere else.
			analyzedName := param.Name()

			if renamed, ok := v.varNames[param]; ok && renamed != "" {
				analyzedName = renamed
			}

			// Direct, aliased, AND named array types all clone: a NAMED array's generated wrapper
			// is a struct over the same shared backing, and its strongly-typed Clone() returns the
			// wrapper (an array-typed VALUE RECEIVER — necessarily a named type — is a per-call
			// copy in Go and clones here too).
			if typeIsArrayValue(param.Type()) {
				// A heap-boxed array param folds its by-value clone into the box init below
				// (the plain reassignment would reference the pre-rename name — CS0103).
				if !v.paramNeedsHeapBox(param) {
					v.writeString(arrayClones, "%s%s%s = %s.Clone();", v.newline, v.indent(v.indentLevel+1), getSanitizedIdentifier(analyzedName), getSanitizedIdentifier(analyzedName))
				}
			}

			// All pointers in Go can be implicitly dereferenced, so setup a "local ref" instance to each.
			// paramPointerType also classifies an ERASED pointer-core type parameter (`p P` under
			// `[P *T]` renders as `ж<T> Ꮡp` — see pointerCoreConstraint), so it takes the same
			// deref-alias and box conventions as a plain pointer parameter.
			if pointerType, ok := v.paramPointerType(param.Type()); ok {
				if i == 0 && funcDecl.Recv != nil && !directBoxReceiver {
					// Skip receiver parameter (direct-ж receivers get the deref below, so
					// the box parameter `Ꮡx` resolves to the value `x` in the body).
					continue
				}

				// An unnamed (`func(*T)`) or blank (`_`) pointer parameter is never referenced in the
				// body, so it gets no deref alias; it is emitted in the signature with a synthetic name
				// and no box (`Ꮡ`) convention. Emitting the deref would produce `ref var  = ref Ꮡ.Value;`.
				if param.Name() == "" || param.Name() == "_" {
					continue
				}

				// A NAMED pointer param whose deref'd VALUE alias is never referenced in the body —
				// every use goes through the box `Ꮡp` (`unsafe.Pointer(p)`, `p == nil`, or passing p
				// as a pointer) — gets no alias either. The alias `ref var p = ref Ꮡp.Value` would be
				// a dead local that DEREFERENCES the box, so a nil argument NREs at function entry even
				// though Go never touches the pointee (syscall's `writeFile(…, overlapped *Overlapped)`
				// called with a nil overlapped, used only as `unsafe.Pointer(overlapped)`). Skipping an
				// unreferenced alias is behavior-preserving and removes the spurious nil deref. The
				// scan errs toward KEEPING the alias — a coincidental match in a field selector,
				// string, or comment — so a genuinely live value alias is never dropped (no CS0103).
				if !bodyReferencesIdentAsValue(bodyText, getSanitizedIdentifier(analyzedName)) {
					// The param still needs the box rename (`Ꮡ<name>`) the body's uses reference —
					// force the signature rebuild even if this leaves implicitPointers empty.
					skippedDeadPointerAlias = true
					continue
				}

				// A pointer param walked to a nil terminator (`for p != nil { …; p = p.next }`) derefs
				// through the nil-safe accessor so a nil argument (or, after the in-loop re-alias, a
				// nil box) yields a ref to default(T) instead of throwing — the ref is never read while
				// the box is nil (the `Ꮡp != nil` guard excludes it). Other pointer params keep `.Value`.
				derefAccessor := "Value"

				if v.nilSafePtrParamNames.Contains(param.Name()) {
					derefAccessor = NilSafeDerefAccessor
				} else if isInherentlyHeapAllocatedType(pointerType.Elem()) {
					// The POINTEE is itself a reference type (`*error`, `*[]T`, `*map`, `**T`,
					// `*func`, `*chan`): the box is a real, structurally non-nil pointer (`Ꮡ<name>`),
					// but its held value is legitimately null when the pointee is the zero value —
					// a nil interface/slice/map/etc. Establishing the entry deref ALIAS is a read of
					// the held value, NOT a dereference of the box (in Go, `*(&err)` of a nil `error`
					// yields nil, no panic), yet `.Value`'s IsNull check fires on `m_val is null` and
					// throws a spurious NilPointerDereference at function entry (e.g. tabwriter's
					// `handlePanic(err *error)`). Use the nil-check-free `.ValueSlot`, mirroring
					// namedResultBoxAccessor (a named result of the same type already reads this way);
					// it returns the same real slot as `.Value` in every non-throwing case, so
					// write-through and non-null reads are byte-behaviorally identical.
					derefAccessor = "ValueSlot"
				}

				if v.options.preferVarDecl {
					v.writeString(implicitPointers, "%s%sref var %s = ref %s%s.%s;", v.newline, v.indent(v.indentLevel+1), getSanitizedIdentifier(analyzedName), AddressPrefix, param.Name(), derefAccessor)
				} else {
					v.writeString(implicitPointers, "%s%sref %s %s = ref %s%s.%s;", v.newline, v.indent(v.indentLevel+1), convertToCSTypeName(pointerType.Elem().String()), getSanitizedIdentifier(analyzedName), AddressPrefix, param.Name(), derefAccessor)
				}
			}

			// A value parameter marked for an entry-time heap box (a capture-mode/direct-ж
			// method is called on it — see paramNeedsHeapBox): the incoming value arrives as
			// `<name>ʗp` (renamed in the signature) and is boxed here, so body uses read and
			// write the boxed storage — the same storage the callee mutates through the
			// receiver pointer — and convSelectorExpr routes the call through `Ꮡ<name>`. The
			// box keeps the ANALYZED (rendered) name, matching an escaping value local's
			// convention (see convertToHeapTypeDecl / boxBaseName). An ARRAY param folds its
			// Go by-value clone into the box init (its plain clone line above is skipped).
			if v.paramNeedsHeapBox(param) {
				incomingName := getHeapBoxParamName(param)

				if typeIsArrayValue(param.Type()) {
					incomingName += ".Clone()"
				}

				if v.options.preferVarDecl {
					v.writeString(paramHeapBoxes, "%s%sref var %s = ref heap(%s, out var %s%s);", v.newline, v.indent(v.indentLevel+1), getSanitizedIdentifier(analyzedName), incomingName, AddressPrefix, analyzedName)
				} else {
					csTypeName := v.getCSTypeName(param.Type())
					v.writeString(paramHeapBoxes, "%s%sref %s %s = ref heap(%s, out %s<%s> %s%s);", v.newline, v.indent(v.indentLevel+1), csTypeName, getSanitizedIdentifier(analyzedName), incomingName, PointerPrefix, csTypeName, AddressPrefix, analyzedName)
				}
			}

			// Check if parameter is variadic, in this case parameter is a C# params array that needs to be converted to a Go slice<T>
			if i == parameters.Len()-1 && signature.Variadic() {
				useSSlice := v.ssliceEligible[param]
				sliceMethod := "slice"
				sliceType := "slice"

				if useSSlice {
					sliceMethod = "sslice"
					sliceType = "sslice"
				}

				if v.options.preferVarDecl {
					v.writeString(resultParameters, "%s%svar %s = %s.%s();", v.newline, v.indent(v.indentLevel+1), getSanitizedIdentifier(param.Name()), getVariadicParamName(param), sliceMethod)
				} else {
					v.writeString(resultParameters, "%s%s%s<%s> %s = %s.%s();", v.newline, v.indent(v.indentLevel+1), sliceType, v.getCSTypeName(param.Type().(*types.Slice).Elem()), getSanitizedIdentifier(param.Name()), getVariadicParamName(param), sliceMethod)
				}

			}
		}

		if namedReturnDecls.Len() > 0 {
			namedReturnDeclsStr = namedReturnDecls.String()
		}

		if resultParameters.Len() > 0 {
			resultParameters.WriteString(v.newline)
			blockPrefix += resultParameters.String()
		}

		if arrayClones.Len() > 0 {
			if blockPrefix == "" {
				arrayClones.WriteString(v.newline)
			}

			blockPrefix += arrayClones.String()
		}

		if implicitPointers.Len() > 0 {
			if blockPrefix == "" {
				implicitPointers.WriteString(v.newline)
			}

			blockPrefix += implicitPointers.String()
		}

		// Entry-time heap boxes for capture-mode value params follow the pointer derefs; the
		// signature rebuild below renames each boxed param to its incoming `ʗp` form.
		if paramHeapBoxes.Len() > 0 {
			if blockPrefix == "" {
				paramHeapBoxes.WriteString(v.newline)
			}

			blockPrefix += paramHeapBoxes.String()
		}

		// In-wrapper value aliases for heap-box-backed named results (namedReturnDeferMode):
		// the box was created outside the wrapper; body statements keep referencing the plain
		// name through this ref alias, exactly like a deref'd pointer parameter.
		if namedResultAliases.Len() > 0 {
			if blockPrefix == "" {
				namedResultAliases.WriteString(v.newline)
			}

			blockPrefix += namedResultAliases.String()
		}

		if implicitPointers.Len() > 0 || paramHeapBoxes.Len() > 0 || skippedDeadPointerAlias {
			updatedSignature := strings.Builder{}
			dupBlankParams := hasDuplicateBlankParams(parameters) || bodyUsesBlankDiscard(funcDecl)

			for i := 0; i < parameters.Len(); i++ {
				param := parameters.At(i)

				if i == 0 && funcDecl.Recv != nil {
					updatedSignature.WriteString("this ")

					// Get receiver parameter type
					recvTypeName := v.getRefParamTypeName(param.Type())

					// Method accessibility is the more restrictive of the receiver type and the method's
					// own (Go) name: an unexported method on an exported type stays package-private
					// (internal) -- otherwise a public method returning that method's own unexported
					// types is CS0050 (inconsistent accessibility). A PUBLICIZED unexported receiver
					// type is emitted `public`, so its exported methods count as public here.
					if (getAccess(recvTypeName) == "public" || receiverTypeIsPublicized(param.Type())) && getAccess(goFunctionName) == "public" {
						functionAccess = "public"
					} else {
						functionAccess = "internal"
					}

					if directBoxReceiver {
						// Direct-ж: emit the box itself (`ж<Box<T>> Ꮡb`) as the receiver. The
						// deref `ref var b = ref Ꮡb.Value;` is emitted above so the body's value
						// references still read as `b`, while `&b.field` uses the box `Ꮡb`.
						updatedSignature.WriteString(v.getCSTypeName(param.Type()))
						updatedSignature.WriteRune(' ')
						updatedSignature.WriteString(AddressPrefix + param.Name())
					} else {
						updatedSignature.WriteString(recvTypeName)
						updatedSignature.WriteRune(' ')

						recvParamName := param.Name()

						// A BLANK receiver keeps the literal `_` only when the body has no
						// `_ = …` discard — a parameter named `_` hijacks C# discards
						// (encoding/binary's bounds-check hints, CS0029 ×12).
						if recvParamName == "" || recvParamName == "_" {
							if dupBlankParams {
								recvParamName = fmt.Sprintf("_Δp%d", i)
							} else {
								recvParamName = "_"
							}
						}

						updatedSignature.WriteString(getSanitizedIdentifier(recvParamName))
					}

					continue
				}

				if i > 0 {
					updatedSignature.WriteString(", ")
				}

				if i == parameters.Len()-1 && signature.Variadic() {
					updatedSignature.WriteString("params ")

					// If parameter is a slice, convert it to a Span
					if sliceType, ok := param.Type().(*types.Slice); ok {
						updatedSignature.WriteString(v.variadicParamType(sliceType.Elem()))
					} else {
						updatedSignature.WriteString("object[]")
					}

					// Variadic parameters are passed as C# param arrays, so we use a temporary
					// parameter name that will be later converted to a Go slice<T>
					updatedSignature.WriteRune(' ')
					updatedSignature.WriteString(getVariadicParamName(param))
				} else {
					updatedSignature.WriteString(v.getCSTypeName(param.Type()))
					updatedSignature.WriteRune(' ')

					if _, ok := v.paramPointerType(param.Type()); ok {
						// An unnamed or blank (`_`) pointer param is never referenced (no deref alias
						// above), so emit a plain name without the box `Ꮡ` convention — synthesized
						// unique only when blanks would collide (else a lone `_` is kept).
						if param.Name() == "" || param.Name() == "_" {
							if dupBlankParams {
								updatedSignature.WriteString(fmt.Sprintf("_Δp%d", i))
							} else {
								updatedSignature.WriteString("_")
							}
						} else {
							updatedSignature.WriteString(AddressPrefix)
							updatedSignature.WriteString(param.Name())
						}
					} else if param.Name() == "" || param.Name() == "_" {
						// Unnamed or blank (`_`) non-pointer param — keep a lone `_`, but synthesize a
						// unique placeholder when blanks would collide (Go allows repeated blank params;
						// C# forbids duplicate parameter names — CS0100).
						if dupBlankParams {
							updatedSignature.WriteString(fmt.Sprintf("_Δp%d", i))
						} else {
							updatedSignature.WriteString("_")
						}
					} else if v.paramNeedsHeapBox(param) {
						// A heap-boxed value parameter arrives under the `ʗp` name; the parameter
						// preamble re-declares the analyzed name as the boxed ref alias.
						updatedSignature.WriteString(getHeapBoxParamName(param))
					} else {
						// A shadow-renamed value parameter must emit its renamed name so the declaration
						// matches its usages (see generateParametersSignature) — this rebuilt-signature
						// path is taken for a function that ALSO has a pointer param (crypto/rsa's
						// `EncryptOAEP(hash hash.Hash, …, *PublicKey)`, where `hash` shadows the `hash`
						// package → `hashΔ1`), so it bypassed the generateParametersSignature lookup and
						// left the decl raw (`hash.Hash hash`) while its uses were `hashΔ1` (CS0103).
						paramName := param.Name()

						if adjusted, ok := v.varNames[param]; ok && adjusted != "" {
							paramName = adjusted
						}

						updatedSignature.WriteString(getSanitizedIdentifier(paramName))
					}
				}
			}

			parameterSignature = updatedSignature.String()
		}
	}

	// Replace function markers
	v.replaceMarker(functionAccessMarker, functionAccess)

	if v.useUnsafeFunc {
		v.replaceMarker(functionUnsafeMarker, " unsafe")
		usesUnsafeCode = true
	} else {
		v.replaceMarker(functionUnsafeMarker, "")
	}

	// A bodyless func carrying `//go:linkname localName otherPkg.func` PULLS another package's
	// (often unexported) function by symbol — golang.org/x/sys/windows's LazyDLL/LazyProc reach
	// syscall.loadlibrary/loadsystemlibrary/getprocaddress this way. Route it: emit a forwarder body
	// that calls the target, instead of a throwing partial stub. Detected here so the `partial`
	// modifier is dropped (it now has a body); the body is written at the signatureOnly branch below.
	linknameAlias, linknameFunc, hasLinknameForward := "", "", false

	if funcDecl.Body == nil {
		linknameAlias, linknameFunc, hasLinknameForward = v.funcLinknameForward(funcDecl)
	}

	// A nil body means the Go function is implemented externally (assembly or cgo):
	// emit a `partial` declaration. Its implementation is supplied either by a
	// hand-written companion (e.g. sync/atomic's doc_impl.cs) or, when none exists, by
	// the PartialStubGenerator (go2cs-gen), which emits a throwing default so the code
	// still compiles.
	if funcDecl.Body == nil && !hasLinknameForward {
		v.replaceMarker(functionPartialMarker, " partial")
	} else {
		v.replaceMarker(functionPartialMarker, "")
	}

	v.replaceMarker(functionParametersMarker, parameterSignature)

	if isModuleInitializer {
		// The `runtime` package's own init functions are Go's runtime SELF-BOOTSTRAP (arena
		// sizing checks, GC/proc setup): in real Go they run only after the assembly bootstrap
		// (osinit/schedinit) has populated globals like physPageSize. Converted code has no such
		// bootstrap — .NET is the runtime — so running them as module initializers executes
		// self-checks against zero-valued stub globals (arena's `% physPageSize` divides by
		// zero at assembly load, before Main). The faithful conversion of the Go runtime
		// bootstrap is to not run it: emit them as plain (never-called) methods.
		if v.pkg.Path() == "runtime" {
			v.replaceMarker(functionAttributeMarker, "/* [GoInit] runtime bootstrap init - not run; .NET is the runtime */ ")
		} else {
			v.replaceMarker(functionAttributeMarker, "[GoInit] ")
		}
	} else if strings.HasPrefix(parameterSignature, "this ref ") {
		v.replaceMarker(functionAttributeMarker, "[GoRecv] ")
	} else {
		v.replaceMarker(functionAttributeMarker, "")
	}

	var funcExecutionContext string

	if useFuncExecutionContext {
		// Always name the wrapper's two parameters by their roles, even when one is unused.
		// Using `_` for an unused parameter is unsafe: a single `_` is a *named* C# lambda
		// parameter (not a discard), so a `_ = expr` discard in the body would bind to it
		// rather than discarding (e.g. `_ = call()` → CS0029). Unused-parameter warnings are
		// already suppressed (IDE0060) for the converted projects.
		deferParam := "defer"
		recoverParam := "recover"

		if v.namedReturnDeferMode {
			// Named results stay outside the wrapper so deferred code can mutate them. A variadic
			// params Span is passed by ref into the wrapper rather than captured from outer scope.
			wrapperHead := fmt.Sprintf("func((%s, %s) =>", deferParam, recoverParam)

			if variadicExecRefMode {
				wrapperHead = fmt.Sprintf("func(ref %s, (ref %s %s, Defer %s, Recover %s) =>", variadicExecParamName, variadicExecParamType, variadicExecParamName, deferParam, recoverParam)
			}

			funcExecutionContext = fmt.Sprintf(" {%s%s%s%s", namedReturnDeclsStr, v.newline, v.indent(v.indentLevel+1), wrapperHead)
		} else if variadicExecRefMode {
			resultTypeArg := ""

			if signature.Results().Len() > 0 && (v.allExecWrapperReturnsAreTypeless(funcDecl) || v.execWrapperReturnsLackCommonType(funcDecl)) {
				resultTypeArg = fmt.Sprintf("<%s, %s>", variadicExecParamType, v.generateResultSignature(signature))
			}

			funcExecutionContext = fmt.Sprintf(" => func%s(ref %s, (ref %s %s, Defer %s, Recover %s) =>", resultTypeArg, variadicExecParamName, variadicExecParamType, variadicExecParamName, deferParam, recoverParam)
		} else {
			// C# infers the value-returning wrapper's T (func<T>, builtin.cs GoFunction) from the
			// lambda's return statements; a return whose expression contains a typeless `default!`
			// (Go nil) has no natural type — a tuple literal is typed only when ALL elements are.
			// When NO return contributes a type, inference fails and overload resolution binds the
			// void GoAction overload instead (CS8030 on every value return — syscall
			// getProcessEntry). Emit the explicit result type argument for exactly that shape;
			// any function with one fully-typed return keeps the inferred (unchanged) form.
			resultTypeArg := ""

			if signature.Results().Len() > 0 && (v.allExecWrapperReturnsAreTypeless(funcDecl) || v.execWrapperReturnsLackCommonType(funcDecl)) {
				resultTypeArg = fmt.Sprintf("<%s>", v.generateResultSignature(signature))
			}

			funcExecutionContext = fmt.Sprintf(" => func%s((%s, %s) =>", resultTypeArg, deferParam, recoverParam)
		}
	} else {
		funcExecutionContext = ""
	}

	v.replaceMarker(functionExecContextMarker, funcExecutionContext)
	v.replaceMarker(functionBlockPrefixMarker, blockPrefix)

	if v.currentFuncPrefix.Len() > 0 {
		v.currentFuncPrefix.WriteString(v.newline)
	}

	v.replaceMarker(functionPrefixMarker, v.currentFuncPrefix.String())

	if useFuncExecutionContext {
		if v.namedReturnDeferMode {
			// Close the func(...) call (attaches to the lambda's `}` → `});`), then return the
			// named results — which the deferred code / recover may have mutated — and close the
			// block body opened in the exec-context above. A heap-box-backed result's value alias
			// lives INSIDE the wrapper, so the return reads it through the box (`Ꮡerr.ValueSlot`).
			returnNames := v.namedReturnBoxReadNames(signature, v.namedReturnNames)
			returnExpr := strings.Join(returnNames, ", ")

			if len(returnNames) > 1 {
				returnExpr = "(" + returnExpr + ")"
			}

			indentInner := v.indent(v.indentLevel + 1)
			indentOuter := v.indent(v.indentLevel)
			savedIndent := v.indentLevel
			v.indentLevel = 0
			v.writeOutputLn(");")
			v.writeOutputLn("%sreturn %s;", indentInner, returnExpr)
			v.writeOutputLn("%s}", indentOuter)
			v.indentLevel = savedIndent
		} else {
			v.writeOutputLn(");")
		}
	} else if signatureOnly {
		if hasLinknameForward {
			// Cross-package //go:linkname pull — emit a forwarder body calling the target.
			v.writeLinknameForwarder(signature, linknameAlias, linknameFunc)
		} else {
			// Bodyless (assembly/cgo) function: emit a `partial` declaration; the body is
			// supplied by a hand-written companion or the PartialStubGenerator.
			v.writeOutputLn(";")
		}
	} else {
		v.targetFile.WriteString(v.newline)
	}

	v.inFunction = false
}

// allExecWrapperReturnsAreTypeless reports whether no top-level return statement in the function
// body carries a C#-inferable natural type: every return (if any) includes at least one untyped-nil
// result, which renders as a typeless `default!`. Nested function literals are skipped — they get
// their own execution wrappers. Zero-return bodies (all paths loop/panic) also report true: with no
// returns C# infers a VOID lambda, which cannot convert to the value-returning wrapper either.
func (v *Visitor) allExecWrapperReturnsAreTypeless(funcDecl *ast.FuncDecl) bool {
	if funcDecl.Body == nil {
		return false
	}

	unreliableReturnFound := false

	ast.Inspect(funcDecl.Body, func(n ast.Node) bool {
		if unreliableReturnFound {
			return false
		}

		if _, ok := n.(*ast.FuncLit); ok {
			return false
		}

		returnStmt, ok := n.(*ast.ReturnStmt)

		if !ok {
			return true
		}

		if len(returnStmt.Results) == 0 {
			return true
		}

		for _, result := range returnStmt.Results {
			tv, ok := v.info.Types[result]

			if !ok {
				continue
			}

			// A Go-nil result renders `default!` (no natural type). A CONSTANT result
			// renders as a bare literal typed by C#'s defaults (`return 0, err` in a
			// (uint32, error) function types its tuple as (int, error)) — either way this
			// return's natural type can DISAGREE with the declared results, and C# lambda
			// inference needs every return to agree (one poisoned return binds the void
			// GoAction overload — CS8030 ×2, poll GetFileType's `return 0, err` beside a
			// fully-typed call return). ANY such result forces the explicit form.
			if tv.IsNil() || tv.Value != nil {
				unreliableReturnFound = true
				return false
			}
		}

		return true
	})

	return unreliableReturnFound
}

// execWrapperReturnsLackCommonType reports whether the function's top-level return statements yield,
// at some result position, expressions whose types have NO best-common-type — i.e. no single one of
// them that every other is identical or assignable to. C# infers a value-returning exec wrapper's T
// (`func<T>`, builtin.cs GoFunction) from the lambda's return expressions using best-common-type; two
// returns of unrelated concrete types that share only the declared interface (go/parser parseTypeName:
// `return &ast.SelectorExpr{...}` beside `return ident` where the result type is ast.Expr) have no
// common type, so T cannot be inferred and overload resolution binds the void GoAction overload
// (CS8030 on every value return). Emit the explicit result type for exactly that shape; a single
// return, or returns that DO share a best-common-type (a concrete beside its interface), keep the
// inferred (unchanged) form and churn no goldens. Nested function literals get their own wrappers.
func (v *Visitor) execWrapperReturnsLackCommonType(funcDecl *ast.FuncDecl) bool {
	if funcDecl.Body == nil {
		return false
	}

	var posTypes [][]types.Type // per result position: the return-expression types seen

	ast.Inspect(funcDecl.Body, func(n ast.Node) bool {
		if _, ok := n.(*ast.FuncLit); ok {
			return false
		}

		returnStmt, ok := n.(*ast.ReturnStmt)

		if !ok {
			return true
		}

		for i, result := range returnStmt.Results {
			tv, ok := v.info.Types[result]

			if !ok || tv.Type == nil {
				continue
			}

			for len(posTypes) <= i {
				posTypes = append(posTypes, nil)
			}

			posTypes[i] = append(posTypes[i], tv.Type)
		}

		return true
	})

	for _, atPos := range posTypes {
		if len(atPos) < 2 {
			continue
		}

		// A best-common-type exists iff some candidate accepts every other type at this position.
		hasCommon := false

		for _, cand := range atPos {
			acceptsAll := true

			for _, t := range atPos {
				if !types.Identical(t, cand) && !types.AssignableTo(t, cand) {
					acceptsAll = false
					break
				}
			}

			if acceptsAll {
				hasCommon = true
				break
			}
		}

		if !hasCommon {
			return true
		}
	}

	return false
}

// identIsParameter checks if the given identifier is a parameter in the current function.
func (v *Visitor) identIsParameter(ident *ast.Ident) bool {
	if v.paramNames == nil || !v.paramNames.Contains(ident.Name) {
		return false
	}

	// The name matches a parameter, but a local can SHADOW a parameter of the same name. Only the
	// actual parameter object gets the deref-aliased box treatment (`Ꮡp`); a shadowing local that is
	// already a pointer (`ж<T> tΔ2`) must keep its plain form, not get a spurious `&` (CS0103 on the
	// undefined `ᏑtΔ2`). Verify the resolved object is genuinely a parameter; fall back to the name
	// match when it cannot be resolved.
	if obj := v.info.ObjectOf(ident); obj != nil && v.paramObjects != nil {
		return v.paramObjects[obj]
	}

	return true
}

// collectNilSafePtrParams populates v.nilSafePtrParamNames with the raw names of the pointer
// parameters that are compared with `==`/`!=` anywhere in body. A compared param signals nil is a
// LEGAL argument (Go panics only on an actual deref, not at entry), so the eager entry alias
// `ref var p = ref Ꮡp.Value` must not throw for it — it uses the nil-safe accessor instead. This
// covers both the nil-terminated walk (`for p != nil { …; p = p.next }`, where the reassignment
// repoints the box to the terminator) and a nil-testing body invoked with a nil argument
// (`defer closeIt(nil, …)` → `p == nil`). Valid value reads of such a param sit behind non-nil
// guards, never touching the shared default slot; the accepted trade-off (same as the walk case)
// is that an UNGUARDED deref of an actually-nil argument reads default(T) instead of raising Go's
// nil-deref panic — a path that only a program already panicking in Go would observe. A param that
// is never nil-compared keeps the plain `.Value` form. The set is reset each function.
//
// The RECEIVER of a direct-ж method joins the set under the same predicate: Go permits calling a
// method through a nil pointer when the body nil-checks first (`func (b *Buffer) String() string {
// if b == nil { return "<nil>" } … }`), and any method whose body `==`/`!=`-compares its bare
// receiver is direct-ж (the comparison arm of bodyUsesReceiverAsPointerValue promotes it), so its
// entry preamble `ref var b = ref Ꮡb.Value;` deref'd the box BEFORE the body's guard could run —
// an entry NRE where Go returns cleanly (bytes TestNil). Matched by OBJECT identity (a shadowing
// local comparison does not qualify) and gated on the direct-ж form (only it has a receiver box
// to deref); a receiver that is never compared keeps the plain `.Value` form, so emission is
// unchanged for every method that does not test its receiver.
func (v *Visitor) collectNilSafePtrParams(funcDecl *ast.FuncDecl) {
	if v.nilSafePtrParamNames == nil {
		v.nilSafePtrParamNames = HashSet[string]{}
	} else {
		v.nilSafePtrParamNames.Clear()
	}

	if funcDecl == nil {
		return
	}

	// A pointer parameter passed the untyped `nil` at some call site is legally nil at run time even
	// when this body never nil-COMPARES it — the deref sits behind an ordinary value guard (see
	// packageNilArgPtrParams / text/scanner's `digits(…, invalid *rune)` called `digits(ch, 10, nil)`).
	// The nil-arg positions were recorded package-wide in the pre-pass; map them to parameter names so
	// their entry deref alias takes the nil-safe accessor. Resolved by OBJECT identity via the func's
	// signature, so a name-only match cannot leak in.
	if funcObj, ok := v.info.Defs[funcDecl.Name].(*types.Func); ok {
		if indices := packageNilArgPtrParams[funcObj]; indices != nil {
			if sig, ok := funcObj.Type().(*types.Signature); ok {
				for i := 0; i < sig.Params().Len(); i++ {
					if indices.Contains(i) {
						if name := sig.Params().At(i).Name(); name != "" && name != "_" {
							v.nilSafePtrParamNames.Add(name)
						}
					}
				}
			}
		}
	}

	if funcDecl.Body == nil {
		return
	}

	ast.Inspect(funcDecl.Body, func(node ast.Node) bool {
		if n, ok := node.(*ast.BinaryExpr); ok && (n.Op == token.EQL || n.Op == token.NEQ) {
			for _, operand := range []ast.Expr{n.X, n.Y} {
				if ident, ok := operand.(*ast.Ident); ok && (v.isDerefdPointerParamIdent(ident) || v.isComparedDirectBoxReceiverIdent(ident)) {
					v.nilSafePtrParamNames.Add(ident.Name)
				}
			}
		}

		return true
	})
}

// isComparedDirectBoxReceiverIdent reports whether ident is the current method's pointer receiver
// (object identity — a shadowing local does not match) and the method is direct-ж, i.e. its
// receiver box `Ꮡrecv` is a parameter whose entry deref alias exists to be made nil-safe. Scoped
// to the `==`/`!=` operand scan of collectNilSafePtrParams — see the receiver paragraph there.
func (v *Visitor) isComparedDirectBoxReceiverIdent(ident *ast.Ident) bool {
	if ident == nil || ident.Name == "" || ident.Name == "_" {
		return false
	}

	isPtrRecv, recvName := v.isPointerReceiver()

	if !isPtrRecv || !v.identResolvesToReceiver(ident, recvName) {
		return false
	}

	return isDirectBoxReceiverMethod(v.currentFuncDecl, v.info)
}

// isDerefdPointerParamIdent reports whether ident resolves to a non-blank pointer (`*T`) PARAMETER
// — one that is emitted as a deref alias `ref var p = ref Ꮡp.Value` over its box `Ꮡp`. A pointer
// LOCAL (which already holds the box directly) and an unsafe.Pointer param are excluded. Used both
// to drive the box (`Ꮡp`) form in `==`/`!=` comparisons and to gate the nil-safe deref accessor.
func (v *Visitor) isDerefdPointerParamIdent(ident *ast.Ident) bool {
	if ident == nil || ident.Name == "" || ident.Name == "_" || !v.identIsParameter(ident) {
		return false
	}

	identType := v.getIdentType(ident)

	if identType == nil {
		return false
	}

	if _, isPtr := identType.Underlying().(*types.Pointer); isPtr {
		return true
	}

	// An ERASED pointer-core type parameter (`p P` under `[P *T]`) is a deref-aliased pointer
	// parameter too — its box drives `==`/`!=` comparisons and the nil-safe accessor gate the
	// same way a plain `*T` parameter's does.
	if typeParam, ok := types.Unalias(identType).(*types.TypeParam); ok {
		_, erased := v.typeParamErased(typeParam)
		return erased
	}

	return false
}

// bodyReferencesIdentAsValue reports whether name appears in the CONVERTED body text as a
// STANDALONE identifier — not as a substring of a larger identifier and, decisively, not as the
// suffix of its own box form `Ꮡname` (the address marker Ꮡ is a Unicode LETTER, so a preceding
// letter excludes the box occurrence). Used to decide whether a pointer parameter's deref VALUE
// alias (`ref var name = ref Ꮡparam.Value`) is live: every box/`unsafe.Pointer`/`== nil`/pass-as-
// pointer use renders through `Ꮡparam`, so a param touched only through its box leaves name absent
// and its alias is a dead local. A real value use always emits name as an identifier, so it always
// matches — the boundary test only ever ADDS spurious matches (a field selector `x.name`, a string,
// a comment), which keep the alias, so a genuinely live alias is never dropped.
func bodyReferencesIdentAsValue(bodyText, name string) bool {
	if name == "" {
		return false
	}

	for offset := 0; ; {
		index := strings.Index(bodyText[offset:], name)

		if index < 0 {
			return false
		}

		start := offset + index
		end := start + len(name)

		beforeOK := start == 0

		if !beforeOK {
			r, _ := utf8.DecodeLastRuneInString(bodyText[:start])
			beforeOK = !isIdentifierRune(r)
		}

		afterOK := end == len(bodyText)

		if !afterOK {
			r, _ := utf8.DecodeRuneInString(bodyText[end:])
			afterOK = !isIdentifierRune(r)
		}

		if beforeOK && afterOK {
			return true
		}

		offset = start + 1
	}
}

// isIdentifierRune reports whether r can appear within a C# identifier — a Unicode letter (which
// includes the go2cs marker glyphs Ꮡ/ж/Δ/ᴛ), a Unicode digit, or underscore.
func isIdentifierRune(r rune) bool {
	return r == '_' || unicode.IsLetter(r) || unicode.IsDigit(r)
}

// isDerefdPointerReceiverIdent reports whether ident is the current method's POINTER (`*T`)
// RECEIVER — which, like a deref'd pointer parameter, is emitted as a value alias
// `ref var r = ref Ꮡr.Value` over its box `Ꮡr`. Go's `r == nil` on such a receiver is a POINTER
// comparison (`func (f *File) checkValid() { if f == nil … }`), so it must compare the box
// `Ꮡr == nil`, not the deref'd struct value `r == nil` (which binds the generated
// `T.operator==(T, NilType)` — a null-embed-box NRE for a promoted-embed struct). The receiver is
// deliberately NOT a "parameter" in identIsParameter's model (paramNames excludes Recv), so it needs
// its own recognizer; scoped to the `==`/`!=` operand handling in convBinaryExpr (unlike a pointer
// PARAMETER it is not folded into nilSafePtrParamNames, so the receiver's deref-alias form is
// unchanged — only the comparison switches to the box). Object identity via identResolvesToReceiver,
// so a local shadowing the receiver name keeps its own render.
func (v *Visitor) isDerefdPointerReceiverIdent(ident *ast.Ident) bool {
	if ident == nil || ident.Name == "" || ident.Name == "_" {
		return false
	}

	isPtrRecv, recvName := v.isPointerReceiver()

	return isPtrRecv && v.identResolvesToReceiver(ident, recvName)
}

func getParameters(signature *types.Signature, addRecv bool) *types.Tuple {
	var parameters *types.Tuple

	if addRecv && signature.Recv() != nil {
		// Concatenate receiver parameter with the rest of the parameters
		parameterVars := make([]*types.Var, 0, 1+signature.Params().Len())
		parameterVars = append(parameterVars, signature.Recv())

		for i := 0; i < signature.Params().Len(); i++ {
			parameterVars = append(parameterVars, signature.Params().At(i))
		}

		parameters = types.NewTuple(parameterVars...)
	} else {
		parameters = signature.Params()
	}

	return parameters
}

func (v *Visitor) generateParametersSignature(signature *types.Signature, addRecv bool) (string, string) {
	parameters := getParameters(signature, addRecv)

	if parameters == nil {
		return "", ""
	}

	result := strings.Builder{}
	var receiverAccess string
	dupBlankParams := hasDuplicateBlankParams(parameters) || bodyUsesBlankDiscard(v.currentFuncDecl)

	for i := 0; i < parameters.Len(); i++ {
		param := parameters.At(i)

		if i == 0 && addRecv && signature.Recv() != nil {
			result.WriteString("this ")

			// Get receiver parameter type
			recvTypeName := v.getRefParamTypeName(param.Type())

			// Update function access to match receiver type. A PUBLICIZED unexported receiver
			// type is emitted `public`, so it does not restrict the method's access.
			receiverAccess = getAccess(recvTypeName)

			if receiverAccess != "public" && receiverTypeIsPublicized(param.Type()) {
				receiverAccess = "public"
			}

			result.WriteString(v.getRefParamTypeName(param.Type()))
			result.WriteRune(' ')

			paramName := param.Name()

			if paramName == "" || paramName == "_" {
				if dupBlankParams {
					paramName = fmt.Sprintf("_Δp%d", i)
				} else {
					paramName = "_"
				}
			}

			result.WriteString(getSanitizedIdentifier(paramName))
			continue
		}

		if i > 0 {
			result.WriteString(", ")
		}

		if i == parameters.Len()-1 && signature.Variadic() {
			result.WriteString("params ")

			// If parameter is a slice, convert it to a Span
			if sliceType, ok := param.Type().(*types.Slice); ok {
				result.WriteString(v.variadicParamType(sliceType.Elem()))
			} else {
				result.WriteString("object[]")
			}

			// Variadic parameters are passed as C# param arrays, so we use a temporary
			// parameter name that will be later converted to a Go slice<T>
			result.WriteRune(' ')
			result.WriteString(getVariadicParamName(param))
		} else {
			paramTypeName := v.getCSTypeName(param.Type())

			// A FUNC-LITERAL parameter typed as a `string | []byte`-union TYPE PARAMETER
			// renders as the type parameter itself (`(T part) => ...`): the enclosing
			// method's type parameter is in scope inside a lambda, and every union-typed
			// argument renders T-typed too (sub-slices cast back to the type param in
			// convSliceExpr), so the inferred delegate binds exactly and the emission
			// matches the Go. (A historical IByteSeq<byte> widening here worked around
			// the interface-typed sub-slice arguments - obsolete once the sub-slice cast
			// landed, and it hid the type identity: user-flagged.)
			result.WriteString(paramTypeName)
			result.WriteRune(' ')

			paramName := param.Name()

			// Keep a lone `_`, but synthesize a unique placeholder when blanks would collide
			// (Go allows repeated blank params; C# forbids duplicate parameter names — CS0100).
			if paramName == "" || paramName == "_" {
				if dupBlankParams {
					paramName = fmt.Sprintf("_Δp%d", i)
				} else {
					paramName = "_"
				}
			} else if adjusted, ok := v.varNames[param]; ok && adjusted != "" {
				// A parameter whose name was shadow-renamed by the variable analysis — because it
				// shadows an imported package, a called builtin, a function, or an outer-scope var —
				// must emit the RENAMED name so it matches every usage, which convIdent renders from
				// v.varNames. crypto/rsa's `func emsaPSSEncode(…, hash hash.Hash)` param `hash` shadows
				// the `hash` package (`using hash = hash_package;`); the declaration kept the raw
				// `hash` while its uses rendered `hashΔ1`, so every use was CS0103 (40 sites in
				// crypto/rsa, 27 in testing/quick's `rand`). Mirrors iifeParamName's lookup.
				paramName = adjusted
			}

			// A heap-boxed value parameter arrives under the `ʗp` name; the parameter preamble
			// re-declares the analyzed name as the boxed ref alias (see paramNeedsHeapBox). This
			// path serves a function with NO pointer params (otherwise the rebuilt-signature
			// path above applies the same rename). A FUNCTION LITERAL's boxed params flow in
			// through the transient name set instead — its signature is generated from
			// SYNTHESIZED vars (see getSignature) that never match the identEscapesHeap entries.
			if v.paramNeedsHeapBox(param) || v.funcLitHeapBoxParamNames.Contains(param.Name()) {
				result.WriteString(getHeapBoxParamName(param))
			} else {
				result.WriteString(getSanitizedIdentifier(paramName))
			}
		}
	}

	return result.String(), receiverAccess
}

func (v *Visitor) generateResultSignature(signature *types.Signature) string {
	results := signature.Results()

	if results == nil {
		return "void"
	}

	result := strings.Builder{}

	if results.Len() == 1 {
		param := results.At(0)

		result.WriteString(v.getCSTypeName(param.Type()))

		if param.Name() != "" {
			result.WriteString(" /*")
			result.WriteString(param.Name())
			result.WriteString("*/")
		}

		return result.String()
	}

	result.WriteRune('(')

	for i := 0; i < results.Len(); i++ {
		if i > 0 {
			result.WriteString(", ")
		}

		param := results.At(i)

		result.WriteString(v.getCSTypeName(param.Type()))

		// A BLANK Go result name (`func match(x, y Value) (_, _ Value)`, go/constant) must NOT
		// become a C# tuple element name — two `_` elements collide (CS8127). Emit the type
		// only; C# permits a mixed named/unnamed tuple, so real names are still kept.
		if param.Name() != "" && param.Name() != "_" {
			result.WriteRune(' ')
			result.WriteString(getSanitizedIdentifier(param.Name()))
		}
	}

	result.WriteRune(')')

	return result.String()
}

func getVariadicParamName(param *types.Var) string {
	return fmt.Sprintf("%s%sp", getSanitizedIdentifier(param.Name()), CapturedVarMarker)
}

// getHeapBoxParamName returns the incoming-parameter name for a heap-boxed value parameter
// (see paramNeedsHeapBox) — the same `ʗp` rename convention as a variadic parameter: both
// re-declare the Go parameter name in the function prologue. A parameter is never both
// (a variadic parameter's unnamed []T type carries no capture-mode methods).
func getHeapBoxParamName(param *types.Var) string {
	return getVariadicParamName(param)
}

// getHeapBoxLitParamName renders the incoming `ʗp` parameter name for a heap-boxed value
// parameter of a FUNCTION LITERAL from its rendered body name (getHeapBoxParamName serves
// declaration parameters via their *types.Var; a literal's signature is generated from
// SYNTHESIZED vars — see getSignature — that already carry the rendered name).
func getHeapBoxLitParamName(renderedName string) string {
	return fmt.Sprintf("%s%sp", getSanitizedIdentifier(renderedName), CapturedVarMarker)
}

// paramNeedsHeapBox reports whether the value parameter needs an entry-time heap box: the
// body calls a capture-mode (direct-ж) method on it, whose only emitted receiver form is the
// box `ж<T>` — go/format's `cfg printer.Config` + `cfg.Fprint(…)`, CS1929 ×2 without it.
// The signature takes the incoming value under the `ʗp` name and the parameter preamble
// declares `ref var cfg = ref heap(cfgʗp, out var Ꮡcfg);` — ENTRY-TIME boxing, never a
// call-site Ꮡ(value) copy-box: the copy form compiles but silently drops the callee's writes
// through the receiver pointer for the rest of the body (Go auto-addresses the same storage).
// The identHasHeapBox gate keeps the box decision in lockstep with the isHeapBoxedExpr
// routing in convSelectorExpr, but is NOT sufficient by itself: a parameter can land in
// identEscapesHeap outside markCaptureModeBoxedParams — a mixed `data, pc, line := …` define
// re-uses the param object, so the define walker escape-analyzes it (debug/gosym's slice,
// whose `pc` is stored into a composite literal). Those params keep their historical unboxed
// emission; the box fires only for the capture-mode trigger, re-verified here against the
// declaring ident, or for a param the capture analysis routed to SHARED storage: one WRITTEN
// after a closure captured it (see processPotentialCapture's varShareFacts arm) is referenced
// through its box inside every capturing lambda (`Ꮡctx.ValueSlot`), so the prologue must
// declare that box — database/sql beginDC's `ctx` (redeclared by a body-top-level
// `ctx, cancel := …` after withLock's closure captured it) and go/types nify's `x, y`
// (swapped after the trace defer captured them) rendered a box that was never declared
// (CS0103). The box-ref check rides the declaring-ident lookups below so a box-ref'd value
// RECEIVER (never `ʗp`-renamed by the signature paths) can never take the param form.
func (v *Visitor) paramNeedsHeapBox(param *types.Var) bool {
	if param == nil || param.Name() == "" || param.Name() == "_" {
		return false
	}

	if _, isPointer := param.Type().(*types.Pointer); isPointer {
		return false
	}

	if !v.identHasHeapBox(param, param.Type()) {
		return false
	}

	funcDecl := v.currentFuncDecl

	if funcDecl == nil || funcDecl.Body == nil || funcDecl.Type.Params == nil {
		return false
	}

	for _, field := range funcDecl.Type.Params.List {
		for _, ident := range field.Names {
			if v.info.ObjectOf(ident) == param {
				return v.bodyCallsCaptureModeMethodOn(ident, funcDecl.Body) || v.isLambdaBoxRefVar(param)
			}
		}
	}

	// A FUNCTION LITERAL's own value parameter (whose prologue/rename convFuncLit emits — see
	// funcLitHeapBoxParamIdents): the analysis phase reaches here when a NESTED closure inside
	// the literal references the param, and the box-ref arm of processPotentialCapture must see
	// the same verdict emission uses. Find the declaring literal within the current declaration
	// and re-verify against ITS body.
	needsBox := false

	ast.Inspect(funcDecl.Body, func(n ast.Node) bool {
		funcLit, ok := n.(*ast.FuncLit)

		if !ok || funcLit.Type.Params == nil {
			return true
		}

		for _, field := range funcLit.Type.Params.List {
			if _, isVariadic := field.Type.(*ast.Ellipsis); isVariadic {
				continue
			}

			for _, ident := range field.Names {
				if v.info.ObjectOf(ident) == param {
					needsBox = v.bodyCallsCaptureModeMethodOn(ident, funcLit.Body) || v.isLambdaBoxRefVar(param)
					return false
				}
			}
		}

		return true
	})

	return needsBox
}

func (v *Visitor) getTempVarName(varPrefix string) string {
	if v.tempVarCount == nil {
		v.tempVarCount = make(map[string]int)
	}

	count := v.tempVarCount[varPrefix]
	count++
	v.tempVarCount[varPrefix] = count

	return fmt.Sprintf("%s%s%d", varPrefix, TempVarMarker, count)
}

// linknameForwardTargets is the whitelist of cross-package //go:linkname PULL targets the converter
// emits a forwarder body for — the specific NATIVE functions hand-implemented in the converted
// standard library (syscall's Windows DLL loaders, reached by golang.org/x/sys/windows's LazyDLL /
// LazyProc). A linkname target is INDISTINGUISHABLE at conversion time from any other bodyless
// assembly/intrinsic Go function — syscall.loadlibrary and runtime.reflectcall are both bodyless asm
// in Go — so forwarding is gated on this explicit set: only these have a real C# implementation to
// call. Every other linkname pull (a method-receiver PUSH like reflect's badlinkname.go, a
// same-package pull, or an unimplemented intrinsic like runtime.reflectcall) stays a bodyless stub,
// the pre-forwarder behavior. Extend this set when a new native linkname target gains a hand-written
// C# implementation.
var linknameForwardTargets = map[string]bool{
	"syscall.loadlibrary":       true,
	"syscall.loadsystemlibrary": true,
	"syscall.getprocaddress":    true,
}

// linknameForwardBuiltins is the whitelist of cross-package //go:linkname PULL targets whose
// implementation is a golib BUILTIN — a compiler intrinsic Go defines in the runtime and links
// into another package by symbol, for which golib carries the real C# implementation. The map
// value is the golib builtin's C# name; it is in scope UNQUALIFIED via `using static go.builtin`,
// so the forwarder emits a bare `<builtin>(args)` call (an empty package alias signals this). The
// canonical case is maps.clone — Go implements it as runtime.mapclone (`//go:linkname mapclone
// maps.clone`) and the maps package pulls it as a bodyless `func clone(m any) any`; golib's
// builtin.mapclone returns a shallow, independent clone of the boxed map. Extend this set when a
// new linkname intrinsic gains a golib builtin implementation.
var linknameForwardBuiltins = map[string]string{
	"maps.clone": "mapclone",
}

// funcLinknameForward recognizes a bodyless function carrying a `//go:linkname <thisFunc>
// <pkgpath>.<targetFunc>` directive whose target is a hand-implemented native function
// (linknameForwardTargets). It returns the C# alias for the target package (the last path segment,
// the `using <name> = <name>_package;` alias the importing file emits) and the target function name,
// so the converter can emit a forwarder call to it instead of a throwing stub.
func (v *Visitor) funcLinknameForward(funcDecl *ast.FuncDecl) (alias string, targetFunc string, ok bool) {
	if funcDecl.Doc == nil || funcDecl.Name == nil {
		return "", "", false
	}

	for _, comment := range funcDecl.Doc.List {
		fields := strings.Fields(comment.Text)

		// //go:linkname <local> <pkgpath>.<func>
		if len(fields) != 3 || fields[0] != "//go:linkname" || fields[1] != funcDecl.Name.Name {
			continue
		}

		target := fields[2]

		// A linkname target implemented as a golib BUILTIN — in scope unqualified via
		// `using static go.builtin`, so the forwarder emits a bare `<builtin>(args)` call. The
		// empty alias is the sentinel writeLinknameForwarder reads to omit the package qualifier
		// (maps.clone → mapclone, Go's runtime.mapclone shallow-clone intrinsic).
		if builtin, isBuiltin := linknameForwardBuiltins[target]; isBuiltin {
			return "", builtin, true
		}

		if !linknameForwardTargets[target] {
			return "", "", false
		}

		dot := strings.LastIndex(target, ".")
		pkgPath := target[:dot]
		targetFunc = getSanitizedFunctionName(target[dot+1:])

		// The C# using-alias is the package's simple name — the last path segment (`syscall`).
		if slash := strings.LastIndex(pkgPath, "/"); slash >= 0 {
			pkgPath = pkgPath[slash+1:]
		}

		return getSanitizedIdentifier(pkgPath), targetFunc, true
	}

	return "", "", false
}

// linknameForwardArgName returns the C# identifier a parameter was emitted under in the forwarder's
// signature, so the forwarder can pass it through to the target. It mirrors the naming decisions of
// generateParametersSignature (blank/duplicate-blank synthesis, variable-analysis shadow renames,
// heap-box aliasing, variadic naming) so the argument matches the declared parameter exactly.
func (v *Visitor) linknameForwardArgName(param *types.Var, i int, dupBlank bool, variadic bool) string {
	if variadic {
		return getVariadicParamName(param)
	}

	paramName := param.Name()

	if paramName == "" || paramName == "_" {
		if dupBlank {
			return fmt.Sprintf("_%sp%d", ShadowVarMarker, i)
		}

		return "_"
	}

	if adjusted, ok := v.varNames[param]; ok && adjusted != "" {
		paramName = adjusted
	}

	if v.paramNeedsHeapBox(param) || v.funcLitHeapBoxParamNames.Contains(param.Name()) {
		return getHeapBoxParamName(param)
	}

	return getSanitizedIdentifier(paramName)
}

// writeLinknameForwarder emits the body of a cross-package //go:linkname forwarder: a call to
// `<alias>.<targetFunc>(args)` with the local parameters passed through and the results returned.
// Because the target and local signatures are linkname-compatible (structurally identical Go
// types), any nominal C# type difference is between `num:uintptr`-kind types on both sides, so a
// mismatch is bridged through `uintptr` — an integer/uintptr parameter is passed `(uintptr)p`, and
// an integer/uintptr result is returned `(LocalType)(uintptr)r`. Non-integer params/results
// (pointers, slices, strings) are the same golib type on both sides and pass through directly.
func (v *Visitor) writeLinknameForwarder(signature *types.Signature, alias string, targetFunc string) {
	params := signature.Params()
	dupBlank := hasDuplicateBlankParams(params) || bodyUsesBlankDiscard(v.currentFuncDecl)
	args := make([]string, params.Len())

	for i := 0; i < params.Len(); i++ {
		param := params.At(i)
		name := v.linknameForwardArgName(param, i, dupBlank, signature.Variadic() && i == params.Len()-1)

		if v.isUintptrBridgeable(param.Type()) {
			name = "(uintptr)" + name
		}

		args[i] = name
	}

	// An empty alias signals a golib-builtin target (in scope unqualified via `using static
	// go.builtin`); any other alias is the target package's using-alias (`syscall.loadlibrary`).
	call := fmt.Sprintf("%s(%s)", targetFunc, strings.Join(args, ", "))

	if alias != "" {
		call = fmt.Sprintf("%s.%s(%s)", alias, targetFunc, strings.Join(args, ", "))
	}

	results := signature.Results()

	savedIndent := v.indentLevel
	v.indentLevel = 0

	bodyIndent := v.indent(savedIndent + 1)
	closeIndent := v.indent(savedIndent)

	var body strings.Builder
	body.WriteString(" {")
	body.WriteString(v.newline)

	switch results.Len() {
	case 0:
		body.WriteString(fmt.Sprintf("%s%s;", bodyIndent, call))
	case 1:
		body.WriteString(fmt.Sprintf("%sreturn %s;", bodyIndent, v.bridgeLinknameResult(call, results.At(0).Type())))
	default:
		names := make([]string, results.Len())
		bridged := make([]string, results.Len())

		for i := 0; i < results.Len(); i++ {
			names[i] = fmt.Sprintf("%s%d", TempVarMarker, i+1)
			bridged[i] = v.bridgeLinknameResult(names[i], results.At(i).Type())
		}

		body.WriteString(fmt.Sprintf("%svar (%s) = %s;", bodyIndent, strings.Join(names, ", "), call))
		body.WriteString(v.newline)
		body.WriteString(fmt.Sprintf("%sreturn (%s);", bodyIndent, strings.Join(bridged, ", ")))
	}

	body.WriteString(v.newline)
	body.WriteString(closeIndent)
	body.WriteString("}")

	v.writeOutputLn(body.String())
	v.indentLevel = savedIndent
}

// bridgeLinknameResult wraps a target-call result expression so it matches the local result type:
// an integer/uintptr result is cast through `uintptr` (`(LocalType)(uintptr)expr`), covering the
// nominal difference between two `num:uintptr` types across the linkname; any other type is the
// same golib type on both sides and passes through unchanged.
func (v *Visitor) bridgeLinknameResult(expr string, localType types.Type) string {
	if v.isUintptrBridgeable(localType) {
		return fmt.Sprintf("(%s)(uintptr)%s", v.getCSTypeName(localType), expr)
	}

	return expr
}

// isUintptrBridgeable reports whether t is an integer/uintptr-kind type — one that converts to and
// from `uintptr`, so a linkname forwarder can bridge a nominal mismatch between two such types.
func (v *Visitor) isUintptrBridgeable(t types.Type) bool {
	if t == nil {
		return false
	}

	basic, ok := t.Underlying().(*types.Basic)

	return ok && basic.Info()&types.IsInteger != 0
}
