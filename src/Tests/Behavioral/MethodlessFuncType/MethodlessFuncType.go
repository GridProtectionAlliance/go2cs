// Guards the methodless-named-func-type collapse: a non-generic named func type with NO
// methods (`type releaser func(error)`, mirroring database/sql's releaseConn / context's
// CancelFunc) renders as its base C# delegate (Action/Func<...>) everywhere — no distinct
// delegate type — so Go's free interconversion between the named type and its underlying
// `func(...)` becomes identity in C#. This exercises the seams that previously broke:
// a function RETURNING the named type, another TAKING the anonymous underlying, a struct
// FIELD of the named type, and a tuple-deconstruction assignment across the two.
package main

import "fmt"

// releaser is a methodless named func type — collapses to Action<error>.
type releaser func(error)

// lookup is a methodless named func type with a MULTI-RESULT signature carrying Go result NAMES.
// Declared below as an UNINITIALIZED local (`var find lookup`, no initializer), its TYPE must render
// through the same structural signature path (getCSTypeName -> iifeDelegateType, which drops the Go
// result names -> `Func<@string, (@string, bool)>`) that the parameter and initialized-var forms use —
// NOT the string path, which keeps the Go-ordered result names and (for a cross-package element like
// go/parser's `var f parseSpecFunction`, `func(*go/ast.CommentGroup, go/token.Token, int) ast.Spec`)
// mangles the element qualifier to the nonexistent `go.go.ast.CommentGroup` (CS0234). Both forms
// compile and run identically; the byte-golden guards the var-decl routing.
type lookup func(string) (path string, ok bool)

// makeReleaser returns the NAMED type (like grabConn returning releaseConn).
func makeReleaser(tag string, log *[]string) releaser {
	return func(err error) {
		if err != nil {
			*log = append(*log, tag+":"+err.Error())
		} else {
			*log = append(*log, tag+":ok")
		}
	}
}

// consume takes the ANONYMOUS underlying func(error) (like queryDC's releaseConn param) —
// a releaser value must pass here without conversion.
func consume(r func(error), err error) {
	r(err)
}

// holder has a FIELD of the named type.
type holder struct {
	release releaser
	name    string
}

// grab returns a tuple whose middle element is the NAMED type — deconstructed into a var
// that then flows to a func(error)-typed parameter (the database/sql tuple seam).
func grab(tag string, log *[]string) (int, releaser, error) {
	return len(tag), makeReleaser(tag, log), nil
}

func main() {
	var log []string

	// tuple-deconstruction: middle element is `releaser`, assigned then passed to consume.
	n, rel, err := grab("a", &log)
	consume(rel, nil) // releaser -> func(error) param, identity
	fmt.Println("n:", n, "err:", err == nil)

	// struct field of the named type, invoked and also passed to the anonymous-param consumer.
	h := holder{release: makeReleaser("field", &log), name: "h"}
	h.release(fmt.Errorf("boom"))
	consume(h.release, nil)

	// direct named value both called and widened to the underlying.
	var r releaser = makeReleaser("direct", &log)
	r(nil)
	consume(r, fmt.Errorf("late"))

	// UNINITIALIZED local var of a methodless named func type with named multi-results — the Root B
	// seam (visitValueSpec's no-initializer branch). Assigned a lambda, then invoked.
	var find lookup
	find = func(s string) (string, bool) {
		return s + "!", len(s) > 0
	}
	p, okp := find("q")
	fmt.Println("lookup:", p, okp)

	for _, line := range log {
		fmt.Println(line)
	}
}
