package main

import (
    "fmt"

    "CrossPkgLib"
)

type opTable map[CrossPkgLib.Ticks]func(int, int) int

func main() {
    ops := opTable{
        CrossPkgLib.Ticks(2): func(a, b int) int { return a + b },
        CrossPkgLib.Ticks(3): func(a, b int) int { return a * b },
    }

    fmt.Println(ops[CrossPkgLib.Ticks(2)](4, 5), ops[CrossPkgLib.Ticks(3)](4, 5), len(ops))
}