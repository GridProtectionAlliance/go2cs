// _Switch statements_ express conditionals across many
// branches.

package main

import "fmt"
import "time"

var x = 1

func getNext() int {
	x++
	return x
}

func main() {

	// Here's a basic `switch`.
	i := 2
	fmt.Print("Write ", i, " as ")
	switch i { // Intra-switch comment
	case 1: // Case 1 comment
		fmt.Println("one") /* Case 1 eol comment */
	case 2: // Case 2 comment
		// Comment before
		fmt.Println("two") // Case 2 eol comment
		// Comment after
	case 3:
		{ // Start of block comment
			// Before statement comment
			fmt.Println("three") // eol comment
			// After statement comment
		} // End of block comment
	case 4:
		// Comment before
		fmt.Println("four") // Case 2 eol comment
		// Comment after
	default: // Default case comment
		// Comment before
		fmt.Println("unknown") // Default case eol comment
		// Comment after
	} // End of switch comment

	// You can use commas to separate multiple expressions
	// in the same `case` statement. We use the optional
	// `default` case in this example as well.
	switch time.Now().Weekday() { // Intra-switch comment
	case time.Saturday, time.Sunday: // Case Sat/Sun comment
		// Weekend comment
		fmt.Println("It's the weekend")
	case time.Monday: // Case Mon comment
		// Pre-Monday comment
		fmt.Println("Ugh, it's Monday")
		// Post-Monday comment
	default: // Case default comment
		// Pre-default comment
		fmt.Println("It's a weekday")
		// Post-default comment
	} // End of switch comment

	// `switch` without an expression is an alternate way
	// to express if/else logic. Here we also show how the
	// `case` expressions can be non-constants.
	t := time.Now()
	switch { // Intra-switch comment
	case t.Hour() < 12: // Before noon
		fmt.Println("It's before noon")
	default: // After noon
		fmt.Println("It's after noon")
	} // End of switch comment

	// "i" before should be saved
	fmt.Printf("i before = %d\n", i)

	// Here is a switch with simple statement and a redeclared identifier plus a fallthrough
	switch i := 1; getNext() { // Intra-switch comment
	case -1:
		fmt.Println("negative")
	case 0: // Single-value comment
		// Before zero comment
		fmt.Println("zero")
		// After zero comment
	case 1, 2: // Multi-value comment
		// Before one-or-two comment
		fmt.Println("one or two") // eol comment
		// After one-or-two comment
		fallthrough // fallthrough comment
	case 3:
		fmt.Printf("three, but x=%d and i now = %d\n", x, i)
		fallthrough
	default: // Default case comment
		// Pre-default-op comments
		fmt.Println("plus, always a default here because of fallthrough") // eol comment
		// Post-default-op comments
	} // end of switch comment

	// "i" after should be restored
	fmt.Printf("i after = %d\n", i)
}
