package main

import "fmt"

// A struct FIELD whose type is a VARIADIC func type — the go/build Context shape
// (`JoinPath func(elem ...string) string`, build.go:84). The field's declared type must render
// structurally (the string display path destroyed the ellipsis: `Func<.@string, @string>`,
// CS1031), a func literal and a named function must assign to it, and calls through the field
// must accept loose args, an empty tail, and a spread.

type Context struct {
	Name     string
	JoinPath func(elem ...string) string
	Log      func(format string, args ...any)
}

// slashJoin is a named variadic function assigned to the field (method-group conversion).
func slashJoin(elem ...string) string {
	joined := ""

	for i, e := range elem {
		if i > 0 {
			joined += "/"
		}

		joined += e
	}

	return joined
}

func (c *Context) join(elem ...string) string {
	if c.JoinPath != nil {
		return c.JoinPath(elem...)
	}

	return slashJoin(elem...)
}

func main() {
	// Composite-literal initialization: a func literal in one field, a named func in another.
	ctxt := Context{
		Name:     "demo",
		JoinPath: slashJoin,
		Log: func(format string, args ...any) {
			fmt.Printf(format+"\n", args...)
		},
	}

	// Loose args through the field.
	fmt.Println(ctxt.JoinPath("a", "b", "c"))

	// Empty variadic call through the field.
	fmt.Println(ctxt.JoinPath() == "")

	// Spread through the field (the go/build `ctxt.joinPath(elem...)` shape).
	parts := []string{"x", "y"}
	fmt.Println(ctxt.join(parts...))

	// Reassign the field with a literal; nil-check the other.
	ctxt.JoinPath = func(elem ...string) string {
		return fmt.Sprint(len(elem))
	}
	fmt.Println(ctxt.JoinPath("p", "q"))

	ctxt.Log("%s:%d", ctxt.Name, 3)
	ctxt.Log("done")
}
