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
    switch i {
    case 1:
        fmt.Println("one")
    case 2:
        fmt.Println("two")
    case 3: {
        fmt.Println("three")
	}
    default:
        fmt.Println("unknown")
    }

    // You can use commas to separate multiple expressions
    // in the same `case` statement. We use the optional
    // `default` case in this example as well.
    switch time.Now().Weekday() {
    case time.Saturday, time.Sunday:
        fmt.Println("It's the weekend")
    case time.Monday:
        fmt.Println("Ugh, it's Monday")
    default:
        fmt.Println("It's a weekday")
    }

    // `switch` without an expression is an alternate way
    // to express if/else logic. Here we also show how the
    // `case` expressions can be non-constants.
    t := time.Now()
    switch {
    case t.Hour() < 12:
        fmt.Println("It's before noon")
    default:
        fmt.Println("It's after noon")
    }

    // Here is a switch with simple statement and a fallthrough
    switch j:=1; getNext() {
    case -1:
        fmt.Println("negative")
    case 0:
        fmt.Println("zero")
    case 1, 2:
        fmt.Println("one or two")
        fallthrough
    case 3:
        fmt.Printf("three, but x=%d and j = %d\n", x, j)
        fallthrough
    default:
        fmt.Println("plus, always a default here because of fallthrough")
    }
 }
