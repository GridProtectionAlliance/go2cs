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
    fmt.Printf("%v\n", f()) // error: foo
}