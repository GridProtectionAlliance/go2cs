package main

import "fmt"

type Counter int

func (c Counter) String() string {
    return fmt.Sprint(int(c))
}

func main() {
    var c Counter = 5
    f := c.String
    fmt.Println(f())
}
