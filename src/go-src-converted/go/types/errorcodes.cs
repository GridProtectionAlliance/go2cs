// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2022 March 13 05:52:56 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Program Files\Go\src\go\types\errorcodes.go
namespace go.go;

public static partial class types_package {

private partial struct errorCode { // : nint
}

// This file defines the error codes that can be produced during type-checking.
// Collectively, these codes provide an identifier that may be used to
// implement special handling for certain types of errors.
//
// Error code values should not be changed: add new codes at the end.
//
// Error codes should be fine-grained enough that the exact nature of the error
// can be easily determined, but coarse enough that they are not an
// implementation detail of the type checking algorithm. As a rule-of-thumb,
// errors should be considered equivalent if there is a theoretical refactoring
// of the type checker in which they are emitted in exactly one place. For
// example, the type checker emits different error messages for "too many
// arguments" and "too few arguments", but one can imagine an alternative type
// checker where this check instead just emits a single "wrong number of
// arguments", so these errors should have the same code.
//
// Error code names should be as brief as possible while retaining accuracy and
// distinctiveness. In most cases names should start with an adjective
// describing the nature of the error (e.g. "invalid", "unused", "misplaced"),
// and end with a noun identifying the relevant language object. For example,
// "_DuplicateDecl" or "_InvalidSliceExpr". For brevity, naming follows the
// convention that "bad" implies a problem with syntax, and "invalid" implies a
// problem with types.

private static readonly errorCode _ = iota; 

// _Test is reserved for errors that only apply while in self-test mode.
private static readonly var _Test = 0; 

// _BlankPkgName occurs when a package name is the blank identifier "_".
//
// Per the spec:
//  "The PackageName must not be the blank identifier."
private static readonly var _BlankPkgName = 1; 

// _MismatchedPkgName occurs when a file's package name doesn't match the
// package name already established by other files.
private static readonly var _MismatchedPkgName = 2; 

// _InvalidPkgUse occurs when a package identifier is used outside of a
// selector expression.
//
// Example:
//  import "fmt"
//
//  var _ = fmt
private static readonly var _InvalidPkgUse = 3; 

// _BadImportPath occurs when an import path is not valid.
private static readonly var _BadImportPath = 4; 

// _BrokenImport occurs when importing a package fails.
//
// Example:
//  import "amissingpackage"
private static readonly var _BrokenImport = 5; 

// _ImportCRenamed occurs when the special import "C" is renamed. "C" is a
// pseudo-package, and must not be renamed.
//
// Example:
//  import _ "C"
private static readonly var _ImportCRenamed = 6; 

// _UnusedImport occurs when an import is unused.
//
// Example:
//  import "fmt"
//
//  func main() {}
private static readonly var _UnusedImport = 7; 

// _InvalidInitCycle occurs when an invalid cycle is detected within the
// initialization graph.
//
// Example:
//  var x int = f()
//
//  func f() int { return x }
private static readonly var _InvalidInitCycle = 8; 

// _DuplicateDecl occurs when an identifier is declared multiple times.
//
// Example:
//  var x = 1
//  var x = 2
private static readonly var _DuplicateDecl = 9; 

// _InvalidDeclCycle occurs when a declaration cycle is not valid.
//
// Example:
//  import "unsafe"
//
//  type T struct {
//      a [n]int
//  }
//
//  var n = unsafe.Sizeof(T{})
private static readonly var _InvalidDeclCycle = 10; 

// _InvalidTypeCycle occurs when a cycle in type definitions results in a
// type that is not well-defined.
//
// Example:
//  import "unsafe"
//
//  type T [unsafe.Sizeof(T{})]int
private static readonly var _InvalidTypeCycle = 11; 

// _InvalidConstInit occurs when a const declaration has a non-constant
// initializer.
//
// Example:
//  var x int
//  const _ = x
private static readonly var _InvalidConstInit = 12; 

// _InvalidConstVal occurs when a const value cannot be converted to its
// target type.
//
// TODO(findleyr): this error code and example are not very clear. Consider
// removing it.
//
// Example:
//  const _ = 1 << "hello"
private static readonly var _InvalidConstVal = 13; 

// _InvalidConstType occurs when the underlying type in a const declaration
// is not a valid constant type.
//
// Example:
//  const c *int = 4
private static readonly var _InvalidConstType = 14; 

// _UntypedNil occurs when the predeclared (untyped) value nil is used to
// initialize a variable declared without an explicit type.
//
// Example:
//  var x = nil
private static readonly var _UntypedNil = 15; 

// _WrongAssignCount occurs when the number of values on the right-hand side
// of an assignment or initialization expression does not match the number
// of variables on the left-hand side.
//
// Example:
//  var x = 1, 2
private static readonly var _WrongAssignCount = 16; 

// _UnassignableOperand occurs when the left-hand side of an assignment is
// not assignable.
//
// Example:
//  func f() {
//      const c = 1
//      c = 2
//  }
private static readonly var _UnassignableOperand = 17; 

// _NoNewVar occurs when a short variable declaration (':=') does not declare
// new variables.
//
// Example:
//  func f() {
//      x := 1
//      x := 2
//  }
private static readonly var _NoNewVar = 18; 

// _MultiValAssignOp occurs when an assignment operation (+=, *=, etc) does
// not have single-valued left-hand or right-hand side.
//
// Per the spec:
//  "In assignment operations, both the left- and right-hand expression lists
//  must contain exactly one single-valued expression"
//
// Example:
//  func f() int {
//      x, y := 1, 2
//      x, y += 1
//      return x + y
//  }
private static readonly var _MultiValAssignOp = 19; 

// _InvalidIfaceAssign occurs when a value of type T is used as an
// interface, but T does not implement a method of the expected interface.
//
// Example:
//  type I interface {
//      f()
//  }
//
//  type T int
//
//  var x I = T(1)
private static readonly var _InvalidIfaceAssign = 20; 

// _InvalidChanAssign occurs when a chan assignment is invalid.
//
// Per the spec, a value x is assignable to a channel type T if:
//  "x is a bidirectional channel value, T is a channel type, x's type V and
//  T have identical element types, and at least one of V or T is not a
//  defined type."
//
// Example:
//  type T1 chan int
//  type T2 chan int
//
//  var x T1
//  // Invalid assignment because both types are named
//  var _ T2 = x
private static readonly var _InvalidChanAssign = 21; 

// _IncompatibleAssign occurs when the type of the right-hand side expression
// in an assignment cannot be assigned to the type of the variable being
// assigned.
//
// Example:
//  var x []int
//  var _ int = x
private static readonly var _IncompatibleAssign = 22; 

// _UnaddressableFieldAssign occurs when trying to assign to a struct field
// in a map value.
//
// Example:
//  func f() {
//      m := make(map[string]struct{i int})
//      m["foo"].i = 42
//  }
private static readonly var _UnaddressableFieldAssign = 23; 

// _NotAType occurs when the identifier used as the underlying type in a type
// declaration or the right-hand side of a type alias does not denote a type.
//
// Example:
//  var S = 2
//
//  type T S
private static readonly var _NotAType = 24; 

// _InvalidArrayLen occurs when an array length is not a constant value.
//
// Example:
//  var n = 3
//  var _ = [n]int{}
private static readonly var _InvalidArrayLen = 25; 

// _BlankIfaceMethod occurs when a method name is '_'.
//
// Per the spec:
//  "The name of each explicitly specified method must be unique and not
//  blank."
//
// Example:
//  type T interface {
//      _(int)
//  }
private static readonly var _BlankIfaceMethod = 26; 

// _IncomparableMapKey occurs when a map key type does not support the == and
// != operators.
//
// Per the spec:
//  "The comparison operators == and != must be fully defined for operands of
//  the key type; thus the key type must not be a function, map, or slice."
//
// Example:
//  var x map[T]int
//
//  type T []int
private static readonly var _IncomparableMapKey = 27; 

// _InvalidIfaceEmbed occurs when a non-interface type is embedded in an
// interface.
//
// Example:
//  type T struct {}
//
//  func (T) m()
//
//  type I interface {
//      T
//  }
private static readonly var _InvalidIfaceEmbed = 28; 

// _InvalidPtrEmbed occurs when an embedded field is of the pointer form *T,
// and T itself is itself a pointer, an unsafe.Pointer, or an interface.
//
// Per the spec:
//  "An embedded field must be specified as a type name T or as a pointer to
//  a non-interface type name *T, and T itself may not be a pointer type."
//
// Example:
//  type T *int
//
//  type S struct {
//      *T
//  }
private static readonly var _InvalidPtrEmbed = 29; 

// _BadRecv occurs when a method declaration does not have exactly one
// receiver parameter.
//
// Example:
//  func () _() {}
private static readonly var _BadRecv = 30; 

// _InvalidRecv occurs when a receiver type expression is not of the form T
// or *T, or T is a pointer type.
//
// Example:
//  type T struct {}
//
//  func (**T) m() {}
private static readonly var _InvalidRecv = 31; 

// _DuplicateFieldAndMethod occurs when an identifier appears as both a field
// and method name.
//
// Example:
//  type T struct {
//      m int
//  }
//
//  func (T) m() {}
private static readonly var _DuplicateFieldAndMethod = 32; 

// _DuplicateMethod occurs when two methods on the same receiver type have
// the same name.
//
// Example:
//  type T struct {}
//  func (T) m() {}
//  func (T) m(i int) int { return i }
private static readonly var _DuplicateMethod = 33; 

// _InvalidBlank occurs when a blank identifier is used as a value or type.
//
// Per the spec:
//  "The blank identifier may appear as an operand only on the left-hand side
//  of an assignment."
//
// Example:
//  var x = _
private static readonly var _InvalidBlank = 34; 

// _InvalidIota occurs when the predeclared identifier iota is used outside
// of a constant declaration.
//
// Example:
//  var x = iota
private static readonly var _InvalidIota = 35; 

// _MissingInitBody occurs when an init function is missing its body.
//
// Example:
//  func init()
private static readonly var _MissingInitBody = 36; 

// _InvalidInitSig occurs when an init function declares parameters or
// results.
//
// Deprecated: no longer emitted by the type checker. _InvalidInitDecl is
// used instead.
private static readonly var _InvalidInitSig = 37; 

// _InvalidInitDecl occurs when init is declared as anything other than a
// function.
//
// Example:
//  var init = 1
//
// Example:
//  func init() int { return 1 }
private static readonly var _InvalidInitDecl = 38; 

// _InvalidMainDecl occurs when main is declared as anything other than a
// function, in a main package.
private static readonly var _InvalidMainDecl = 39; 

// _TooManyValues occurs when a function returns too many values for the
// expression context in which it is used.
//
// Example:
//  func ReturnTwo() (int, int) {
//      return 1, 2
//  }
//
//  var x = ReturnTwo()
private static readonly var _TooManyValues = 40; 

// _NotAnExpr occurs when a type expression is used where a value expression
// is expected.
//
// Example:
//  type T struct {}
//
//  func f() {
//      T
//  }
private static readonly var _NotAnExpr = 41; 

// _TruncatedFloat occurs when a float constant is truncated to an integer
// value.
//
// Example:
//  var _ int = 98.6
private static readonly var _TruncatedFloat = 42; 

// _NumericOverflow occurs when a numeric constant overflows its target type.
//
// Example:
//  var x int8 = 1000
private static readonly var _NumericOverflow = 43; 

// _UndefinedOp occurs when an operator is not defined for the type(s) used
// in an operation.
//
// Example:
//  var c = "a" - "b"
private static readonly var _UndefinedOp = 44; 

// _MismatchedTypes occurs when operand types are incompatible in a binary
// operation.
//
// Example:
//  var a = "hello"
//  var b = 1
//  var c = a - b
private static readonly var _MismatchedTypes = 45; 

// _DivByZero occurs when a division operation is provable at compile
// time to be a division by zero.
//
// Example:
//  const divisor = 0
//  var x int = 1/divisor
private static readonly var _DivByZero = 46; 

// _NonNumericIncDec occurs when an increment or decrement operator is
// applied to a non-numeric value.
//
// Example:
//  func f() {
//      var c = "c"
//      c++
//  }
private static readonly var _NonNumericIncDec = 47; 

// _UnaddressableOperand occurs when the & operator is applied to an
// unaddressable expression.
//
// Example:
//  var x = &1
private static readonly var _UnaddressableOperand = 48; 

// _InvalidIndirection occurs when a non-pointer value is indirected via the
// '*' operator.
//
// Example:
//  var x int
//  var y = *x
private static readonly var _InvalidIndirection = 49; 

// _NonIndexableOperand occurs when an index operation is applied to a value
// that cannot be indexed.
//
// Example:
//  var x = 1
//  var y = x[1]
private static readonly var _NonIndexableOperand = 50; 

// _InvalidIndex occurs when an index argument is not of integer type,
// negative, or out-of-bounds.
//
// Example:
//  var s = [...]int{1,2,3}
//  var x = s[5]
//
// Example:
//  var s = []int{1,2,3}
//  var _ = s[-1]
//
// Example:
//  var s = []int{1,2,3}
//  var i string
//  var _ = s[i]
private static readonly var _InvalidIndex = 51; 

// _SwappedSliceIndices occurs when constant indices in a slice expression
// are decreasing in value.
//
// Example:
//  var _ = []int{1,2,3}[2:1]
private static readonly var _SwappedSliceIndices = 52; 

// _NonSliceableOperand occurs when a slice operation is applied to a value
// whose type is not sliceable, or is unaddressable.
//
// Example:
//  var x = [...]int{1, 2, 3}[:1]
//
// Example:
//  var x = 1
//  var y = 1[:1]
private static readonly var _NonSliceableOperand = 53; 

// _InvalidSliceExpr occurs when a three-index slice expression (a[x:y:z]) is
// applied to a string.
//
// Example:
//  var s = "hello"
//  var x = s[1:2:3]
private static readonly var _InvalidSliceExpr = 54; 

// _InvalidShiftCount occurs when the right-hand side of a shift operation is
// either non-integer, negative, or too large.
//
// Example:
//  var (
//      x string
//      y int = 1 << x
//  )
private static readonly var _InvalidShiftCount = 55; 

// _InvalidShiftOperand occurs when the shifted operand is not an integer.
//
// Example:
//  var s = "hello"
//  var x = s << 2
private static readonly var _InvalidShiftOperand = 56; 

// _InvalidReceive occurs when there is a channel receive from a value that
// is either not a channel, or is a send-only channel.
//
// Example:
//  func f() {
//      var x = 1
//      <-x
//  }
private static readonly var _InvalidReceive = 57; 

// _InvalidSend occurs when there is a channel send to a value that is not a
// channel, or is a receive-only channel.
//
// Example:
//  func f() {
//      var x = 1
//      x <- "hello!"
//  }
private static readonly var _InvalidSend = 58; 

// _DuplicateLitKey occurs when an index is duplicated in a slice, array, or
// map literal.
//
// Example:
//  var _ = []int{0:1, 0:2}
//
// Example:
//  var _ = map[string]int{"a": 1, "a": 2}
private static readonly var _DuplicateLitKey = 59; 

// _MissingLitKey occurs when a map literal is missing a key expression.
//
// Example:
//  var _ = map[string]int{1}
private static readonly var _MissingLitKey = 60; 

// _InvalidLitIndex occurs when the key in a key-value element of a slice or
// array literal is not an integer constant.
//
// Example:
//  var i = 0
//  var x = []string{i: "world"}
private static readonly var _InvalidLitIndex = 61; 

// _OversizeArrayLit occurs when an array literal exceeds its length.
//
// Example:
//  var _ = [2]int{1,2,3}
private static readonly var _OversizeArrayLit = 62; 

// _MixedStructLit occurs when a struct literal contains a mix of positional
// and named elements.
//
// Example:
//  var _ = struct{i, j int}{i: 1, 2}
private static readonly var _MixedStructLit = 63; 

// _InvalidStructLit occurs when a positional struct literal has an incorrect
// number of values.
//
// Example:
//  var _ = struct{i, j int}{1,2,3}
private static readonly var _InvalidStructLit = 64; 

// _MissingLitField occurs when a struct literal refers to a field that does
// not exist on the struct type.
//
// Example:
//  var _ = struct{i int}{j: 2}
private static readonly var _MissingLitField = 65; 

// _DuplicateLitField occurs when a struct literal contains duplicated
// fields.
//
// Example:
//  var _ = struct{i int}{i: 1, i: 2}
private static readonly var _DuplicateLitField = 66; 

// _UnexportedLitField occurs when a positional struct literal implicitly
// assigns an unexported field of an imported type.
private static readonly var _UnexportedLitField = 67; 

// _InvalidLitField occurs when a field name is not a valid identifier.
//
// Example:
//  var _ = struct{i int}{1: 1}
private static readonly var _InvalidLitField = 68; 

// _UntypedLit occurs when a composite literal omits a required type
// identifier.
//
// Example:
//  type outer struct{
//      inner struct { i int }
//  }
//
//  var _ = outer{inner: {1}}
private static readonly var _UntypedLit = 69; 

// _InvalidLit occurs when a composite literal expression does not match its
// type.
//
// Example:
//  type P *struct{
//      x int
//  }
//  var _ = P {}
private static readonly var _InvalidLit = 70; 

// _AmbiguousSelector occurs when a selector is ambiguous.
//
// Example:
//  type E1 struct { i int }
//  type E2 struct { i int }
//  type T struct { E1; E2 }
//
//  var x T
//  var _ = x.i
private static readonly var _AmbiguousSelector = 71; 

// _UndeclaredImportedName occurs when a package-qualified identifier is
// undeclared by the imported package.
//
// Example:
//  import "go/types"
//
//  var _ = types.NotAnActualIdentifier
private static readonly var _UndeclaredImportedName = 72; 

// _UnexportedName occurs when a selector refers to an unexported identifier
// of an imported package.
//
// Example:
//  import "reflect"
//
//  type _ reflect.flag
private static readonly var _UnexportedName = 73; 

// _UndeclaredName occurs when an identifier is not declared in the current
// scope.
//
// Example:
//  var x T
private static readonly var _UndeclaredName = 74; 

// _MissingFieldOrMethod occurs when a selector references a field or method
// that does not exist.
//
// Example:
//  type T struct {}
//
//  var x = T{}.f
private static readonly var _MissingFieldOrMethod = 75; 

// _BadDotDotDotSyntax occurs when a "..." occurs in a context where it is
// not valid.
//
// Example:
//  var _ = map[int][...]int{0: {}}
private static readonly var _BadDotDotDotSyntax = 76; 

// _NonVariadicDotDotDot occurs when a "..." is used on the final argument to
// a non-variadic function.
//
// Example:
//  func printArgs(s []string) {
//      for _, a := range s {
//          println(a)
//      }
//  }
//
//  func f() {
//      s := []string{"a", "b", "c"}
//      printArgs(s...)
//  }
private static readonly var _NonVariadicDotDotDot = 77; 

// _MisplacedDotDotDot occurs when a "..." is used somewhere other than the
// final argument in a function declaration.
//
// Example:
//     func f(...int, int)
private static readonly var _MisplacedDotDotDot = 78;

private static readonly var _ = 79; // _InvalidDotDotDotOperand was removed.

// _InvalidDotDotDot occurs when a "..." is used in a non-variadic built-in
// function.
//
// Example:
//  var s = []int{1, 2, 3}
//  var l = len(s...)
private static readonly var _InvalidDotDotDot = 80; 

// _UncalledBuiltin occurs when a built-in function is used as a
// function-valued expression, instead of being called.
//
// Per the spec:
//  "The built-in functions do not have standard Go types, so they can only
//  appear in call expressions; they cannot be used as function values."
//
// Example:
//  var _ = copy
private static readonly var _UncalledBuiltin = 81; 

// _InvalidAppend occurs when append is called with a first argument that is
// not a slice.
//
// Example:
//  var _ = append(1, 2)
private static readonly var _InvalidAppend = 82; 

// _InvalidCap occurs when an argument to the cap built-in function is not of
// supported type.
//
// See https://golang.org/ref/spec#Length_and_capacity for information on
// which underlying types are supported as arguments to cap and len.
//
// Example:
//  var s = 2
//  var x = cap(s)
private static readonly var _InvalidCap = 83; 

// _InvalidClose occurs when close(...) is called with an argument that is
// not of channel type, or that is a receive-only channel.
//
// Example:
//  func f() {
//      var x int
//      close(x)
//  }
private static readonly var _InvalidClose = 84; 

// _InvalidCopy occurs when the arguments are not of slice type or do not
// have compatible type.
//
// See https://golang.org/ref/spec#Appending_and_copying_slices for more
// information on the type requirements for the copy built-in.
//
// Example:
//  func f() {
//      var x []int
//      y := []int64{1,2,3}
//      copy(x, y)
//  }
private static readonly var _InvalidCopy = 85; 

// _InvalidComplex occurs when the complex built-in function is called with
// arguments with incompatible types.
//
// Example:
//  var _ = complex(float32(1), float64(2))
private static readonly var _InvalidComplex = 86; 

// _InvalidDelete occurs when the delete built-in function is called with a
// first argument that is not a map.
//
// Example:
//  func f() {
//      m := "hello"
//      delete(m, "e")
//  }
private static readonly var _InvalidDelete = 87; 

// _InvalidImag occurs when the imag built-in function is called with an
// argument that does not have complex type.
//
// Example:
//  var _ = imag(int(1))
private static readonly var _InvalidImag = 88; 

// _InvalidLen occurs when an argument to the len built-in function is not of
// supported type.
//
// See https://golang.org/ref/spec#Length_and_capacity for information on
// which underlying types are supported as arguments to cap and len.
//
// Example:
//  var s = 2
//  var x = len(s)
private static readonly var _InvalidLen = 89; 

// _SwappedMakeArgs occurs when make is called with three arguments, and its
// length argument is larger than its capacity argument.
//
// Example:
//  var x = make([]int, 3, 2)
private static readonly var _SwappedMakeArgs = 90; 

// _InvalidMake occurs when make is called with an unsupported type argument.
//
// See https://golang.org/ref/spec#Making_slices_maps_and_channels for
// information on the types that may be created using make.
//
// Example:
//  var x = make(int)
private static readonly var _InvalidMake = 91; 

// _InvalidReal occurs when the real built-in function is called with an
// argument that does not have complex type.
//
// Example:
//  var _ = real(int(1))
private static readonly var _InvalidReal = 92; 

// _InvalidAssert occurs when a type assertion is applied to a
// value that is not of interface type.
//
// Example:
//  var x = 1
//  var _ = x.(float64)
private static readonly var _InvalidAssert = 93; 

// _ImpossibleAssert occurs for a type assertion x.(T) when the value x of
// interface cannot have dynamic type T, due to a missing or mismatching
// method on T.
//
// Example:
//  type T int
//
//  func (t *T) m() int { return int(*t) }
//
//  type I interface { m() int }
//
//  var x I
//  var _ = x.(T)
private static readonly var _ImpossibleAssert = 94; 

// _InvalidConversion occurs when the argument type cannot be converted to the
// target.
//
// See https://golang.org/ref/spec#Conversions for the rules of
// convertibility.
//
// Example:
//  var x float64
//  var _ = string(x)
private static readonly var _InvalidConversion = 95; 

// _InvalidUntypedConversion occurs when an there is no valid implicit
// conversion from an untyped value satisfying the type constraints of the
// context in which it is used.
//
// Example:
//  var _ = 1 + ""
private static readonly var _InvalidUntypedConversion = 96; 

// _BadOffsetofSyntax occurs when unsafe.Offsetof is called with an argument
// that is not a selector expression.
//
// Example:
//  import "unsafe"
//
//  var x int
//  var _ = unsafe.Offsetof(x)
private static readonly var _BadOffsetofSyntax = 97; 

// _InvalidOffsetof occurs when unsafe.Offsetof is called with a method
// selector, rather than a field selector, or when the field is embedded via
// a pointer.
//
// Per the spec:
//
//  "If f is an embedded field, it must be reachable without pointer
//  indirections through fields of the struct. "
//
// Example:
//  import "unsafe"
//
//  type T struct { f int }
//  type S struct { *T }
//  var s S
//  var _ = unsafe.Offsetof(s.f)
//
// Example:
//  import "unsafe"
//
//  type S struct{}
//
//  func (S) m() {}
//
//  var s S
//  var _ = unsafe.Offsetof(s.m)
private static readonly var _InvalidOffsetof = 98; 

// _UnusedExpr occurs when a side-effect free expression is used as a
// statement. Such a statement has no effect.
//
// Example:
//  func f(i int) {
//      i*i
//  }
private static readonly var _UnusedExpr = 99; 

// _UnusedVar occurs when a variable is declared but unused.
//
// Example:
//  func f() {
//      x := 1
//  }
private static readonly var _UnusedVar = 100; 

// _MissingReturn occurs when a function with results is missing a return
// statement.
//
// Example:
//  func f() int {}
private static readonly var _MissingReturn = 101; 

// _WrongResultCount occurs when a return statement returns an incorrect
// number of values.
//
// Example:
//  func ReturnOne() int {
//      return 1, 2
//  }
private static readonly var _WrongResultCount = 102; 

// _OutOfScopeResult occurs when the name of a value implicitly returned by
// an empty return statement is shadowed in a nested scope.
//
// Example:
//  func factor(n int) (i int) {
//      for i := 2; i < n; i++ {
//          if n%i == 0 {
//              return
//          }
//      }
//      return 0
//  }
private static readonly var _OutOfScopeResult = 103; 

// _InvalidCond occurs when an if condition is not a boolean expression.
//
// Example:
//  func checkReturn(i int) {
//      if i {
//          panic("non-zero return")
//      }
//  }
private static readonly var _InvalidCond = 104; 

// _InvalidPostDecl occurs when there is a declaration in a for-loop post
// statement.
//
// Example:
//  func f() {
//      for i := 0; i < 10; j := 0 {}
//  }
private static readonly var _InvalidPostDecl = 105;

private static readonly var _ = 106; // _InvalidChanRange was removed.

// _InvalidIterVar occurs when two iteration variables are used while ranging
// over a channel.
//
// Example:
//  func f(c chan int) {
//      for k, v := range c {
//          println(k, v)
//      }
//  }
private static readonly var _InvalidIterVar = 107; 

// _InvalidRangeExpr occurs when the type of a range expression is not array,
// slice, string, map, or channel.
//
// Example:
//  func f(i int) {
//      for j := range i {
//          println(j)
//      }
//  }
private static readonly var _InvalidRangeExpr = 108; 

// _MisplacedBreak occurs when a break statement is not within a for, switch,
// or select statement of the innermost function definition.
//
// Example:
//  func f() {
//      break
//  }
private static readonly var _MisplacedBreak = 109; 

// _MisplacedContinue occurs when a continue statement is not within a for
// loop of the innermost function definition.
//
// Example:
//  func sumeven(n int) int {
//      proceed := func() {
//          continue
//      }
//      sum := 0
//      for i := 1; i <= n; i++ {
//          if i % 2 != 0 {
//              proceed()
//          }
//          sum += i
//      }
//      return sum
//  }
private static readonly var _MisplacedContinue = 110; 

// _MisplacedFallthrough occurs when a fallthrough statement is not within an
// expression switch.
//
// Example:
//  func typename(i interface{}) string {
//      switch i.(type) {
//      case int64:
//          fallthrough
//      case int:
//          return "int"
//      }
//      return "unsupported"
//  }
private static readonly var _MisplacedFallthrough = 111; 

// _DuplicateCase occurs when a type or expression switch has duplicate
// cases.
//
// Example:
//  func printInt(i int) {
//      switch i {
//      case 1:
//          println("one")
//      case 1:
//          println("One")
//      }
//  }
private static readonly var _DuplicateCase = 112; 

// _DuplicateDefault occurs when a type or expression switch has multiple
// default clauses.
//
// Example:
//  func printInt(i int) {
//      switch i {
//      case 1:
//          println("one")
//      default:
//          println("One")
//      default:
//          println("1")
//      }
//  }
private static readonly var _DuplicateDefault = 113; 

// _BadTypeKeyword occurs when a .(type) expression is used anywhere other
// than a type switch.
//
// Example:
//  type I interface {
//      m()
//  }
//  var t I
//  var _ = t.(type)
private static readonly var _BadTypeKeyword = 114; 

// _InvalidTypeSwitch occurs when .(type) is used on an expression that is
// not of interface type.
//
// Example:
//  func f(i int) {
//      switch x := i.(type) {}
//  }
private static readonly var _InvalidTypeSwitch = 115; 

// _InvalidExprSwitch occurs when a switch expression is not comparable.
//
// Example:
//  func _() {
//      var a struct{ _ func() }
//      switch a /* ERROR cannot switch on a */ {
//      }
//  }
private static readonly var _InvalidExprSwitch = 116; 

// _InvalidSelectCase occurs when a select case is not a channel send or
// receive.
//
// Example:
//  func checkChan(c <-chan int) bool {
//      select {
//      case c:
//          return true
//      default:
//          return false
//      }
//  }
private static readonly var _InvalidSelectCase = 117; 

// _UndeclaredLabel occurs when an undeclared label is jumped to.
//
// Example:
//  func f() {
//      goto L
//  }
private static readonly var _UndeclaredLabel = 118; 

// _DuplicateLabel occurs when a label is declared more than once.
//
// Example:
//  func f() int {
//  L:
//  L:
//      return 1
//  }
private static readonly var _DuplicateLabel = 119; 

// _MisplacedLabel occurs when a break or continue label is not on a for,
// switch, or select statement.
//
// Example:
//  func f() {
//  L:
//      a := []int{1,2,3}
//      for _, e := range a {
//          if e > 10 {
//              break L
//          }
//          println(a)
//      }
//  }
private static readonly var _MisplacedLabel = 120; 

// _UnusedLabel occurs when a label is declared but not used.
//
// Example:
//  func f() {
//  L:
//  }
private static readonly var _UnusedLabel = 121; 

// _JumpOverDecl occurs when a label jumps over a variable declaration.
//
// Example:
//  func f() int {
//      goto L
//      x := 2
//  L:
//      x++
//      return x
//  }
private static readonly var _JumpOverDecl = 122; 

// _JumpIntoBlock occurs when a forward jump goes to a label inside a nested
// block.
//
// Example:
//  func f(x int) {
//      goto L
//      if x > 0 {
//      L:
//          print("inside block")
//      }
// }
private static readonly var _JumpIntoBlock = 123; 

// _InvalidMethodExpr occurs when a pointer method is called but the argument
// is not addressable.
//
// Example:
//  type T struct {}
//
//  func (*T) m() int { return 1 }
//
//  var _ = T.m(T{})
private static readonly var _InvalidMethodExpr = 124; 

// _WrongArgCount occurs when too few or too many arguments are passed by a
// function call.
//
// Example:
//  func f(i int) {}
//  var x = f()
private static readonly var _WrongArgCount = 125; 

// _InvalidCall occurs when an expression is called that is not of function
// type.
//
// Example:
//  var x = "x"
//  var y = x()
private static readonly var _InvalidCall = 126; 

// _UnusedResults occurs when a restricted expression-only built-in function
// is suspended via go or defer. Such a suspension discards the results of
// these side-effect free built-in functions, and therefore is ineffectual.
//
// Example:
//  func f(a []int) int {
//      defer len(a)
//      return i
//  }
private static readonly var _UnusedResults = 127; 

// _InvalidDefer occurs when a deferred expression is not a function call,
// for example if the expression is a type conversion.
//
// Example:
//  func f(i int) int {
//      defer int32(i)
//      return i
//  }
private static readonly var _InvalidDefer = 128; 

// _InvalidGo occurs when a go expression is not a function call, for example
// if the expression is a type conversion.
//
// Example:
//  func f(i int) int {
//      go int32(i)
//      return i
//  }
private static readonly var _InvalidGo = 129; 

// All codes below were added in Go 1.17.

// _BadDecl occurs when a declaration has invalid syntax.
private static readonly var _BadDecl = 130; 

// _RepeatedDecl occurs when an identifier occurs more than once on the left
// hand side of a short variable declaration.
//
// Example:
//  func _() {
//      x, y, y := 1, 2, 3
//  }
private static readonly var _RepeatedDecl = 131; 

// _InvalidUnsafeAdd occurs when unsafe.Add is called with a
// length argument that is not of integer type.
//
// Example:
//  import "unsafe"
//
//  var p unsafe.Pointer
//  var _ = unsafe.Add(p, float64(1))
private static readonly var _InvalidUnsafeAdd = 132; 

// _InvalidUnsafeSlice occurs when unsafe.Slice is called with a
// pointer argument that is not of pointer type or a length argument
// that is not of integer type, negative, or out of bounds.
//
// Example:
//  import "unsafe"
//
//  var x int
//  var _ = unsafe.Slice(x, 1)
//
// Example:
//  import "unsafe"
//
//  var x int
//  var _ = unsafe.Slice(&x, float64(1))
//
// Example:
//  import "unsafe"
//
//  var x int
//  var _ = unsafe.Slice(&x, -1)
//
// Example:
//  import "unsafe"
//
//  var x int
//  var _ = unsafe.Slice(&x, uint64(1) << 63)
private static readonly var _InvalidUnsafeSlice = 133; 

// _Todo is a placeholder for error codes that have not been decided.
// TODO(rFindley) remove this error code after deciding on errors for generics code.
private static readonly var _Todo = 134;

} // end types_package
