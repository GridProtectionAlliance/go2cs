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
        {
            object v = i.type(); // "v" is scoped to switch

            switch (v)
            {
                case int v_i:
                    println($"Twice {v_i} is {v_i * 2}");
                    break;
                case @string v_s:
                    println($"\"{v_s}\" is {len(v_s)} bytes long");
                    break;
                default:
                    println($"I don't know about type {GetGoTypeName(v.GetType())}!");
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