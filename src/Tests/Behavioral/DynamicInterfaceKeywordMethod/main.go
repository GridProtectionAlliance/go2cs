package main

import "fmt"

// TB mirrors testing.TB: a named interface with an unexported "sealing" method whose
// name (private) collides with a C# reserved keyword. The converter escapes it to
// @private, but Roslyn's IMethodSymbol.Name strips the @ when the method is pulled into
// a dynamic interface's runtime conversion class via the base-interface (AllInterfaces)
// method walk — the escape must be re-applied for the emitted C# to parse.
type TB interface {
	Name() string
	private()
}

type harness struct {
	name     string
	deadline int
}

func (h harness) Name() string          { return h.name }
func (h harness) private()              {}
func (h harness) Deadline() (int, bool) { return h.deadline, true }

// commandContext mirrors testenv.CommandContext: it type-asserts its TB argument to an
// anonymous interface that EMBEDS TB and adds a method. go2cs lifts that anonymous
// assertion target to a [GoType("dyn")] interface; go2cs-gen then generates a runtime
// conversion class implementing every inherited method — including the escaped @private().
func commandContext(t TB) {
	if td, ok := t.(interface {
		TB
		Deadline() (int, bool)
	}); ok {
		d, _ := td.Deadline()
		fmt.Println("Name:", td.Name(), "deadline:", d)
	} else {
		fmt.Println("no deadline")
	}
}

func main() {
	commandContext(harness{name: "cmd", deadline: 100})
}
