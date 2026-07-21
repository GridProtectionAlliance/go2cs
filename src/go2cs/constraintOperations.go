// constraintOperations.go - Gbtc
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
	"sort"
	"strings"
)

// In the case of generic constraints, restrictions in C# work somewhat differently than in Go. In C# a constraint
// can be a class, an interface and a few special cases. In Go, all constraints are interfaces and can be restricted
// to types, i.e., structs and heap-allocated types alike. Since at the point of the C# code conversion Go will have
// already parsed and validated the code, we can assume that all the type-based constraints will have been satisfied.
// Also, any defined method-set constraints will be handled as normal for existing interface conversion handling.
// The remaining step to be handled is to determine the set of operators that all types in the constraint type-set
// have in common, which is the set of operators that the C# code will need to account for. There are five sets of
// operators to be considered: Sum, Arithmetic, Integer, Comparison and Ordered. See "Operators" section in the Go
// specification for more details: https://go.dev/ref/spec#Operators

/*
	Sum operator:
	+    sum                    integers, floats, complex values, string

	Arithmetic operators:
	-    difference             integers, floats, complex values
	*    product                integers, floats, complex values
	/    quotient               integers, floats, complex values

	Integer operators:
	%    remainder              integers
	&    bitwise AND            integers
	|    bitwise OR             integers
	^    bitwise XOR            integers
	&^   bit clear (AND NOT)    integers
	<<   left shift             integer << integer >= 0
	>>   right shift            integer >> integer >= 0

	Comparison operators:
	==    equal                 [comparable types]
	!=    not equal             [comparable types]

	Ordered operators:
	<     less                  [ordered types]
	<=    less or equal         [ordered types]
	>     greater               [ordered types]
	>=    greater or equal      [ordered types]

	[comparable types]:         bool, integers, floats, complex values, string, pointer, channel, struct, array
	[ordered types]:            integers, floats, string
*/

// ConstraintType represents the possible range of constraint types
// in Go where operators can be applied.
type ConstraintType int

// Comparable types are the widest set of types that can be used with the
// `==` and `!=` operators. Each type in Go that can support these operators
// should be represented by a unique enum value here:

const (
	Invalid ConstraintType = iota
	Bool
	Int
	Int8
	Int16
	Int32
	Int64
	Uint
	Uint8
	Uint16
	Uint32
	Uint64
	Uintptr
	Float32
	Float64
	Complex64
	Complex128
	String
	Pointer
	Array
	Channel
	Struct
	// Map, Slice, Function, etc., types are not comparable, thus do not
	// have common operator sets in a generic type constaint context.
	// Technically, interfaces are comparable, but they represent the
	// type constraint, so also will not have a common operator set.
)

// OperatorSet represents the set of operators that can be applied to
// the types in the constraint type set.
type OperatorSet int

const (
	// SumOperator represents the `+` operator
	SumOperator OperatorSet = iota

	// ArithmeticOperators represents the `-`, `*`, `/` operators
	ArithmeticOperators

	// IntegerOperators represents the `%`, `&`, `|`, `^`, `&^`, `<<`, `>>` operators
	IntegerOperators

	// ComparableOperators represents the `==`, `!=` operators
	ComparableOperators

	// OrderedOperators represents the `<`, `<=`, `>`, `>=` operators
	OrderedOperators
)

// operators is a map of operator sets to the operators string representations.
// Only valid C# operators are defined here, so "&^" is not included.
var operators = map[OperatorSet][]string{
	SumOperator:         {"+"},
	ArithmeticOperators: {"-", "*", "/"},
	IntegerOperators:    {"%", "&", "|", "^", "<<", ">>"},
	ComparableOperators: {"==", "!="},
	OrderedOperators:    {"<", "<=", ">", ">="},
}

// sumOperatorTypes are types that can be used with the sum operator, i.e.,
// `+`, which includes all numeric types plus strings.
var sumOperatorTypes = NewHashSet([]ConstraintType{
	Int, Int8, Int16, Int32, Int64, Uint, Uint8, Uint16, Uint32, Uint64,
	Float32, Float64, Complex64, Complex128, String,
})

// arithmeticOperatorTypes are types that can be used with arithmetic operators,
// i.e.: `-`, `*`, `/`, which includes all numeric types.
var arithmeticOperatorTypes = NewHashSet([]ConstraintType{
	Int, Int8, Int16, Int32, Int64, Uint, Uint8, Uint16, Uint32, Uint64,
	Float32, Float64, Complex64, Complex128,
})

// integerOperatorTypes are types that can be used with integer arithmetic
// operators, i.e.: `%`, `&`, `|`, `^`, `&^`, `<<`, `>>`
var integerOperatorTypes = NewHashSet([]ConstraintType{
	Int, Int8, Int16, Int32, Int64, Uint, Uint8, Uint16, Uint32, Uint64,
})

// comparableOperatorTypes are types that can be compared for equality, i.e.,
// those that support the `==` and `!=` operators. This is the widest set of
// supported operator types.
var comparableOperatorTypes = NewHashSet([]ConstraintType{
	Bool, Int, Int8, Int16, Int32, Int64, Uint, Uint8, Uint16, Uint32, Uint64,
	Float32, Float64, Complex64, Complex128, String, Pointer, Channel, Array, Struct,
})

// orderedOperatorTypes are types that can be ordered, i.e., those that support
// the `<`, `<=`, `>`, `>=` operators. This is a subset of the comparable types.
var orderedOperatorTypes = NewHashSet([]ConstraintType{
	Int, Int8, Int16, Int32, Int64, Uint, Uint8, Uint16, Uint32, Uint64,
	Float32, Float64, String,
})

// getOperatorSet takes a set of constraint types and returns the set of
// operators that can be applied to those types. This is used to determine
// which operators can be used in generic functions and methods.
func getOperatorSet(constraintTypes HashSet[ConstraintType]) HashSet[OperatorSet] {
	operatorSet := HashSet[OperatorSet]{}

	// An empty constraint type set (e.g. a slice or map constraint) supports no
	// lifted operators. Without this guard the empty set would count as a subset
	// of every operator-type set below and incorrectly gain all operators.
	if constraintTypes.IsEmpty() {
		return operatorSet
	}

	if constraintTypes.IsSubsetOfSet(comparableOperatorTypes) {
		operatorSet.Add(ComparableOperators)
	}

	if constraintTypes.IsSubsetOfSet(orderedOperatorTypes) {
		operatorSet.Add(OrderedOperators)
	}

	if constraintTypes.IsSubsetOfSet(arithmeticOperatorTypes) {
		operatorSet.Add(ArithmeticOperators)
	}

	if constraintTypes.IsSubsetOfSet(integerOperatorTypes) {
		operatorSet.Add(IntegerOperators)
	}

	if constraintTypes.IsSubsetOfSet(sumOperatorTypes) {
		operatorSet.Add(SumOperator)
	}

	return operatorSet
}

// getOperatorSetAsString takes a set of operator sets and returns a string
// representation of the operators in those sets.
func getOperatorSetAsString(operatorSets HashSet[OperatorSet]) string {
	operatorSetKeys := operatorSets.Keys()

	sort.Slice(operatorSetKeys, func(i, j int) bool {
		return int(operatorSetKeys[i]) < int(operatorSetKeys[j])
	})

	results := []string{}

	for _, opSet := range operatorSetKeys {
		results = append(results, operators[opSet]...)
	}

	if len(results) == 0 {
		return "none"
	}

	return strings.Join(results, ", ")
}

// getOperatorSetAttributes takes a set of operator sets and returns a string
// representation of the attribute targets of those sets.
func getOperatorSetAttributes(operatorSets HashSet[OperatorSet]) string {
	operatorSetKeys := operatorSets.Keys()

	sort.Slice(operatorSetKeys, func(i, j int) bool {
		return int(operatorSetKeys[i]) < int(operatorSetKeys[j])
	})

	targets := []string{}

	for _, opSet := range operatorSetKeys {
		var setName string

		switch opSet {
		case SumOperator:
			setName = "Sum"
		case ArithmeticOperators:
			setName = "Arithmetic"
		case IntegerOperators:
			setName = "Integer"
		case ComparableOperators:
			setName = "Comparable"
		case OrderedOperators:
			setName = "Ordered"
		default:
			setName = ""
		}

		if len(setName) > 0 {
			targets = append(targets, setName)
		}
	}

	return strings.Join(targets, ", ")
}

// getLiftedConstraints takes a constraint type and its name and returns the
// lifted C# operator constraints for that type.
func (v *Visitor) getLiftedConstraints(typ types.Type, name string) string {
	typeConstraints := v.getConstraintTypeSetFromType(typ)
	operatorSets := getOperatorSet(typeConstraints)

	operatorSetKeys := operatorSets.Keys()

	sort.Slice(operatorSetKeys, func(i, j int) bool {
		return int(operatorSetKeys[i]) < int(operatorSetKeys[j])
	})

	liftedConstraints := []string{}

	for _, opSet := range operatorSetKeys {
		var constraints []string

		switch opSet {
		case SumOperator:
			constraints = []string{
				fmt.Sprintf("IAdditionOperators<%s, %s, %s>", name, name, name),
			}
		case ArithmeticOperators:
			constraints = []string{
				fmt.Sprintf("ISubtractionOperators<%s, %s, %s>", name, name, name),
				fmt.Sprintf("IMultiplyOperators<%s, %s, %s>", name, name, name),
				fmt.Sprintf("IDivisionOperators<%s, %s, %s>", name, name, name),
				// `i++` / `i--` on a type parameter (reflect rangeNum's loop, CS0023) binds
				// these. They live in the numeric-only Arithmetic set — NOT the
				// string-including Sum set (@string implements neither). Mirrors the
				// go2cs-gen InterfaceTypeTemplate "Arithmetic" list — keep the two in sync.
				fmt.Sprintf("IIncrementOperators<%s>", name),
				fmt.Sprintf("IDecrementOperators<%s>", name),
				// Unary `-x` on a type parameter (math/rand/v2's
				// `keep[T int | uint | … | uint64](x T) T { return -x }`, CS0023). Numeric-only,
				// like increment/decrement — @string has no negation. Every .NET primitive numeric
				// satisfies this through INumberBase; a go2cs-gen NAMED numeric type satisfies it
				// because NumericTypeTemplate now emits the operator for unsigned types too (as
				// Go's wrap-around `0 - x`) — keep the three lists in sync.
				fmt.Sprintf("IUnaryNegationOperators<%s, %s>", name, name),
			}
		case IntegerOperators:
			constraints = []string{
				fmt.Sprintf("IModulusOperators<%s, %s, %s>", name, name, name),
				fmt.Sprintf("IBitwiseOperators<%s, %s, %s>", name, name, name),
				// The shift-count type parameter is `int`, matching the BCL: every binary
				// integer implements IShiftOperators<TSelf, int, TSelf> (only `int` itself
				// also satisfies the self-typed shape). The emitted shift form is
				// `x << (int)(k)` (intCastOperand coerces every shift count to int), so this
				// is exactly the shape a generic body requires (strconv bsearch
				// ~uint16|~uint32 — CS0315 on ushort/uint instantiations).
				fmt.Sprintf("IShiftOperators<%s, int, %s>", name, name),
			}
		case ComparableOperators:
			constraints = []string{
				fmt.Sprintf("IEqualityOperators<%s, %s, bool>", name, name),
			}
		case OrderedOperators:
			constraints = []string{
				fmt.Sprintf("IComparisonOperators<%s, %s, bool>", name, name),
			}
		default:
			constraints = []string{}
		}

		if len(constraints) > 0 {
			liftedConstraints = append(liftedConstraints, constraints...)
		}
	}

	return strings.Join(liftedConstraints, ", ")
}

// isTypeConstraint determines if an interface type or type expression represents a type constraint
func (v *Visitor) isTypeConstraint(expr ast.Expr) (bool, int) {
	// Check if we're dealing with an interface type
	if ifaceType, ok := expr.(*ast.InterfaceType); ok {
		// Empty interface{} is not a type constraint
		if len(ifaceType.Methods.List) == 0 {
			return false, 0
		}

		// Check if any method in the interface is an embedded type constraint
		for _, method := range ifaceType.Methods.List {
			// If there's no name, it's an embedded type
			if len(method.Names) == 0 {
				// Check if this embedded type is a constraint
				if ok, count := v.exprIsTypeConstraint(method.Type); ok {
					return true, count
				}
			}
		}

		return false, 0
	}

	// If it's not an interface type, check if it's a constraint directly
	return v.exprIsTypeConstraint(expr)
}

// exprIsTypeConstraint determines if a expression represents a type constraint
func (v *Visitor) exprIsTypeConstraint(expr ast.Expr) (bool, int) {
	switch t := expr.(type) {
	case *ast.UnaryExpr:
		// Check for the ~ operator, an approximate type constraint
		return t.Op == token.TILDE, 0

	case *ast.BinaryExpr:
		// If it's a binary expression with | operator, it's a union type constraint
		return t.Op == token.OR, 0

	case *ast.StarExpr:
		// A pointer type literal (`interface{ *T }`) is a type-set term, not an embeddable
		// interface — without this it rode the embedded-interface path and emitted an interface
		// inheriting the struct ж<T> (CS0527). The declaration keeps the constraint-comment
		// convention; its USE sites erase (see pointerCoreConstraint).
		return true, 0

	case *ast.Ident, *ast.SelectorExpr:
		// For named types, we check if they're type constraints
		// by looking at their type definition
		obj := v.info.TypeOf(expr)

		if obj != nil {
			if ok, count := v.typeIsTypeConstraint(obj); ok {
				return true, count
			}
		}
	}

	return false, 0
}

// isConstraintInterface checks if an interface represents a type constraint
func (v *Visitor) isConstraintInterface(iface *types.Interface) (bool, int) {
	for i := range iface.NumEmbeddeds() {
		if ok, _ := v.typeIsTypeConstraint(iface.EmbeddedType(i)); ok {
			return true, iface.NumMethods()
		}
	}

	return false, 0
}

// typeIsTypeConstraint determines if a type represents a type constraint
func (v *Visitor) typeIsTypeConstraint(typ types.Type) (bool, int) {
	switch t := typ.(type) {
	case *types.Interface:
		return v.isConstraintInterface(t)

	case *types.Union:
		// Union types are always constraints
		return true, 0

	case *types.Named:
		// For a named type, recursively check its underlying type
		return v.typeIsTypeConstraint(t.Underlying())

	case *types.TypeParam:
		// Type parameters themselves aren’t constraints
		return false, 0

	default:
		// Any other concrete type (basic, map, slice, channel, array, etc.)
		// is a valid type literal constraint
		return true, 0
	}
}

// getConstraintTypeSetFromExpr collects all underlying type constraints
func (v *Visitor) getConstraintTypeSetFromExpr(expr ast.Expr) HashSet[ConstraintType] {
	var results []types.Type

	// Helper to process expressions recursively
	var process func(ast.Expr)

	process = func(e ast.Expr) {
		switch t := e.(type) {
		case *ast.UnaryExpr:
			// For ~ operator, we need to process the underlying type
			// meaning "any type whose underlying type is X"
			if t.Op == token.TILDE {
				// Get the type information for the operand
				operandType := v.info.TypeOf(t.X)

				if operandType == nil {
					// Fallback to processing the expression directly
					process(t.X)
				} else {
					results = append(results, operandType)
				}

				return
			}

		case *ast.BinaryExpr:
			if t.Op == token.OR {
				// Process both sides of the OR expression
				process(t.X)
				process(t.Y)
				return
			}

		case *ast.Ident:
			obj := v.info.Uses[t]
			if typeName, ok := obj.(*types.TypeName); ok {
				typ := typeName.Type()
				underlying := typ.Underlying()

				// Check for composite types directly
				switch underlying.(type) {
				case *types.Pointer, *types.Array, *types.Chan, *types.Struct:
					// These are valid constraint types, add them directly
					results = append(results, typ)
					return
				case *types.Basic:
					// It's a basic type, add it directly
					results = append(results, typ)
					return
				}

				if iface, isIface := underlying.(*types.Interface); isIface && !iface.Empty() {
					// It's an interface type set, so recursively extract its embedded constraints
					results = append(results, v.getConstraintsFromType(typ)...)
				}
			}

		case *ast.SelectorExpr:
			sel := v.info.Uses[t.Sel]
			if typeName, ok := sel.(*types.TypeName); ok {
				typ := typeName.Type()
				underlying := typ.Underlying()

				// Check if it's a composite type
				switch underlying.(type) {
				case *types.Pointer, *types.Array, *types.Chan, *types.Struct:
					// Composite types are valid constraints
					results = append(results, typ)
				default:
					results = append(results, typ)
				}
			}

		case *ast.InterfaceType:
			for _, method := range t.Methods.List {
				if len(method.Names) == 0 {
					// It's an embedded type
					process(method.Type)
				}
			}

		// Handle composite types in AST directly
		case *ast.StarExpr, *ast.ArrayType, *ast.ChanType, *ast.StructType:
			// If we encounter one of these directly in the AST, try to get its type
			if typeInfo := v.info.TypeOf(t); typeInfo != nil {
				results = append(results, typeInfo)
			}
		default:
			results = append(results, types.Default(nil))
		}
	}

	process(expr)

	// Convert the type results to a HashSet of ConstraintType
	return getConstraintTypeSet(results)
}

// getConstraintsFromType recursively collects the concrete underlying types from a given types.Type,
// ensuring that union types (and similar constructs) are fully expanded.
func (v *Visitor) getConstraintsFromType(typ types.Type) []types.Type {
	var constraints []types.Type

	switch t := typ.(type) {
	case *types.Named:
		// Check if the underlying type is a composite type
		underlying := t.Underlying()
		switch underlying.(type) {
		case *types.Pointer, *types.Array, *types.Chan, *types.Struct:
			// Composite types are valid constraints, add the named type directly
			constraints = append(constraints, t)
		default:
			// For other named types, continue recursing into the underlying type
			constraints = append(constraints, v.getConstraintsFromType(underlying)...)
		}

	case *types.Interface:
		// If it's an interface type set, iterate over embedded types.
		// This covers the case where the interface inherits constraints.
		if !t.Empty() {
			for i := range t.NumEmbeddeds() {
				embedded := t.EmbeddedType(i)
				constraints = append(constraints, v.getConstraintsFromType(embedded)...)
			}
		} else {
			// Otherwise, treat the interface itself as a constraint.
			constraints = append(constraints, t)
		}

	case *types.Union:
		// Instead of returning the union type directly, recursively extract its terms.
		for i := range t.Len() {
			term := t.Term(i) // Each term has a .Type field.
			constraints = append(constraints, v.getConstraintsFromType(term.Type())...)
		}

	case *types.Pointer, *types.Array, *types.Chan, *types.Struct:
		// These composite types are valid constraints, add them directly
		constraints = append(constraints, t)

	default:
		// For any other concrete type (basic, etc.), add it directly.
		constraints = append(constraints, t)
	}

	return constraints
}

func (v *Visitor) getConstraintTypeSetFromType(typ types.Type) HashSet[ConstraintType] {
	return getConstraintTypeSet(v.getConstraintsFromType(typ))
}

func getConstraintTypeSet(constraintTypes []types.Type) HashSet[ConstraintType] {
	// Convert the type results to a HashSet of ConstraintType
	constraintTypeSet := HashSet[ConstraintType]{}

	for _, typ := range constraintTypes {
		switch t := typ.(type) {
		case *types.Basic:
			switch t.Kind() {
			case types.Bool:
				constraintTypeSet.Add(Bool)
			case types.Int:
				constraintTypeSet.Add(Int)
			case types.Int8:
				constraintTypeSet.Add(Int8)
			case types.Int16:
				constraintTypeSet.Add(Int16)
			case types.Int32:
				constraintTypeSet.Add(Int32)
			case types.Int64:
				constraintTypeSet.Add(Int64)
			case types.Uint:
				constraintTypeSet.Add(Uint)
			case types.Uint8:
				constraintTypeSet.Add(Uint8)
			case types.Uint16:
				constraintTypeSet.Add(Uint16)
			case types.Uint32:
				constraintTypeSet.Add(Uint32)
			case types.Uint64:
				constraintTypeSet.Add(Uint64)
			case types.Float32:
				constraintTypeSet.Add(Float32)
			case types.Float64:
				constraintTypeSet.Add(Float64)
			case types.Complex64:
				constraintTypeSet.Add(Complex64)
			case types.Complex128:
				constraintTypeSet.Add(Complex128)
			case types.String:
				constraintTypeSet.Add(String)
			}
		case *types.Pointer:
			constraintTypeSet.Add(Pointer)
		case *types.Array:
			// Arrays are comparable in Go when their element type is comparable.
			constraintTypeSet.Add(Array)
		case *types.Slice:
			// Slices are not comparable in Go (only against nil), so they
			// contribute no operator-bearing constraint type.
		case *types.Chan:
			constraintTypeSet.Add(Channel)
		case *types.Struct:
			constraintTypeSet.Add(Struct)
		case *types.Named:
			// For named types, check it they are structs
			if _, ok := t.Underlying().(*types.Struct); ok {
				constraintTypeSet.Add(Struct)
			}
		default:
			constraintTypeSet.Add(Invalid)
		}
	}

	return constraintTypeSet
}

// pointerCoreConstraint reports whether the type parameter's constraint type-set is a single,
// non-tilde pointer term (`[P *T]`), returning the pointer type. Such a term's type set is a
// singleton — P is definitionally its pointer type at every instantiation (Go spec, "Interface
// types") — so the emission ERASES P: it is dropped from the C# generic parameter list and every
// occurrence renders as the pointer type itself (`ж<T>`), letting the normal pointer machinery
// (deref alias, escape heap box, argument passing) apply unchanged. Approximate (`~*T`), union,
// and method-carrying pointer constraints are declined (zero stdlib occurrences; callers keep
// the current emission and warn): a named pointer type emits as a `[GoType("ж<E>")]` wrapper
// class, which is not identity with `ж<E>`. See DESIGN-pointer-core-typeparam.md.
func pointerCoreConstraint(typeParam *types.TypeParam) (*types.Pointer, bool) {
	if typeParam == nil {
		return nil, false
	}

	iface, ok := typeParam.Constraint().Underlying().(*types.Interface)

	if !ok {
		return nil, false
	}

	return interfacePointerTerm(iface, 0)
}

// interfacePointerTerm reports whether the interface's type-set is a single non-tilde pointer
// term, unwrapping a NAMED (or aliased) constraint interface embedded inside another —
// `interface{ PtrOf[X] }` must resolve identically to the direct `PtrOf[X]` spelling (both name
// the same singleton type set). The depth cap guards degenerate self-referential constraint
// shapes; real constraints nest one or two levels.
func interfacePointerTerm(iface *types.Interface, depth int) (*types.Pointer, bool) {
	if depth > 8 || iface.NumMethods() > 0 || iface.NumEmbeddeds() != 1 {
		return nil, false
	}

	// The single embedded element is the term type directly, a one-term union (go/types wraps
	// explicit terms in a Union; both shapes appear in practice), or a named constraint interface.
	switch embedded := iface.EmbeddedType(0).(type) {
	case *types.Union:
		if embedded.Len() != 1 || embedded.Term(0).Tilde() {
			return nil, false
		}

		pointer, ok := embedded.Term(0).Type().(*types.Pointer)
		return pointer, ok

	case *types.Pointer:
		return embedded, true

	default:
		if nested, ok := embedded.Underlying().(*types.Interface); ok {
			return interfacePointerTerm(nested, depth+1)
		}
	}

	return nil, false
}

// constraintHasPointerTerm reports whether the type parameter's constraint type-set mentions any
// pointer term at all. Used only to WARN about the pointer-core shapes pointerCoreConstraint
// declines to erase (approximate `~*T`, unions, method-carrying interfaces) — those keep the
// operator-lift fallback emission, which cannot express pointer semantics on the parameter.
func constraintHasPointerTerm(typeParam *types.TypeParam) bool {
	iface, ok := typeParam.Constraint().Underlying().(*types.Interface)

	if !ok {
		return false
	}

	return interfaceHasPointerTerm(iface, 0)
}

func interfaceHasPointerTerm(iface *types.Interface, depth int) bool {
	if depth > 8 {
		return false
	}

	for i := range iface.NumEmbeddeds() {
		switch embedded := iface.EmbeddedType(i).(type) {
		case *types.Union:
			for j := range embedded.Len() {
				if _, ok := embedded.Term(j).Type().(*types.Pointer); ok {
					return true
				}
			}
		case *types.Pointer:
			return true
		default:
			if nested, ok := embedded.Underlying().(*types.Interface); ok && interfaceHasPointerTerm(nested, depth+1) {
				return true
			}
		}
	}

	return false
}

// collectErasedTypeParams returns the pointer-core (erased) type parameters of a FUNCTION's own
// type-parameter list, keyed by identity — nil when none. Populated into v.erasedTypeParams at
// visitFuncDecl entry: erasure is strictly a property of a plain function declaration (a generic
// NAMED type's parameters — including a method's receiver type parameters — are never erased, so
// every consumer gates on this identity set rather than re-deriving from the constraint alone,
// which would half-erase declined shapes).
func collectErasedTypeParams(signature *types.Signature) map[*types.TypeParam]*types.Pointer {
	if signature == nil {
		return nil
	}

	typeParams := signature.TypeParams()

	if typeParams == nil {
		return nil
	}

	var erased map[*types.TypeParam]*types.Pointer

	for i := range typeParams.Len() {
		typeParam := typeParams.At(i)

		if pointer, ok := pointerCoreConstraint(typeParam); ok {
			if erased == nil {
				erased = map[*types.TypeParam]*types.Pointer{}
			}

			erased[typeParam] = pointer
		}
	}

	return erased
}

// typeParamErased reports whether the type parameter is ERASED in the current emission context —
// i.e. it belongs to the function declaration being emitted and its constraint is a single
// non-tilde pointer term. Identity-keyed, so a same-named parameter of some other declaration
// (a generic named type, a named func type, another function) never matches.
func (v *Visitor) typeParamErased(typeParam *types.TypeParam) (*types.Pointer, bool) {
	pointer, ok := v.erasedTypeParams[typeParam]
	return pointer, ok
}

// paramPointerType resolves a type's pointer form for parameter/ident classification: a
// *types.Pointer directly, or the erased pointer of a pointer-core type parameter of the CURRENT
// function (see typeParamErased) — so a `p P` parameter under `[P *T]` takes the same deref-alias
// and box (`Ꮡ`) conventions as a plain `p *T` parameter.
func (v *Visitor) paramPointerType(t types.Type) (*types.Pointer, bool) {
	if pointer, ok := t.(*types.Pointer); ok {
		return pointer, true
	}

	if typeParam, ok := types.Unalias(t).(*types.TypeParam); ok {
		return v.typeParamErased(typeParam)
	}

	return nil, false
}

// signatureErasedParamPointer reports the erased pointer behind a CALLEE's declared parameter
// type: a pointer-core type parameter counts only when the signature ITSELF declares it (its own
// TypeParams list — a method's receiver type parameters are never erased, so an argument landing
// in such a slot keeps the plain render).
func signatureErasedParamPointer(sig *types.Signature, t types.Type) (*types.Pointer, bool) {
	typeParam, ok := types.Unalias(t).(*types.TypeParam)

	if !ok || sig == nil {
		return nil, false
	}

	typeParams := sig.TypeParams()

	if typeParams == nil {
		return nil, false
	}

	for i := range typeParams.Len() {
		if typeParams.At(i) == typeParam {
			return pointerCoreConstraint(typeParam)
		}
	}

	return nil, false
}

// signatureErasedParamPointerOk is the boolean form of signatureErasedParamPointer for use in
// compound conditions.
func signatureErasedParamPointerOk(sig *types.Signature, t types.Type) bool {
	_, ok := signatureErasedParamPointer(sig, t)
	return ok
}

// typeIsErasedPointerCore reports whether t is an ERASED pointer-core type parameter of the
// current function (the boolean form of typeParamErased over a types.Type).
func (v *Visitor) typeIsErasedPointerCore(t types.Type) bool {
	typeParam, ok := types.Unalias(t).(*types.TypeParam)

	if !ok {
		return false
	}

	_, erased := v.typeParamErased(typeParam)
	return erased
}

// explicitTypeArgsAfterErasure filters an EXPLICIT Go instantiation's written type-argument
// expressions (`clone[*thing, thing]`, `setThrough[*int](…)` — including partial lists), removing
// positions whose declared callee type parameter is erased (pointer-core): those positions no
// longer exist in the emitted C# generic parameter list, so rendering them verbatim is an arity
// mismatch (CS0305) or a mis-bound argument (CS1503 on a partial list). A non-function target, an
// unresolvable base, or a callee with nothing erased returns the original slice unchanged (false)
// — those paths stay byte-identical.
func (v *Visitor) explicitTypeArgsAfterErasure(x ast.Expr, indices []ast.Expr) ([]ast.Expr, bool) {
	var funIdent *ast.Ident

	switch e := x.(type) {
	case *ast.Ident:
		funIdent = e
	case *ast.SelectorExpr:
		funIdent = e.Sel
	}

	if funIdent == nil {
		return indices, false
	}

	funcObj, ok := v.info.ObjectOf(funIdent).(*types.Func)

	if !ok {
		return indices, false
	}

	sig, ok := funcObj.Type().(*types.Signature)

	if !ok || sig.TypeParams() == nil {
		return indices, false
	}

	typeParams := sig.TypeParams()
	kept := make([]ast.Expr, 0, len(indices))
	erasedAny := false

	for i, index := range indices {
		if i < typeParams.Len() {
			if _, erased := pointerCoreConstraint(typeParams.At(i)); erased {
				erasedAny = true
				continue
			}
		}

		kept = append(kept, index)
	}

	if !erasedAny {
		return indices, false
	}

	return kept, true
}

// renderedTypeArgs renders an instantiation's type arguments for emission, skipping positions
// whose declared type parameter is erased (pointer-core, see pointerCoreConstraint) — those no
// longer exist in the emitted C# generic parameter list. funIdent resolves the callee's declared
// type-parameter list; the result is always non-nil — an EMPTY list means every position was
// erased (emit no `<...>`).
func (v *Visitor) renderedTypeArgs(funIdent *ast.Ident, typeArgs *types.TypeList) []string {
	var typeParams *types.TypeParamList

	if funcObj, ok := v.info.ObjectOf(funIdent).(*types.Func); ok {
		if sig, ok := funcObj.Type().(*types.Signature); ok {
			typeParams = sig.TypeParams()
		}
	}

	names := make([]string, 0, typeArgs.Len())

	for i := range typeArgs.Len() {
		if typeParams != nil && i < typeParams.Len() {
			if _, erased := pointerCoreConstraint(typeParams.At(i)); erased {
				continue
			}
		}

		names = append(names, v.getCSTypeName(typeArgs.At(i)))
	}

	return names
}

func (v *Visitor) getConstraintType(typeConstraint *types.TypeParam) types.Type {
	if typeConstraint == nil {
		return nil
	}

	// Get the constraint
	constraint := typeConstraint.Constraint()

	// The constraint is typically an interface type
	iface, ok := constraint.Underlying().(*types.Interface)

	if !ok {
		return nil
	}

	typeConstraints := v.getConstraintsFromType(iface)

	if len(typeConstraints) > 0 {
		return typeConstraints[0]
	}

	return nil
}

// getArrayConstraintElem reports whether the constraint interface's type-set is a single array
// core (`~[N]E` / `[N]E`, e.g. ML-KEM's `~[256]fieldElement`), returning the shared element type.
// A named-array `[GoType]` wrapper (ringElement, nttElement) implements golib's IArray<E>, so such
// a constraint maps to `where T : IArray<E>` — which exposes the array surface (indexing, length,
// `(nint, E)` ranging) the generic body needs — rather than the spurious IEqualityOperators<T,T,bool>
// the Array member of the comparable operator set would otherwise lift (CS0315: the array-wrapper
// struct cannot satisfy it, and it exposes no indexer/enumerator for the body's `t[i]`/`range t`).
func (v *Visitor) getArrayConstraintElem(iface *types.Interface) (types.Type, bool) {
	if iface == nil {
		return nil, false
	}

	constraints := v.getConstraintsFromType(iface)

	if len(constraints) == 0 {
		return nil, false
	}

	var elem types.Type

	for _, constraint := range constraints {
		array, ok := constraint.Underlying().(*types.Array)

		if !ok {
			return nil, false
		}

		if elem == nil {
			elem = array.Elem()
		} else if !types.Identical(elem, array.Elem()) {
			// A union of array cores with differing element types has no single IArray<E>.
			return nil, false
		}
	}

	return elem, true
}
