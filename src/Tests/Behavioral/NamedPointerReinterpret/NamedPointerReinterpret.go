package main

import "fmt"

// base is a struct; view is a DEFINED type over base, so the [GoType] value
// conversions view<->base exist. The (*view)(*base) pointer reinterpret below
// is the construct under test: the arg is a genuine pointer box (a call result),
// which the converter must dereference before the value conversion binds.
type base struct {
	a int64
	b int64
}

type view base

func makeBase() *base {
	x := base{a: 10, b: 20}
	return &x
}

// intRef is a DEFINED POINTER type (`type dequeueNil *struct{}`, sync/poolqueue): the TypeSpec
// must emit a [GoType("zh<T>")] partial class forward decl (a bare star-type line is CS1585),
// with nil-conversions (`intRef(nil)`), any-boxing comparisons, and derefs working through it.
type intRef *int64

func classify(v any) string {
	if v == intRef(nil) {
		return "nilref"
	}
	return "other"
}

func main() {
	pb := makeBase()  // *base — a genuine box (call result), not a deref-aliased param
	pv := (*view)(pb) // reinterpret *base -> *view (identical underlying struct)
	bb := base(*pv)   // convert the view value back to base to read its fields
	fmt.Println(bb.a, bb.b)

	var boxed any = intRef(nil)
	fmt.Println(classify(boxed))
	n := int64(5)
	var r intRef = &n
	fmt.Println(*r, classify(r))
}
