/*
package main

import (
    "fmt"
    "io"
    "strings"
)

func main() {
    r := strings.NewReader("Hello, Reader!")

    b := make([]byte, 8)
    for {
        n, err := r.Read(b)
        fmt.Printf("n = %v err = %v b = %v\n", n, err, b)
        fmt.Printf("b[:n] = %q\n", b[:n])
        if err == io.EOF {
            break
        }
    }
}
*/
#region source
using go;
using static go.builtin;
using fmt = go.fmt_package;
using io = go.io_package;
using strings = go.strings_package;

static class main_package
{
    static void Main() {
        var r = strings.NewReader("Hello, Reader!");

        var b = make_slice<byte>(8);
        while (true) {
            var (n, err) = r.Read(b);
            fmt.Printf("n = {0} err = {1:v} b = {2}\n", n, err, b);
            fmt.Printf("b[:n] = {0}\n", new @string(b[..n]));
            if (err == io.EOF) {
                break;
            }
        }
    }
}
#endregion