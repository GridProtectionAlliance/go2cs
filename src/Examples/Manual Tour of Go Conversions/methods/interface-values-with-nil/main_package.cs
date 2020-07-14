/*
package main

import "fmt"

type I interface {
    M()
}

type T struct {
    S string
}

func (t *T) M() {
    if t == nil {
        fmt.Println("<nil>")
        return
    }
    fmt.Println(t.S)
}

func main() {
    var i I

    var t *T
    i = t
    describe(i)
    i.M()

    i = &T{"hello"}
    describe(i)
    i.M()
}

func describe(i I) {
    fmt.Printf("(%v, %T)\n", i, i)
}
*/
#region source
using go;
using fmt = go.fmt_package;
using static go.builtin;

static partial class main_package
{
    partial interface I {
        void M();
    }

    partial struct T {
        public @string S;
    }

    public static void M(this ref T t) {
        if (t == nil) {
            fmt.Println("<nil>");
            return;
        }
        fmt.Println(t.S);
    }

    static void Main() {
        I i;

        ptr<T> t = default!;
        i = I.As(t);
        describe(i);
        i.M();

        i = I.As(ptr(new T("hello")));
        describe(i);
        i.M();
    }

    static void describe(I i) {
        fmt.Println($"({i:v}, {i:T})");
    }
}
#endregion