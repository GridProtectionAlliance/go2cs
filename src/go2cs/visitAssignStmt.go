package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"strings"
)

// rhsPointerCopyContext returns an IdentContext that forces the pointer (box) form when the
// assignment RHS is a plain pointer-typed identifier — a Go pointer copy (`r := p` / `r = p`
// where p is *T). A deref'd pointer parameter then emits its box `Ꮡp` so the target is a
// ж<T> (the rest of the converter already treats such a target as a pointer via `.val`/`~`).
// A pointer *local* holds the pointer directly, so convIdent returns it unchanged. Returns
// nil when rhs is not a plain non-nil pointer identifier (callers append nothing).
func (v *Visitor) rhsPointerCopyContext(rhs ast.Expr) ExprContext {
	rhsIdent, isIdent := rhs.(*ast.Ident)

	if !isIdent || rhsIdent.Name == "nil" {
		return nil
	}

	if _, isPtr := v.getIdentType(rhsIdent).(*types.Pointer); !isPtr {
		return nil
	}

	ptrContext := DefaultIdentContext()
	ptrContext.isPointer = true

	return ptrContext
}

// appendRhsPtrContext returns base with a pointer-copy IdentContext appended when rhs is a
// plain pointer identifier and the target is not an interface. A fresh slice is returned so
// the caller's backing array (often shared between LHS and RHS conversion) is not mutated.
func (v *Visitor) appendRhsPtrContext(base []ExprContext, rhs ast.Expr, lhsIsInterface bool) []ExprContext {
	if lhsIsInterface {
		return base
	}

	ptrContext := v.rhsPointerCopyContext(rhs)

	if ptrContext == nil {
		return base
	}

	out := make([]ExprContext, len(base), len(base)+1)
	copy(out, base)

	return append(out, ptrContext)
}

// narrowArithmeticCastType returns the C# narrow-integer type a binary/unary arithmetic assignment
// RHS must be cast to when its Go type matches a narrow-integer LHS (int8/uint8/int16/uint16), or
// "" when no cast applies. Go evaluates such arithmetic at the operand's own width (wrapping); C#
// promotes sub-int arithmetic to int, so the result needs a cast back to compile (CS0266) and to
// preserve the wrap. The optional alreadyCast guard skips the cast when the converted RHS already
// starts with `(type)(` — another path narrowed it (e.g. `(byte)(b | 128)`) — to avoid a double cast.
func (v *Visitor) narrowArithmeticCastType(lhs, rhs ast.Expr, alreadyCast string) string {
	return v.narrowArithmeticCastTypeFor(v.getType(lhs, false), rhs, alreadyCast)
}

// narrowArithmeticCastTypeFor is the type-keyed form of narrowArithmeticCastType — used where the
// narrow target is a declared variable's type (a `var x uint8 = a + b` value-spec initializer)
// rather than an LHS expression.
func (v *Visitor) narrowArithmeticCastTypeFor(targetType types.Type, rhs ast.Expr, alreadyCast string) string {
	switch rhs.(type) {
	case *ast.BinaryExpr, *ast.UnaryExpr:
	default:
		return ""
	}

	if targetType == nil {
		return ""
	}

	targetBasic, ok := targetType.Underlying().(*types.Basic)

	if !ok || !isNarrowIntegerKind(targetBasic.Kind()) {
		return ""
	}

	if rhsType := v.getType(rhs, false); rhsType == nil || !types.Identical(rhsType, targetType) {
		return ""
	}

	castType := convertToCSTypeName(v.getTypeName(targetType, false))

	if len(alreadyCast) > 0 && strings.HasPrefix(alreadyCast, fmt.Sprintf("(%s)(", castType)) {
		return ""
	}

	return castType
}

func (v *Visitor) visitAssignStmt(assignStmt *ast.AssignStmt, format FormattingContext) {
	result := &strings.Builder{}

	// A func literal on the RHS (or inside a composite-literal element of it) emits its captured-
	// variable snapshot declarations inline at the literal's position — invalid C# in an expression
	// slot. For a standalone statement, collect them in a buffer and write them before the statement.
	// Save/restore so a nested statement's hoisted decls don't leak into this buffer.
	savedHoist := v.hoistedDecls
	var hoistBuf *strings.Builder

	if format.useNewLine {
		hoistBuf = &strings.Builder{}
		v.hoistedDecls = hoistBuf
	}

	defer func() { v.hoistedDecls = savedHoist }()

	lhsExprs := assignStmt.Lhs
	rhsExprs := assignStmt.Rhs

	lhsLen := len(lhsExprs)
	rhsLen := len(rhsExprs)

	reassignedCount := 0
	declaredCount := 0

	// Check for interface types in LHS as RHS will need to be casted to the interface type
	lhsTypeIsInterface := make([]bool, lhsLen)

	// Check for string types in LHS, u8 readonly spans are not supported in value tuple
	lhsTypeIsString := make([]bool, lhsLen)
	anyTypeIsString := false

	// Ensure that the correct type is used for integer, we do this since int and uint in
	// converted Go code target C# nint or nuint to match original Go code behavior and a
	// "var" based assignment to an int type could result in a very subtle type mismatch
	lhsTypeIsInt := make([]bool, lhsLen)
	anyTypeIsInt := false

	// Ensure that the correct type is used for unsafe.Pointer, we do this since unsafe.Pointer
	// in converted Go code is usually cast to uintptr in C# - in these cases we need to ensure
	// that the type specifically @unsafe.Pointer in C# and not uintptr when using var
	lhsTypeIsUnsafePointer := make([]bool, lhsLen)
	anyTypeIsUnsafePointer := false

	// Check if rhs is a call with a tuple result
	var tupleResult bool

	if rhsLen == 1 {
		if callExpr, ok := rhsExprs[0].(*ast.CallExpr); ok {
			funType := v.info.TypeOf(callExpr.Fun)

			if signature, ok := funType.(*types.Signature); ok {
				results := signature.Results()

				if results != nil {
					tupleResult = results.Len() > 1
				}
			}
		} else if unaryExpr, ok := rhsExprs[0].(*ast.UnaryExpr); ok {
			if unaryExpr.Op == token.ARROW {
				tupleResult = lhsLen > 1
			}
		} else if _, ok := rhsExprs[0].(*ast.TypeAssertExpr); ok {
			tupleResult = lhsLen > 1
		} else if indexExpr, ok := rhsExprs[0].(*ast.IndexExpr); ok && lhsLen > 1 {
			// Comma-ok map access: `v, ok := m[k]`. Detecting it as a tuple result routes
			// the RHS through golib's two-value map indexer `m[key, ꟷ]` (see convIndexExpr).
			if _, isMap := v.getType(indexExpr.X, true).(*types.Map); isMap {
				tupleResult = true
			}
		}
	}

	// Count the number of reassigned and declared variables
	for i, lhs := range lhsExprs {
		ident := getIdentifier(lhs)

		// A selector LHS (`pkg.Var = …`, `x.field = …`) is always an assignment to existing
		// storage, never a declaration. getIdentifier would return its BASE ident (e.g. the
		// package name `sub`), which — not being a local reassignment — would wrongly be counted
		// as a new declaration and later prefixed with `var`. Treat it as the struct-member case.
		if _, ok := lhs.(*ast.SelectorExpr); ok {
			ident = nil
		}

		if ident == nil {
			// Check if lhs is a struct member
			if selectorExpr, ok := lhs.(*ast.SelectorExpr); ok {
				ident = getIdentifier(selectorExpr.Sel)

				isInterface, isEmpty := v.isInterface(ident)
				lhsTypeIsInterface[i] = isInterface && !isEmpty
				typeName := v.getExprTypeName(ident, true)

				lhsTypeIsString[i] = typeName == "string"

				if !anyTypeIsString && lhsTypeIsString[i] {
					anyTypeIsString = true
				}

				lhsTypeIsInt[i] = typeName == "int" || typeName == "uint"

				if !anyTypeIsInt && lhsTypeIsInt[i] {
					anyTypeIsInt = true
				}
			} else if indexExpr, ok := lhs.(*ast.IndexExpr); ok {
				ident = getIdentifier(indexExpr.X)

				isInterface, isEmpty := v.isInterface(ident)
				lhsTypeIsInterface[i] = isInterface && !isEmpty
			}
		} else {
			if v.isReassignment(ident) {
				reassignedCount++
			} else {
				obj := v.info.ObjectOf(ident)

				if obj != nil {
					if !v.identEscapesHeap[obj] {
						declaredCount++
					} else if lhsLen > 1 && v.convertToHeapTypeDecl(ident, false) == "" {
						// An escaping TUPLE element is excluded from declaredCount only when it actually
						// gets a heap-type decl — the escaping-tuple path at the gate below owns its
						// declaration (`ref var list = ref heap(…)`). An escaping element with NO heap
						// decl (e.g. an already-pointer local that merely gets returned, so escape
						// analysis flags it but convertToHeapTypeDecl yields nothing) is declared nowhere
						// else, so it must still be counted to receive a `var`; otherwise the tuple emits
						// `(pp, now) = …` with `pp` never declared (CS0103). Single-var (lhsLen==1)
						// escaping declarations keep the original exclusion — they route to the
						// capture-hoist emission path which relies on declaredCount staying 0.
						declaredCount++
					}
				}
			}

			isInterface, isEmpty := v.isInterface(ident)
			lhsTypeIsInterface[i] = isInterface && !isEmpty
			typeName := v.getExprTypeName(ident, true)

			lhsTypeIsString[i] = typeName == "string"

			if !anyTypeIsString && lhsTypeIsString[i] {
				anyTypeIsString = true
			}

			lhsTypeIsInt[i] = typeName == "int" || typeName == "uint"

			if !anyTypeIsInt && lhsTypeIsInt[i] {
				anyTypeIsInt = true
			}

			lhsTypeIsUnsafePointer[i] = typeName == "unsafe.Pointer"

			if !anyTypeIsUnsafePointer && lhsTypeIsUnsafePointer[i] {
				anyTypeIsUnsafePointer = true
			}
		}
	}

	// Lift an anonymous-struct composite-literal RHS up front (named after the LHS var) when the
	// local escapes to the heap. The heap declaration (`ref var x = ref heap<T>(…)`) renders its
	// box type explicitly via convertToHeapTypeDecl, which runs BEFORE the RHS composite would
	// normally trigger the lift — so without this the box type emits a raw, un-compilable
	// `struct{…}` (e.g. runtime/mpagealloc's `firstFree := struct{…}{…}` whose address is taken).
	// Mirrors convCompositeLit's lift; the liftedTypeExists guard makes the later one a no-op.
	if assignStmt.Tok == token.DEFINE {
		for i, lhs := range lhsExprs {
			if i >= rhsLen {
				break
			}

			lhsIdent := getIdentifier(lhs)

			if lhsIdent == nil || isDiscardedVar(lhsIdent.Name) {
				continue
			}

			if _, isSel := lhs.(*ast.SelectorExpr); isSel {
				continue
			}

			obj := v.info.ObjectOf(lhsIdent)

			if obj == nil || !v.identEscapesHeap[obj] {
				continue
			}

			if compositeLit, ok := rhsExprs[i].(*ast.CompositeLit); ok {
				if structType, exprType := v.extractStructType(compositeLit.Type); structType != nil && !v.liftedTypeExists(structType) {
					var indentOffset int

					if v.inFunction {
						indentOffset = 1
					}

					v.indentLevel += indentOffset
					v.visitStructType(structType, exprType, v.getIdentName(lhsIdent), nil, true, nil)
					v.indentLevel -= indentOffset
				}
			}
		}
	}

	// Map Go tokens to C# string equivalents
	var operator string

	// andNotUncheckedClose closes the `unchecked((Type)~` wrapper opened by the operator for a
	// narrow/unsigned `&^=` (see the AND_NOT_ASSIGN case). The matching `)` is written immediately
	// after the RHS at every operator-emission site (each statement uses exactly one of them).
	andNotUncheckedClose := false

	switch assignStmt.Tok {
	case token.ADD_ASSIGN:
		operator = " += "
	case token.SUB_ASSIGN:
		operator = " -= "
	case token.MUL_ASSIGN:
		operator = " *= "
	case token.QUO_ASSIGN:
		operator = " /= "
	case token.REM_ASSIGN:
		operator = " %= "
	case token.AND_ASSIGN:
		operator = " &= "
	case token.OR_ASSIGN:
		operator = " |= "
	case token.XOR_ASSIGN:
		operator = " ^= "
	case token.SHL_ASSIGN:
		operator = " <<= "
	case token.SHR_ASSIGN:
		operator = " >>= "
	case token.AND_NOT_ASSIGN:
		// C# doesn't have a direct AND NOT equivalent, so expand `&^=` to `&= ~`. The `~` promotes
		// its operand to `int`, and `int` is not implicitly convertible to a narrower or unsigned LHS
		// type (byte/ushort/uint/ulong/…), so `flags &= ~X` is CS0266. Cast the complemented RHS back
		// to the LHS type for those: `flags &= unchecked((uint8)~X)`. The cast is `unchecked` because
		// for a CONSTANT operand `~X` folds to a negative `int` constant whose checked narrowing
		// overflows (CS0221). Types that `int` widens to implicitly (int/int32/int64) need no cast.
		operator = " &= ~"

		if len(assignStmt.Lhs) == 1 {
			if lhsType := v.info.TypeOf(assignStmt.Lhs[0]); lhsType != nil {
				if basic, ok := lhsType.Underlying().(*types.Basic); ok && basic.Info()&types.IsInteger != 0 {
					switch basic.Kind() {
					case types.Int, types.Int32, types.Int64:
						// int widens to these implicitly; no cast needed
					default:
						operator = fmt.Sprintf(" &= unchecked((%s)~", convertToCSTypeName(v.getTypeName(lhsType, false)))
						andNotUncheckedClose = true
					}
				}
			}
		}
	default:
		operator = " = "
	}

	bitwiseAssignOp := assignStmt.Tok == token.AND_ASSIGN ||
		assignStmt.Tok == token.OR_ASSIGN ||
		assignStmt.Tok == token.XOR_ASSIGN ||
		assignStmt.Tok == token.SHL_ASSIGN ||
		assignStmt.Tok == token.SHR_ASSIGN ||
		assignStmt.Tok == token.AND_NOT_ASSIGN

	// A for-loop init clause is a single `;`-free clause: multiple declarations of differing
	// types cannot be emitted as `;`-separated decls (the `;` terminates the clause), and the
	// combined `var (a, b) = ...` form is unavailable when the types differ (mixed int/pointer,
	// etc.). Detect a multi-variable, all-new for-init that does not need any heap box so it can
	// be emitted as a single tuple-deconstruction declaration `(nint i, var e) = (..., ...)`.
	forInitTupleDecl := format.forInit && lhsLen > 1 && lhsLen == rhsLen && reassignedCount == 0
	if forInitTupleDecl {
		for i := range lhsLen {
			if ident := getIdentifier(lhsExprs[i]); ident == nil || ident.Name == "_" || v.convertToHeapTypeDecl(ident, false) != "" {
				forInitTupleDecl = false
				break
			}
		}
	}

	if tupleResult || lhsLen == reassignedCount || lhsLen == declaredCount && !anyTypeIsString && !anyTypeIsInt && !anyTypeIsUnsafePointer {
		leftExprs := HashSet[string]{}

		// Go's partial redeclaration `a, b := f()` reuses any already-declared LHS variable and
		// declares only the new ones. A single blanket `var` prefix would re-declare the reused
		// variable (CS0136, and its earlier uses become "before declaration" CS0841), so for a
		// mixed declared/reassigned tuple, emit `var` per newly-declared element instead.
		mixedDeclare := declaredCount > 0 && reassignedCount > 0

		// A tuple element whose address is taken (`list, delta := netpoll(0); injectglist(&list)`)
		// must be heap-boxed so its `Ꮡlist` companion exists — the combined `var (list, delta) = …`
		// deconstruction cannot create it (a missing box → CS0103, and a `Ꮡ(value)` copy fallback
		// would silently lose writes through the pointer). Emit each escaping element's heap decl
		// (`ref var list = ref heap<gList>(out var Ꮡlist);`) first, then a mixed deconstruction-
		// ASSIGNMENT: the escaping element is the pre-declared ref-local (so the deconstructed value
		// lands in the box) and the rest declare with `var` — handled by the mixedDeclare path with a
		// per-element `escaping` skip. Only when at least one element escapes; otherwise unchanged.
		escaping := make([]bool, lhsLen)
		escapingHeapDecls := ""

		// Only a pure new-declaration tuple (`:=`, no reassigned element). Escaping elements are not
		// counted in declaredCount (the heap-decl path owns them), so the gate is reassignedCount.
		if assignStmt.Tok == token.DEFINE && reassignedCount == 0 && lhsLen > 1 {
			for i := range lhsExprs {
				if ident := getIdentifier(lhsExprs[i]); ident != nil {
					if decl := v.convertToHeapTypeDecl(ident, false); decl != "" {
						escaping[i] = true
						escapingHeapDecls += decl + v.newline + v.indent(v.indentLevel)
					}
				}
			}
		}

		if escapingHeapDecls != "" {
			mixedDeclare = true
			result.WriteString(escapingHeapDecls)
		}

		// Handle LHS
		if declaredCount > 0 && !mixedDeclare {
			if declaredCount > 1 || v.options.preferVarDecl {
				isDiscarded := lhsLen == 1 && getIdentifier(lhsExprs[0]).Name == "_"

				if !isDiscarded {
					result.WriteString("var ")
				}
			} else {
				ident := getIdentifier(lhsExprs[0])
				lhsType := convertToCSTypeName(v.getExprTypeName(ident, false))
				result.WriteString(lhsType)
				result.WriteRune(' ')
			}
		}

		if lhsLen > 1 {
			result.WriteRune('(')
		}

		lambdaContext := DefaultLambdaContext()
		lambdaContext.isAssignment = true

		for i, lhs := range lhsExprs {
			if i > 0 {
				result.WriteString(", ")
			}

			ident := getIdentifier(lhsExprs[i])
			context := DefaultIdentContext()

			if (!v.isPointer(ident) || v.identIsParameter(ident)) && i < rhsLen {
				// If rhs is a address of expression, we need to convert identifier to its pointer variable
				if unaryExpr, ok := rhsExprs[i].(*ast.UnaryExpr); ok {
					if unaryExpr.Op == token.AND {
						context.isPointer = true
					}
				}
			}

			// Reassigning the direct-ж receiver to a pointer value (`r = r.prev`, a ring walk):
			// emit the box `Ꮡr` on the LHS so the pointer-reassignment path below repoints the box
			// and re-aliases the value var (`Ꮡr = r.prev; r = ref Ꮡr.val;`). The deref'd value var
			// alone cannot be repointed at a different node.
			if i < rhsLen && v.exprIsCurrentDirectBoxReceiver(lhsExprs[i]) && isPointer(v.getExprType(rhsExprs[i])) {
				context.isPointer = true
			}

			// Reassigning a deref'd pointer PARAMETER to a new pointer value (`bits = addb(bits, n)`,
			// a *byte memory walk in the runtime). Every named pointer param is deref-aliased to a
			// value var (`ref var bits = ref Ꮡbits.val`), which cannot be repointed; emit the box
			// `Ꮡbits` on the LHS so the pointer-reassignment path below repoints it and re-aliases the
			// value var (`Ꮡbits = addb(Ꮡbits, n); bits = ref Ꮡbits.val;`). The RHS already references
			// the box form. (The `&`-RHS case above is a subset; setting isPointer twice is harmless.)
			if i < rhsLen && v.identIsParameter(ident) && v.isPointer(ident) && isPointer(v.getExprType(rhsExprs[i])) {
				context.isPointer = true
			}

			lhsExpr := v.convExpr(lhs, []ExprContext{context, lambdaContext})
			leftExprs.Add(lhsExpr)

			// Per-element `var` for the newly-declared members of a mixed redeclaration tuple. An
			// escaping element is already declared by its heap decl above, so it takes no `var`.
			if mixedDeclare && ident != nil && ident.Name != "_" && !v.isReassignment(ident) && !escaping[i] {
				result.WriteString("var ")
			}

			result.WriteString(lhsExpr)
		}

		if lhsLen > 1 {
			result.WriteRune(')')
		}

		result.WriteString(operator)

		// Handle RHS
		if rhsLen > 1 {
			result.WriteRune('(')
		}

		for i, rhs := range rhsExprs {
			var lhs ast.Expr

			if i < lhsLen {
				lhs = lhsExprs[i]
			} else {
				lhs = lhsExprs[lhsLen-1]
			}

			ident := getIdentifier(lhs)

			if i > 0 {
				result.WriteString(", ")
			}

			contexts := []ExprContext{lambdaContext}

			// A plain pointer-typed identifier on the RHS is a pointer copy (Go `r := p` /
			// `r = p` where p is *T): emit its pointer form so the target is a ж<T>, not a
			// copy of the pointed-to value. For a deref'd pointer parameter this yields the
			// box `Ꮡp` (without this it emits the value alias `p`, and the target — which the
			// rest of the converter treats as a pointer via `.val`/`~` — fails to compile).
			// A pointer *local* already holds the pointer directly, so convIdent returns it
			// unchanged. Skip interface targets (handled by the interface-cast path).
			if i < lhsLen && !lhsTypeIsInterface[i] {
				if ptrCtx := v.rhsPointerCopyContext(rhs); ptrCtx != nil {
					contexts = append(contexts, ptrCtx)
				}
			}

			// A u8 string literal is a ReadOnlySpan<byte> (ref struct) and cannot be
			// a ValueTuple element, so suppress the u8 form for string literals in a
			// multi-value (tuple) assignment, e.g. `field, env = env, ""`.
			if rhsLen > 1 {
				basicLitContext := DefaultBasicLitContext()
				basicLitContext.u8StringOK = false
				contexts = append(contexts, basicLitContext)
			}

			if selectorExpr, ok := rhs.(*ast.SelectorExpr); ok {
				if v.isMethodValue(selectorExpr, false) {
					v.enterLambdaConversion(selectorExpr)
					defer v.exitLambdaConversion()

					// First prepare the captures
					v.prepareStmtCaptures(selectorExpr)

					// Then generate declarations
					if decls := v.generateCaptureDeclarations(); decls != "" {
						result.WriteString(decls)
					}
				}
			}

			if _, ok := rhs.(*ast.CompositeLit); ok {
				if ident != nil {
					// Track the name of the variable on the LHS for composite literals,
					// this is needed for sparse array initializations
					keyValueContext := DefaultKeyValueContext()
					keyValueContext.ident = ident
					contexts = append(contexts, keyValueContext)
				}
			}

			if tupleResult {
				tupleResultContext := DefaultUnaryExprContext()
				tupleResultContext.isTupleResult = tupleResult
				contexts = append(contexts, tupleResultContext)

				// A comma-ok map access (`v, ok := m[k]`) reaches convIndexExpr, which
				// needs the tuple-result flag to emit the two-value indexer `m[key, ꟷ]`.
				indexResultContext := DefaultIndexExprContext()
				indexResultContext.isTupleResult = true
				contexts = append(contexts, indexResultContext)
			}

			rhsExpr := v.convExpr(rhs, contexts)

			// Narrow-integer arithmetic RHS assigned to a narrow LHS needs a cast back to the LHS
			// type (see narrowArithmeticCastType). The existing bitwise-assign / `&^=` wrappers take
			// precedence (their own cast already applies).
			var narrowCastType string

			if !bitwiseAssignOp && !andNotUncheckedClose && i < lhsLen && !lhsTypeIsInterface[i] {
				narrowCastType = v.narrowArithmeticCastType(lhsExprs[i], rhs, rhsExpr)
			}

			var binaryTypeName string

			if bitwiseAssignOp {
				if assignStmt.Tok == token.SHL_ASSIGN || assignStmt.Tok == token.SHR_ASSIGN {
					// The shift count in a C# compound shift-assignment must be `int`;
					// casting it to the RHS's own (possibly unsigned or native-width)
					// type — e.g. `y <<= (nuint)s` — is rejected with CS0019.
					binaryTypeName = "int"
				} else {
					binaryType := v.info.Types[rhs].Type

					if binaryType != nil {
						binaryTypeName = convertToCSTypeName(v.getTypeName(binaryType, false))
					}
				}
			}

			if len(binaryTypeName) > 0 {
				result.WriteString(fmt.Sprintf("(%s)(", binaryTypeName))
			}

			if len(narrowCastType) > 0 {
				result.WriteString(fmt.Sprintf("(%s)(", narrowCastType))
			}

			if lhsTypeIsInterface[i] {
				result.WriteString(v.convertExprToInterfaceType(lhsExprs[i], rhs, rhsExpr))
			} else {
				result.WriteString(rhsExpr)
			}

			if len(narrowCastType) > 0 {
				result.WriteRune(')')
			}

			if len(binaryTypeName) > 0 {
				result.WriteRune(')')
			}

			if andNotUncheckedClose {
				// Close the `unchecked((Type)~` wrapper opened by the narrow `&^=` operator.
				result.WriteRune(')')
			}
		}

		if rhsLen > 1 {
			result.WriteRune(')')
		}

		if format.includeSemiColon {
			result.WriteRune(';')
		}

		if len(leftExprs) > 0 && operator == " = " {
			for _, leftExpr := range leftExprs.Keys() {
				// Only a bare pointer-box reassignment (`Ꮡp = …`) needs the deref ref-local
				// re-aliased (`p = ref Ꮡp.val`). A write *through* the box (`Ꮡp.val = …`, emitted
				// for `*p = …` inside a lambda — see convStarExpr) has member access and must not
				// trigger this; it is a plain value assignment.
				if strings.HasPrefix(leftExpr, AddressPrefix) && !strings.Contains(leftExpr, ".") {
					// This is a special case for pointer reassignments which should be extended
					// to also update local deference variable as well, e.g.: `x = ref Ꮡx.val`
					derefExpr := getSanitizedIdentifier(leftExpr[len(AddressPrefix):])
					result.WriteString(fmt.Sprintf(" %s = ref %s.val;", derefExpr, leftExpr))
				}
			}
		}

	} else if forInitTupleDecl {
		// Emit a single tuple-deconstruction declaration with per-element types:
		// `(nint i, var e) = (other.Len(), other.Front())`.
		lambdaContext := DefaultLambdaContext()
		lambdaContext.isAssignment = true

		result.WriteRune('(')

		for i := range lhsLen {
			if i > 0 {
				result.WriteString(", ")
			}

			ident := getIdentifier(lhsExprs[i])

			if lhsTypeIsString[i] {
				result.WriteString("@string ")
			} else if v.options.preferVarDecl && !(lhsTypeIsInt[i] || lhsTypeIsUnsafePointer[i]) {
				result.WriteString("var ")
			} else {
				lhsType := convertToCSTypeName(v.getExprTypeName(ident, false))
				result.WriteString(lhsType)
				result.WriteRune(' ')
			}

			context := DefaultIdentContext()

			if (!v.isPointer(ident) || v.identIsParameter(ident)) && i < rhsLen {
				if unaryExpr, ok := rhsExprs[i].(*ast.UnaryExpr); ok && unaryExpr.Op == token.AND {
					context.isPointer = true
				}
			}

			result.WriteString(v.convExpr(lhsExprs[i], []ExprContext{context, lambdaContext}))
		}

		result.WriteString(") = (")

		for i, rhs := range rhsExprs {
			if i > 0 {
				result.WriteString(", ")
			}

			contexts := []ExprContext{lambdaContext}

			// A u8 string literal is a ReadOnlySpan<byte> (ref struct) and cannot be a
			// ValueTuple element, so suppress the u8 form for string literals here.
			basicLitContext := DefaultBasicLitContext()
			basicLitContext.u8StringOK = false
			contexts = append(contexts, basicLitContext)

			rhsExpr := v.convExpr(rhs, contexts)

			if i < lhsLen && lhsTypeIsInterface[i] {
				result.WriteString(v.convertExprToInterfaceType(lhsExprs[i], rhs, rhsExpr))
			} else {
				result.WriteString(rhsExpr)
			}
		}

		result.WriteRune(')')

		// No trailing semicolon: visitForStmt appends "; " after the init clause.
	} else {
		// Some variables are declared and some are reassigned, or one of the types is a string or integer
		for i := range lhsLen {
			lhs := lhsExprs[i]

			var rhs ast.Expr

			if i < rhsLen {
				rhs = rhsExprs[i]
			} else {
				rhs = rhsExprs[rhsLen-1]
			}

			lambdaContext := DefaultLambdaContext()
			lambdaContext.isAssignment = true

			if i > 0 {
				if format.useNewLine {
					result.WriteString(v.newline)
				}

				if format.useIndent {
					result.WriteString(v.indent(v.indentLevel))
				}
			}

			contexts := []ExprContext{lambdaContext}
			ident := getIdentifier(lhs)

			// A selector LHS is a plain assignment to existing storage, not a declaration — force
			// the `ident == nil` path so it is never prefixed with `var` (see the counting loop).
			if _, ok := lhs.(*ast.SelectorExpr); ok {
				ident = nil
			}

			if selectorExpr, ok := rhs.(*ast.SelectorExpr); ok {
				if v.isMethodValue(selectorExpr, false) {
					v.enterLambdaConversion(selectorExpr)
					defer v.exitLambdaConversion()

					// First prepare the captures
					v.prepareStmtCaptures(selectorExpr)

					// Then generate declarations
					if decls := v.generateCaptureDeclarations(); decls != "" {
						result.WriteString(decls)
					}
				}
			}

			if _, ok := rhs.(*ast.CompositeLit); ok {
				if ident != nil {
					// Track the name of the variable on the LHS for composite literals,
					// this is needed for sparse array initializations
					keyValueContext := DefaultKeyValueContext()
					keyValueContext.ident = ident
					contexts = append(contexts, keyValueContext)
				}
			}

			if ident == nil {
				if _, ok := lhs.(*ast.StarExpr); ok {
					starExprContext := DefaultStarExprContext()
					starExprContext.inLhsAssign = true
					contexts = append(contexts, starExprContext)
				}

				result.WriteString(v.convExpr(lhs, contexts))
				result.WriteString(operator)

				rhsExpr := v.convExpr(rhs, v.appendRhsPtrContext(contexts, rhs, lhsTypeIsInterface[i]))

				// A narrow-integer arithmetic RHS assigned to a narrow struct-field LHS (a pure
				// selector, e.g. `it.i = i + 1`) routes through this block (its base ident is nil'd in
				// the counting loop), so it needs the same narrow cast as the var/element forms above.
				var narrowCastType string

				if !andNotUncheckedClose && !lhsTypeIsInterface[i] {
					narrowCastType = v.narrowArithmeticCastType(lhs, rhs, rhsExpr)
				}

				if len(narrowCastType) > 0 {
					result.WriteString(fmt.Sprintf("(%s)(", narrowCastType))
				}

				if lhsTypeIsInterface[i] {
					result.WriteString(v.convertExprToInterfaceType(lhs, rhs, rhsExpr))
				} else {
					result.WriteString(rhsExpr)
				}

				if len(narrowCastType) > 0 {
					result.WriteRune(')')
				}

				if andNotUncheckedClose {
					// Close the `unchecked((Type)~` wrapper opened by the narrow `&^=` operator.
					result.WriteRune(')')
				}

				if format.includeSemiColon || i < lhsLen-1 {
					result.WriteRune(';')
				}
			} else {
				if v.isReassignment(ident) {
					result.WriteString(v.convExpr(lhs, contexts))
					result.WriteString(operator)

					rhsExpr := v.convExpr(rhs, v.appendRhsPtrContext(contexts, rhs, lhsTypeIsInterface[i]))

					if lhsTypeIsInterface[i] {
						result.WriteString(v.convertExprToInterfaceType(lhs, rhs, rhsExpr))
					} else {
						result.WriteString(rhsExpr)
					}

					if andNotUncheckedClose {
						// Close the `unchecked((Type)~` wrapper opened by the narrow `&^=` operator.
						result.WriteRune(')')
					}

					if format.includeSemiColon || i < lhsLen-1 {
						result.WriteRune(';')
					}
				} else if lhsTypeIsString[i] {
					// Handle string variables
					result.WriteString("@string ")
					result.WriteString(v.convExpr(lhs, contexts))
					result.WriteString(operator)
					result.WriteString(v.convExpr(rhs, contexts))
					if format.includeSemiColon || i < lhsLen-1 {
						result.WriteRune(';')
					}
				} else {
					// Check if the variable needs to be allocated on the heap
					heapTypeDecl := v.convertToHeapTypeDecl(ident, false)

					if len(heapTypeDecl) > 0 {
						if format.heapTypeDeclTarget == nil {
							result.WriteString(heapTypeDecl)
							result.WriteString(v.newline)
							result.WriteString(v.indent(v.indentLevel))
						} else {
							format.heapTypeDeclTarget.WriteString(heapTypeDecl)
						}
					} else {
						if v.options.preferVarDecl && !(lhsTypeIsInt[i] || lhsTypeIsUnsafePointer[i]) {
							// A blank-identifier LHS is a C# discard, never a declaration — emit `_ = x;`
							// with no `var`. Testing the current per-element `ident` (not just the
							// single-LHS case) keeps each `_` in a split multi-assign like
							// `_, _, _, _ = a, b, c, d` a discard, so they don't collide (CS0128).
							isDiscarded := ident.Name == "_"

							if !isDiscarded {
								result.WriteString("var ")
							}
						} else {
							lhsType := convertToCSTypeName(v.getExprTypeName(ident, false))
							result.WriteString(lhsType)
							result.WriteRune(' ')
						}
					}

					result.WriteString(v.convExpr(lhs, contexts))
					result.WriteString(operator)

					rhsExpr := v.convExpr(rhs, v.appendRhsPtrContext(contexts, rhs, lhsTypeIsInterface[i]))

					_, rhsIsTypeAssert := rhs.(*ast.TypeAssertExpr)

					if lhsTypeIsInterface[i] && !rhsIsTypeAssert {
						result.WriteString(v.convertExprToInterfaceType(lhs, rhs, rhsExpr))
					} else {
						result.WriteString(rhsExpr)
					}

					if format.includeSemiColon || i < lhsLen-1 {
						result.WriteRune(';')
					}
				}
			}
		}
	}

	if hoistBuf != nil && hoistBuf.Len() > 0 {
		// The hoisted decls carry their own leading newline + per-line indentation.
		v.targetFile.WriteString(hoistBuf.String())
	} else if format.useNewLine {
		v.targetFile.WriteString(v.newline)
	}

	if format.useIndent {
		v.targetFile.WriteString(v.indent(v.indentLevel))
	}

	v.targetFile.WriteString(result.String())
}
