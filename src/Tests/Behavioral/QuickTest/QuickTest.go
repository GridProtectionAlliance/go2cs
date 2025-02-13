package main

import "fmt"

type P = *bool
type M = map[int]int

func test() struct { string; *int; P; M } {
    var x struct {
        string // a defined non-pointer type
        *int   // a non-defined pointer type
        P      // an alias of a non-defined pointer type
        M      // an alias of a non-defined type
    }
    x.string = "Go"
    x.int = new(int)
    x.P = new(bool)
    x.M = make(M)
    return x;
}

func main() {
    x := test()
    fmt.Println(x)
}
