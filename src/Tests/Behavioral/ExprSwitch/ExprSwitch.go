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

func getStr(test string) string {
    return "string" + test
}

func getStr2(test1 interface{}, test2 string) string {
    return test1.(string) + test2
}

func getStr3(format string, a ...interface{}) string {
	return fmt.Sprintf(format, a...)
}

func main() {

    fmt.Println(getStr("test"))    
    fmt.Println(getStr2("hello, ", "world"))
    fmt.Println(getStr3("hello, %s", "world"))

	// Here's a basic `switch`.
	i := 2
	fmt.Print("Write ", i, " as ")
	switch i {
	case 1:
		fmt.Println("one")
	case 2:
		fmt.Println("two")
	case 3:
		{
			fmt.Println("three")
		}
	case 4:
		fmt.Println("four")
	default:
		fmt.Println("unknown")
	}

    x := 5
    fmt.Println(x)

    {
        x := 6
        fmt.Println(x)
    }

    fmt.Println(x)

	// You can use commas to separate multiple expressions
	// in the same `case` statement. We use the optional
	// `default` case in this example as well.
	switch time.Now().Weekday() {
	case time.Saturday, time.Sunday:
		fmt.Println("It's the weekend")
	case time.Monday: // Case Mon comment
		fmt.Println("Ugh, it's Monday")
	default:
		fmt.Println("It's a weekday")
	}

	// `switch` without an expression is an alternate way
	// to express if/else logic. Here we also show how the
	// `case` expressions can be non-constants.
	t := time.Now()
	switch {
	case t.Hour() < 12: // Before noon
		fmt.Println("It's before noon")
	default: // After noon
		fmt.Println("It's after noon")
	}

	// "i" before should be saved
	fmt.Printf("i before = %d\n", i)

	// Here is a switch with simple statement and a redeclared identifier plus a fallthrough
	switch i := 1; getNext() {
	case -1:
		fmt.Println("negative")
	case 0:
		fmt.Println("zero")
	case 1, 2:
		fmt.Println("one or two")
		fallthrough
	case 3:
		fmt.Printf("three, but x=%d and i now = %d\n", x, i)
		fallthrough
	default:
		fmt.Println("plus, always a default here because of fallthrough")
	}

	// "i" after should be restored
	fmt.Printf("i after = %d\n", i)
}
