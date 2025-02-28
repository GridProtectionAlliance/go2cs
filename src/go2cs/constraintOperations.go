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
// Only valid C# operators are included here, so "&^" is not included.
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

	results := []string{}

	for _, opSet := range operatorSetKeys {
		var constName string

		switch opSet {
		case SumOperator:
			constName = "Sum"
		case ArithmeticOperators:
			constName = "Arithmetic"
		case IntegerOperators:
			constName = "Integer"
		case ComparableOperators:
			constName = "Comparable"
		case OrderedOperators:
			constName = "Ordered"
		default:
			constName = ""
		}

		if len(constName) > 0 {
			results = append(results, constName)
		}
	}

	return strings.Join(results, ", ")
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

	results := []string{}

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
			}
		case IntegerOperators:
			constraints = []string{
				fmt.Sprintf("IModulusOperators<%s, %s, %s>", name, name, name),
				fmt.Sprintf("IBitwiseOperators<%s, %s, %s>", name, name, name),
				fmt.Sprintf("IShiftOperators<%s, %s, %s>", name, name, name),
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
			results = append(results, constraints...)
		}
	}

	return strings.Join(results, ", ")
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
			constraintTypeSet.Add(Array)
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
