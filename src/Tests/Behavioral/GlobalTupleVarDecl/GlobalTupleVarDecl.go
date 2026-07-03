package main

import "fmt"

type box struct{ v int }

var calls int

func makeBox(v int) (*box, error) { return &box{v: v}, nil }

func pair() (int, string) {
	calls++
	return 42, "hello"
}

// single non-blank name: component read appended to the inline call (edwards25519's
// `var identity, _ = new(Point).SetBytes(...)` shape - the per-name field loop assigned
// the WHOLE result tuple to the first field, CS0029)
var defaultBox, _ = makeBox(7)

// two non-blank names: hidden once-evaluated tuple field + component reads
var n, s = pair()

func main() {
	fmt.Println(defaultBox.v) // 7
	fmt.Println(n, s)         // 42 hello
	fmt.Println(calls)        // 1 - proves the initializer call ran exactly once
}
