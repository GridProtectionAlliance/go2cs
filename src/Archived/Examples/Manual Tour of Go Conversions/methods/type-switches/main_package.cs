/*
package main

import "fmt"

func do(i interface{}) {
    switch v := i.(type) {
    case int:
        fmt.Printf("Twice %v is %v\n", v, v*2)
    case string:
        fmt.Printf("%q is %v bytes long\n", v, len(v))
    default:
        fmt.Printf("I don't know about type %T!\n", v)
    }
}

func main() {
    do(21)
    do("hello")
    do(true)
}
*/
#region source
using go;
using fmt = go.fmt_package;
using static go.builtin;

static class main_package
{
    static void @do(object i) {
        switch (i.type()) {
            case int v:
                fmt.Printf("Twice {0} is {1}\n", v, v*2);
                break;
            case @string v:
                fmt.Printf("{0} is {1} bytes long\n", v, len(v));
                break;
            default: {
                var v = i.type();
                fmt.Printf("I don't know about type {0}!\n", GetGoTypeName(v));
                break;
            }
        }
    }

    static void Main() {
        @do(21);
        @do("hello");
        @do(true);
    }
}
#endregion
