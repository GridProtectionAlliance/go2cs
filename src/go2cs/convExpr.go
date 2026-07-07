package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"strings"
)

type KeyValueSource int

const (
	StructSource KeyValueSource = iota
	MapSource
	ArraySource
)

type ExprContext interface {
	getDefault() StmtContext
}

type CallExprContext struct {
	u8StringArgOK     map[int]bool
	useGoStringArg    map[int]bool
	argTypeIsPtr      map[int]bool
	interfaceTypes    map[int]types.Type
	hasSpreadOperator bool
	keyValueSource    KeyValueSource
	keyValueIdent     *ast.Ident
	// keyValueArrayBacked marks a keyed composite that is backed by a C# array/SparseArray (an
	// indexed slice/array literal, `[]T{i: v}`), not a real map. Its indexer takes a Go `int`
	// (nint), so a key whose Go type is a defined integer type must be cast to int (a `num:nint`
	// key like runtime's `lockRank` cannot implicitly narrow to the indexer type otherwise).
	keyValueArrayBacked bool
	// keyValueCompositeType carries the keyed composite's resolved type through to
	// KeyValueContext.compositeType (see that field's note; runtime/metrics CS1739).
	keyValueCompositeType types.Type
	forceMultiLine      bool
	sourceIsRuneArray   bool
	sourceIsTypeParams  bool
	callArgs            []string
	replacementArgs     []string
	castArgToType       map[int]string
	// wrapArgWithNew wraps the indexed argument in a constructor call (`new slice<E>(arg)`) — the
	// S-where-[]E-expected materialization (see convExprList).
	wrapArgWithNew map[int]string
	// wrapArgWithLambda re-wraps a FUNC-typed argument (a method group / func value) as a lambda
	// `(p0, p1) => value(p0, p1)` so the enclosing delegate's return/param positions can apply the
	// user-defined implicit conversion a C# method-group conversion won't (a constraint-proxy
	// `func() Point` field assigned `nistec.NewP224Point`, whose ж<P224Point> return needs the
	// proxy — CS0407). The map value is the comma-joined lambda parameter list ("" for niladic).
	wrapArgWithLambda map[int]string
	// deferredDecls hoists a func-literal argument's capture declarations out of the call's
	// argument list (where a `var mʗ1 = m;` statement is invalid C#) up to the enclosing
	// statement. Threaded from the statement emitter (visitExprStmt/visitAssignStmt) through
	// convCallExpr → convExprList → the func-literal arg's convFuncLit. Nil when not hoisting.
	deferredDecls *strings.Builder
}

func DefaultCallExprContext() *CallExprContext {
	return &CallExprContext{
		u8StringArgOK:       make(map[int]bool),
		useGoStringArg:      make(map[int]bool),
		argTypeIsPtr:        make(map[int]bool),
		interfaceTypes:      make(map[int]types.Type),
		hasSpreadOperator:   false,
		keyValueSource:      StructSource,
		keyValueIdent:       nil,
		keyValueArrayBacked: false,
		forceMultiLine:      false,
		sourceIsRuneArray:   false,
		sourceIsTypeParams:  false,
		callArgs:            nil,
		replacementArgs:     nil,
		castArgToType:       nil,
		deferredDecls:       nil,
	}
}

func (c CallExprContext) getDefault() StmtContext {
	return DefaultCallExprContext()
}

type BasicLitContext struct {
	u8StringOK        bool
	sourceIsRuneArray bool
	castToGoString    bool
}

func DefaultBasicLitContext() BasicLitContext {
	return BasicLitContext{
		u8StringOK:        true,
		sourceIsRuneArray: false,
		castToGoString:    false,
	}
}

func (c BasicLitContext) getDefault() StmtContext {
	return DefaultBasicLitContext()
}

type ArrayTypeContext struct {
	compositeInitializer bool
	indexedInitializer   bool
	maxLength            int
}

func DefaultArrayTypeContext() ArrayTypeContext {
	return ArrayTypeContext{
		compositeInitializer: false,
		indexedInitializer:   false,
		maxLength:            0,
	}
}

func (c ArrayTypeContext) getDefault() StmtContext {
	return DefaultArrayTypeContext()
}

type LambdaContext struct {
	isAssignment  bool
	isCallExpr    bool
	renderParams  bool
	isPointerCast bool
	deferredDecls *strings.Builder
	callArgs      []string
	// isIIFE marks an immediately-invoked, no-argument function literal — emitted as a
	// `func((defer, recover) => body)` execution-context call so it runs with its OWN
	// defer/recover scope (a bare C# lambda cannot be invoked directly, and an inner defer
	// must not bind to the enclosing function's wrapper). See convCallExpr / convFuncLit.
	isIIFE bool
	// deferOrGoCall marks a call that is the target of a defer/go statement. Such a
	// `defer func(){…}()` / `go func(){…}()` literal must NOT be treated as an IIFE — the
	// deferred/goroutine body is inlined by visitDeferStmt/visitGoStmt.
	deferOrGoCall bool
	// deferCall marks specifically a defer-statement target. A deferred closure's recover()
	// recovers the *enclosing* function (which is itself wrapped, since it contains the defer
	// statement), so the closure must NOT get its own func() execution context. (A goroutine or
	// assigned closure is independent and does get one when it uses defer/recover.)
	deferCall bool
}

func DefaultLambdaContext() LambdaContext {
	return LambdaContext{
		isAssignment:  false,
		isCallExpr:    false,
		renderParams:  false,
		isPointerCast: false,
		deferredDecls: nil,
		callArgs:      nil,
		isIIFE:        false,
	}
}

func (c LambdaContext) getDefault() StmtContext {
	return DefaultLambdaContext()
}

type UnaryExprContext struct {
	isTupleResult bool

	// deferredDecls threads the enclosing statement's hoist target through `&composite`
	// operands so a func-literal FIELD value's capture decls hoist (elf file.go's
	// `&readSeekerFromReader{reset: func() {…}}` in return position, CS1003 ×6).
	deferredDecls *strings.Builder
}

func DefaultUnaryExprContext() UnaryExprContext {
	return UnaryExprContext{
		isTupleResult: false,
	}
}

func (c UnaryExprContext) getDefault() StmtContext {
	return DefaultUnaryExprContext()
}

type IndexExprContext struct {
	// isTupleResult marks a map index used in comma-ok form (`v, ok := m[k]`), so it is
	// emitted via golib's two-value indexer `m[key, ꟷ]` (returning `(value, present)`).
	isTupleResult bool
}

func DefaultIndexExprContext() IndexExprContext {
	return IndexExprContext{
		isTupleResult: false,
	}
}

func (c IndexExprContext) getDefault() StmtContext {
	return DefaultIndexExprContext()
}

type IdentContext struct {
	// isField marks a FIELD selection (vs a method/function name) — fields skip the
	// function-name Main→ΔMain special (see convIdent's isMethod arm).
	isField bool

	isPointer bool
	isType    bool
	isMethod  bool
	ident     *ast.Ident
	// fieldCollidesWithType marks a struct-field selector whose name equals its enclosing
	// struct's type name. C# forbids a member sharing the enclosing type's name (CS0542), so
	// the field is emitted with the disambiguation marker (matching its renamed declaration).
	fieldCollidesWithType bool
	// fieldTypeIsRenamed marks that the field's enclosing type is itself Δ-renamed for a
	// type-vs-method collision in ITS OWN package (a FOREIGN such type, invisible to the current
	// package's nameCollisions). The field access then DOUBLES the marker to match the
	// declaration's ΔΔ form (see typeCollidingFieldName / fieldTypeIsRenamed).
	fieldTypeIsRenamed bool
}

func DefaultIdentContext() IdentContext {
	return IdentContext{
		isPointer:             false,
		isType:                false,
		isMethod:              false,
		ident:                 nil,
		fieldCollidesWithType: false,
		fieldTypeIsRenamed:    false,
	}
}

func (c IdentContext) getDefault() StmtContext {
	return DefaultIdentContext()
}

type KeyValueContext struct {
	// deferredDecls threads the enclosing statement's hoist target into a KEYED composite
	// VALUE's conversion — a func-literal field value with captures otherwise dumps its
	// snapshot decls INLINE in the argument list (elf file.go's readSeekerFromReader{reset:
	// func() {…zrd…}}, CS1003 syntax cascade ×6).
	deferredDecls *strings.Builder

	source      KeyValueSource
	ident       *ast.Ident
	arrayBacked bool

	// compositeType carries the keyed composite literal's resolved type so a struct-field key
	// can detect the field-named-like-its-own-type collision (the declaration applies
	// typeCollidingFieldName; the keyed ctor argument must match — runtime/metrics CS1739).
	compositeType types.Type
}

func DefaultKeyValueContext() KeyValueContext {
	return KeyValueContext{
		source:      StructSource,
		ident:       nil,
		arrayBacked: false,
	}
}

func (c KeyValueContext) getDefault() StmtContext {
	return DefaultKeyValueContext()
}

// Handles pattern match expressions, e.g.: "x is 1 or > 3"
type PatternMatchExprContext struct {
	usePattenMatch bool
	declareIsExpr  bool
}

func DefaultPatternMatchExprContext() PatternMatchExprContext {
	return PatternMatchExprContext{
		usePattenMatch: false,
		declareIsExpr:  false,
	}
}

func (c PatternMatchExprContext) getDefault() StmtContext {
	return DefaultPatternMatchExprContext()
}

type StarExprContext struct {
	inParenExpr bool
	inLhsAssign bool
}

func DefaultStarExprContext() StarExprContext {
	return StarExprContext{
		inParenExpr: false,
		inLhsAssign: false,
	}
}

func (c StarExprContext) getDefault() StmtContext {
	return DefaultStarExprContext()
}

func getExprContext[TContext ExprContext](contexts []ExprContext) TContext {
	var zeroValue TContext

	if len(contexts) == 0 {
		return zeroValue.getDefault().(TContext)
	}

	for _, context := range contexts {
		if context != nil {
			if targetContext, ok := context.(TContext); ok {
				return targetContext
			}
		}
	}

	return zeroValue.getDefault().(TContext)
}

func (v *Visitor) convExpr(expr ast.Expr, contexts []ExprContext) string {
	switch exprType := expr.(type) {
	case *ast.ArrayType:
		context := getExprContext[ArrayTypeContext](contexts)
		return v.convArrayType(exprType, context)
	case *ast.BasicLit:
		context := getExprContext[BasicLitContext](contexts)
		return v.convBasicLit(exprType, context)
	case *ast.BinaryExpr:
		context := getExprContext[PatternMatchExprContext](contexts)
		litContext := getExprContext[BasicLitContext](contexts)
		return v.convBinaryExpr(exprType, context, litContext)
	case *ast.CallExpr:
		context := getExprContext[LambdaContext](contexts)
		return v.convCallExpr(exprType, context)
	case *ast.ChanType:
		return v.convChanType(exprType)
	case *ast.CompositeLit:
		context := getExprContext[KeyValueContext](contexts)

		// Adopt the ambient LambdaContext's hoist target when the KeyValueContext carries
		// none — a `&composite` in RETURN position arrives without a KeyValueContext, and a
		// func-literal FIELD value's capture decls otherwise dump inline (elf file.go's
		// `&readSeekerFromReader{reset: func() {…}}`, CS1003 cascade ×6).
		if context.deferredDecls == nil {
			if lambdaContext := getExprContext[LambdaContext](contexts); lambdaContext.deferredDecls != nil {
				context.deferredDecls = lambdaContext.deferredDecls
			}
		}

		return v.convCompositeLit(exprType, context)
	case *ast.FuncLit:
		context := getExprContext[LambdaContext](contexts)
		return v.convFuncLit(exprType, context)
	case *ast.FuncType:
		// A func TYPE in expression position — the target of a conversion like
		// `(func())(nil)` (reflect FuncOf's prototype) — renders as its C# delegate type name.
		return convertToCSTypeName(v.getExprTypeName(exprType, false))
	case *ast.Ident:
		context := getExprContext[IdentContext](contexts)
		return v.convIdent(exprType, context)
	case *ast.IndexExpr:
		context := getExprContext[IndexExprContext](contexts)
		return v.convIndexExpr(exprType, context)
	case *ast.IndexListExpr:
		return v.convIndexListExpr(exprType)
	case *ast.KeyValueExpr:
		context := getExprContext[KeyValueContext](contexts)
		return v.convKeyValueExpr(exprType, context)
	case *ast.MapType:
		return v.convMapType(exprType)
	case *ast.ParenExpr:
		context := getExprContext[LambdaContext](contexts)
		return v.convParenExpr(exprType, context)
	case *ast.SelectorExpr:
		context := getExprContext[LambdaContext](contexts)
		return v.convSelectorExpr(exprType, context)
	case *ast.SliceExpr:
		return v.convSliceExpr(exprType)
	case *ast.StarExpr:
		context := getExprContext[StarExprContext](contexts)
		return v.convStarExpr(exprType, context)
	case *ast.TypeAssertExpr:
		return v.convTypeAssertExpr(exprType)
	case *ast.StructType:
		context := getExprContext[IdentContext](contexts)
		return v.convStructType(exprType, context)
	case *ast.InterfaceType:
		context := getExprContext[IdentContext](contexts)
		return v.convInterfaceType(exprType, context)
	case *ast.UnaryExpr:
		context := getExprContext[UnaryExprContext](contexts)

		// Adopt the ambient hoist target (see UnaryExprContext.deferredDecls).
		if context.deferredDecls == nil {
			if lambdaContext := getExprContext[LambdaContext](contexts); lambdaContext.deferredDecls != nil {
				context.deferredDecls = lambdaContext.deferredDecls
			}
		}

		return v.convUnaryExpr(exprType, context)
	case *ast.BadExpr:
		v.showWarning("@convExpr - BadExpr encountered: %#v", exprType)
		return ""
	default:
		panic(fmt.Sprintf("@convExpr - Unexpected Expr type: %#v", v.getPrintedNode(exprType)))
	}
}
