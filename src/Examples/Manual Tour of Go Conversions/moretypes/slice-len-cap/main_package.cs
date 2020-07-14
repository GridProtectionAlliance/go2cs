/*
package main

import "fmt"

func main() {
    s := []int{2, 3, 5, 7, 11, 13}
    printSlice(s)

    // Slice the slice to give it zero length.
    s = s[:0]
    printSlice(s)

    // Extend its length.
    s = s[:4]
    printSlice(s)

    // Drop its first two values.
    s = s[2:]
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
        var s = slice(new[]{2, 3, 5, 7, 11, 13});
        printSlice(ref s);

        // Slice the slice to give it zero length.
        s = s[..0];
        printSlice(ref s);

        // Extend its length.
        s = s[..4];
        printSlice(ref s);

        // Drop its first two values.
        s = s[2..];
        printSlice(ref s);
    }

    static void printSlice(ref slice<int> s) {
        fmt.Printf("len={0} cap={1} {2}\n", len(s), cap(s), s);
    }
}
#endregion