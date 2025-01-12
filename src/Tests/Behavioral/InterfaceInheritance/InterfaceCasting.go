package main

import "fmt"

type MyError struct {
    description string
}

func (err MyError) Error() string {
    return fmt.Sprintf("error: %s", err.description)
}

// error is an interface - MyError is cast to error interface upon return
func f() error {
    return MyError{"foo"}
}

func main() {
    var err error

    err = MyError{"bar"}

    fmt.Printf("%v %v\n", f(), err) // error: foo
}
