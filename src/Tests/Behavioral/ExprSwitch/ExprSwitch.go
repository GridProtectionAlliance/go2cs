// _Switch statements_ express conditionals across many
// branches.

package main

import (
	"fmt"
	"time"
)

var x = 1

func getNext() int32 {
	x++
	return int32(x)
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
	case 4, 5, 6:
		fmt.Println("four, five or siz")
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

	// Here is a more complex switch
	hour := 1
	hour1 := time.Now().Hour()

	switch hour := time.Now().Hour(); { // missing expression means "true"
	case hour == 1, hour < 12, hour == 2:
		fmt.Println("Good morning!")
	case hour == 1, hour < 12, hour == 2 || hour1 == 4:
		fmt.Println("Good morning (opt 2)!")
	case hour < 17:
		fmt.Println("Good afternoon!")
	case hour == 0:
		fmt.Println("Midnight!")
	case hour == 0 && hour1 == 1:
		fmt.Println("Midnight (opt 2)!")
	default:
		fmt.Println("Good evening!")
	}

	fmt.Println(hour)

	c := '\r'

	switch c {
	case ' ', '\t', '\n', '\f', '\r':
		fmt.Println("whitespace")
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

	switch next := getNext(); {
	case next <= -1:
		fmt.Println("negative")

        switch getNext() {
        case 1, 2:
            fmt.Println("sub0 one or two")
        case 3:
            fmt.Println("sub0 three")
            fallthrough
        default:
            fmt.Println("sub0 default")
        }
    case next == 0:
		fmt.Println("zero")

        switch next := getNext(); {
        case next == 1, next <= 2:
            fmt.Println("sub1 one or two")
        case next == 3:
            fmt.Println("sub1 three")
            fallthrough
        default:
            fmt.Println("sub1 default")
        }
	case next == 1, next == 2:
		fmt.Println("one or two")

        switch next {
        case 1, 2:
            fmt.Println("sub2 one or two")
        case 3:
            fmt.Println("sub2 three")
        default:
            fmt.Println("sub2 default")
        }

		fallthrough
	case next >= 3 && next < 100:
		fmt.Printf("three or greater < 100: %d\n", x)
		fallthrough
	default:
		fmt.Println("plus, always a default here because of fallthrough")
	}
}
