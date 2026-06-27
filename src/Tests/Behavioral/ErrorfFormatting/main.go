// Regression test (temporary-fix scope): fmt.Errorf formats its message.
//
// The baseline stub fmt.Errorf returned nil, so every error it produced printed as "<nil>"
// (e.g. `fmt.Errorf("recovered: %v", r)` on a recovered panic value). The stub now formats
// the message via Sprintf and returns a real error. NOTE: this is a temporary fix until
// stdlib fmt resolution lands — `%w` is rendered like `%v` (no error-chain Unwrap), so this
// test checks only the rendered message, not errors.Is/Unwrap behavior.
package main

import "fmt"

func main() {
	fmt.Println(fmt.Errorf("plain message"))      // plain message
	fmt.Println(fmt.Errorf("got %v", 42))         // got 42
	fmt.Println(fmt.Errorf("name %s = %d", "x", 7)) // name x = 7
	fmt.Println(fmt.Errorf("%v and %v", true, "y")) // true and y

	// %v on a recovered panic value (the original trigger).
	func() {
		defer func() {
			if r := recover(); r != nil {
				fmt.Println(fmt.Errorf("recovered: %v", r)) // recovered: kaboom
			}
		}()
		panic("kaboom")
	}()

	// %w renders the wrapped error's message (message only, in this temporary fix).
	base := fmt.Errorf("base failure")
	wrapped := fmt.Errorf("while doing X: %w", base)
	fmt.Println(wrapped)        // while doing X: base failure
	fmt.Println(wrapped.Error()) // while doing X: base failure
}
