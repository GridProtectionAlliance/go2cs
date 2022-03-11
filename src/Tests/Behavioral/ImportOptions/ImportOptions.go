package main

import "a"
import "b"

import /* Outer upper import comment */ ( // Inner upper import comment
    "fmt"         // fmt comment
    _ "math"      /* math comment */
    _ "path/file" // path/file comment
    // Intra import comments
    . "math/rand" // math/rand comment
    "os"          // os comment
    implicit "text/tabwriter" // implicit comment
/* Inner lower import comment */ ) // Outer lower import comment
import "time"
import "sync"

func main() {
    fmt.Println(Int())
    w := implicit.NewWriter(os.Stdout, 1, 1, 1, ' ', 0)
    defer w.Flush()
}
