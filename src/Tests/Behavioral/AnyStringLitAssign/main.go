package main

import "fmt"

type holder struct {
	value any
}

func sprint(x any) string {
	switch v := x.(type) {
	case string:
		return "str:" + v
	case nil:
		return "none"
	default:
		return fmt.Sprintf("other:%v", v)
	}
}

func main() {
	// A string literal assigned to an EMPTY-interface variable must box a Go
	// string (later type assertion on string succeeds), not a raw byte span.
	var arg any
	arg = "<nil>"
	fmt.Println(sprint(arg))

	// Reassigning an any-typed range variable with a literal (the go/types
	// format.go sprintf shape).
	args := []any{1, nil, "keep"}
	for i, a := range args {
		if a == nil {
			a = "<missing>"
		}
		fmt.Println(i, sprint(a))
	}

	// A string literal assigned to an any-typed struct field.
	var h holder
	h.value = "field"
	fmt.Println(sprint(h.value))
}
