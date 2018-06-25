package main

import (
    "fmt"
    _ "math"
    _ "path/file"
    . "math/rand"
    "os"
    implicit "text/tabwriter"
)

func main() {
    fmt.Println(Int())
    w := implicit.NewWriter(os.Stdout, 1, 1, 1, ' ', 0)
    defer w.Flush()
}