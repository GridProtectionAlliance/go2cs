package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"strings"
)

// numericBasicLit returns the (optionally SUB-negated) INT or FLOAT basic literal behind
// expr, if any — the numeric cousin of isStringBasicLit, serving the multi-result func-lit
// inference scan (an untyped numeric constant element emits bare, so its arm infers the
// literal's natural C# type instead of the declared result element's).
func numericBasicLit(expr ast.Expr) (*ast.BasicLit, bool) {
	if unary, ok := expr.(*ast.UnaryExpr); ok && unary.Op == token.SUB {
		expr = unary.X
	}

	lit, ok := expr.(*ast.BasicLit)

	if !ok || (lit.Kind != token.INT && lit.Kind != token.FLOAT) {
		return nil, false
	}

	return lit, true
}

// funcLitReturnsUntypedNamedConst reports whether any of the literal's OWN top-level return arms
// returns a bare reference to a named untyped numeric constant — OR a constant operator expression
// containing one that no literal fold rescues. Both shapes emit with a golib `Untyped*` wrapper
// (see isUntypedNamedConstRef) and defeat C# lambda return-type inference in natural-inference
// position (see the single-result numeric arm in convFuncLit): bytes TestMap's `invalidRune :=
// func(r rune) rune { return utf8.MaxRune + 1 }` renders the arm `utf8.MaxRune + 1`, whose C#
// operator result keeps the wrapper type, so the inferred delegate was `Func<int, UntypedInt>`
// (CS1503 at the invariant Map call) exactly like the bare-reference case. An arm the constant
// folds rewrite to a plain literal (overflowingConstLiteral / floatContextConstLiteral) emits
// concretely typed and is excluded, as are literal-only arms (`return 'a'`, `return -1`).
func (v *Visitor) funcLitReturnsUntypedNamedConst(funcLit *ast.FuncLit) bool {
	found := false

	ast.Inspect(funcLit.Body, func(n ast.Node) bool {
		if found {
			return false
		}

		if _, isLit := n.(*ast.FuncLit); isLit && n != funcLit.Body {
			return false // a nested literal's returns belong to it
		}

		if ret, ok := n.(*ast.ReturnStmt); ok && len(ret.Results) == 1 && v.returnArmKeepsUntypedWrapper(ret.Results[0]) {
			found = true
			return false
		}

		return true
	})

	return found
}

// returnArmKeepsUntypedWrapper reports whether a single-result return arm's emission keeps a golib
// `Untyped*` wrapper type: a bare named untyped-const reference, or a CONSTANT paren/unary/binary
// operator expression containing one — unless a constant fold (overflowingConstLiteral /
// floatContextConstLiteral) rewrites the whole arm to a plain literal, which emits concretely typed.
func (v *Visitor) returnArmKeepsUntypedWrapper(expr ast.Expr) bool {
	if v.isUntypedNamedConstRef(expr) {
		return true
	}

	switch expr.(type) {
	case *ast.ParenExpr, *ast.UnaryExpr, *ast.BinaryExpr:
	default:
		return false
	}

	tv, ok := v.info.Types[expr]

	if !ok || tv.Value == nil {
		return false
	}

	if v.overflowingConstLiteral(expr) != "" || v.floatContextConstLiteral(expr) != "" {
		return false
	}

	return v.constExprContainsUntypedNamedConstRef(expr)
}

// constExprContainsUntypedNamedConstRef reports whether a named untyped numeric constant reference
// appears as a leaf of the paren/unary/binary operator tree — the operand shape that renders as a
// golib `Untyped*` wrapper and makes the whole operator result wrapper-typed.
func (v *Visitor) constExprContainsUntypedNamedConstRef(expr ast.Expr) bool {
	switch e := expr.(type) {
	case *ast.ParenExpr:
		return v.constExprContainsUntypedNamedConstRef(e.X)
	case *ast.UnaryExpr:
		return v.constExprContainsUntypedNamedConstRef(e.X)
	case *ast.BinaryExpr:
		return v.constExprContainsUntypedNamedConstRef(e.X) || v.constExprContainsUntypedNamedConstRef(e.Y)
	default:
		return v.isUntypedNamedConstRef(expr)
	}
}

func (v *Visitor) convFuncLit(funcLit *ast.FuncLit, context LambdaContext) string {
	if v.currentFuncSignature == nil {
		v.currentFuncSignature = v.info.Types[funcLit].Type.(*types.Signature)
	}

	litSig, _ := v.info.TypeOf(funcLit).(*types.Signature)

	// visitReturnStmt derives a bare `return`'s emitted RESULTS from the return signature. A nested
	// literal must return against ITS OWN results, not the enclosing function's — a bare `return` inside
	// a VOID closure otherwise gets the OUTER function's named results (`forEachGRace(func(gp1 *g) { …
	// return … })` inside a func returning named `(n, ok)` emitted `return (n, ok);` → CS8030). This is a
	// SEPARATE field from currentFuncSignature, which must stay the ENCLOSING function's signature so the
	// receiver/parameter detection (isBoxedPointerLocal, varIsDerefdPointerParam) still resolves a
	// CAPTURED pointer param (an outer parameter) correctly. Save/restore around the body.
	savedReturnSignature := v.currentReturnSignature

	if litSig != nil {
		v.currentReturnSignature = litSig
	}

	// Does THIS literal need its own func() execution context, and additionally the
	// named-return-defer handling (named results that deferred code, including recover, mutates)?
	// A deferred-call target is excluded — its defer/recover belong to the enclosing function.
	var litHasDefer, litHasRecover, litNamedDefer bool
	var litNamedNames []string

	if litSig != nil && !context.deferCall {
		litHasDefer, litHasRecover = v.funcBodyDeferRecover(funcLit.Body)
		litNamedDefer, litNamedNames = v.detectNamedReturnDefer(litSig, litHasDefer, litHasRecover)
	}

	// A function literal with NAMED results needs their declarations at the top of its block —
	// Go zero-initializes them and a bare `return` returns them. iter.Pull's
	// `next = func() (v1 V, ok1 bool) { …; return }` emitted `return (v1, ok1);` with nothing
	// declared (CS0103 — the wave-1 iter errors). The litNamedDefer path composes its own decls;
	// this covers plain and defer-without-mutation literals.
	litHasNamedResults := false

	if litSig != nil && funcLit.Type.Results != nil {
		for _, field := range funcLit.Type.Results.List {
			for _, name := range field.Names {
				if !isDiscardedVar(name.Name) {
					litHasNamedResults = true
				}
			}
		}
	}

	// A function literal's body is converted with ITS OWN namedReturnDeferMode, not the enclosing
	// function's (which must not leak in — otherwise the closure's `return expr` would be rewritten
	// against the OUTER named results). Save/restore around the body conversion.
	savedNamedReturnDeferMode := v.namedReturnDeferMode
	savedNamedReturnNames := v.namedReturnNames
	v.namedReturnDeferMode = litNamedDefer
	v.namedReturnNames = litNamedNames

	// A func literal BODY is function scope even when the literal sits in a PACKAGE-LEVEL var
	// initializer (`var Support = sync.OnceValue(func() bool { var size uint32; … })` —
	// internal/syscall/windows): visitFuncDecl never ran, so inFunction was false and the
	// literal's locals emitted as package fields (`internal static uint32 size;` inside the
	// lambda — a CS1002 syntax cascade gating os/fmt). Save/restore around the body.
	savedInFunction := v.inFunction
	v.inFunction = true

	defer func() {
		v.namedReturnDeferMode = savedNamedReturnDeferMode
		v.namedReturnNames = savedNamedReturnNames
		v.currentReturnSignature = savedReturnSignature
		v.inFunction = savedInFunction
	}()

	if v.lambdaCapture == nil {
		v.lambdaCapture = newLambdaCapture()
		v.capturedVarCount = make(map[string]int)
	}

	v.enterLambdaConversion(funcLit)
	defer v.exitLambdaConversion()

	// Create a map of parameters to avoid capturing them
	paramNames := make(map[string]bool)

	if funcLit.Type.Params != nil {
		for _, field := range funcLit.Type.Params.List {
			for _, name := range field.Names {
				paramNames[name.Name] = true
			}
		}
	}

	// A NAMED RESULT is declared in the function literal's OWN scope — Go zero-initializes it and a bare
	// `return` returns it (litHasNamedResults below emits its declaration inside the lambda), so a
	// reference to it in the body is the result, NEVER an outer-scope capture. text/template's readFileFS
	// returns `func(file string) (name string, b []byte, err error)`; `b` was mis-hoisted as a capture
	// (`var bʗ1 = b;` — CS0103, `b` is undefined in the enclosing func). Exclude named results from the
	// capture set exactly as parameters are.
	if funcLit.Type.Results != nil {
		for _, field := range funcLit.Type.Results.List {
			for _, name := range field.Names {
				paramNames[name.Name] = true
			}
		}
	}

	// Filter out any captures that are actually parameters
	if captures, exists := v.lambdaCapture.stmtCaptures[funcLit]; exists {
		for ident := range captures {
			if paramNames[ident.Name] {
				delete(captures, ident)
			}
		}

		// If no captures remain, remove the empty map
		if len(captures) == 0 {
			delete(v.lambdaCapture.stmtCaptures, funcLit)
		}
	}

	v.prepareStmtCaptures(funcLit)

	result := strings.Builder{}

	if decls := v.generateCaptureDeclarations(); decls != "" {
		switch {
		case context.deferredDecls != nil:
			// go/defer/return thread an explicit builder for their own hoisting.
			context.deferredDecls.WriteString(strings.TrimRight(decls, " "))
		case v.hoistedDecls != nil:
			// The enclosing statement (assignment RHS, composite-literal element, call argument)
			// hoists these decls to a valid position before the statement.
			v.hoistedDecls.WriteString(strings.TrimRight(decls, " "))
		default:
			result.WriteString(decls)
		}
	}

	// The literal's own VALUE parameters on which its body calls a capture-mode (direct-ж)
	// method need the same ENTRY-TIME heap box as a declaration's parameters (see
	// paramNeedsHeapBox): the signature takes the incoming value under the `ʗp` name and the
	// prologue injected below re-declares the Go name as the boxed ref alias.
	boxedParamIdents := v.funcLitHeapBoxParamIdents(funcLit)
	var boxedParamNames HashSet[string]

	if len(boxedParamIdents) > 0 {
		boxedParamNames = HashSet[string]{}

		for _, ident := range boxedParamIdents {
			boxedParamNames.Add(v.getIdentName(ident))
		}
	}

	var parameterSignature string

	// For C#, lambda return type is inferred and not explicitly declared. The transient
	// boxed-param name set is scoped to exactly this call: the literal's signature is
	// generated from SYNTHESIZED vars (see getSignature) that carry the rendered names but
	// never match identEscapesHeap, so generateParametersSignature reads the set to emit a
	// boxed param under its incoming `ʗp` name. Cleared before the body conversion so a
	// NESTED literal's signature cannot inherit it.
	v.funcLitHeapBoxParamNames = boxedParamNames
	_, parameterSignature = v.convFuncType(funcLit.Type)
	v.funcLitHeapBoxParamNames = nil

	blockStatementContext := DefaultBlockStmtContext()
	blockStatementContext.format.useNewLine = false

	// In namedReturnDefer mode the literal's body sits inside an extra block + func() wrapper, so
	// indent it one level deeper.
	if litNamedDefer {
		v.indentLevel++
	}

	v.pushBlock()
	v.visitBlockStmt(funcLit.Body, blockStatementContext)
	body := v.popBlockAppend(false)

	if litNamedDefer {
		v.indentLevel--
	}

	// A boxed VALUE parameter (see funcLitHeapBoxParamIdents) arrives under the `ʗp` name; the
	// literal's first statements re-declare the Go name as the boxed ref alias — the exact
	// parameter-preamble form of a declaration's boxed param (see paramNeedsHeapBox) — so body
	// uses hit the boxed storage and convSelectorExpr routes the capture-mode call through
	// `Ꮡ<name>` (CS1929 on the raw value without it; a call-site Ꮡ(value) copy-box would
	// compile but silently drop the callee's writes). Injected BEFORE the return-collapse
	// below, which also keeps the body a block. Unlike the variadic prologue, an IIFE is NOT
	// excluded: iifeParamName emits the `ʗp` name for a boxed param, so the rebinding is
	// required there too. When both prologues apply, the variadic injection below prepends its
	// line above these, matching visitFuncDecl's preamble order.
	if len(boxedParamIdents) > 0 {
		trimmedBody := strings.TrimSpace(body)

		if strings.HasPrefix(trimmedBody, "{") {
			prologue := strings.Builder{}

			for _, ident := range boxedParamIdents {
				renderedName := v.getIdentName(ident)
				incomingName := getHeapBoxLitParamName(renderedName)

				// An ARRAY param (direct, aliased, or named — see visitFuncDecl's parameter
				// preamble) folds its Go by-value clone into the box init.
				if typeIsArrayValue(v.getIdentType(ident)) {
					incomingName += ".Clone()"
				}

				if v.options.preferVarDecl {
					prologue.WriteString(fmt.Sprintf("%s%sref var %s = ref heap(%s, out var %s%s);", v.newline, v.indent(v.indentLevel+1), getSanitizedIdentifier(renderedName), incomingName, AddressPrefix, renderedName))
				} else {
					csTypeName := v.getCSTypeName(v.getIdentType(ident))
					prologue.WriteString(fmt.Sprintf("%s%sref %s %s = ref heap(%s, out %s<%s> %s%s);", v.newline, v.indent(v.indentLevel+1), csTypeName, getSanitizedIdentifier(renderedName), incomingName, PointerPrefix, csTypeName, AddressPrefix, renderedName))
				}
			}

			body = "{" + prologue.String() + strings.TrimPrefix(trimmedBody, "{")
		}
	}

	// A plain (non-boxed) ARRAY-typed literal parameter is a per-call COPY in Go; mirror
	// visitFuncDecl's parameter-preamble clone (`a = a.Clone();`) so writes through the
	// parameter cannot reach the caller's backing store. A heap-boxed array param already
	// folds its clone into the box init above.
	if funcLit.Type.Params != nil {
		prologue := strings.Builder{}

		for _, field := range funcLit.Type.Params.List {
			for _, name := range field.Names {
				if name.Name == "_" || boxedParamNames.Contains(v.getIdentName(name)) {
					continue
				}

				if !typeIsArrayValue(v.getIdentType(name)) {
					continue
				}

				renderedName := getSanitizedIdentifier(v.getIdentName(name))
				prologue.WriteString(fmt.Sprintf("%s%s%s = %s.Clone();", v.newline, v.indent(v.indentLevel+1), renderedName, renderedName))
			}
		}

		if prologue.Len() > 0 {
			trimmedBody := strings.TrimSpace(body)

			if strings.HasPrefix(trimmedBody, "{") {
				body = "{" + prologue.String() + strings.TrimPrefix(trimmedBody, "{")
			}
		}
	}

	// A variadic parameter arrives as a C# `params` array named `<name>ʗp` (see getVariadicParamName);
	// the Go body references the bare `<name>` as a slice<T>. A top-level func gets a
	// `var <name> = <name>ʗp.slice();` prologue (visitFuncDecl); a function LITERAL emitted no such
	// prologue, so a closure that references its variadic param was undefined (CS0103 — internal/dag's
	// `errorf := func(format string, a ...any) { … fmt.Sprintf(format, a...) }` spread bare `a`).
	// Inject the same prologue at the top of the literal's block. Doing it BEFORE the return-collapse
	// below also keeps the body a block (the param is a statement-scoped local), which is correct.
	if litSig != nil && litSig.Variadic() && litSig.Params().Len() > 0 && !context.isIIFE {
		// (An IIFE is excluded: it emits param NAMES only — the raw `a`, not `<name>ʗp` — via
		// iifeParamNames, with the delegate cast supplying the `params` type, so there is no
		// `<name>ʗp` array to .slice() and no rename to undo.)
		// body may still carry leading whitespace here (it is TrimSpace'd only in the collapse/else
		// arms below) — trim before probing for the opening brace.
		trimmedBody := strings.TrimSpace(body)

		if strings.HasPrefix(trimmedBody, "{") {
			param := litSig.Params().At(litSig.Params().Len() - 1)
			var prologue string

			if v.options.preferVarDecl {
				prologue = fmt.Sprintf("%s%svar %s = %s.slice();", v.newline, v.indent(v.indentLevel+1), getSanitizedIdentifier(param.Name()), getVariadicParamName(param))
			} else {
				prologue = fmt.Sprintf("%s%sslice<%s> %s = %s.slice();", v.newline, v.indent(v.indentLevel+1), v.getCSTypeName(param.Type().(*types.Slice).Elem()), getSanitizedIdentifier(param.Name()), getVariadicParamName(param))
			}

			body = "{" + prologue + strings.TrimPrefix(trimmedBody, "{")
		}
	}

	// An IIFE keeps a block body (it may need a func() wrapper and reads more like the Go
	// source); other single-return literals collapse to an expression-bodied lambda. A
	// namedReturnDefer literal always keeps a block (it returns its named results after the
	// func() wrapper).
	if v.firstStatementIsReturn && !context.isIIFE && !litNamedDefer && !litHasNamedResults {
		// Find return statement in string and remove it
		returnIndex := strings.Index(body, "return ")

		// The visited block can carry HOISTED statements ahead of the return —
		// visitReturnStmt's tuple-conversion arm writes `var (ᴛ1, ᴛ2) = call;` before
		// `return (ᴛ1, ᴛ2);` (net lookup.go's `DoChan(key, func() (any, error) {
		// return testHookLookupIP(…) })`). Chopping at "return " dropped the call and
		// left the bare markers (CS0103 ×2). Collapse only when nothing but the block's
		// opening brace precedes the return; otherwise keep the block body.
		if returnIndex != -1 {
			if prefix := strings.TrimSpace(body[:returnIndex]); prefix != "{" && prefix != "" {
				returnIndex = -1
				body = strings.TrimSpace(body)
			}
		}

		if returnIndex != -1 {
			body = body[returnIndex+7:]

			// Remove the BLOCK's closing brace — always the last non-whitespace rune of the
			// visited block; the statement's `;` always separates it from the expression. The
			// old TrimSuffix+LastIndex pair cut at the last `}` ANYWHERE, truncating a return
			// expression containing its own `}` — `return []Value{ValueOf(yield(in[0]))}`
			// emitted `new ΔValue[]{ValueOf(yield(@in[0]))` with `}.slice()` chopped
			// (reflect/iter.go MakeFunc literals, CS1513 x2).
			body = strings.TrimSpace(body)
			body = strings.TrimSuffix(body, "}")
			body = strings.TrimSpace(body)
			body = strings.TrimSuffix(body, ";")
		}
	} else {
		body = strings.TrimSpace(body)
	}

	// Declare plain named results at the top of the literal's block (the litNamedDefer arm
	// below declares its own, outside the func() wrapper).
	if litHasNamedResults && !litNamedDefer && strings.HasPrefix(body, "{") {
		body = "{" + v.namedReturnDeclLines(litSig, v.indentLevel+1, false) + strings.TrimPrefix(body, "{")
	}

	// Build the lambda body (what follows `=>`). A function literal that uses defer/recover gets
	// its own `func((defer, recover) => …)` execution context (so its deferred code runs and
	// recovers when invoked); when it also has named results that deferred code mutates, the
	// named results are declared outside that wrapper and returned after it. A deferred-call
	// target is the exception (its defer/recover belong to the already-wrapped enclosing function).
	var inner string

	switch {
	case litNamedDefer:
		// `{ T r = default!; func((defer, recover) => <body>); return r; }`
		// A heap-box-backed named result declares only its box out here (the wrapper lambda
		// cannot capture a ref local — CS8175); the wrapper re-aliases the value name inside,
		// and the trailing return reads through the box (`Ꮡe.ValueSlot`).
		returnNames := v.namedReturnBoxReadNames(litSig, litNamedNames)
		returnExpr := strings.Join(returnNames, ", ")

		if len(returnNames) > 1 {
			returnExpr = "(" + returnExpr + ")"
		}

		if aliases := v.namedResultBoxAliasLines(litSig, v.indentLevel+2); aliases != "" && strings.HasPrefix(body, "{") {
			body = "{" + aliases + strings.TrimPrefix(body, "{")
		}

		inner = fmt.Sprintf("{%s%s%sfunc((defer, recover) => %s);%s%sreturn %s;%s%s}",
			v.namedReturnDeclLines(litSig, v.indentLevel+1, true),
			v.newline, v.indent(v.indentLevel+1), body,
			v.newline, v.indent(v.indentLevel+1), returnExpr,
			v.newline, v.indent(v.indentLevel))
	case litHasDefer || litHasRecover:
		// A result-RETURNING literal wraps in the VALUE execution context — the void
		// `func((defer, recover) => …)` cannot return a value (CS8030 ×4, net
		// lookup_windows' `getaddr := func() ([]IPAddr, error) { defer …; return … }`).
		if litSig != nil && litSig.Results() != nil && litSig.Results().Len() > 0 && !litHasNamedResults {
			inner = fmt.Sprintf("func<%s>((defer, recover) => %s)", v.generateResultSignature(litSig), body)
		} else {
			inner = wrapIIFEFuncContext(body, litHasDefer, litHasRecover)
		}
	default:
		inner = body
	}

	if context.isIIFE {
		// Immediately-invoked function literal: emit `paramNames => BODY` (names only — the
		// delegate-cast in convCallExpr supplies the types). The transient boxed-param set is
		// scoped to the name rendering so a boxed param emits its incoming `ʗp` name here too.
		v.funcLitHeapBoxParamNames = boxedParamNames
		iifeParams := v.iifeParamNames(litSig)
		v.funcLitHeapBoxParamNames = nil

		result.WriteString(iifeParams + " => " + inner)
	} else {
		// A literal with a single unsafe.Pointer result can mix return arms of DIFFERENT C#
		// types — reflect deepEqual's ptrval returns `(uintptr)v.pointer()` on one arm and the
		// raw `v.ptr` on the other — which defeats C# lambda return-type inference (CS8917).
		// State the return type explicitly (`@unsafe.Pointer (ΔValue v) => …`); each arm then
		// converts implicitly through the golib operators.
		returnTypePrefix := ""

		if results := litSig.Results(); results != nil && results.Len() == 1 {
			if basic, ok := results.At(0).Type().(*types.Basic); ok && basic.Kind() == types.UnsafePointer {
				returnTypePrefix = convertToCSTypeName(v.getTypeName(results.At(0).Type(), false)) + " "
			} else if declaredIsIface, isEmpty := isInterface(results.At(0).Type()); declaredIsIface && !isEmpty {
				// An INTERFACE-returning literal whose arms return DISTINCT concrete types —
				// net ipsock.go's `inetaddr := func(ip IPAddr) Addr` returns TCPAddrжΔAddr /
				// UDPAddrжΔAddr / IPAddrжΔAddr adapter classes — has no best common type
				// either (CS8917); each arm converts implicitly once the return type is
				// explicit. Single-typed literals keep the inferred form (zero churn).
				var armTypes []types.Type

				ast.Inspect(funcLit.Body, func(n ast.Node) bool {
					if _, isLit := n.(*ast.FuncLit); isLit && n != funcLit.Body {
						return false // a nested literal's returns belong to it
					}

					if ret, ok := n.(*ast.ReturnStmt); ok && len(ret.Results) == 1 {
						if retType := v.getType(ret.Results[0], false); retType != nil {
							known := false

							for _, t := range armTypes {
								if types.Identical(t, retType) {
									known = true
									break
								}
							}

							if !known {
								armTypes = append(armTypes, retType)
							}
						}
					}

					return true
				})

				if len(armTypes) > 1 {
					returnTypePrefix = convertToCSTypeName(v.getTypeName(results.At(0).Type(), false)) + " "
				}
			} else if context.isAssignment {
				// A STRING-returning literal in natural-inference position (`pick := func(v any)
				// string {…}` → `var pick = …`) can mix return arms of DIFFERENT C# types even
				// though every arm is a Go string: a string literal is a `"…"u8` ReadOnlySpan<byte>,
				// a literal+var concat binds golib's `operator +(@string, @string)` (so it is
				// @string regardless of u8 suppression), and a call into a hand-written stub can
				// return C# `string` (the baseline fmt.Sprintf does). @string↔string convert
				// implicitly BOTH ways, so no unique best common type exists and the delegate is
				// not inferable (CS8917). State the return type explicitly (`var pick = @string
				// (any v) => …`); each arm then converts to @string in place. Gated to the basic
				// string kind (a named string type would need its own conversions — see
				// lambdaConstReturnCastType's named-type rationale) and to assignment position:
				// an argument/return/composite-element literal is target-typed by its delegate
				// (no inference to fail), where an explicit return type could only add an
				// identity-match constraint against stub delegate types.
				if basic, ok := types.Unalias(results.At(0).Type()).(*types.Basic); ok {
					if basic.Kind() == types.String {
						returnTypePrefix = convertToCSTypeName(v.getTypeName(results.At(0).Type(), false)) + " "
					} else if basic.Info()&types.IsNumeric != 0 && v.funcLitReturnsUntypedNamedConst(funcLit) {
						// A NUMERIC-result literal in natural-inference position with a NAMED
						// untyped-constant return arm — `maxRune := func(rune) rune { return
						// unicode.MaxRune }` (strings TestMap): the const ref renders as a golib
						// `Untyped*` wrapper reference, whose implicit conversions run BOTH ways
						// with every numeric type. An all-const arm set therefore infers the
						// wrapper delegate (`Func<rune, UntypedInt>` — rejected at the invariant-
						// delegate use site, CS1503), and a mixed const/typed arm set has no
						// unique best common type at all (CS8917). Stating the declared return
						// type explicitly (`var maxRune = rune (rune _) => …`) converts each arm
						// in place. Gated to a BASIC numeric result (a named type would need a
						// second user conversion the wrapper cannot chain — see
						// lambdaConstReturnCastType's named-type rationale); literal-only arm
						// sets (`return 'a'`, `return -1`) keep inferred typing — those render at
						// concrete C# types already (no churn).
						returnTypePrefix = convertToCSTypeName(v.getTypeName(results.At(0).Type(), false)) + " "
					}
				}
			}
		} else if results := litSig.Results(); results != nil && results.Len() > 1 && context.isAssignment {
			// A MULTI-result literal where EVERY return arm carries a typeless element —
			// `return (default!, err)` on the error arms and `return (b, default!)` on the
			// success arm (macho file.go's sectionData, func(s *Section) ([]byte, error)):
			// a tuple literal with any untyped element has no natural type, so NO arm
			// contributes to inference (CS8917 + CS8130/CS8716 cascade). State the tuple
			// return type explicitly; nil elements then take the target element type.
			// NAMED results are included: crypto/x509 parseNameConstraintsExtension's
			// `getValues := func(subtrees) (dnsNames []string, ips []*net.IPNet, emails,
			// uriDomains []string, err error)` returns `nil,nil,nil,nil,err` on every error
			// arm and `…, nil` on the success arm — no fully-typed arm, CS8917. A bare `return`
			// (len 0) never matches results.Len(), so it neither sets hasReturn nor a false
			// hasFullyTypedArm; a named literal that DOES have a fully-typed explicit arm keeps
			// inferred typing (no return-type prefix, no churn).
			//
			// A basic-STRING literal element is ALSO inference-defeating — worse than typeless,
			// it is WRONGLY typed: inside a tuple the literal emits as a bare C# string (u8
			// spans cannot be tuple elements — see visitReturnStmt), and @string↔string convert
			// implicitly both ways, so an arm like `return dur, coverageSnapshot, ""` infers a
			// C# `string` element where the Go result is @string (internal/fuzz fuzzOnce: the
			// destructured errMsg then has no `!=` against a u8 literal, CS0019 — the tuple-
			// element sibling of the single-string-result CS8917 arm above). Gated on the
			// literal's presence AND a declared basic-string element, so a literal whose string
			// elements are all variables keeps inferred typing (no churn).
			//
			// An untyped NUMERIC constant literal element is the same shape when the declared
			// result element is a differently-SIZED basic type: the literal emits bare, so the
			// arm infers the literal's natural C# type — an INT literal is C# `int`, a FLOAT
			// literal C# `double` — where the Go result is e.g. int64 (net/http ServeContent's
			// `sizeFunc := func() (int64, error) { …; return 0, errSeeker }` inferred
			// `Func<(int, error errSeeker)>`, rejected at the serveContent call: delegate types
			// are invariant, CS1662/CS0029/CS1503; the explicit tuple type also drops the
			// leaked `errSeeker` element name). A declared element the literal's natural type
			// already matches (int32 for INT, float64 for FLOAT) infers correctly, and Go `int`
			// (C# nint) is deliberately exempt — the `return 0, err` shape against (int, error)
			// results is pervasive and green today (int→nint converts at every use site), so
			// marking it would churn stdlib-wide for no observed defect (the same reasoning
			// keeps lambdaConstReturnCastType away from signed single results).
			hasReturn := false
			hasFullyTypedArm := false

			// Per-result-position tracking for the Go-`int` (C# nint) MIXED-arm conflict below: at a
			// declared-`int` position, an INT LITERAL arm (`0`) is naturally C# `int` while a
			// non-literal Go-`int` arm (`i + 1`) is C# `nint`. When both occur — and the other tuple
			// positions are typeless (`default!`) on the non-literal arms — the ONLY arm with a natural
			// tuple type is a literal one, so C# infers the delegate's first element as `int` and the
			// `nint` arm then fails to convert to it (CS0029/CS1662; the var's inferred delegate is
			// also `int`-first, rejected at the invariant use site, CS0407 — bufio ExampleScanner_*'s
			// `onComma := func(...) (advance int, ...) { …; return 0, data, ErrFinalToken; …; return
			// i+1, …; }`). The plain `types.Int` exemption below intentionally lets the literal count as
			// matching (the pervasive all-literal `return 0, err` shape is green and must not churn), so
			// this narrower conflict is detected separately and forces the explicit return type.
			posHasIntLiteral := make([]bool, results.Len())
			posHasNintExpr := make([]bool, results.Len())

			ast.Inspect(funcLit.Body, func(n ast.Node) bool {
				if _, isLit := n.(*ast.FuncLit); isLit && n != funcLit.Body {
					return false // a nested literal's returns belong to it
				}

				if ret, ok := n.(*ast.ReturnStmt); ok && len(ret.Results) == results.Len() {
					hasReturn = true
					fullyTyped := true

					for i, res := range ret.Results {
						if tv, ok := v.info.Types[res]; ok {
							if basic, isBasic := tv.Type.(*types.Basic); isBasic && basic.Kind() == types.UntypedNil {
								fullyTyped = false
								break
							}
						}

						if isStringBasicLit(res) {
							if declared, ok := types.Unalias(results.At(i).Type()).(*types.Basic); ok && declared.Kind() == types.String {
								fullyTyped = false
								break
							}
						}

						if lit, isNumeric := numericBasicLit(res); isNumeric {
							if declared, ok := types.Unalias(results.At(i).Type()).(*types.Basic); ok && declared.Info()&types.IsNumeric != 0 {
								naturalKind := types.Int32

								if lit.Kind == token.FLOAT {
									naturalKind = types.Float64
								}

								if declared.Kind() != naturalKind && declared.Kind() != types.Int {
									fullyTyped = false
									break
								}
							}
						}
					}

					if fullyTyped {
						hasFullyTypedArm = true
					}

					// Record int-literal vs nint-expression occupancy per declared-`int` position
					// (independent of the fullyTyped break above, which is why it runs in its own loop).
					for i, res := range ret.Results {
						declared, ok := types.Unalias(results.At(i).Type()).(*types.Basic)

						if !ok || declared.Kind() != types.Int {
							continue
						}

						if lit, isNumeric := numericBasicLit(res); isNumeric && lit.Kind == token.INT {
							posHasIntLiteral[i] = true
						} else if resType := v.getType(res, false); resType != nil {
							if resBasic, ok := types.Unalias(resType).(*types.Basic); ok && resBasic.Kind() == types.Int {
								posHasNintExpr[i] = true
							}
						}
					}
				}

				return true
			})

			mixedIntConflict := false

			for i := range posHasIntLiteral {
				if posHasIntLiteral[i] && posHasNintExpr[i] {
					mixedIntConflict = true
					break
				}
			}

			if hasReturn && (!hasFullyTypedArm || mixedIntConflict) {
				returnTypePrefix = v.generateResultSignature(litSig) + " "
			}
		}

		result.WriteString(returnTypePrefix + "(" + parameterSignature + ") => " + inner)
	}

	return result.String()
}

// funcLitHeapBoxParamIdents returns the literal's own VALUE parameters that need an
// entry-time heap box, in declaration order — the function-literal analogue of
// paramNeedsHeapBox, which serves declaration parameters via currentFuncDecl (a literal's
// params never enter its walk, and package-level initializer literals have no declaration at
// all). A literal param qualifies exactly like a declaration param: marked escaping by
// markCaptureModeBoxedParams AND re-verified against the declaring ident, so a param that
// leaked into identEscapesHeap via a mixed `t, y := …` define keeps its historical unboxed
// emission — or routed to SHARED storage by the capture analysis (written after a NESTED
// closure captured it — see processPotentialCapture's varShareFacts arm), whose renders
// reference the box inside every capturing lambda, so the literal's prologue must declare it
// (the declaration-param cousins are database/sql beginDC's `ctx` / go/types nify's `x, y`,
// CS0103). A variadic parameter is excluded (its `ʗp` rename/prologue is the variadic slice
// convention, and its unnamed []T type carries no methods).
func (v *Visitor) funcLitHeapBoxParamIdents(funcLit *ast.FuncLit) []*ast.Ident {
	if funcLit.Type.Params == nil {
		return nil
	}

	var boxed []*ast.Ident

	for _, field := range funcLit.Type.Params.List {
		if _, isVariadic := field.Type.(*ast.Ellipsis); isVariadic {
			continue
		}

		for _, ident := range field.Names {
			if isDiscardedVar(ident.Name) {
				continue
			}

			obj := v.info.ObjectOf(ident)

			if obj == nil {
				continue
			}

			if _, isPointer := obj.Type().(*types.Pointer); isPointer {
				continue
			}

			if v.identHasHeapBox(obj, obj.Type()) && (v.bodyCallsCaptureModeMethodOn(ident, funcLit.Body) || v.isLambdaBoxRefVar(obj)) {
				boxed = append(boxed, ident)
			}
		}
	}

	return boxed
}
