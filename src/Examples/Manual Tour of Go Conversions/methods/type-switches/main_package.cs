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
using static go.builtin;

static class main_package
{
    static void @do(object i) {
        switch (i.type()) {
            case int v:
                println($"Twice {v} is {v * 2}");
                break;
            case @string v:
                println($"\"{v}\" is {len(v)} bytes long");
                break;
            default: {
                var v = i.type();
                println($"I don't know about type {GetGoTypeName(v)}!");
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
