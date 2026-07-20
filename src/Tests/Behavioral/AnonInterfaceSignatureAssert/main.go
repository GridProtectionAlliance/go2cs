// Guards the golib duck-typing SIGNATURE-aware match: two ANONYMOUS interfaces that share a
// method NAME but differ in SIGNATURE — interface{ Unwrap() error } vs interface{ Unwrap() []error }
// — must stay distinct at run time. Before the fix, golib's structural interface check
// (Cache<TInterface>.Implements in builtin.cs) matched by method NAME only, so a value whose
// Unwrap returns []error satisfied BOTH interfaces; the generated duck-typed adapter for the wrong
// one then failed to bind a delegate whose return type didn't match (a NotImplementedException).
// This is exactly the conflation that broke errors.Is's is_typeᴛ1/is_typeᴛ2 switch. It COMPILES
// either way; only running the assertions reveals the conflation.
package main

import "fmt"

type simpleErr struct{ msg string }

func (e simpleErr) Error() string { return e.msg }

// multiWrap implements Unwrap() []error (the "multi" shape).
type multiWrap struct{ errs []error }

func (m multiWrap) Unwrap() []error { return m.errs }

// singleWrap implements Unwrap() error (the "single" shape).
type singleWrap struct{ err error }

func (s singleWrap) Unwrap() error { return s.err }

// classify reports which of the two same-named/different-signature anonymous interfaces the
// dynamic value satisfies. A signature-blind match would report both true for multiWrap.
func classify(v any) (single, multi bool) {
	_, single = v.(interface{ Unwrap() error })
	_, multi = v.(interface{ Unwrap() []error })
	return
}

func main() {
	e1 := simpleErr{"boom"}
	m := multiWrap{errs: []error{e1}}
	s := singleWrap{err: e1}

	ms, mm := classify(m)
	fmt.Printf("multiWrap:  single=%v multi=%v\n", ms, mm)
	ss, sm := classify(s)
	fmt.Printf("singleWrap: single=%v multi=%v\n", ss, sm)
	es, em := classify(e1)
	fmt.Printf("simpleErr:  single=%v multi=%v\n", es, em)

	// The matched interface must actually dispatch to the right Unwrap method.
	if u, ok := any(m).(interface{ Unwrap() []error }); ok {
		fmt.Println("multi unwrap count:", len(u.Unwrap()))
	}
	if u, ok := any(s).(interface{ Unwrap() error }); ok {
		fmt.Println("single unwrap:", u.Unwrap())
	}
}
