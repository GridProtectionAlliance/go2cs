package main

import (
	"go/ast"
	"go/token"
	"go/types"
)

// markUntypedConstContexts records the CONTEXTUAL type of every untyped constant subexpression
// in the file. go/types resolves a constant expression's context (`var c complex64 = complex(2.5,
// -3.5)`) on the OUTERMOST expression only — updateExprType deliberately does not descend into
// constant operands (they never materialize at runtime in Go), so the inner literals stay recorded
// as `untyped float`/`untyped complex` and later render from their untyped DEFAULT (float64 →
// `2.5D`, no implicit conversion to float32/complex64 → CS0266/CS0019). The emitted C# preserves
// the operand structure (`complex(2.5F, -3.5F)`, `2.5F - 3.5F.i()`), so each literal needs
// the context go/types dropped: propagate a typed constant expression's resolved type down through
// the constant shapes — parens, unary `+`/`-`/`^`, arithmetic binary operands, and the complex/
// real/imag/min/max builtin arguments — into untypedConstContexts, which convBasicLit consults for
// the float32-vs-float64 (`F`/`D`) and complex64-vs-complex128 suffix decisions.
func (v *Visitor) markUntypedConstContexts(file *ast.File) {
	if v.untypedConstContexts == nil {
		v.untypedConstContexts = map[ast.Expr]types.Type{}
	}

	ast.Inspect(file, func(n ast.Node) bool {
		expr, ok := n.(ast.Expr)

		if !ok {
			return true
		}

		tv, ok := v.info.Types[expr]

		if !ok || tv.Value == nil || tv.Type == nil {
			return true
		}

		// Only a TYPED constant expression supplies context; an untyped one gets ITS context
		// from its own enclosing expression (assigned by the recursion below, not this trigger)
		if basic, ok := tv.Type.Underlying().(*types.Basic); ok && basic.Info()&types.IsUntyped == 0 {
			v.propagateUntypedConstContext(expr, tv.Type)
		}

		return true
	})
}

// propagateUntypedConstContext pushes a resolved constant context type into the untyped constant
// children of expr, per expr's shape. Only float/complex contexts propagate — those are the kinds
// whose C# literal rendering is context-sensitive (`F`/`D` suffixes, builtin.i overload choice).
func (v *Visitor) propagateUntypedConstContext(expr ast.Expr, context types.Type) {
	basic, ok := context.Underlying().(*types.Basic)

	if !ok || basic.Info()&(types.IsFloat|types.IsComplex) == 0 {
		return
	}

	switch e := expr.(type) {
	case *ast.ParenExpr:
		v.assignUntypedConstContext(e.X, context)
	case *ast.UnaryExpr:
		switch e.Op {
		case token.ADD, token.SUB, token.XOR:
			v.assignUntypedConstContext(e.X, context)
		}
	case *ast.BinaryExpr:
		// Arithmetic operands adopt the result type; shifts, comparisons, and the
		// integer-only operators never carry a float/complex context
		switch e.Op {
		case token.ADD, token.SUB, token.MUL, token.QUO:
			v.assignUntypedConstContext(e.X, context)
			v.assignUntypedConstContext(e.Y, context)
		}
	case *ast.CallExpr:
		ident, ok := e.Fun.(*ast.Ident)

		if !ok {
			return
		}

		if _, isBuiltin := v.info.ObjectOf(ident).(*types.Builtin); !isBuiltin {
			return
		}

		var argContext types.Type

		switch ident.Name {
		case "complex":
			// The arguments are the ELEMENT type of the resulting complex type
			switch basic.Kind() {
			case types.Complex64:
				argContext = types.Typ[types.Float32]
			case types.Complex128:
				argContext = types.Typ[types.Float64]
			}
		case "real", "imag":
			// The argument is the complex COUNTERPART of the resulting float type
			switch basic.Kind() {
			case types.Float32:
				argContext = types.Typ[types.Complex64]
			case types.Float64:
				argContext = types.Typ[types.Complex128]
			}
		case "min", "max":
			argContext = context
		}

		if argContext == nil {
			return
		}

		for _, arg := range e.Args {
			v.assignUntypedConstContext(arg, argContext)
		}
	}
}

// assignUntypedConstContext records the context for one untyped constant subexpression and
// recurses into its children. An already-typed child keeps the type go/types recorded for it
// (context propagation stops there — that type IS its context).
func (v *Visitor) assignUntypedConstContext(expr ast.Expr, context types.Type) {
	tv, ok := v.info.Types[expr]

	if !ok || tv.Value == nil || tv.Type == nil {
		return
	}

	if basic, ok := tv.Type.(*types.Basic); !ok || basic.Info()&types.IsUntyped == 0 {
		return
	}

	v.untypedConstContexts[expr] = context
	v.propagateUntypedConstContext(expr, context)
}

// untypedConstContext returns the propagated contextual type of an untyped constant
// subexpression as its basic underlying type, or nil when none was recorded.
func (v *Visitor) untypedConstContext(expr ast.Expr) *types.Basic {
	if context, ok := v.untypedConstContexts[expr]; ok {
		if basic, ok := context.Underlying().(*types.Basic); ok {
			return basic
		}
	}

	return nil
}
