// Regression test for the comma-ok form of map access: `v, ok := m[k]`.
//
// The converter detects a tuple-result RHS for call, channel-receive and type-assert
// expressions, but not for a map index. So `v, ok := m[k]` (and the blank `_, ok := m[k]`
// and if-init `if _, ok := m[k]; ok` forms) were mis-lowered: the map was indexed twice and
// the *value* was assigned to `ok` instead of a presence bool — e.g. internal/coverage/rtcov
// emitted `nint _ = m[k]; var ok = m[k];` (CS0029 int->bool). The converter now routes a
// comma-ok map index through golib's two-value indexer `m[key, ꟷ]`, which returns
// `(value, present)`.
package main

import "fmt"

func main() {
	m := map[string]int{"a": 1, "b": 2}

	// value + ok, present.
	v, ok := m["a"]
	fmt.Println(v, ok) // 1 true

	// value + ok, absent (value is the zero value).
	v2, ok2 := m["z"]
	fmt.Println(v2, ok2) // 0 false

	// blank value, present.
	_, ok3 := m["b"]
	fmt.Println(ok3) // true

	// if-init comma-ok with a blank value.
	if _, ok4 := m["z"]; !ok4 {
		fmt.Println("z absent") // printed
	}

	// reassignment (not a new declaration) into existing variables.
	var w int
	var present bool
	w, present = m["b"]
	fmt.Println(w, present) // 2 true
}
