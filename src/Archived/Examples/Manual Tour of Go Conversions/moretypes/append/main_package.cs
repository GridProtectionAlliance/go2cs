/*
package main

import "fmt"

func main() {
    var s []int
    printSlice(s)

    // append works on nil slices.
    s = append(s, 0)
    printSlice(s)

    // The slice grows as needed.
    s = append(s, 1)
    printSlice(s)

    // We can add more than one element at a time.
    s = append(s, 2, 3, 4)
    printSlice(s)
}

func printSlice(s []int) {
    fmt.Printf("len=%d cap=%d %v\n", len(s), cap(s), s)
}
*/
#region source
using go;
using fmt = go.fmt_package;
using static go.builtin;

static class main_package
{
    static void Main() {
        slice<int> s = default;
        printSlice(s);

        // append works on nil slices.
        s = append(s, 0);
        printSlice(s);

        // The slice grows as needed.
        s = append(s, 1);
        printSlice(s);

        // We can add more than one element at a time.
        s = append(s, 2, 3, 4);
        printSlice(s);
    }

    static void printSlice(in slice<int> s) {
        fmt.Printf("len={0} cap={1} {2}\n", len(s), cap(s), s);
    }
}
#endregion